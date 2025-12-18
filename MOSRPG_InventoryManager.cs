using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

public class MOSRPG_InventoryManager : UdonSharpBehaviour
{
    [Header("UI References")]
    public Image[] slotUIImages;
    public Image[] slotItemIcons;
    public Image highlightImage;
    public Sprite defaultSlotIcon;

    [Header("Raycast / Spawn Settings")]
    public float raycastDistance = 2f;
    public float spawnDistance = 3f;
    public float surfaceOffset = 0.1f;

    [Header("Keybindings")]
    public KeyCode cycleLeftKey = KeyCode.Q;
    public KeyCode cycleRightKey = KeyCode.R;
    public KeyCode storeKey = KeyCode.F;
    public KeyCode spawnKey = KeyCode.G;
    public KeyCode useKey = KeyCode.E;

    private MOSRPG_Interactable[] inventory;
    private int currentIndex;

    private VRCPlayerApi localPlayer;

    // ---------------- Initialization ----------------

    private void Start()
    {
        localPlayer = Networking.LocalPlayer;

        int slotCount = slotUIImages.Length;
        inventory = new MOSRPG_Interactable[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            if (slotUIImages[i] != null)
            {
                slotUIImages[i].sprite = defaultSlotIcon;
                slotUIImages[i].enabled = true;
            }

            if (slotItemIcons != null && i < slotItemIcons.Length && slotItemIcons[i] != null)
            {
                slotItemIcons[i].sprite = null;
                slotItemIcons[i].enabled = false;
            }
        }

        UpdateUI();
    }

    // ---------------- Update ----------------

    private void Update()
    {
        if (!Utilities.IsValid(localPlayer)) return;

        if (Input.GetKeyDown(cycleLeftKey)) CycleSlot(-1);
        if (Input.GetKeyDown(cycleRightKey)) CycleSlot(1);
        if (Input.GetKeyDown(storeKey)) TryStoreItem();
        if (Input.GetKeyDown(spawnKey)) TrySpawnItem();
        if (Input.GetKeyDown(useKey)) TryUseItem();
    }

    // ---------------- Inventory Logic ----------------

    private void CycleSlot(int direction)
    {
        int slotCount = inventory.Length;
        currentIndex = (currentIndex + direction + slotCount) % slotCount;
        UpdateUI();
    }

    private void TryStoreItem()
    {
        if (inventory[currentIndex] != null) return;

        Ray ray = new Ray(
            localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position,
            localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * Vector3.forward
        );

        if (!Physics.Raycast(ray, out RaycastHit hit, raycastDistance)) return;

        // IMPORTANT FIX: allow child colliders
        MOSRPG_Interactable item = hit.collider.GetComponentInParent<MOSRPG_Interactable>();
        if (!Utilities.IsValid(item)) return;
        if (!item.canBeStored) return;

        Networking.SetOwner(localPlayer, item.gameObject);

        // Reset pickup & physics
        VRC_Pickup pickup = item.GetComponent<VRC_Pickup>();
        if (pickup != null && pickup.IsHeld)
            pickup.Drop();

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        item.gameObject.SetActive(false);
        inventory[currentIndex] = item;

        Debug.Log($"[MOSRPG_InventoryManager] Stored item: {item.interactableName}");
        UpdateUI();
    }

    private void TrySpawnItem()
    {
        MOSRPG_Interactable item = inventory[currentIndex];
        if (!Utilities.IsValid(item)) return;

        Vector3 origin = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
        Vector3 direction = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * Vector3.forward;

        Vector3 spawnPos = origin + direction * spawnDistance;
        Quaternion spawnRot = Quaternion.LookRotation(direction);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, spawnDistance))
        {
            spawnPos = hit.point + hit.normal * surfaceOffset;
            spawnRot = Quaternion.LookRotation(-hit.normal);
        }

        item.transform.SetPositionAndRotation(spawnPos, spawnRot);
        item.gameObject.SetActive(true);

        Networking.SetOwner(localPlayer, item.gameObject);

        inventory[currentIndex] = null;
        UpdateUI();
    }

    private void TryUseItem()
    {
        MOSRPG_Interactable item = inventory[currentIndex];
        if (!Utilities.IsValid(item)) return;

        item.UseItem();

        if (item.destroyAfterUse)
        {
            inventory[currentIndex] = null;
            UpdateUI();
        }
    }

    // ---------------- UI ----------------

    private void UpdateUI()
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (slotUIImages[i] != null)
            {
                slotUIImages[i].sprite = defaultSlotIcon;
                slotUIImages[i].enabled = true;
            }

            if (slotItemIcons[i] != null)
            {
                Sprite icon = inventory[i] != null ? inventory[i].icon : null;
                slotItemIcons[i].sprite = icon;
                slotItemIcons[i].enabled = icon != null;
            }
        }

        if (highlightImage != null && currentIndex < slotUIImages.Length)
        {
            highlightImage.transform.position = slotUIImages[currentIndex].transform.position;
        }
    }
}
