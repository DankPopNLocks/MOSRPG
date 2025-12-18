using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class MOSRPG_StartGameButton : UdonSharpBehaviour
{
    [Header("Dependencies")]
    public MOSRPG_RoleManager roleManager;
    public Transform[] teamRespawns; // assign respawn positions in inspector
    public Transform lobbySpawn;      // fallback lobby spawn
    public TMP_Text buttonText;       // optional TMP reference
    public bool enableDebugLogs = true;

    [Header("Game Settings")]
    public float gameDuration = 60f;  // duration in seconds
    public bool autoResetGame = true;

    private bool gameActive = false;
    private float timer;

    void Update()
    {
        if (!gameActive) return;

        timer -= Time.deltaTime;
        if (enableDebugLogs)
            Debug.Log($"[MOSRPG_StartGameButton] Time left: {timer:F1}s");

        if (timer <= 0f)
        {
            EndGame();
        }
    }

    public override void Interact()
    {
        VRCPlayerApi local = Networking.LocalPlayer;

        if (!Networking.IsOwner(gameObject))
        {
            if (enableDebugLogs)
                Debug.Log("[MOSRPG_StartGameButton] Only the instance owner can start the game.");
            return;
        }

        int localRole = roleManager.GetPlayerRole(local.playerId);
        if (localRole == 0)
        {
            if (enableDebugLogs)
                Debug.Log("[MOSRPG_StartGameButton] You must be assigned to a team to start the game.");
            return;
        }

        if (!gameActive)
        {
            StartGame();
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("[MOSRPG_StartGameButton] Game is already active.");
        }
    }

    private void StartGame()
    {
        if (enableDebugLogs)
            Debug.Log("[MOSRPG_StartGameButton] Starting game!");

        gameActive = true;
        timer = gameDuration;

        // Teleport all team players to their respawns
        VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(players);

        foreach (var player in players)
        {
            if (!Utilities.IsValid(player)) continue;

            int role = roleManager.GetPlayerRole(player.playerId);
            if (role == 0) continue; // skip None

            if (role - 1 < teamRespawns.Length)
            {
                player.TeleportTo(teamRespawns[role - 1].position, teamRespawns[role - 1].rotation);
                if (enableDebugLogs)
                    Debug.Log($"[MOSRPG_StartGameButton] Teleported {player.displayName} to Team {role} spawn.");
            }
            else
            {
                if (enableDebugLogs)
                    Debug.LogWarning($"[MOSRPG_StartGameButton] No spawn assigned for Team {role}, player stays in lobby.");
            }
        }
    }

    private void EndGame()
    {
        if (enableDebugLogs)
            Debug.Log("[MOSRPG_StartGameButton] Ending game, returning players to lobby.");

        gameActive = false;

        if (!autoResetGame) return;

        VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(players);

        foreach (var player in players)
        {
            if (!Utilities.IsValid(player)) continue;

            int role = roleManager.GetPlayerRole(player.playerId);
            if (role == 0 || lobbySpawn == null) continue;

            player.TeleportTo(lobbySpawn.position, lobbySpawn.rotation);
            if (enableDebugLogs)
                Debug.Log($"[MOSRPG_StartGameButton] {player.displayName} returned to lobby.");
        }
    }
}
