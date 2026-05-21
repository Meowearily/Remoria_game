using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Remoria.Core;

namespace Remoria.UI
{
    /// <summary>
    /// Screen-space health bar for the player.
    /// The green fill bar shrinks from right to left as health decreases.
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        // ─── Inspector Fields ──────────────────────────────────────────
        [Header("References")]
        [Tooltip("The Image component that represents the health fill")]
        [SerializeField] private Image fillImage;

        [Tooltip("Optional: text showing current/max HP")]
        [SerializeField] private TMPro.TextMeshProUGUI healthText;

        [Tooltip("The player's Health component (auto-finds by tag if empty)")]
        [SerializeField] private Health playerHealth;

        [Header("Appearance")]
        [Tooltip("Color of the health fill bar")]
        [SerializeField] private Color fillColor = new Color(0.2f, 0.9f, 0.3f, 1f); // Bright green

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void Awake()
        {
            // Force the fill image to use Filled mode so fillAmount works.
            if (fillImage != null)
            {
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                fillImage.color = fillColor;
            }
        }

        private void OnEnable()
        {
            // Subscribe to scene loading to re-find the player
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Initial find
            InitializePlayerSubscription();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnsubscribeFromPlayer();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Re-find player when a new scene is loaded
            InitializePlayerSubscription();
        }

        private void InitializePlayerSubscription()
        {
            // Clean up old reference if it exists
            UnsubscribeFromPlayer();

            // Auto-find the player's health.
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealthBar;
                UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.MaxHealth);
                Debug.Log($"[HealthBarUI] Successfully subscribed to player: {player.name}");
            }
        }

        private void UnsubscribeFromPlayer()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= UpdateHealthBar;
                playerHealth = null;
            }
        }

        // ─── Update Logic ──────────────────────────────────────────────

        private void UpdateHealthBar(float current, float max)
        {
            if (fillImage == null) return;

            float percent = max > 0 ? current / max : 0f;

            // Shrink the green bar from right to left.
            fillImage.fillAmount = percent;

            // Update text if assigned.
            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            }
        }
    }
}
