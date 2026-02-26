using UnityEngine;

namespace Remoria.Core
{
    /// <summary>
    /// Reusable health component. Attach to ANY GameObject that should have hit points.
    /// Works for the Player, Enemies, destructible objects, etc.
    /// 
    /// This component fires EVENTS when health changes or the object dies.
    /// Other scripts (like UI health bars) subscribe to these events
    /// and react automatically — no tight coupling needed.
    /// 
    /// Setup in Inspector:
    ///   - Set "Max Health" to the desired value (e.g., 100).
    ///   - Current health is set to max automatically on start.
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        // ─── Inspector Fields ──────────────────────────────────────────
        // [SerializeField] makes a private field visible in Unity's Inspector panel.
        // [Header] creates a label/section in the Inspector for organization.

        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;

        // ─── Runtime State ─────────────────────────────────────────────
        private float _currentHealth;

        // ─── Public Properties ─────────────────────────────────────────
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => _currentHealth <= 0f;

        /// <summary>
        /// Health as a 0-1 fraction. Useful for health bar fill amount.
        /// Example: 75 health / 100 max = 0.75
        /// </summary>
        public float HealthPercent => maxHealth > 0 ? _currentHealth / maxHealth : 0f;

        // ─── Events ────────────────────────────────────────────────────
        // Action<float, float> passes (currentHealth, maxHealth) to subscribers.
        
        /// <summary>Fired whenever health changes. Parameters: (currentHealth, maxHealth)</summary>
        public event System.Action<float, float> OnHealthChanged;

        /// <summary>Fired once when health reaches zero.</summary>
        public event System.Action OnDied;

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void Awake()
        {
            _currentHealth = maxHealth;
        }

        // ─── Public Methods ────────────────────────────────────────────

        /// <summary>
        /// Deal damage. Reduces health and fires events.
        /// </summary>
        public void TakeDamage(float amount)
        {
            if (IsDead) return; // Already dead, ignore further damage.
            if (amount <= 0) return; // No negative damage (use Heal for that).

            _currentHealth -= amount;
            _currentHealth = Mathf.Max(_currentHealth, 0f); // Don't go below zero.

            Debug.Log($"[Health] {gameObject.name} took {amount} damage. HP: {_currentHealth}/{maxHealth}");

            // Notify subscribers (e.g., health bar UI).
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);

            if (_currentHealth <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Restore health. Does NOT resurrect dead objects.
        /// </summary>
        public void Heal(float amount)
        {
            if (IsDead) return;
            if (amount <= 0) return;

            _currentHealth += amount;
            _currentHealth = Mathf.Min(_currentHealth, maxHealth); // Don't exceed max.

            Debug.Log($"[Health] {gameObject.name} healed {amount}. HP: {_currentHealth}/{maxHealth}");

            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        /// <summary>
        /// Reset health to max. Useful for respawning.
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = maxHealth;
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        // ─── Private Methods ───────────────────────────────────────────

        private void Die()
        {
            Debug.Log($"[Health] {gameObject.name} has died.");
            OnDied?.Invoke();
        }
    }
}
