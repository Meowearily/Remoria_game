using System;
using System.Collections.Generic;
using UnityEngine;

namespace Remoria.Enemy
{
    /// <summary>
    /// Entry for an enemy in the database, including its prefab and power level (cost).
    /// </summary>
    [Serializable]
    public class EnemyEntry
    {
        [Tooltip("The enemy prefab to spawn")]
        public GameObject prefab;

        [Tooltip("Complexity cost of this enemy. Used for room budget balancing.")]
        [Range(1, 100)]
        public int powerLevel = 10;
    }

    /// <summary>
    /// ScriptableObject that categorizes enemies by difficulty Tiers.
    /// Used by the LevelGenerator to filter enemies based on the current floor.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Remoria/Enemy Database")]
    public class EnemyDatabase : ScriptableObject
    {
        [Header("Tier 1 - Level 1 Enemies")]
        [Tooltip("Enemies available on the first procedural level")]
        public List<EnemyEntry> tier1Enemies = new List<EnemyEntry>();

        [Header("Tier 2 - Level 2 Enemies")]
        [Tooltip("Enemies available on the second procedural level")]
        public List<EnemyEntry> tier2Enemies = new List<EnemyEntry>();

        [Header("Tier 3 - Final Boss")]
        [Tooltip("The boss for the final static level")]
        public GameObject finalBossPrefab;

        /// <summary>
        /// Returns a list of enemies for a specific floor.
        /// </summary>
        /// <param name="floorIndex">1 or 2 for procedural tiers, 3 for boss.</param>
        public List<EnemyEntry> GetEnemiesForFloor(int floorIndex)
        {
            switch (floorIndex)
            {
                case 1: return tier1Enemies;
                case 2: return tier2Enemies;
                case 3: 
                    // Special case for boss, wrapping it in an entry if needed
                    var bossList = new List<EnemyEntry>();
                    if (finalBossPrefab != null)
                        bossList.Add(new EnemyEntry { prefab = finalBossPrefab, powerLevel = 100 });
                    return bossList;
                default:
                    return new List<EnemyEntry>();
            }
        }
    }
}
