using UnityEngine;
using UnityEngine.UI;
using Remoria.Core;

namespace Remoria.UI
{
    /// <summary>
    /// Singleton that manages all UI panels.
    /// 
    /// Responsibilities:
    ///   - Toggle panels on/off (inventory, pause menu)
    ///   - Provide centralized access to UI state
    /// 
    /// Setup in Unity:
    ///   1. Create a Canvas (UI → Canvas).
    ///   2. Set Canvas Scaler to "Scale With Screen Size" (1920x1080).
    ///   3. Attach this script to the Canvas.
    ///   4. Create child panels and drag them into the Inspector fields.
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        // ─── Panel References ──────────────────────────────────────────
        // Drag these from the Canvas hierarchy in the Inspector.
        [Header("UI Panels")]
        [Tooltip("The inventory panel (toggle with I key)")]
        [SerializeField] private GameObject inventoryPanel;

        [Tooltip("The dialogue panel (controlled by DialogueManager)")]
        [SerializeField] private GameObject dialoguePanel;

        [Tooltip("The pause menu panel")]
        [SerializeField] private GameObject pausePanel;

        [Tooltip("Game over screen")]
        [SerializeField] private GameObject gameOverPanel;

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void Start()
        {
            // Hide all panels at start.
            HideAllPanels();

            // Subscribe to game state changes.
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        private void Update()
        {
            // I key toggles inventory (only during gameplay).
            if (Input.GetKeyDown(KeyCode.I) && GameManager.Instance.IsPlaying)
            {
                TogglePanel(inventoryPanel);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        // ─── Public Methods ────────────────────────────────────────────

        /// <summary>Show a specific panel.</summary>
        public void ShowPanel(GameObject panel)
        {
            if (panel != null)
                panel.SetActive(true);
        }

        /// <summary>Hide a specific panel.</summary>
        public void HidePanel(GameObject panel)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        /// <summary>Toggle a panel's visibility.</summary>
        public void TogglePanel(GameObject panel)
        {
            if (panel != null)
                panel.SetActive(!panel.activeSelf);
        }

        /// <summary>Hide all UI panels.</summary>
        public void HideAllPanels()
        {
            HidePanel(inventoryPanel);
            HidePanel(dialoguePanel);
            HidePanel(pausePanel);
            HidePanel(gameOverPanel);
        }

        // ─── Panel Accessors ───────────────────────────────────────────
        public GameObject InventoryPanel => inventoryPanel;
        public GameObject DialoguePanel => dialoguePanel;

        // ─── State Handling ────────────────────────────────────────────

        private void OnGameStateChanged(GameManager.GameState newState)
        {
            switch (newState)
            {
                case GameManager.GameState.Playing:
                    HidePanel(pausePanel);
                    HidePanel(dialoguePanel);
                    HidePanel(gameOverPanel);
                    break;

                case GameManager.GameState.Paused:
                    ShowPanel(pausePanel);
                    break;

                case GameManager.GameState.Dialogue:
                    ShowPanel(dialoguePanel);
                    break;

                case GameManager.GameState.GameOver:
                    HideAllPanels();
                    ShowPanel(gameOverPanel);
                    break;
            }
        }
    }
}
