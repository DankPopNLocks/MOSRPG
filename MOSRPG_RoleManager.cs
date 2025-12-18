using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class MOSRPG_RoleManager : UdonSharpBehaviour
{
    [Header("Role Settings")]
    [Tooltip("Index 0 must always be 'None'")]
    public string[] roleNames;
    public bool allowMultipleRoles = false;

    [Header("Team Spawns")]
    public Transform[] teamCheckpoints;

    [Header("UI Lists")]
    public UdonBehaviour[] lobbyPlayerLists;

    [Header("Sync")]
    [UdonSynced] private int[] syncedPlayerIds = new int[64];
    [UdonSynced] private int[] syncedRoles = new int[64];
    [UdonSynced] private int syncedPlayerCount = 0;

    private const int ROLE_NONE = 0;

    [Header("Debug")]
    public bool debugLogs = false;

    // ---------------- Initialization ----------------

    private void Start()
    {
        if (roleNames == null || roleNames.Length == 0)
        {
            roleNames = new string[] { "None" };
        }

        if (string.IsNullOrWhiteSpace(roleNames[0]))
        {
            roleNames[0] = "None";
        }
    }

    // ---------------- Role Assignment ----------------

    public void AssignLocalPlayerRole(int roleIndex)
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (!Utilities.IsValid(local)) return;
        if (!IsValidRoleIndex(roleIndex)) return;

        int playerId = local.playerId;
        int currentRole = GetPlayerRole(playerId);

        if (debugLogs)
            Debug.Log($"[RoleManager] Player {playerId} current={currentRole}, requested={roleIndex}");

        // --- TOGGLE OFF (leave team) ---
        if (currentRole == roleIndex)
        {
            SetPlayerRole(playerId, ROLE_NONE);
            NotifyLobbyLists();
            return;
        }

        // --- SINGLE ROLE ENFORCEMENT ---
        if (!allowMultipleRoles && currentRole != ROLE_NONE)
        {
            SetPlayerRole(playerId, ROLE_NONE);
        }

        // --- ASSIGN NEW ROLE ---
        SetPlayerRole(playerId, roleIndex);
        NotifyLobbyLists();
    }

    private void SetPlayerRole(int playerId, int roleIndex)
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        int index = FindPlayerIndex(playerId);

        if (index >= 0)
        {
            syncedRoles[index] = roleIndex;
        }
        else
        {
            if (syncedPlayerCount >= syncedPlayerIds.Length)
            {
                Debug.LogWarning("[RoleManager] Player limit reached");
                return;
            }

            syncedPlayerIds[syncedPlayerCount] = playerId;
            syncedRoles[syncedPlayerCount] = roleIndex;
            syncedPlayerCount++;
        }

        if (debugLogs)
            Debug.Log($"[RoleManager] Player {playerId} set to role {roleIndex}");

        RequestSerialization();
    }

    // ---------------- Queries ----------------

    public int GetPlayerRole(int playerId)
    {
        int idx = FindPlayerIndex(playerId);
        return (idx >= 0) ? syncedRoles[idx] : ROLE_NONE;
    }

    public bool IsPlayerInRole(int playerId, int roleIndex)
    {
        return GetPlayerRole(playerId) == roleIndex;
    }

    public string GetRoleName(int roleIndex)
    {
        if (!IsValidRoleIndex(roleIndex)) return "None";
        return roleNames[roleIndex];
    }

    public Transform GetTeamCheckpoint(int roleIndex)
    {
        if (teamCheckpoints == null || roleIndex < 0 || roleIndex >= teamCheckpoints.Length)
            return null;

        return teamCheckpoints[roleIndex];
    }

    // ---------------- Helpers ----------------

    private int FindPlayerIndex(int playerId)
    {
        for (int i = 0; i < syncedPlayerCount; i++)
        {
            if (syncedPlayerIds[i] == playerId)
                return i;
        }
        return -1;
    }

    private bool IsValidRoleIndex(int index)
    {
        return roleNames != null && index >= 0 && index < roleNames.Length;
    }

    // ---------------- Lobby Refresh ----------------

    private void NotifyLobbyLists()
    {
        if (lobbyPlayerLists == null) return;

        foreach (var list in lobbyPlayerLists)
        {
            if (list != null)
            {
                list.SendCustomEvent("RefreshList");
            }
        }
    }

    public override void OnDeserialization()
    {
        NotifyLobbyLists();
    }
}
