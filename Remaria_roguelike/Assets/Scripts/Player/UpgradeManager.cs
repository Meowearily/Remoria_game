using UnityEngine;
using Remoria.Core;

namespace Remoria.Player
{
    /// <summary>
    /// Handles the logic of buying meta-upgrades.
    /// </summary>
    public class UpgradeManager : Singleton<UpgradeManager>
    {
        [Header("Upgrade Costs")]
        [Tooltip("Base cost for the first level of any upgrade")]
        [SerializeField] private int baseCost = 100;

        [Tooltip("How much the cost increases per level (multiplier)")]
        [SerializeField] private float costMultiplier = 1.5f;

        /// <summary>
        /// Calculates the cost for the NEXT level of health upgrade.
        /// </summary>
        public int GetHealthUpgradeCost()
        {
            int currentLevel = SaveManager.Instance.Data.healthUpgradeLevel;
            return Mathf.FloorToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
        }

        /// <summary>
        /// Calculates the cost for the NEXT level of damage upgrade.
        /// </summary>
        public int GetDamageUpgradeCost()
        {
            int currentLevel = SaveManager.Instance.Data.damageUpgradeLevel;
            return Mathf.FloorToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
        }

        /// <summary>
        /// Tries to purchase a Health upgrade.
        /// </summary>
        public bool TryUpgradeHealth()
        {
            int cost = GetHealthUpgradeCost();

            if (CurrencyManager.Instance.SpendCurrency(cost))
            {
                SaveManager.Instance.Data.healthUpgradeLevel++;
                SaveManager.Instance.Save();
                Debug.Log($"[UpgradeManager] Health upgraded to Level {SaveManager.Instance.Data.healthUpgradeLevel}!");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to purchase a Damage upgrade.
        /// </summary>
        public bool TryUpgradeDamage()
        {
            int cost = GetDamageUpgradeCost();

            if (CurrencyManager.Instance.SpendCurrency(cost))
            {
                SaveManager.Instance.Data.damageUpgradeLevel++;
                SaveManager.Instance.Save();
                Debug.Log($"[UpgradeManager] Damage upgraded to Level {SaveManager.Instance.Data.damageUpgradeLevel}!");
                return true;
            }

            return false;
        }
    }
}
