using UnityEngine;
using Remoria.Interaction;

namespace Remoria.NPC
{
    /// <summary>
    /// NPC that the player can talk to.
    /// Implements IInteractable so the PlayerInteraction system detects it.
    /// 
    /// When the player presses E near this NPC:
    ///   1. NPC rotates to face the player.
    ///   2. DialogueManager starts the assigned dialogue.
    ///   3. Player movement is disabled during the conversation.
    /// 
    /// Setup:
    ///   1. Create a capsule (or any shape) for the NPC.
    ///   2. Add a Collider and check "Is Trigger".
    ///   3. Attach this script.
    ///   4. Create a DialogueData asset (Create → RPG → Dialogue).
    ///   5. Drag the DialogueData into the "Dialogue" field.
    /// </summary>
    public class NPCController : MonoBehaviour, IInteractable
    {
        // ─── Inspector Fields ──────────────────────────────────────────
        [Header("NPC Settings")]
        [Tooltip("The NPC's display name")]
        [SerializeField] private string npcName = "Villager";

        [Header("Dialogue")]
        [Tooltip("The dialogue data to play when the player talks to this NPC")]
        [SerializeField] private Dialogue.DialogueData dialogue;

        [Header("Interaction")]
        [Tooltip("Text shown in the interaction prompt")]
        [SerializeField] private string promptText = "Talk";

        [Header("Look At Player")]
        [Tooltip("How quickly the NPC turns to face the player")]
        [SerializeField] private float rotationSpeed = 5f;

        // ─── Runtime ───────────────────────────────────────────────────
        private Transform _playerTransform;
        private bool _isTalking = false;

        // ─── IInteractable Implementation ──────────────────────────────

        public string InteractionPrompt => $"{promptText} to {npcName}";

        public void Interact(GameObject interactor)
        {
            if (_isTalking) return;

            _playerTransform = interactor.transform;

            if (dialogue != null)
            {
                _isTalking = true;

                // Subscribe to know when dialogue ends.
                Dialogue.DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;

                // Start the dialogue.
                Dialogue.DialogueManager.Instance.StartDialogue(dialogue);

                Debug.Log($"[NPCController] {npcName} started talking.");
            }
            else
            {
                Debug.LogWarning($"[NPCController] {npcName} has no dialogue assigned!");
            }
        }

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void Update()
        {
            // While talking, smoothly rotate to face the player.
            if (_isTalking && _playerTransform != null)
            {
                Vector3 lookDirection = _playerTransform.position - transform.position;
                lookDirection.y = 0f;

                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
                }
            }
        }

        // ─── Event Handlers ────────────────────────────────────────────

        private void OnDialogueEnded()
        {
            _isTalking = false;

            // Unsubscribe to prevent memory leaks.
            Dialogue.DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;

            Debug.Log($"[NPCController] {npcName} stopped talking.");
        }

        private void OnDestroy()
        {
            // Safety: always unsubscribe when destroyed.
            if (Dialogue.DialogueManager.Instance != null)
            {
                Dialogue.DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
            }
        }
    }
}
