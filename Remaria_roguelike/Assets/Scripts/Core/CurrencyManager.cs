using UnityEngine;

namespace Remoria.Core
{
    /// <summary>
    /// Manages the player's meta-currency (Shards) within a run and persistently.
    /// </summary>
    public class CurrencyManager : Singleton<CurrencyManager>
    {
        [Header("Runtime Info")]
        [SerializeField] private int _currentRunCurrency = 0;
        [SerializeField] private int _totalPersistentDisplay = 0; // For inspector view only

        private void Update()
        {
            // Sync display field with real data for easy debugging in Inspector
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                _totalPersistentDisplay = SaveManager.Instance.Data.totalCurrency;
            }
        }

        /// <summary>
        /// Currency collected in the current run.
        /// </summary>
        public int CurrentRunCurrency => _currentRunCurrency;

        /// <summary>
        /// Total currency available for upgrades (from SaveManager).
        /// </summary>
        public int TotalPersistentCurrency => SaveManager.Instance.Data.totalCurrency;

        public event System.Action<int> OnRunCurrencyChanged;
        public event System.Action<int> OnTotalCurrencyChanged;

        /// <summary>
        /// Adds currency to the current run's total.
        /// </summary>
        public void AddCurrency(int amount)
        {
            if (amount <= 0) return;

            _currentRunCurrency += amount;
            Debug.Log($"[CurrencyManager] Added {amount} shards. Run total: {_currentRunCurrency}");
            OnRunCurrencyChanged?.Invoke(_currentRunCurrency);
        }

        /// <summary>
        /// Transfers run currency to persistent currency and saves.
        /// Call this when the player dies or completes a run.
        /// </summary>
        public void FinalizeRun()
        {
            SaveManager.Instance.Data.totalCurrency += _currentRunCurrency;
            int total = SaveManager.Instance.Data.totalCurrency;
            
            Debug.Log($"[CurrencyManager] Finalizing run. Added {_currentRunCurrency} to total. New total: {total}");
            
            _currentRunCurrency = 0;
            SaveManager.Instance.Save();
            
            OnTotalCurrencyChanged?.Invoke(total);
            OnRunCurrencyChanged?.Invoke(0);
        }

        /// <summary>
        /// Attempts to spend persistent currency.
        /// </summary>
        /// <returns>True if the transaction was successful.</returns>
        public bool SpendCurrency(int amount)
        {
            if (amount > SaveManager.Instance.Data.totalCurrency)
            {
                Debug.Log("[CurrencyManager] Not enough currency!");
                return false;
            }

            SaveManager.Instance.Data.totalCurrency -= amount;
            SaveManager.Instance.Save();
            
            OnTotalCurrencyChanged?.Invoke(SaveManager.Instance.Data.totalCurrency);
            return true;
        }
    }
}
