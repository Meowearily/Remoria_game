using UnityEngine;
using Remoria.Core;
using Remoria.Interaction;

namespace Remoria.World
{
    /// <summary>
    /// A portal that takes the player from the Hub back to the start of the dungeon.
    /// Implements IInteractable for 'E' key interaction.
    /// </summary>
    public class HubPortal : MonoBehaviour, IInteractable
    {
        [Header("Settings")]
        [SerializeField] private string promptText = "Enter Dungeon";

        public string InteractionPrompt => promptText;

        public void Interact(GameObject interactor)
        {
            Debug.Log("[HubPortal] Player entering portal. Starting new run.");

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.StartNewRun();
            }
            else
            {
                Debug.LogError("[HubPortal] LevelManager not found! Cannot start run.");
            }
        }
    }
}
