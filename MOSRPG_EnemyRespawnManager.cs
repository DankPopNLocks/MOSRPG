using UdonSharp;
using UnityEngine;
using VRC.Udon;

public class MOSRPG_EnemyRespawnManager : UdonSharpBehaviour
{
    [Header("Enemy Respawn Settings")]
    public Transform enemyRespawnPoint;
    public float enemyRespawnDelay = 5f;

    private GameObject _cachedEnemyObject;
    private MOSRPG_ResourceManager _pendingEnemy; // Ensure this matches your ResourceManager script

    // Called by MOSRPG_ResourceManager when enemy dies
    public void HandleEnemyDeath(MOSRPG_ResourceManager enemy)
    {
        if (enemy == null) return;

        Animator animator = enemy.GetComponent<Animator>();
        if (animator != null)
            animator.SetTrigger("Die");

        AudioSource deathSound = enemy.GetComponent<AudioSource>();
        if (deathSound != null)
            deathSound.Play();

        _pendingEnemy = enemy;
        SendCustomEventDelayedSeconds(nameof(DisableEnemy), 2f); // delay for animation/sound
    }

    private void DisableEnemy()
    {
        if (_pendingEnemy == null) return;

        GameObject enemyGO = _pendingEnemy.gameObject;
        enemyGO.SetActive(false);

        RespawnEnemyAfterDelay(enemyGO);
        _pendingEnemy = null;
    }

    public void RespawnEnemyAfterDelay(GameObject enemyObject)
    {
        if (enemyObject == null) return;

        _cachedEnemyObject = enemyObject;
        SendCustomEventDelayedSeconds(nameof(RespawnEnemyInternal), enemyRespawnDelay);
    }

    private void RespawnEnemyInternal()
    {
        if (_cachedEnemyObject == null || enemyRespawnPoint == null) return;

        _cachedEnemyObject.transform.position = enemyRespawnPoint.position;
        _cachedEnemyObject.transform.rotation = enemyRespawnPoint.rotation;

        // Reset resource (formerly HealthSync)
        MOSRPG_ResourceManager resource = _cachedEnemyObject.GetComponent<MOSRPG_ResourceManager>();
        if (resource != null)
        {
            resource.ResetResource();
        }

        _cachedEnemyObject.SetActive(true);
        Debug.Log("[MOSRPG_EnemyRespawnManager] Enemy respawned at: " + enemyRespawnPoint.position);
        _cachedEnemyObject = null;
    }
}
