using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Remoria.Core;

namespace Remoria.UI
{
    /// <summary>
    /// Manages the Game Over screen, showing run results and providing navigation.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI currencyCollectedText;
        [SerializeField] private Button returnToHubButton;
        [SerializeField] private Button quitButton;

        [Header("Scene Settings")]
        [SerializeField] private string hubSceneName = "HubScene";

        private void OnEnable()
        {
            // Update the text with currency collected in the last run
            if (CurrencyManager.Instance != null && currencyCollectedText != null)
            {
                // Note: Since FinalizeRun() is called on death, we might need to cache 
                // the value before it's reset. But for now, let's assume we show the 
                // total persistent currency or we update this right before the reset.
                currencyCollectedText.text = $"Shards Collected: {CurrencyManager.Instance.CurrentRunCurrency}";
            }

            // Setup buttons
            if (returnToHubButton != null)
            {
                returnToHubButton.onClick.RemoveAllListeners();
                returnToHubButton.onClick.AddListener(ReturnToHub);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveAllListeners();
                quitButton.onClick.AddListener(QuitGame);
            }
        }

        /// <summary>
        /// Loads the Hub scene and resets time scale.
        /// </summary>
        public void ReturnToHub()
        {
            Debug.Log("[GameOverUI] Returning to Hub...");

            if (LevelTransitionUI.Instance != null)
            {
                LevelTransitionUI.Instance.ShowTransition("Returning to Hub...", () =>
                {
                    // This runs when the screen is black
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.SetState(GameManager.GameState.Playing);
                    }
                    else
                    {
                        Time.timeScale = 1f;
                    }

                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.HideAllPanels();
                    }

                    SceneManager.LoadScene(hubSceneName);
                });
            }
            else
            {
                // Fallback if no transition UI
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetState(GameManager.GameState.Playing);
                }
                else
                {
                    Time.timeScale = 1f;
                }

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.HideAllPanels();
                }

                SceneManager.LoadScene(hubSceneName);
            }
        }

        /// <summary>
        /// Quits the application.
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("[GameOverUI] Quitting game...");
            Application.Quit();
        }
    }
}
