using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class MOSRPG_Interactable : UdonSharpBehaviour
{
    [Header("General")]
    public bool isInteractable = true;
    public string interactableName;
    public Sprite icon;

    [Header("Inventory / Consumable")]
    public bool canBeStored = false;
    public bool destroyAfterUse = true;

    [Tooltip("Target resource affected by this item")]
    public MOSRPG_ResourceManager resourceTarget;

    [Tooltip("Positive = heal / add currency, Negative = damage")]
    public float effectAmount = 0f;

    public MOSRPG_EconomyManager economyManager;

    [Header("Animator / Button")]
    public Animator targetAnimator;
    public string[] animatorBoolParameters;
    public bool toggleMode = true;
    private bool currentToggleState;

    [Header("Role Restrictions")]
    public bool useRoleRestrictions = false;
    public MOSRPG_RoleManager roleManager;
    public int requiredRoleIndex = 0;
    public bool allowNoneRole = true;

    [Header("Role Assignment")]
    public bool assignRoleOnInteract = false;
    public int roleIndexToAssign = 0;
    public MOSRPG_Interactable[] otherRoleButtons;

    [Header("Lobby / UI")]
    public MOSRPG_LobbyPlayerList lobbyPlayerList;
    public TextMeshProUGUI interactText;
    public string activeText = "Deactivate";
    public string inactiveText = "Activate";

    [Header("Debug")]
    public bool debugLogs = false;

    private VRC_Pickup pickup;
    private VRCPlayerApi localPlayer;

    // ================= INITIALIZATION =================

    private void Start()
    {
        pickup = GetComponent<VRC_Pickup>();
        localPlayer = Networking.LocalPlayer;
        UpdateInteractText();
    }

    private void Update()
    {
        if (pickup != null &&
            pickup.IsHeld &&
            pickup.currentPlayer == localPlayer &&
            isInteractable)
        {
            if (Input.GetKeyDown(KeyCode.E) ||
                Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                UseItem();
            }
        }
    }

    // ================= CONSUMABLE LOGIC =================

    public void UseItem()
    {
        if (!isInteractable || !CanInteract()) return;

        if (resourceTarget != null && effectAmount != 0f)
        {
            if (effectAmount > 0f)
                resourceTarget.Heal(effectAmount);
            else
                resourceTarget.TakeDamage(-effectAmount);
        }

        if (economyManager != null && effectAmount != 0f)
        {
            economyManager.TryAddCurrency(
                localPlayer,
                Mathf.RoundToInt(effectAmount)
            );
        }

        if (destroyAfterUse)
            CleanupAfterUse();
    }

    private void CleanupAfterUse()
    {
        if (pickup != null && pickup.IsHeld)
            pickup.Drop();

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        gameObject.SetActive(false);
    }

    // ================= INTERACTION =================

    public override void Interact()
    {
        if (!isInteractable || !CanInteract()) return;

        HandleRoleAssignment();
        HandleAnimatorToggle();
        UpdateInteractText();
    }

    private bool CanInteract()
    {
        if (!Utilities.IsValid(localPlayer)) return false;
        if (!useRoleRestrictions) return true;
        if (!Utilities.IsValid(roleManager)) return true;

        int role = roleManager.GetPlayerRole(localPlayer.playerId);
        if (allowNoneRole && role == 0) return true;

        return role == requiredRoleIndex;
    }

    // ================= ROLE ASSIGNMENT =================

    private void HandleRoleAssignment()
    {
        if (!assignRoleOnInteract || !Utilities.IsValid(roleManager)) return;

        roleManager.AssignLocalPlayerRole(roleIndexToAssign);

        if (otherRoleButtons != null)
        {
            foreach (var btn in otherRoleButtons)
            {
                if (Utilities.IsValid(btn))
                    btn.RefreshButtonText();
            }
        }

        RefreshButtonText();

        if (Utilities.IsValid(lobbyPlayerList))
            lobbyPlayerList.SendCustomEvent("RefreshList");
    }

    public void RefreshButtonText()
    {
        UpdateInteractText();
    }

    // ================= ANIMATOR =================

    private void HandleAnimatorToggle()
    {
        if (!Utilities.IsValid(targetAnimator)) return;

        currentToggleState = toggleMode
            ? !currentToggleState
            : true;

        foreach (var param in animatorBoolParameters)
        {
            if (!string.IsNullOrEmpty(param))
                targetAnimator.SetBool(param, currentToggleState);
        }
    }

    // ================= UI =================

    private void UpdateInteractText()
    {
        if (!Utilities.IsValid(interactText)) return;

        if (assignRoleOnInteract && Utilities.IsValid(roleManager))
        {
            int localId = Networking.LocalPlayer.playerId;
            bool inRole = roleManager.IsPlayerInRole(
                localId,
                roleIndexToAssign
            );

            interactText.text = inRole
                ? "Leave Team"
                : "Join Team";
        }
        else
        {
            interactText.text = currentToggleState
                ? activeText
                : inactiveText;
        }
    }

    public override void OnDeserialization()
    {
        UpdateInteractText();
    }
}
