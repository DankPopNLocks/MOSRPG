using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MOSRPG_RespawnManager : UdonSharpBehaviour
{
    [Header("Player Respawn Settings")]
    [Tooltip("Starting position where players respawn if no checkpoint is active.")]
    public Transform playerStartingLocation;

    private Transform currentPlayerCheckpoint;
    private MOSRPG_Checkpoint activeCheckpoint;

    [Header("Enemy Respawn Settings")]
    [Tooltip("Default respawn point for enemies if they have no individual point.")]
    public Transform enemyRespawnPoint;
    public float deathDisableDelay = 2f;
    public float respawnDelay = 5f;

    // Use a fixed-size array for pending enemies
    private GameObject[] pendingEnemies = new GameObject[32];
    private int pendingCount = 0;

    // ================= PLAYER CHECKPOINT =================

    public void SetPlayerCheckpoint(MOSRPG_Checkpoint checkpoint)
    {
        if (checkpoint == null) return;

        if (activeCheckpoint != null && activeCheckpoint != checkpoint)
            activeCheckpoint.DeactivateCheckpoint();

        activeCheckpoint = checkpoint;
        currentPlayerCheckpoint = checkpoint.GetRespawnTransform();
        checkpoint.ActivateCheckpoint();

        Debug.Log("[MOSRPG_RespawnManager] Player checkpoint set: " + checkpoint.name);
    }

    // ================= HANDLE DEATH =================

    public void HandleDeath(MOSRPG_ResourceManager resource)
    {
        if (resource == null) return;

        if (resource.isPlayer)
        {
            RespawnPlayer(resource);
        }
        else
        {
            HandleEnemyDeath(resource);
        }
    }

    // ================= PLAYER RESPAWN =================

    private void RespawnPlayer(MOSRPG_ResourceManager playerResource)
    {
        if (playerResource == null || Networking.LocalPlayer == null) return;

        Transform respawnPoint = currentPlayerCheckpoint != null
            ? currentPlayerCheckpoint
            : playerStartingLocation;

        if (respawnPoint != null)
        {
            Networking.LocalPlayer.TeleportTo(respawnPoint.position, respawnPoint.rotation);
            Debug.Log("[MOSRPG_RespawnManager] Player respawned at: " +
                      (currentPlayerCheckpoint != null ? currentPlayerCheckpoint.name : "Starting Location"));
        }
        else
        {
            Debug.LogWarning("[MOSRPG_RespawnManager] No valid respawn point set!");
        }

        playerResource.ResetResource();
    }

    // ================= ENEMY RESPAWN =================

    public void HandleEnemyDeath(MOSRPG_ResourceManager enemyResource)
    {
        if (enemyResource == null) return;

        if (!Networking.IsOwner(enemyResource.gameObject))
            Networking.SetOwner(Networking.LocalPlayer, enemyResource.gameObject);

        Animator animator = enemyResource.GetComponent<Animator>();
        if (animator != null) animator.SetTrigger("Die");

        AudioSource audio = enemyResource.GetComponent<AudioSource>();
        if (audio != null) audio.Play();

        // Only add if not already in the array
        bool alreadyPending = false;
        for (int i = 0; i < pendingCount; i++)
        {
            if (pendingEnemies[i] == enemyResource.gameObject)
            {
                alreadyPending = true;
                break;
            }
        }

        if (!alreadyPending && pendingCount < pendingEnemies.Length)
        {
            pendingEnemies[pendingCount] = enemyResource.gameObject;
            pendingCount++;
        }

        SendCustomEventDelayedSeconds(nameof(DisablePendingEnemies), deathDisableDelay);
    }

    public void DisablePendingEnemies()
    {
        // Disable all currently pending enemies
        for (int i = 0; i < pendingCount; i++)
        {
            GameObject enemyGO = pendingEnemies[i];
            if (enemyGO != null)
                enemyGO.SetActive(false);
        }

        SendCustomEventDelayedSeconds(nameof(RespawnPendingEnemies), respawnDelay);
    }

    public void RespawnPendingEnemies()
    {
        for (int i = 0; i < pendingCount; i++)
        {
            GameObject enemyGO = pendingEnemies[i];
            if (enemyGO == null) continue;

            Transform respawnPoint = enemyRespawnPoint != null ? enemyRespawnPoint : enemyGO.transform;

            enemyGO.transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);

            MOSRPG_ResourceManager resource = enemyGO.GetComponent<MOSRPG_ResourceManager>();
            if (resource != null) resource.ResetResource();

            enemyGO.SetActive(true);
            Debug.Log("[MOSRPG_RespawnManager] Enemy respawned at: " + enemyGO.transform.position);

            pendingEnemies[i] = null; // Clear reference
        }

        pendingCount = 0; // Reset count
    }
}
