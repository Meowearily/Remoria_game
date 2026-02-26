using UnityEngine;

namespace Remoria.World
{
    /// <summary>
    /// ScriptableObject that holds a collection of room templates.
    /// The LevelGenerator picks rooms from this database.
    /// 
    /// Create: right-click → Create → RPG → Room Database
    /// Then drag all your RoomTemplate assets into the list.
    /// </summary>
    [CreateAssetMenu(fileName = "RoomDatabase", menuName = "RPG/Room Database")]
    public class RoomDatabase : ScriptableObject
    {
        [Header("Available Rooms")]
        [Tooltip("All room templates available for level generation")]
        public RoomTemplate[] rooms;

        /// <summary>
        /// Get all rooms of a specific category.
        /// </summary>
        public RoomTemplate[] GetRoomsByCategory(RoomCategory category)
        {
            var result = new System.Collections.Generic.List<RoomTemplate>();
            foreach (var room in rooms)
            {
                if (room != null && room.category == category)
                    result.Add(room);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Get a random room, weighted by each room's weight value.
        /// Higher weight = more likely to be picked.
        /// </summary>
        public RoomTemplate GetRandomRoom(RoomCategory? excludeCategory = null)
        {
            if (rooms == null || rooms.Length == 0) return null;

            // Build a weighted list.
            var candidates = new System.Collections.Generic.List<RoomTemplate>();
            int totalWeight = 0;

            foreach (var room in rooms)
            {
                if (room == null) continue;
                if (excludeCategory.HasValue && room.category == excludeCategory.Value) continue;

                // Add the room 'weight' number of times (simple weighted random).
                for (int i = 0; i < room.weight; i++)
                {
                    candidates.Add(room);
                }
                totalWeight += room.weight;
            }

            if (candidates.Count == 0) return null;

            return candidates[Random.Range(0, candidates.Count)];
        }
    }
}
