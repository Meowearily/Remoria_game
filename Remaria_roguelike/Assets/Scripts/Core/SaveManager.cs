using System;
using System.IO;
using UnityEngine;

namespace Remoria.Core
{
    /// <summary>
    /// Handles saving and loading persistent game data.
    /// Data is stored as a JSON file in the application's persistent data path.
    /// </summary>
    public class SaveManager : Singleton<SaveManager>
    {
        [Header("Debug View")]
        [SerializeField] private SaveData _cachedData;
        private string _savePath;

        // Path to the save file
        private string SaveFilePath => Path.Combine(Application.persistentDataPath, "remoria_save.json");

        protected override void Awake()
        {
            base.Awake();
            _savePath = SaveFilePath;
            Load();
        }

        /// <summary>
        /// Current persistent data.
        /// </summary>
        public SaveData Data => _cachedData ??= new SaveData();

        /// <summary>
        /// Saves the current data to disk.
        /// </summary>
        public void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(Data, true);
                File.WriteAllText(_savePath, json);
                Debug.Log($"[SaveManager] Data saved to {_savePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save data: {e.Message}");
            }
        }

        /// <summary>
        /// Loads data from disk.
        /// </summary>
        public void Load()
        {
            if (File.Exists(_savePath))
            {
                try
                {
                    string json = File.ReadAllText(_savePath);
                    _cachedData = JsonUtility.FromJson<SaveData>(json);
                    Debug.Log("[SaveManager] Data loaded successfully.");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Failed to load data: {e.Message}");
                    _cachedData = new SaveData();
                }
            }
            else
            {
                Debug.Log("[SaveManager] No save file found. Creating new data.");
                _cachedData = new SaveData();
                Save(); // Create the initial file
            }
        }

        /// <summary>
        /// Deletes the save file and resets data.
        /// </summary>
        public void ResetData()
        {
            if (File.Exists(_savePath))
            {
                File.Delete(_savePath);
            }
            _cachedData = new SaveData();
            Debug.Log("[SaveManager] Data reset.");
        }
    }

    /// <summary>
    /// Serializable structure for persistent game data.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        [Header("Currency")]
        public int totalCurrency = 0;

        [Header("Meta-Progression Levels")]
        public int healthUpgradeLevel = 0;
        public int damageUpgradeLevel = 0;

        // Future-proofing: add more fields here as needed
        // public int speedUpgradeLevel = 0;
        // public int critUpgradeLevel = 0;
    }
}
