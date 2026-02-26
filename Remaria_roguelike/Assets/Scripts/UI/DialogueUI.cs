using UnityEngine;
using Remoria.Dialogue;

namespace Remoria.UI
{
    /// <summary>
    /// Dialogue panel UI with typewriter text effect.
    /// Shows speaker name, dialogue text, and a "Press E" prompt.
    /// 
    /// The typewriter effect reveals text character by character.
    /// Pressing E during the effect shows the full line immediately.
    /// Pressing E after the full line is shown advances to the next line.
    /// 
    /// Setup:
    ///   1. Create a Panel at the bottom of the Canvas.
    ///   2. Add a TextMeshProUGUI for the speaker name (top).
    ///   3. Add a TextMeshProUGUI for the dialogue text (center).
    ///   4. Add a TextMeshProUGUI for "Press E to continue" (bottom-right).
    ///   5. Attach this script and assign references.
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        // ─── Inspector Fields ──────────────────────────────────────────
        [Header("References")]
        [SerializeField] private TMPro.TextMeshProUGUI speakerText;
        [SerializeField] private TMPro.TextMeshProUGUI dialogueText;
        [SerializeField] private TMPro.TextMeshProUGUI continueText;

        [Header("Typewriter Effect")]
        [Tooltip("Time between each character appearing (seconds)")]
        [SerializeField] private float typewriterSpeed = 0.03f;

        // ─── Runtime ───────────────────────────────────────────────────
        private string _fullText = "";
        private string _displayedText = "";
        private float _typewriterTimer = 0f;
        private int _charIndex = 0;
        private bool _isTyping = false;

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void OnEnable()
        {
            // Subscribe to dialogue events.
            DialogueManager.Instance.OnDialogueLineChanged += ShowLine;
            DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
        }

        private void OnDisable()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueLineChanged -= ShowLine;
                DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
            }
        }

        private void Update()
        {
            if (!_isTyping) return;

            // Typewriter effect: reveal one character at a time.
            _typewriterTimer += Time.unscaledDeltaTime;

            if (_typewriterTimer >= typewriterSpeed)
            {
                _typewriterTimer = 0f;
                _charIndex++;

                if (_charIndex >= _fullText.Length)
                {
                    // All characters revealed.
                    _displayedText = _fullText;
                    _isTyping = false;
                    ShowContinuePrompt(true);
                }
                else
                {
                    _displayedText = _fullText.Substring(0, _charIndex);
                }

                if (dialogueText != null)
                {
                    dialogueText.text = _displayedText;
                }
            }
        }

        // ─── Event Handlers ────────────────────────────────────────────

        private void ShowLine(string speaker, string line)
        {
            if (speakerText != null)
                speakerText.text = speaker;

            _fullText = line;
            _displayedText = "";
            _charIndex = 0;
            _typewriterTimer = 0f;
            _isTyping = true;

            if (dialogueText != null)
                dialogueText.text = "";

            ShowContinuePrompt(false);
        }

        private void OnDialogueEnded()
        {
            _isTyping = false;
            // The panel will be hidden by UIManager via game state change.
        }

        // ─── Helpers ───────────────────────────────────────────────────

        private void ShowContinuePrompt(bool show)
        {
            if (continueText != null)
            {
                continueText.enabled = show;
                continueText.text = "Press E to continue...";
            }
        }
    }
}
