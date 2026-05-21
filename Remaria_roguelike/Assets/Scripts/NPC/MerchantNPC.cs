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
            
            // Wait one frame to ensure DialogueManager has finished its state changes
            StartCoroutine(OpenShopRoutine());
        }

        private System.Collections.IEnumerator OpenShopRoutine()
        {
            yield return null; // Wait for the end of the frame
            OpenShop();
        }

        private void OpenShop()
        {
            // 1. Try using the static GlobalInstance (Fastest and most reliable)
            if (UpgradeUI.GlobalInstance != null)
            {
                UpgradeUI.GlobalInstance.gameObject.SetActive(true);
                Debug.Log("[MerchantNPC] Opened shop via GlobalInstance.");
                return;
            }

            // 2. Try using direct reference if assigned
            if (upgradeUI != null)
            {
                upgradeUI.gameObject.SetActive(true);
                Debug.Log("[MerchantNPC] Opened shop via direct reference.");
                return;
            }

            // 3. Last resort: Find it in the scene by type
            UpgradeUI foundUI = FindObjectOfType<UpgradeUI>(true);
            if (foundUI != null)
            {
                foundUI.gameObject.SetActive(true);
                Debug.Log("[MerchantNPC] Opened shop via FindObjectOfType.");
                return;
            }

            // 4. Ultra last resort: Find by name
            GameObject shopObj = GameObject.Find("UpgradeShopPannel");
            if (shopObj != null)
            {
                shopObj.SetActive(true);
                Debug.Log("[MerchantNPC] Opened shop via Find(UpgradeShopPannel).");
            }
            else
            {
                Debug.LogError("[MerchantNPC] CRITICAL: UpgradeShopPannel not found by name, type, or reference!");
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
