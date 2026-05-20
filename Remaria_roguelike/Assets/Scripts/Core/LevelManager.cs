using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Remoria.UI;
using Remoria.World;

namespace Remoria.Core
{
    /// <summary>
    /// Manages the progression between levels.
    /// Handles loading procedural levels and the final boss arena.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] private string proceduralSceneName = "SampleScene";
        [SerializeField] private string bossSceneName = "BossArena";

        [Header("State")]
        [SerializeField] private int currentFloor = 1;
        [SerializeField] private List<LevelSettings> floorSettings;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public int CurrentFloor => currentFloor;

        /// <summary>
        /// Call this when the player enters the exit portal.
        /// </summary>
        public void AdvanceToNextLevel()
        {
            if (LevelTransitionUI.Instance != null)
            {
                LevelTransitionUI.Instance.ShowTransition("FLOOR CLEARED", 
                    onFadedIn: () => {
                        PerformLevelAdvance();
                    }
                );
            }
            else
            {
                PerformLevelAdvance();
            }
        }

        private void PerformLevelAdvance()
        {
            currentFloor++;

            if (currentFloor <= 2)
            {
                Debug.Log($"[LevelManager] Regenerating Procedural Floor {currentFloor}...");
                
                // Find the generator in the current scene
                var generator = FindObjectOfType<HybridLevelGenerator>();
                if (generator != null)
                {
                    // Update settings for the new floor
                    if (floorSettings != null && currentFloor - 1 < floorSettings.Count)
                    {
                        generator.SetSettings(floorSettings[currentFloor - 1]);
                    }
                    
                    generator.Generate();
                }
                else
                {
                    // If not in the procedural scene, load it
                    SceneManager.LoadScene(proceduralSceneName);
                }
            }
            else if (currentFloor == 3)
            {
                Debug.Log("[LevelManager] Loading Final Boss Arena...");
                SceneManager.LoadScene(bossSceneName);
            }
            else
            {
                Debug.Log("[LevelManager] Game Complete!");
                // Optionally reset or return to menu
            }
        }
    }
}
