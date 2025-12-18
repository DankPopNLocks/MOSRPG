using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MOSRPG_EnemyWeapon : UdonSharpBehaviour
{
    [Header("Damage Settings")]
    public float minDamage = 5f;
    public float maxDamage = 15f;
    public float repeatInterval = 1f;

    [Header("Target Settings")]
    public bool affectPlayers = true;
    public bool affectEnemies = false;
    public MOSRPG_ResourceManager localPlayerResource;
    public float detectionRadius = 1.5f;

    [Header("Movement Settings")]
    public bool modifyPlayerSpeed = false;
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float strafeSpeed = 2f;

    private float lastDamageTime;

    // Internal state for restoring movement
    private bool speedApplied = false;
    private float originalWalk;
    private float originalRun;
    private float originalStrafe;

    private void Update()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (localPlayer == null) return;

        float currentTime = Time.time;
        float distance = Vector3.Distance(transform.position, localPlayer.GetPosition());

        // --- DAMAGE LOGIC ---
        if (affectPlayers && localPlayerResource != null)
        {
            if (distance <= detectionRadius)
            {
                if (currentTime - lastDamageTime >= repeatInterval)
                {
                    float damage = Random.Range(minDamage, maxDamage);
                    localPlayerResource.TakeDamage(damage);
                    lastDamageTime = currentTime;
                }
            }
        }

        // --- SPEED MODIFICATION LOGIC ---
        if (modifyPlayerSpeed)
        {
            if (distance <= detectionRadius)
            {
                if (!speedApplied)
                {
                    ApplySpeed(localPlayer);
                }
            }
            else
            {
                if (speedApplied)
                {
                    RestoreSpeed(localPlayer);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!affectEnemies) return;

        MOSRPG_ResourceManager resource = other.GetComponent<MOSRPG_ResourceManager>();
        if (resource == null) return;

        float currentTime = Time.time;
        if (currentTime - lastDamageTime < repeatInterval) return;

        float damage = Random.Range(minDamage, maxDamage);
        resource.TakeDamage(damage);
        lastDamageTime = currentTime;
    }

    // --- SPEED CONTROL FUNCTIONS ---

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
}
