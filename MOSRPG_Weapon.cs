using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MOSRPG_Weapon : UdonSharpBehaviour
{
    [Header("Damage Settings")]
    public float minDamage = 5f;
    public float maxDamage = 15f;
    public float repeatInterval = 1f;
    public float detectionRadius = 1.5f;

    [Header("Target Settings")]
    public bool affectPlayers = true;
    public bool affectEnemies = false;

    [Tooltip("Reference to the local player's ResourceManager")]
    public MOSRPG_ResourceManager localPlayerResource;

    [Header("Movement Settings (Players Only)")]
    public bool modifyPlayerSpeed = false;
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float strafeSpeed = 2f;

    private float lastDamageTime;

    private bool speedApplied;
    private float originalWalk;
    private float originalRun;
    private float originalStrafe;

    private void Update()
    {
        float currentTime = Time.time;

        // --- PLAYER DAMAGE ---
        HandlePlayerDamage(currentTime);

        // --- ENEMY DAMAGE ---
        HandleEnemyDamage(currentTime);
    }

    private void HandlePlayerDamage(float currentTime)
    {
        if (!affectPlayers || localPlayerResource == null) return;

        float distance = Vector3.Distance(transform.position, Networking.LocalPlayer.GetPosition());

        if (distance <= detectionRadius)
        {
            if (currentTime - lastDamageTime >= repeatInterval)
            {
                float damage = Random.Range(minDamage, maxDamage);
                localPlayerResource.TakeDamage(damage);
                lastDamageTime = currentTime;
            }

            if (modifyPlayerSpeed && !speedApplied)
                ApplySpeed(Networking.LocalPlayer);
        }
        else
        {
            if (modifyPlayerSpeed && speedApplied)
                RestoreSpeed(Networking.LocalPlayer);
        }
    }

    private void HandleEnemyDamage(float currentTime)
    {
        if (!affectEnemies) return;
        if (currentTime - lastDamageTime < repeatInterval) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        if (hits == null || hits.Length == 0) return;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hit = hits[i];
            if (hit == null) continue;

            MOSRPG_ResourceManager resource = hit.GetComponentInParent<MOSRPG_ResourceManager>();
            if (resource == null || resource.isPlayer) continue;

            float damage = Random.Range(minDamage, maxDamage);
            resource.TakeDamage(damage);

            lastDamageTime = currentTime;
            break; // one hit per interval
        }
    }

    private void ApplySpeed(VRCPlayerApi player)
    {
        originalWalk = player.GetWalkSpeed();
        originalRun = player.GetRunSpeed();
        originalStrafe = player.GetStrafeSpeed();

        player.SetWalkSpeed(walkSpeed);
        player.SetRunSpeed(runSpeed);
        player.SetStrafeSpeed(strafeSpeed);

        speedApplied = true;
    }

    private void RestoreSpeed(VRCPlayerApi player)
    {
        player.SetWalkSpeed(originalWalk);
        player.SetRunSpeed(originalRun);
        player.SetStrafeSpeed(originalStrafe);

        speedApplied = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
#endif
}
