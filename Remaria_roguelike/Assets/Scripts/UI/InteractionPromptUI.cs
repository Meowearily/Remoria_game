using UnityEngine;

namespace Remoria.UI
{
    /// <summary>
    /// Simple floating text that shows "Press E — [action]" near an interactable.
    /// 
    /// This is a lightweight singleton — PlayerInteraction calls Show()/Hide() on it.
    /// 
    /// Setup:
    ///   1. Create a TextMeshProUGUI in the Canvas.
    ///   2. Position it center-bottom or wherever you prefer.
    ///   3. Attach this script.
    ///   4. Assign the text reference.
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        // Lightweight singleton (no DontDestroyOnLoad needed for UI).
        public static InteractionPromptUI Instance { get; private set; }

        // ─── Inspector Fields ──────────────────────────────────────────
        [SerializeField] private TMPro.TextMeshProUGUI promptText;

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void Awake()
        {
            Instance = this;
            Hide();
        }

        // ─── Public Methods ────────────────────────────────────────────

        public void Show(string text)
        {
            if (promptText != null)
            {
                promptText.text = text;
                promptText.enabled = true;
            }
        }

        public void Hide()
        {
            if (promptText != null)
            {
                promptText.enabled = false;
            }
        }
    }
}
