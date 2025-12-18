using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class MOSRPG_EconomyManager : UdonSharpBehaviour
{
    [Header("PvP Settings")]
    public bool allowCurrencyDropOnDeath = false;

    [Header("Default Settings")]
    [Tooltip("Starting currency for new players")]
    public int defaultCurrency = 0;

    [Header("Passive Currency Settings")]
    public bool enablePassiveCurrency = false;
    public int currencyPerTick = 1;
    public float tickInterval = 5f;

    [Header("UI References (Local Player Only)")]
    public TextMeshProUGUI currencyText;
    public Image currencyIcon;

    [UdonSynced] private int[] syncedPlayerIds = new int[64];
    [UdonSynced] private int[] syncedBalances = new int[64];
    [UdonSynced] private int syncedPlayerCount = 0;

    private int[] playerIds = new int[64];
    private int[] playerBalances = new int[64];
    private int playerCount = 0;

    private float tickTimer = 0f;

    private void Start()
    {
        CopySyncedToLocal();

        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (localPlayer == null) return;

        if (GetBalance(localPlayer) <= 0)
        {
            TryAddCurrency(localPlayer, defaultCurrency);
        }
        else
        {
            UpdateUI(localPlayer);
        }
    }

    private void Update()
    {
        if (!enablePassiveCurrency) return;

        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (localPlayer == null) return;

        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            TryAddCurrency(localPlayer, currencyPerTick);
        }
    }

    private int GetPlayerIndex(VRCPlayerApi player)
    {
        if (player == null) return -1;
        int id = player.playerId;
        for (int i = 0; i < playerCount; i++)
        {
            if (playerIds[i] == id) return i;
        }
        return -1;
    }

    public int GetBalance(VRCPlayerApi player)
    {
        int index = GetPlayerIndex(player);
        if (index == -1) return 0;
        return playerBalances[index];
    }

    public bool TryAddCurrency(VRCPlayerApi player, int amount)
    {
        if (player == null || amount <= 0) return false;

        int index = GetPlayerIndex(player);
        if (index == -1)
        {
            if (playerCount >= playerIds.Length) return false;
            playerIds[playerCount] = player.playerId;
            playerBalances[playerCount] = amount;
            playerCount++;
        }
        else
        {
            playerBalances[index] += amount;
        }

        Debug.Log($"[MOSRPG_Economy] {player.displayName} gained {amount}. Total: {GetBalance(player)}");

        if (player.isLocal) UpdateUI(player);

        SyncLocalToSynced();
        return true;
    }

    public bool TryRemoveCurrency(VRCPlayerApi player, int amount)
    {
        if (player == null || amount <= 0) return false;

        int index = GetPlayerIndex(player);
        if (index == -1 || playerBalances[index] < amount) return false;

        playerBalances[index] -= amount;

        Debug.Log($"[MOSRPG_Economy] {player.displayName} lost {amount}. Total: {GetBalance(player)}");

        if (player.isLocal) UpdateUI(player);

        SyncLocalToSynced();
        return true;
    }

    public bool TryTransferCurrency(VRCPlayerApi from, VRCPlayerApi to, int amount)
    {
        if (from == null || to == null || amount <= 0) return false;
        if (!TryRemoveCurrency(from, amount)) return false;
        return TryAddCurrency(to, amount);
    }

    public void DropCurrency(VRCPlayerApi player, int amount)
    {
        if (player == null || amount <= 0) return;
        if (!TryRemoveCurrency(player, amount)) return;

        Debug.Log($"[MOSRPG_Economy] {player.displayName} dropped {amount} (logical)");

        if (player.isLocal) UpdateUI(player);
    }

    private void UpdateUI(VRCPlayerApi player)
    {
        if (!player.isLocal) return;

        int balance = GetBalance(player);

        if (currencyText != null)
            currencyText.text = balance.ToString();

        if (currencyIcon != null)
            currencyIcon.enabled = true;
    }

    private void CopySyncedToLocal()
    {
        playerCount = syncedPlayerCount;
        for (int i = 0; i < syncedPlayerCount; i++)
        {
            playerIds[i] = syncedPlayerIds[i];
            playerBalances[i] = syncedBalances[i];
        }
    }

    private void SyncLocalToSynced()
    {
        syncedPlayerCount = playerCount;
        for (int i = 0; i < playerCount; i++)
        {
            syncedPlayerIds[i] = playerIds[i];
            syncedBalances[i] = playerBalances[i];
        }

        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        CopySyncedToLocal();

        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (localPlayer != null)
            UpdateUI(localPlayer);
    }
}
