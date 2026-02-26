using UnityEngine;
using Remoria.Interaction;
using Remoria.UI;

namespace Remoria.Player
{
    /// <summary>
    /// Detects nearby interactable objects and lets the player interact with them.
    /// 
    /// How it works:
    ///   1. A trigger collider on the player detects nearby objects.
    ///   2. When an IInteractable enters the trigger, it becomes the "current target."
    ///   3. A UI prompt appears ("Press E to Talk", etc.).
    ///   4. When the player presses E, Interact() is called on the target.
    /// 
    /// Setup:
    ///   - Add a SphereCollider to the Player, set radius ~3, and check "Is Trigger".
    ///   - Interactable objects need a Collider (can be trigger or solid).
    ///   - Interactable objects must implement IInteractable.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PlayerInteraction : MonoBehaviour
    {
        // ─── Inspector Fields ──────────────────────────────────────────
        [Header("Interaction Settings")]
        [Tooltip("Maximum number of interactables to track simultaneously")]
        [SerializeField] private int maxInteractables = 10;

        // ─── Runtime State ─────────────────────────────────────────────
        private IInteractable _currentTarget;
        private readonly System.Collections.Generic.List<IInteractable> _nearbyInteractables
            = new System.Collections.Generic.List<IInteractable>();

        // Cooldown prevents re-triggering interaction on the same E press
        // that ends a dialogue (both scripts see the same GetKeyDown in one frame).
        private float _interactionCooldown = 0f;

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void Update()
        {
            // Tick down cooldown.
            if (_interactionCooldown > 0f)
            {
                _interactionCooldown -= Time.unscaledDeltaTime;
                return;
            }

            if (!Core.GameManager.Instance.IsPlaying) return;

            // Find the closest interactable.
            UpdateCurrentTarget();

            // Press E to interact.
            if (Input.GetKeyDown(KeyCode.E) && _currentTarget != null)
            {
                _currentTarget.Interact(gameObject);
                _interactionCooldown = 0.3f; // Brief cooldown after interacting.
                Debug.Log($"[PlayerInteraction] Interacted with: {(_currentTarget as MonoBehaviour)?.gameObject.name}");
            }
        }

        /// <summary>
        /// Called by Unity when another collider enters our trigger.
        /// We check if it implements IInteractable and add it to our list.
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            IInteractable interactable = other.GetComponent<IInteractable>();
            if (interactable != null && !_nearbyInteractables.Contains(interactable))
            {
                _nearbyInteractables.Add(interactable);
            }
        }

        /// <summary>
        /// Called when a collider leaves our trigger.
        /// Remove it from the list and hide the prompt if it was our target.
        /// </summary>
        private void OnTriggerExit(Collider other)
        {
            IInteractable interactable = other.GetComponent<IInteractable>();
            if (interactable != null)
            {
                _nearbyInteractables.Remove(interactable);

                if (_currentTarget == interactable)
                {
                    _currentTarget = null;
                    HidePrompt();
                }
            }
        }

        // ─── Target Selection ──────────────────────────────────────────

        /// <summary>
        /// Finds the closest interactable from the nearby list and updates the UI prompt.
        /// </summary>
        private void UpdateCurrentTarget()
        {
            // Clean up destroyed objects from the list.
            _nearbyInteractables.RemoveAll(i => (i as MonoBehaviour) == null);

            if (_nearbyInteractables.Count == 0)
            {
                if (_currentTarget != null)
                {
                    _currentTarget = null;
                    HidePrompt();
                }
                return;
            }

            // Find the closest interactable.
            IInteractable closest = null;
            float closestDistance = float.MaxValue;

            foreach (IInteractable interactable in _nearbyInteractables)
            {
                MonoBehaviour mb = interactable as MonoBehaviour;
                if (mb == null) continue;

                float distance = Vector3.Distance(transform.position, mb.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = interactable;
                }
            }

            if (closest != _currentTarget)
            {
                _currentTarget = closest;

                if (_currentTarget != null)
                {
                    ShowPrompt(_currentTarget.InteractionPrompt);
                }
                else
                {
                    HidePrompt();
                }
            }
        }

        // ─── UI Prompt ─────────────────────────────────────────────────

        private void ShowPrompt(string prompt)
        {
            // Try to show via UIManager. If UIManager isn't set up yet, just log.
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.Show($"Press E — {prompt}");
            }
        }

        private void HidePrompt()
        {
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.Hide();
            }
        }
    }
}
