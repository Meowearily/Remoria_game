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

        [Header("Visuals (Tiles)")]
        [Tooltip("Prefab for a 3x3 Floor tile")]
        public GameObject floorTilePrefab;
        
        [Tooltip("Prefab for a 3x3 Wall tile")]
        public GameObject wallTilePrefab;

        [Tooltip("Prefab for the level exit (portal/stairs)")]
        public GameObject exitPrefab;
    }
}
