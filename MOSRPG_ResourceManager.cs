using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

public class MOSRPG_ResourceManager : UdonSharpBehaviour
{
    [UdonSynced] public float health = 100f;

    [Header("Health Settings")]
    public float maxHealth = 100f;

    [Tooltip("Set the UI Image component with Image Type set to Filled, Fill Method Radial 360.")]
    public Image healthBarImage;

    public TextMeshProUGUI healthText;

    [Header("Respawn Manager")]
    [Tooltip("Unified respawn manager for both players and enemies.")]
    public MOSRPG_RespawnManager respawnManager;

    [Tooltip("True = Player, False = Enemy")]
    public bool isPlayer = false;

    // --- Player Speed Modification (optional) ---
    private bool speedApplied = false;
    private float originalWalk, originalRun, originalStrafe;

    private void Start()
    {
        health = Mathf.Clamp(health, 0f, maxHealth);
        UpdateHealthUI();
    }

    // ================= DAMAGE =================

    public void TakeDamage(float damageAmount)
    {
        if (damageAmount <= 0f) return;

        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            return;
        }

        health -= damageAmount;
        health = Mathf.Clamp(health, 0f, maxHealth);

        RequestSerialization();
        UpdateHealthUI();

        if (health <= 0f)
            HandleDeath();
    }

    public void Heal(float healAmount)
    {
        if (healAmount <= 0f) return;

        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            return;
        }

        health += healAmount;
        health = Mathf.Clamp(health, 0f, maxHealth);

        RequestSerialization();
        UpdateHealthUI();
    }

    public void ResetResource()
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            return;
        }

        health = maxHealth;
        RequestSerialization();
        UpdateHealthUI();
    }

    // ================= DEATH =================

    private void HandleDeath()
    {
        if (respawnManager != null)
        {
            respawnManager.HandleDeath(this);
        }
        else
        {
            Debug.LogWarning("[MOSRPG_ResourceManager] No RespawnManager assigned for " + gameObject.name);
        }
    }

    // ================= UI =================

    private void UpdateHealthUI()
    {
        if (healthBarImage != null)
            healthBarImage.fillAmount = health / maxHealth;

        if (healthText != null)
            healthText.text = $"{health:0}/{maxHealth:0}";
    }

    public override void OnDeserialization()
    {
        health = Mathf.Clamp(health, 0f, maxHealth);
        UpdateHealthUI();
    }

    // ================= SPEED MODIFICATION (Optional) =================

    public void ApplySpeed(VRCPlayerApi player, float walk, float run, float strafe)
    {
        if (!player.IsValid() || speedApplied) return;

        originalWalk = player.GetWalkSpeed();
        originalRun = player.GetRunSpeed();
        originalStrafe = player.GetStrafeSpeed();

        player.SetWalkSpeed(walk);
        player.SetRunSpeed(run);
        player.SetStrafeSpeed(strafe);

        speedApplied = true;
    }

    public void RestoreSpeed(VRCPlayerApi player)
    {
        if (!player.IsValid() || !speedApplied) return;

        player.SetWalkSpeed(originalWalk);
        player.SetRunSpeed(originalRun);
        player.SetStrafeSpeed(originalStrafe);

        speedApplied = false;
    }
}
