using UnityEngine;
using Remoria.Core;

namespace Remoria.Player
{
    /// <summary>
    /// Applies meta-progression upgrades to the player's stats at the start of each level.
    /// </summary>
    public class StatApplier : MonoBehaviour
    {
        [Header("HP Scaling")]
        [Tooltip("How much extra HP each level grants")]
        [SerializeField] private float hpPerLevel = 20f;

        [Header("Damage Scaling")]
        [Tooltip("How much extra melee damage each level grants")]
        [SerializeField] private float meleeDamagePerLevel = 5f;
        [Tooltip("How much extra ranged damage each level grants")]
        [SerializeField] private float rangedDamagePerLevel = 3f;

        private void Start()
        {
            ApplyStats();
        }

        /// <summary>
        /// Reads saved levels and applies them to the current player instance.
        /// </summary>
        public void ApplyStats()
        {
            if (SaveManager.Instance == null) return;

            SaveData data = SaveManager.Instance.Data;

            // 1. Apply Health
            Health health = GetComponent<Health>();
            if (health != null)
            {
                float bonusHP = data.healthUpgradeLevel * hpPerLevel;
                float newMaxHP = health.MaxHealth + bonusHP;
                health.SetMaxHealth(newMaxHP);
                Debug.Log($"[StatApplier] Applied HP Upgrade: Level {data.healthUpgradeLevel} (+{bonusHP} HP)");
            }

            // 2. Apply Damage
            PlayerCombat combat = GetComponent<PlayerCombat>();
            if (combat != null)
            {
                float meleeBonus = data.damageUpgradeLevel * meleeDamagePerLevel;
                float rangedBonus = data.damageUpgradeLevel * rangedDamagePerLevel;
                combat.AddBonusDamage(meleeBonus, rangedBonus);
                Debug.Log($"[StatApplier] Applied Damage Upgrade: Level {data.damageUpgradeLevel} (+{meleeBonus} Melee, +{rangedBonus} Ranged)");
            }
        }
    }
}
