using System;
using System.Collections.Generic;
using UnityEngine;

namespace Remoria.World
{
    /// <summary>
    /// Entry for an item in the database, including its prefab and weight/cost.
    /// </summary>
    [Serializable]
    public class ItemEntry
    {
        [Tooltip("The item prefab (must have ItemPickup script)")]
        public GameObject prefab;

        [Tooltip("Spawn weight/cost of this item. Used for room budget balancing.")]
        [Range(1, 100)]
        public int weight = 10;
    }

    /// <summary>
    /// ScriptableObject that stores a list of items that can be spawned in the level.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Remoria/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [Tooltip("List of items that can be spawned in rooms")]
        public List<ItemEntry> items = new List<ItemEntry>();

        /// <summary>
        /// Returns a random item from the pool based on weight.
        /// </summary>
        public ItemEntry GetRandomItem()
        {
            if (items == null || items.Count == 0) return null;

            int totalWeight = 0;
            foreach (var entry in items)
            {
                totalWeight += entry.weight;
            }

            int randomValue = UnityEngine.Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var entry in items)
            {
                currentWeight += entry.weight;
                if (randomValue < currentWeight)
                {
                    return entry;
                }
            }

            return items[0];
        }
    }
}
