using UnityEngine;
using TMPro;
using System.Collections;

namespace Remoria.UI
{
    /// <summary>
    /// Handles the screen fade and "Floor Cleared" message during level transitions.
    /// </summary>
    public class LevelTransitionUI : MonoBehaviour
    {
        public static LevelTransitionUI Instance { get; private set; }

        [Header("References")]
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Settings")]
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private float displayDuration = 1.5f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // UI should be on a Canvas that is marked as DontDestroyOnLoad or handled carefully
                // Here we ensure the instance itself persists.
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (panelGroup != null)
            {
                panelGroup.alpha = 0;
                panelGroup.blocksRaycasts = false;
            }
        }

        public void ShowTransition(string message, System.Action onFadedIn = null, System.Action onComplete = null)
        {
            StopAllCoroutines();
            StartCoroutine(TransitionRoutine(message, onFadedIn, onComplete));
        }

        private IEnumerator TransitionRoutine(string message, System.Action onFadedIn, System.Action onComplete)
        {
            if (statusText != null) statusText.text = message;

            // Fade In
            yield return StartCoroutine(Fade(1f));
            
            // Callback when screen is black
            onFadedIn?.Invoke();

            // Wait a bit while level generates (or just for effect)
            yield return new WaitForSecondsRealtime(displayDuration);

            // Fade Out
            yield return StartCoroutine(Fade(0f));

            onComplete?.Invoke();
        }

        private IEnumerator Fade(float targetAlpha)
        {
            if (panelGroup == null) yield break;

            float startAlpha = panelGroup.alpha;
            float elapsed = 0f;

            panelGroup.blocksRaycasts = targetAlpha > 0.5f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                panelGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                yield return null;
            }

            panelGroup.alpha = targetAlpha;
        }
    }
}
