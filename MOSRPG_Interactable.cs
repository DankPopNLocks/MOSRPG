using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class MOSRPG_Interactable : UdonSharpBehaviour
{
    [Header("Inventory")]
    public bool canBeStored = false;

    [Header("General")]
    public string interactableName;
    public Sprite icon;

    [Header("Pickup / Use (Optional)")]
    public bool destroyAfterUse = true;
    public MOSRPG_ResourceManager resourceTarget;
    public MOSRPG_EconomyManager economyManager;
    [Tooltip("Positive = heal / add currency, Negative = damage")]
    public float effectAmount = 0f;

    private VRC_Pickup pickup;
    private VRCPlayerApi localPlayer;

    [Header("Gear / Weapon (Optional)")]
    public GameObject weaponObject;
    public Collider weaponCollider;
    public float minDamage = 5f;
    public float maxDamage = 15f;
    public float damageCooldown = 0.5f;
    private float lastHitTime;

    [Header("Animator / Button (Optional)")]
    public Animator targetAnimator;
    public string[] animatorBoolParameters;
    public bool toggleMode = true;
    private bool currentToggleState;

    [Header("Role Restrictions (Optional)")]
    public bool useRoleRestrictions = false;
    public MOSRPG_RoleManager roleManager;
    public int requiredRoleIndex = 0;
    public bool allowNoneRole = true;

    [Header("Role Assignment (Optional)")]
    public bool assignRoleOnInteract = false;
    public int roleIndexToAssign = 0;

    [Header("Other Role Buttons (Assign in inspector)")]
    public MOSRPG_Interactable[] otherRoleButtons;

    [Header("Lobby / UI (Optional)")]
    public MOSRPG_LobbyPlayerList lobbyPlayerList;

    [Header("UI (Optional)")]
    public TextMeshProUGUI interactText;
    public string activeText = "Deactivate";
    public string inactiveText = "Activate";

    [Header("Debug")]
    public bool debugLogs = false;

    // ---------------- Initialization ----------------
    private void Start()
    {
        pickup = GetComponent<VRC_Pickup>();
        localPlayer = Networking.LocalPlayer;

        if (weaponCollider != null)
        {
            weaponCollider.isTrigger = true;
            weaponCollider.enabled = false;
        }

        if (weaponObject != null)
            weaponObject.SetActive(false);

        UpdateInteractText();
    }

    private void Update()
    {
        if (pickup == null || localPlayer == null) return;

        if (pickup.IsHeld && pickup.currentPlayer == localPlayer)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton1))
                UseItem();
        }
    }

    // ---------------- Use / Consumable Logic ----------------
    public void UseItem()
    {
        if (!CanInteract()) return;

        if (resourceTarget != null && effectAmount != 0f)
        {
            if (effectAmount > 0f)
                resourceTarget.Heal(effectAmount);
            else
                resourceTarget.TakeDamage(-effectAmount);
        }

        if (economyManager != null && effectAmount != 0f)
        {
            economyManager.TryAddCurrency(localPlayer, Mathf.RoundToInt(effectAmount));
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

    // ---------------- Interaction ----------------
    public override void Interact()
    {
        if (!CanInteract()) return;

        HandleRoleAssignment();
        HandleAnimatorToggle();
        UpdateInteractText();
    }

    private bool CanInteract()
    {
        if (!Utilities.IsValid(localPlayer))
        {
            if (debugLogs) Debug.Log("[MOSRPG_Interactable] Local player invalid");
            return false;
        }

        if (!useRoleRestrictions)
            return true;

        if (!Utilities.IsValid(roleManager))
        {
            if (debugLogs) Debug.Log("[MOSRPG_Interactable] RoleManager not assigned, allowing interaction");
            return true;
        }

        int role = roleManager.GetPlayerRole(localPlayer.playerId);
        if (debugLogs) Debug.Log($"[MOSRPG_Interactable] Player role: {role}, Required: {requiredRoleIndex}");

        if (allowNoneRole && role == 0) return true;

        return role == requiredRoleIndex;
    }

    // ---------------- Role Assignment & Lobby Refresh ----------------
    private void HandleRoleAssignment()
    {
        if (!assignRoleOnInteract || !Utilities.IsValid(roleManager)) return;

        roleManager.AssignLocalPlayerRole(roleIndexToAssign);

        if (otherRoleButtons != null)
        {
            foreach (var btn in otherRoleButtons)
            {
                if (!Utilities.IsValid(btn)) continue;
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

    // ---------------- Animator ----------------
    private void HandleAnimatorToggle()
    {
        if (!Utilities.IsValid(targetAnimator)) return;

        currentToggleState = toggleMode ? !currentToggleState : true;

        foreach (var param in animatorBoolParameters)
        {
            if (!string.IsNullOrEmpty(param))
                targetAnimator.SetBool(param, currentToggleState);
        }
    }

    // ---------------- UI ----------------
    private void UpdateInteractText()
    {
        if (!Utilities.IsValid(interactText)) return;

        if (assignRoleOnInteract && Utilities.IsValid(roleManager))
        {
            int localId = Networking.LocalPlayer.playerId;
            bool inRole = roleManager.IsPlayerInRole(localId, roleIndexToAssign);
            interactText.text = inRole ? "Leave Team" : "Join Team";
        }
        else
        {
            interactText.text = currentToggleState ? activeText : inactiveText;
        }
    }

    public override void OnDeserialization()
    {
        UpdateInteractText();
    }

    // ---------------- Gear / Weapon ----------------
    private void OnTriggerEnter(Collider other)
    {
        if (weaponCollider == null || !weaponCollider.enabled) return;
        if (Time.time - lastHitTime < damageCooldown) return;

        MOSRPG_ResourceManager resource = other.GetComponent<MOSRPG_ResourceManager>();
        if (resource != null)
        {
            float dmg = Random.Range(minDamage, maxDamage);
            resource.TakeDamage(dmg);
            lastHitTime = Time.time;
        }
    }

    public void Equip()
    {
        if (weaponCollider != null) weaponCollider.enabled = true;
        if (weaponObject != null) weaponObject.SetActive(true);
    }

    public void Unequip()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
        if (weaponObject != null) weaponObject.SetActive(false);
    }
}
