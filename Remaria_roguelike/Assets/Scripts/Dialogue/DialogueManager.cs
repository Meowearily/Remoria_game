using UnityEngine;
using Remoria.Core;
using Remoria.UI;

namespace Remoria.Dialogue
{
    /// <summary>
    /// Singleton that manages dialogue flow.
    /// 
    /// When an NPC starts dialogue:
    ///   1. DialogueManager receives the DialogueData.
    ///   2. Sets game state to Dialogue (disables player movement).
    ///   3. Shows the dialogue UI panel.
    ///   4. Displays lines one at a time.
    ///   5. Player presses E to advance each line.
    ///   6. When all lines are shown, dialogue ends and gameplay resumes.
    /// 
    /// Usage from an NPC script:
    ///   DialogueManager.Instance.StartDialogue(myDialogueData);
    /// </summary>
    public class DialogueManager : Singleton<DialogueManager>
    {
        // ─── Runtime State ─────────────────────────────────────────────
        private DialogueData _currentDialogue;
        private int _currentLineIndex;
        private bool _isDialogueActive = false;

        // ─── Events ────────────────────────────────────────────────────
        /// <summary>Fired when dialogue starts. Passes the DialogueData.</summary>
        public event System.Action<DialogueData> OnDialogueStarted;

        /// <summary>Fired when a new line should be displayed. Passes (speakerName, lineText).</summary>
        public event System.Action<string, string> OnDialogueLineChanged;

        /// <summary>Fired when dialogue ends.</summary>
        public event System.Action OnDialogueEnded;

        // ─── Public Properties ─────────────────────────────────────────
        public bool IsDialogueActive => _isDialogueActive;

        // ─── Public Methods ────────────────────────────────────────────

        /// <summary>
        /// Start a dialogue sequence. Called by NPC when player interacts.
        /// </summary>
        public void StartDialogue(DialogueData dialogueData)
        {
            if (dialogueData == null || dialogueData.lines == null || dialogueData.lines.Length == 0)
            {
                Debug.LogWarning("[DialogueManager] Tried to start dialogue with no lines!");
                return;
            }

            if (_isDialogueActive)
            {
                Debug.LogWarning("[DialogueManager] Dialogue already active, ignoring.");
                return;
            }

            _currentDialogue = dialogueData;
            _currentLineIndex = 0;
            _isDialogueActive = true;

            // Tell the GameManager we're in dialogue mode (disables player movement).
            GameManager.Instance.SetState(GameManager.GameState.Dialogue);

            // Fire event so UI can set up.
            OnDialogueStarted?.Invoke(dialogueData);

            // Show the first line.
            ShowCurrentLine();
        }

        /// <summary>
        /// Advance to the next line, or end if we've shown all lines.
        /// Called when the player presses E during dialogue.
        /// </summary>
        public void AdvanceLine()
        {
            if (!_isDialogueActive) return;

            _currentLineIndex++;

            if (_currentLineIndex >= _currentDialogue.lines.Length)
            {
                // No more lines — end the dialogue.
                EndDialogue();
            }
            else
            {
                ShowCurrentLine();
            }
        }

        /// <summary>
        /// End the dialogue immediately.
        /// </summary>
        public void EndDialogue()
        {
            if (!_isDialogueActive) return;

            _isDialogueActive = false;
            _currentDialogue = null;
            _currentLineIndex = 0;

            // Return to playing state.
            GameManager.Instance.SetState(GameManager.GameState.Playing);

            // Notify listeners.
            OnDialogueEnded?.Invoke();

            Debug.Log("[DialogueManager] Dialogue ended.");
        }

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void Update()
        {
            if (!_isDialogueActive) return;

            // Press E or Left Click to advance dialogue.
            if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
            {
                AdvanceLine();
            }
        }

        // ─── Private Methods ───────────────────────────────────────────

        private void ShowCurrentLine()
        {
            string line = _currentDialogue.lines[_currentLineIndex];
            string speaker = _currentDialogue.speakerName;

            Debug.Log($"[DialogueManager] {speaker}: {line}");

            // Notify UI to display this line.
            OnDialogueLineChanged?.Invoke(speaker, line);
        }
    }
}
