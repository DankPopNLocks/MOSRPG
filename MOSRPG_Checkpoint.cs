using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MOSRPG_Checkpoint : UdonSharpBehaviour
{
    [Header("Respawn Manager")]
    [Tooltip("Assign the unified MOSRPG_RespawnManager for handling player respawns.")]
    public MOSRPG_RespawnManager respawnManager;

    [Header("Visual / Audio")]
    public Animator animator;
    public AudioSource audioSource;

    [Tooltip("Optional: Specific transform to use as the respawn point instead of this object's position.")]
    public Transform targetTransform;

    [Tooltip("Name of the bool parameter controlling checkpoint animation.")]
    public string animationBoolName = "IsActive";

    private bool isActive = false;

    // ---------------- Interaction ----------------

    public override void Interact()
    {
        if (respawnManager == null)
        {
            Debug.LogWarning("[MOSRPG_Checkpoint] RespawnManager reference missing!");
            return;
        }

        if (isActive)
        {
            Debug.Log("[MOSRPG_Checkpoint] Already active, skipping: " + name);
            return;
        }

        // Activate this checkpoint via the unified manager
        respawnManager.SetPlayerCheckpoint(this);
    }

    // ---------------- Transform Access ----------------

    public Transform GetRespawnTransform()
    {
        return targetTransform != null ? targetTransform : transform;
    }

    // ---------------- Activation ----------------

    public void ActivateCheckpoint()
    {
        isActive = true;

        if (animator != null && !string.IsNullOrEmpty(animationBoolName))
            animator.SetBool(animationBoolName, true);

        if (audioSource != null)
            audioSource.Play();

        Debug.Log("[MOSRPG_Checkpoint] Activated: " + name);
    }

    public void DeactivateCheckpoint()
    {
        isActive = false;

        if (animator != null && !string.IsNullOrEmpty(animationBoolName))
            animator.SetBool(animationBoolName, false);

        Debug.Log("[MOSRPG_Checkpoint] Deactivated: " + name);
    }
}
