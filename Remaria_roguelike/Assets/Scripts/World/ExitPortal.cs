using UnityEngine;
using Remoria.Interaction;
using Remoria.Core;

namespace Remoria.World
{
    /// <summary>
    /// Handles the exit portal interaction and ensures enemies don't get stuck in it.
    /// By making the portal a trigger, we allow both player and enemies to pass through,
    /// while the player can still interact with it via the Interaction system.
    /// </summary>
    public class ExitPortal : MonoBehaviour, IInteractable
    {
        [SerializeField] private string promptText = "Enter Portal";

        public string InteractionPrompt => promptText;

        public void Interact(GameObject interactor)
        {
            Debug.Log("[ExitPortal] Player interacted with portal.");
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.AdvanceToNextLevel();
            }
            else
            {
                Debug.LogWarning("[ExitPortal] LevelManager instance not found!");
            }
        }

        private void Awake()
        {
            // Ensure all colliders on the portal are triggers.
            // This prevents enemies from physically bumping into it and getting stuck
            // if they are chasing the player through the portal.
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.isTrigger = true;
            }
            
            // Portals should ideally be on the 'Ignore Raycast' or a dedicated interaction layer
            // if we want to be very specific, but making it a trigger is usually enough
            // for the interaction system to pick it up.
        }
    }
}
