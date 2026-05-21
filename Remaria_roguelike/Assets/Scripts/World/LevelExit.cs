using UnityEngine;
using Remoria.Core;
using Remoria.Interaction;
using UnityEngine.SceneManagement;

namespace Remoria.World
{
    /// <summary>
    /// Universal exit script for the Hub and Dungeon floors.
    /// Handles progression to the next floor or starting a new run.
    /// </summary>
    public class LevelExit : MonoBehaviour, IInteractable
    {
        [Header("Settings")]
        [SerializeField] private string promptText = "Enter Portal";
        [SerializeField] private string hubSceneName = "HubScene";

        public string InteractionPrompt => promptText;

        public void Interact(GameObject interactor)
        {
            if (LevelManager.Instance == null)
            {
                Debug.LogError("[LevelExit] LevelManager not found!");
                return;
            }

            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == hubSceneName)
            {
                // If in Hub, start a new run (Floor 1)
                LevelManager.Instance.StartNewRun();
            }
            else
            {
                // If in Dungeon, advance to next floor
                LevelManager.Instance.AdvanceToNextLevel();
            }
        }
    }
}
