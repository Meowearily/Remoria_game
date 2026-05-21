using UnityEngine;
using Remoria.Interaction;
using Remoria.Core;

namespace Remoria.World
{
    /// <summary>
    /// A portal specifically for the Hub area that starts a new dungeon run.
    /// </summary>
    public class HubPortal : MonoBehaviour, IInteractable
    {
        [SerializeField] private string promptText = "Enter Dungeon";

        public string InteractionPrompt => promptText;

        public void Interact(GameObject interactor)
        {
            Debug.Log("[HubPortal] Player entering the dungeon...");
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.StartNewRun();
            }
            else
            {
                Debug.LogWarning("[HubPortal] LevelManager instance not found! Ensure it exists in the Hub scene.");
            }
        }

        private void Awake()
        {
            // Ensure colliders don't block movement
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.isTrigger = true;
            }
        }
    }
}
