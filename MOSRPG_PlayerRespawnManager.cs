using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class MOSRPG_PlayerRespawnManager : UdonSharpBehaviour
{
    [Header("Respawn Settings")]
    [Tooltip("The starting position where players will respawn if no checkpoint has been reached yet.")]
    public Transform startingLocation;

    private Transform currentCheckpoint;
    private MOSRPG_Checkpoint activeCheckpoint; // Updated type

    // ---------------- Checkpoint Handling ----------------

    // Called by a checkpoint to activate itself
    public void ActivateCheckpoint(MOSRPG_Checkpoint checkpoint) // Updated type
    {
        if (checkpoint == null) return;

        // Deactivate the currently active checkpoint (if any and not the same)
        if (activeCheckpoint != null && activeCheckpoint != checkpoint)
        {
            activeCheckpoint.DeactivateCheckpoint();
        }

        // Update the active checkpoint
        activeCheckpoint = checkpoint;

        // Set the new checkpoint transform
        SetCheckpoint(checkpoint.GetRespawnTransform());

        // Tell the checkpoint to activate itself
        checkpoint.ActivateCheckpoint();
    }

    // Set the current checkpoint transform
    public void SetCheckpoint(Transform checkpointTransform)
    {
        if (checkpointTransform == null) return;
        currentCheckpoint = checkpointTransform;
        Debug.Log("[MOSRPG_PlayerRespawnManager] Checkpoint set to: " + checkpointTransform.name);
    }

    // ---------------- Player Respawn ----------------

    // Respawn the local player at the checkpoint or starting location
    public void RespawnPlayer(MOSRPG_ResourceManager playerResource)
    {
        if (playerResource == null || Networking.LocalPlayer == null) return;

        Transform respawnPoint = currentCheckpoint != null ? currentCheckpoint : startingLocation;

        if (respawnPoint != null)
        {
            Networking.LocalPlayer.TeleportTo(
                respawnPoint.position,
                respawnPoint.rotation
            );

            string pointName = currentCheckpoint != null ? currentCheckpoint.name : "Starting Location";
            Debug.Log("[MOSRPG_PlayerRespawnManager] Player respawned at: " + pointName);
        }
        else
        {
            Debug.LogWarning("[MOSRPG_PlayerRespawnManager] No valid respawn location set.");
        }

        // Reset the player's resource (health)
        playerResource.ResetResource();
    }
}
