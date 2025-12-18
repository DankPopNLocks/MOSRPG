using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

public class MOSRPG_ResourceManager : UdonSharpBehaviour
{
    [UdonSynced] public float health = 100.0f;

    [Header("Health Settings")]
    public float maxHealth = 100f;

    [Tooltip("Set the UI Image component with Image Type set to Filled, Fill Method Radial 360.")]
    public Image healthBarImage;

    public TextMeshProUGUI healthText;

    [Header("Respawn Managers")]
    public MOSRPG_PlayerRespawnManager playerRespawnManager;
    public MOSRPG_EnemyRespawnManager enemyRespawnManager;

    [Tooltip("Set true if this is a player, false if an enemy.")]
    public bool isPlayer = false;

    private void Start()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUI();
    }

    // ---------------- Resource Management ----------------

    public void TakeDamage(float damageAmount)
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (damageAmount <= 0) return;

        health -= damageAmount;
        health = Mathf.Clamp(health, 0, maxHealth);

        RequestSerialization();
        UpdateHealthUI();

        if (health <= 0)
            HandleDeath();
    }

    public void Heal(float healAmount)
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (healAmount <= 0) return;

        health += healAmount;
        health = Mathf.Clamp(health, 0, maxHealth);

        RequestSerialization();
        UpdateHealthUI();
    }

    public void ResetResource()
    {
        if (!Networking.IsOwner(gameObject)) return;

        health = maxHealth;
        RequestSerialization();
        UpdateHealthUI();
    }

    // ---------------- Death Handling ----------------

    private void HandleDeath()
    {
        if (isPlayer)
        {
            if (playerRespawnManager != null)
                playerRespawnManager.RespawnPlayer(this);
        }
        else
        {
            if (enemyRespawnManager != null)
                enemyRespawnManager.HandleEnemyDeath(this);
        }
    }

    // ---------------- UI Updates ----------------

    private void UpdateHealthUI()
    {
        if (healthBarImage != null)
            healthBarImage.fillAmount = health / maxHealth;

        if (healthText != null)
            healthText.text = $"{health:0}/{maxHealth:0}";
    }

    // ---------------- Networking Overrides ----------------

    public override void OnDeserialization()
    {
        base.OnDeserialization();
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUI();
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        base.OnOwnershipTransferred(player);
        UpdateHealthUI(); // Refresh UI when ownership changes
    }
}
