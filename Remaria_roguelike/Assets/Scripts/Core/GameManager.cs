using UnityEngine;

namespace Remoria.Core
{
    /// <summary>
    /// Central game state manager. Controls whether the game is playing,
    /// paused, or in a game-over state.
    /// 
    /// Access from anywhere: GameManager.Instance.CurrentState
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        // ─── Game States ───────────────────────────────────────────────
        // An "enum" is a list of named options. Think of it as a dropdown menu.
        public enum GameState
        {
            Playing,    // Normal gameplay
            Paused,     // Game is paused (menus open, etc.)
            Dialogue,   // Player is talking to an NPC
            GameOver    // Player died
        }

        // ─── Public Properties ─────────────────────────────────────────
        // "{ get; private set; }" means anyone can READ this value,
        // but only GameManager can CHANGE it.
        public GameState CurrentState { get; private set; } = GameState.Playing;

        // ─── Events ────────────────────────────────────────────────────
        // Events let other scripts react when something happens,
        // without GameManager needing to know about those scripts.
        // 
        // Usage in another script:
        //   GameManager.Instance.OnGameStateChanged += MyReactionMethod;
        public event System.Action<GameState> OnGameStateChanged;

        // ─── Public Methods ────────────────────────────────────────────

        /// <summary>
        /// Change the game state. Notifies all listeners.
        /// </summary>
        public void SetState(GameState newState)
        {
            if (CurrentState == newState) return; // Already in this state.

            GameState previousState = CurrentState;
            CurrentState = newState;

            // Handle time scale: pause freezes time, resume unfreezes.
            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f; // Normal speed
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f; // Freeze time
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;

                case GameState.Dialogue:
                    // Don't freeze time during dialogue (animations still play).
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;

                case GameState.GameOver:
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
            }

            Debug.Log($"[GameManager] State changed: {previousState} → {newState}");

            // Fire the event so all subscribers know about the change.
            // The "?" is a null check — only fires if someone is listening.
            OnGameStateChanged?.Invoke(newState);
        }

        /// <summary>
        /// Toggle between Playing and Paused states (for Escape key).
        /// </summary>
        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
                SetState(GameState.Paused);
            else if (CurrentState == GameState.Paused)
                SetState(GameState.Playing);
        }

        /// <summary>
        /// Shorthand check: is the game in a state where the player can move/act?
        /// </summary>
        public bool IsPlaying => CurrentState == GameState.Playing;

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void Start()
        {
            // Lock the cursor for gameplay when the game starts.
            SetState(GameState.Playing);
        }

        private void Update()
        {
            // Escape key toggles pause.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
    }
}
