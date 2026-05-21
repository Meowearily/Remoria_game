using UnityEngine;
using Remoria.Core;

namespace Remoria.Player
{
    /// <summary>
    /// Handles the player's death logic and integration with GameManager/CurrencyManager.
    /// </summary>
    public class PlayerDeath : MonoBehaviour
    {
        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            if (_health != null)
                _health.OnDied += HandleDeath;
        }

        private void OnDisable()
        {
            if (_health != null)
                _health.OnDied -= HandleDeath;
        }

        private void HandleDeath()
        {
            Debug.Log("[PlayerDeath] Player has died. Triggering Game Over.");

            // 1. Trigger Game Over state FIRST so UI can read CurrencyManager.CurrentRunCurrency
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameManager.GameState.GameOver);
            }

            // 2. Finalize currency (save and reset)
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.FinalizeRun();
            }
        }
    }
}
