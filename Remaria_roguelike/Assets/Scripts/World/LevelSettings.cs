using UnityEngine;

namespace Remoria.World
{
    /// <summary>
    /// ScriptableObject containing configuration for procedural level generation.
    /// Allows defining different parameters for Level 1 and Level 2.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelSettings", menuName = "Remoria/Level Settings")]
    public class LevelSettings : ScriptableObject
    {
        [Header("Floor Info")]
        [Tooltip("The floor index this setting applies to (1 or 2)")]
        [Range(1, 2)]
        public int floorIndex = 1;

        [Header("Room Count")]
        [Tooltip("Minimum number of rooms to generate")]
        public int minRooms = 6;
        [Tooltip("Maximum number of rooms to generate")]
        public int maxRooms = 10;

        [Header("Room Dimensions (in Tiles)")]
        [Tooltip("Minimum width/length of a room in 3x3 tiles")]
        public int minRoomSize = 3;
        [Tooltip("Maximum width/length of a room in 3x3 tiles")]
        public int maxRoomSize = 8;

        [Header("Corridors")]
        [Tooltip("Base weight for Empty tiles in A* (higher = corridors prefer overlapping)")]
        public int emptyTileWeight = 5;

        [Header("Content Distribution")]
        [Tooltip("Base budget for enemy power levels per room")]
        public int baseEnemyBudget = 20;
        
        [Tooltip("Additional budget added per room from the start")]
        public int budgetMultiplierPerRoom = 5;

        [Tooltip("Chance (0-1) for a bonus item/chest to spawn in a room")]
        [Range(0f, 1f)]
        public float lootChance = 0.2f;

        [Header("Item Distribution")]
        [Tooltip("Database of items that can be spawned")]
        public ItemDatabase itemDatabase;

        [Tooltip("Base budget for items per room")]
        public int baseItemBudget = 10;

        [Tooltip("Additional item budget added per room from the start")]
        public int itemBudgetMultiplierPerRoom = 5;

        [Header("Decorations")]
        [Tooltip("List of possible decoration prefabs (boxes, barrels, columns)")]
        public GameObject[] decorationPrefabs;

        [Tooltip("Percentage of room area filled with decorations (0 to 1)")]
        [Range(0f, 0.5f)]
        public float decorationDensity = 0.1f;

        [Header("Visuals (Tiles)")]
        [Header("Floor Models")]
        [Tooltip("List of possible 3x3 Floor tile prefabs")]
        public GameObject[] floorTilePrefabs;
        
        [Header("Wall Models")]
        [Tooltip("Prefab for a straight wall section")]
        public GameObject wallStraightPrefab;

        [Tooltip("Prefab for an L-shaped corner wall section")]
        public GameObject wallCornerPrefab;

        [Tooltip("Prefab for a column used for external corners")]
        public GameObject wallColumnPrefab;

        [Header("Other Prefabs")]
        [Tooltip("Prefab for the level exit (portal/stairs)")]
        public GameObject exitPrefab;

        [Tooltip("Prefab for the door arch (static part)")]
        public GameObject doorArchPrefab;

        [Tooltip("Prefab for the moving part of the door")]
        public GameObject doorMovingPartPrefab;
    }
}
