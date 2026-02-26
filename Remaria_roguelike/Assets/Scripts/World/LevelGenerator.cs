using System.Collections.Generic;
using UnityEngine;

namespace Remoria.World
{
    /// <summary>
    /// The level generator — the brain of the Room Template system.
    /// 
    /// Takes a RoomDatabase and snaps room prefabs together via their
    /// RoomConnector points to create a complete level.
    /// 
    /// How it works:
    ///   1. Places a Start room at the origin.
    ///   2. Finds all unconnected connectors (open doorways).
    ///   3. Picks a random room from the database.
    ///   4. Snaps the new room to an open connector (aligning opposite directions).
    ///   5. Repeats until the desired room count is reached.
    ///   6. Optionally ensures a Boss room is placed at the end.
    /// 
    /// Key Concepts:
    ///   - "Snapping" means positioning Room B so that its South door 
    ///     lines up with Room A's North door.
    ///   - Overlap detection prevents rooms from stacking on top of each other.
    ///   - A seed value makes generation reproducible (same seed = same level).
    /// 
    /// Setup:
    ///   1. Create a RoomDatabase asset with room templates.
    ///   2. Create an empty GameObject in your scene.
    ///   3. Attach this script.
    ///   4. Assign the RoomDatabase.
    ///   5. Press Play — the level generates automatically.
    ///   
    /// Or call Generate() from your own code at any time.
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        // ─── Inspector Fields ──────────────────────────────────────────
        [Header("Database")]
        [Tooltip("The room database to pick rooms from")]
        [SerializeField] private RoomDatabase roomDatabase;

        [Header("Generation Settings")]
        [Tooltip("Total number of rooms to generate (including start and boss)")]
        [SerializeField] private int targetRoomCount = 8;

        [Tooltip("Random seed. Same seed = same level. 0 = random each time.")]
        [SerializeField] private int seed = 0;

        [Tooltip("Automatically generate on Start")]
        [SerializeField] private bool generateOnStart = true;

        [Header("Required Rooms")]
        [Tooltip("Must include at least one boss room")]
        [SerializeField] private bool requireBossRoom = true;

        [Header("Overlap Prevention")]
        [Tooltip("Minimum distance between room centers to prevent overlaps")]
        [SerializeField] private float minRoomSpacing = 8f;

        // ─── Runtime ───────────────────────────────────────────────────
        private List<GameObject> _spawnedRooms = new List<GameObject>();
        private List<Vector3> _roomPositions = new List<Vector3>();
        private List<RoomConnector> _openConnectors = new List<RoomConnector>();

        // ─── Public Properties ─────────────────────────────────────────
        public List<GameObject> SpawnedRooms => _spawnedRooms;
        public int RoomCount => _spawnedRooms.Count;

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void Start()
        {
            if (generateOnStart)
            {
                Generate();
            }
        }

        // ─── Public Methods ────────────────────────────────────────────

        /// <summary>
        /// Generate a new level. Clears any existing rooms first.
        /// </summary>
        public void Generate()
        {
            ClearLevel();

            if (roomDatabase == null)
            {
                Debug.LogError("[LevelGenerator] No RoomDatabase assigned!");
                return;
            }

            // Initialize random seed.
            if (seed != 0)
            {
                Random.InitState(seed);
            }
            else
            {
                Random.InitState(System.Environment.TickCount);
            }

            Debug.Log($"[LevelGenerator] Generating level with {targetRoomCount} rooms...");

            // Step 1: Place the start room.
            PlaceStartRoom();

            // Step 2: Build outward from open connectors.
            int attempts = 0;
            int maxAttempts = targetRoomCount * 10; // Safety limit.

            while (_spawnedRooms.Count < targetRoomCount && attempts < maxAttempts)
            {
                attempts++;

                // Check if we need to place a boss room (last room).
                bool placeBoss = requireBossRoom && _spawnedRooms.Count == targetRoomCount - 1;

                if (!TryPlaceNextRoom(placeBoss))
                {
                    // Could not place a room this attempt.
                    // This might happen if all connectors are blocked.
                    continue;
                }
            }

            Debug.Log($"[LevelGenerator] Level generated! {_spawnedRooms.Count} rooms placed after {attempts} attempts.");
        }

        /// <summary>
        /// Destroy all generated rooms and reset.
        /// </summary>
        public void ClearLevel()
        {
            foreach (var room in _spawnedRooms)
            {
                if (room != null)
                    Destroy(room);
            }
            _spawnedRooms.Clear();
            _roomPositions.Clear();
            _openConnectors.Clear();
        }

        // ─── Private Methods ───────────────────────────────────────────

        private void PlaceStartRoom()
        {
            // Find a Start-category room.
            RoomTemplate[] startRooms = roomDatabase.GetRoomsByCategory(RoomCategory.Start);
            RoomTemplate startTemplate = startRooms.Length > 0
                ? startRooms[Random.Range(0, startRooms.Length)]
                : roomDatabase.rooms[0]; // Fallback to first room.

            if (startTemplate == null || startTemplate.roomPrefab == null)
            {
                Debug.LogError("[LevelGenerator] No valid start room found!");
                return;
            }

            GameObject startRoom = Instantiate(startTemplate.roomPrefab, Vector3.zero, Quaternion.identity, transform);
            startRoom.name = $"Room_0_Start_{startTemplate.roomName}";

            _spawnedRooms.Add(startRoom);
            _roomPositions.Add(Vector3.zero);

            // Collect open connectors from this room.
            CollectOpenConnectors(startRoom);

            Debug.Log($"[LevelGenerator] Placed start room: {startTemplate.roomName}");
        }

        private bool TryPlaceNextRoom(bool mustBeBoss)
        {
            if (_openConnectors.Count == 0)
            {
                Debug.LogWarning("[LevelGenerator] No open connectors available!");
                return false;
            }

            // Pick a random open connector to build from.
            int connectorIndex = Random.Range(0, _openConnectors.Count);
            RoomConnector sourceConnector = _openConnectors[connectorIndex];

            if (sourceConnector == null)
            {
                _openConnectors.RemoveAt(connectorIndex);
                return false;
            }

            // Pick a room template.
            RoomTemplate template;
            if (mustBeBoss)
            {
                RoomTemplate[] bossRooms = roomDatabase.GetRoomsByCategory(RoomCategory.Boss);
                if (bossRooms.Length == 0)
                {
                    template = roomDatabase.GetRandomRoom(RoomCategory.Start);
                }
                else
                {
                    template = bossRooms[Random.Range(0, bossRooms.Length)];
                }
            }
            else
            {
                // Don't pick Start or Boss rooms for regular placement.
                template = roomDatabase.GetRandomRoom(RoomCategory.Start);
            }

            if (template == null || template.roomPrefab == null)
            {
                return false;
            }

            // Calculate where the new room should go.
            // The new room needs to be offset from the source connector in its direction.
            Vector3 offset = sourceConnector.DirectionVector * template.roomSize.z;
            Vector3 newRoomPosition = sourceConnector.transform.position + offset;

            // Check for overlaps with existing rooms.
            if (IsPositionOccupied(newRoomPosition))
            {
                return false;
            }

            // Place the room!
            GameObject newRoom = Instantiate(
                template.roomPrefab,
                newRoomPosition,
                Quaternion.identity,
                transform
            );
            newRoom.name = $"Room_{_spawnedRooms.Count}_{template.category}_{template.roomName}";

            _spawnedRooms.Add(newRoom);
            _roomPositions.Add(newRoomPosition);

            // Mark the source connector as connected.
            sourceConnector.IsConnected = true;
            _openConnectors.RemoveAt(connectorIndex);

            // Try to connect the matching connector in the new room.
            ConnectMatchingConnector(newRoom, sourceConnector.OppositeDirection);

            // Collect remaining open connectors from the new room.
            CollectOpenConnectors(newRoom);

            Debug.Log($"[LevelGenerator] Placed room: {template.roomName} ({template.category})");
            return true;
        }

        /// <summary>
        /// Finds and marks the connector in the new room that faces the source room.
        /// </summary>
        private void ConnectMatchingConnector(GameObject room, ConnectionDirection targetDirection)
        {
            RoomConnector[] connectors = room.GetComponentsInChildren<RoomConnector>();
            foreach (var connector in connectors)
            {
                if (connector.Direction == targetDirection && !connector.IsConnected)
                {
                    connector.IsConnected = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Collect all unconnected connectors from a room.
        /// </summary>
        private void CollectOpenConnectors(GameObject room)
        {
            RoomConnector[] connectors = room.GetComponentsInChildren<RoomConnector>();
            foreach (var connector in connectors)
            {
                if (!connector.IsConnected)
                {
                    _openConnectors.Add(connector);
                }
            }
        }

        /// <summary>
        /// Check if a position is too close to any existing room.
        /// </summary>
        private bool IsPositionOccupied(Vector3 position)
        {
            foreach (var existingPos in _roomPositions)
            {
                if (Vector3.Distance(position, existingPos) < minRoomSpacing)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
