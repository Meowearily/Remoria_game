using UnityEngine;
using Remoria.Core;

namespace Remoria.Enemy
{
    /// <summary>
    /// High-level controller that bridges EnemyAI, Health, and EnemyStats.
    /// Handles initialization and death-related logic like dropping loot.
    /// 
    /// Think of it as the "manager" for one specific enemy instance.
    /// EnemyAI does the thinking, Health does the HP, this script connects them.
    /// 
    /// Setup:
    ///   - Attach to the same GameObject as EnemyAI and Health.
    ///   - Assign the EnemyStats in EnemyAI (not here).
    ///   - Optionally assign a lootDropPrefab for item drops on death.
    /// </summary>
    [RequireComponent(typeof(EnemyAI))]
    [RequireComponent(typeof(Health))]
    public class EnemyController : MonoBehaviour
    {
        // ─── Inspector Fields ──────────────────────────────────────────
        [Header("Loot")]
        [Tooltip("Prefab to spawn when this enemy dies (optional). Use an ItemPickup prefab.")]
        [SerializeField] private GameObject lootDropPrefab;

        [Tooltip("Chance to drop loot (0 = never, 1 = always)")]
        [Range(0f, 1f)]
        [SerializeField] private float lootDropChance = 0.5f;

        // ─── Private References ────────────────────────────────────────
        private EnemyAI _ai;
        private Health _health;

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void Awake()
        {
            _ai = GetComponent<EnemyAI>();
            _health = GetComponent<Health>();
        }

        private void Start()
        {
            // Subscribe to death to handle loot dropping.
            _health.OnDied += HandleDeath;
        }

        private void OnDestroy()
        {
            // Always unsubscribe from events to prevent memory leaks.
            // -= removes the subscription.
            if (_health != null)
            {
                _health.OnDied -= HandleDeath;
            }
        }

        // ─── Death Handling ────────────────────────────────────────────

        private void HandleDeath()
        {
            // Try to drop loot.
            if (lootDropPrefab != null && Random.value <= lootDropChance)
            {
                // Spawn the loot slightly above the ground so it doesn't clip.
                Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
                Instantiate(lootDropPrefab, dropPosition, Quaternion.identity);
                Debug.Log($"[EnemyController] {_ai.Stats.enemyName} dropped loot!");
            }
        }
    }
}
