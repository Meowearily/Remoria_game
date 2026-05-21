using UnityEngine;
using Remoria.UI;
using Remoria.Interaction;
using Remoria.Dialogue;

namespace Remoria.NPC
{
    /// <summary>
    /// An interactable NPC that shows dialogue and then opens the meta-upgrade shop.
    /// </summary>
    public class MerchantNPC : MonoBehaviour, IInteractable
    {
        [Header("Settings")]
        [Tooltip("The text to display when the player approaches")]
        [SerializeField] private string interactPrompt = "Talk to Merchant";

        [Tooltip("Dialogue to show before opening the shop")]
        [SerializeField] private DialogueData merchantDialogue;

        [Tooltip("Reference to the Upgrade UI panel")]
        [SerializeField] private UpgradeUI upgradeUI;

        public string InteractionPrompt => interactPrompt;

        public void Interact(GameObject interactor)
        {
            if (DialogueManager.Instance.IsDialogueActive) return;

            if (merchantDialogue != null)
            {
                // Subscribe to dialogue end to open shop
                DialogueManager.Instance.OnDialogueEnded += OpenShopAfterDialogue;
                DialogueManager.Instance.StartDialogue(merchantDialogue);
                Debug.Log("[MerchantNPC] Starting dialogue before shop.");
            }
            else
            {
                // If no dialogue, just open shop immediately
                OpenShop();
            }
        }

        private void OpenShopAfterDialogue()
        {
            // Unsubscribe immediately
            DialogueManager.Instance.OnDialogueEnded -= OpenShopAfterDialogue;
            
            // Wait a frame or just open
            OpenShop();
        }

        private void OpenShop()
        {
            if (upgradeUI != null)
            {
                upgradeUI.OpenShop();
            }
            else
            {
                UpgradeUI ui = FindObjectOfType<UpgradeUI>(true);
                if (ui != null)
                {
                    ui.OpenShop();
                }
                else
                {
                    Debug.LogWarning("[MerchantNPC] UpgradeUI not found in the scene!");
                }
            }
        }

        private void OnDestroy()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnded -= OpenShopAfterDialogue;
            }
        }
    }
}
