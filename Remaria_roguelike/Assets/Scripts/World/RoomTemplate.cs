using UnityEngine;

namespace Remoria.World
{
    /// <summary>
    /// ScriptableObject defining a room type (template/blueprint).
    /// 
    /// A room template describes WHAT a room is:
    ///   - Its prefab (the visual/physical layout)
    ///   - Its category (combat room, safe room, loot room, boss room)
    ///   - Where enemies and items can spawn inside it
    /// 
    /// The LevelGenerator picks from a RoomDatabase of these templates
    /// and assembles them into a level.
    /// 
    /// To create a new room type:
    ///   1. Build a room layout in Unity (walls, floors, decorations as a prefab).
    ///   2. Add RoomConnector components to the doorway/exit points.
    ///   3. Add empty GameObjects at positions where enemies/items should spawn.
    ///   4. Save as a prefab.
    ///   5. Create → RPG → Room Template.
    ///   6. Drag the prefab and fill in the details.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRoomTemplate", menuName = "RPG/Room Template")]
    public class RoomTemplate : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name for this room type")]
        public string roomName = "Room";

        [Tooltip("The prefab to instantiate for this room")]
        public GameObject roomPrefab;

        [Header("Classification")]
        [Tooltip("What category this room belongs to")]
        public RoomCategory category = RoomCategory.Combat;

        [Tooltip("How likely this room is to be chosen (higher = more common)")]
        [Range(1, 10)]
        public int weight = 5;

        [Header("Room Dimensions")]
        [Tooltip("Size of the room in world units (used for placement calculations)")]
        public Vector3 roomSize = new Vector3(10f, 5f, 10f);
    }

    /// <summary>
    /// Room categories. The LevelGenerator can require certain categories
    /// (e.g., "at least 1 boss room, at least 2 loot rooms").
    /// </summary>
    public enum RoomCategory
    {
        Start,    // The first room where the player spawns
        Combat,   // Rooms with enemies
        Safe,     // Rest areas, no enemies
        Loot,     // Treasure rooms
        Boss,     // Boss encounter
        Corridor  // Connecting hallways
    }
}
