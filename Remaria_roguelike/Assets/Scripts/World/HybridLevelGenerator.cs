using System.Collections.Generic;
using UnityEngine;
using Remoria.Enemy;

namespace Remoria.World
{
    /// <summary>
    /// The main controller for Stage 2 & 3 hybrid level generation.
    /// Handles Room placement, A* Corridors, physical building, and content distribution.
    /// </summary>
    public class HybridLevelGenerator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private LevelSettings settings;
        
        public void SetSettings(LevelSettings newSettings)
        {
            settings = newSettings;
        }

        [SerializeField] private int seed = 0;
        [SerializeField] private bool generateOnStart = true;

        [Header("Content")]
        [SerializeField] private EnemyDatabase enemyDatabase;
        [SerializeField] private GameObject playerPrefab;

        private DungeonGrid _grid = new DungeonGrid();
        private List<GameObject> _spawnedObjects = new List<GameObject>();
        private const float TileSize = 3f;

        private void Start()
        {
            if (generateOnStart && settings != null)
            {
                Generate();
            }
        }

        public void Generate()
        {
            Clear();

            if (settings == null)
            {
                Debug.LogError("[HybridLevelGenerator] No LevelSettings assigned!");
                return;
            }

            if (seed != 0) Random.InitState(seed);
            else Random.InitState(System.Environment.TickCount);

            // Phase 1: Place Rooms
            GenerateRooms();

            // Phase 2: Connect Rooms with A* Corridors
            GenerateCorridors();

            // Phase 3: Add Walls
            GenerateWalls();

            // Phase 4: Physical Building
            BuildLevel();

            // Phase 5: Distribute Content
            DistributeContent();

            Debug.Log($"[HybridLevelGenerator] Generation complete. Rooms: {_grid.Rooms.Count}");
        }

        public void Clear()
        {
            foreach (var obj in _spawnedObjects)
            {
                if (obj != null)
                {
                    if (Application.isPlaying) Destroy(obj);
                    else DestroyImmediate(obj);
                }
            }
            _spawnedObjects.Clear();
            _grid.Clear();
        }

        private void GenerateRooms()
        {
            int targetCount = Random.Range(settings.minRooms, settings.maxRooms + 1);
            int attempts = 0;
            int maxAttempts = 150;

            while (_grid.Rooms.Count < targetCount && attempts < maxAttempts)
            {
                attempts++;

                int w = Random.Range(settings.minRoomSize, settings.maxRoomSize + 1);
                int h = Random.Range(settings.minRoomSize, settings.maxRoomSize + 1);
                
                // Random position within a reasonable range
                int x = Random.Range(-30, 30);
                int y = Random.Range(-30, 30);

                RectInt area = new RectInt(x, y, w, h);

                if (_grid.IsAreaAvailable(area, padding: 2))
                {
                    _grid.AddRoom(new RoomData(area));
                }
            }
        }

        private void GenerateCorridors()
        {
            if (_grid.Rooms.Count < 2) return;

            // Connect rooms in sequence to ensure 100% connectivity
            for (int i = 0; i < _grid.Rooms.Count - 1; i++)
            {
                Vector2Int start = _grid.Rooms[i].Center;
                Vector2Int end = _grid.Rooms[i + 1].Center;

                List<Vector2Int> path = AStarPathfinder.FindPath(start, end, _grid, settings.emptyTileWeight);

                if (path != null)
                {
                    foreach (var pos in path)
                    {
                        if (_grid.GetCell(pos) == CellType.Empty)
                        {
                            _grid.SetCell(pos, CellType.Corridor);
                        }
                    }
                }
            }
        }

        private void GenerateWalls()
        {
            List<Vector2Int> floorPositions = new List<Vector2Int>();
            foreach (var cell in _grid.Cells)
            {
                if (cell.Value == CellType.Floor || cell.Value == CellType.Corridor)
                {
                    floorPositions.Add(cell.Key);
                }
            }

            foreach (var pos in floorPositions)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        Vector2Int neighbor = pos + new Vector2Int(x, y);
                        if (_grid.GetCell(neighbor) == CellType.Empty)
                        {
                            _grid.SetCell(neighbor, CellType.Wall);
                        }
                    }
                }
            }
        }

        private void BuildLevel()
        {
            GameObject container = new GameObject("GeneratedLevel");
            _spawnedObjects.Add(container);

            foreach (var cell in _grid.Cells)
            {
                Vector3 worldPos = new Vector3(cell.Key.x * TileSize, 0, cell.Key.y * TileSize);
                GameObject prefab = null;

                switch (cell.Value)
                {
                    case CellType.Floor:
                    case CellType.Corridor:
                        prefab = settings.floorTilePrefab;
                        break;
                    case CellType.Wall:
                        prefab = settings.wallTilePrefab;
                        break;
                }

                if (prefab != null)
                {
                    GameObject go = Instantiate(prefab, worldPos, Quaternion.identity, container.transform);
                    _spawnedObjects.Add(go);
                }
            }
        }

        private void DistributeContent()
        {
            if (_grid.Rooms.Count == 0) return;

            // 1. Player Spawn (First Room Center - with safety check)
            RoomData startRoom = _grid.Rooms[0];
            Vector2Int spawnTile = startRoom.Center;

            // Safety: if center is not floor (unlikely but possible), find any floor tile in the room
            if (_grid.GetCell(spawnTile) != CellType.Floor)
            {
                foreach (var tile in startRoom.Tiles)
                {
                    if (_grid.GetCell(tile) == CellType.Floor)
                    {
                        spawnTile = tile;
                        break;
                    }
                }
            }

            Vector3 spawnPos = new Vector3(spawnTile.x * TileSize, 1.5f, spawnTile.y * TileSize);
            
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                // Move existing player and reset velocity
                existingPlayer.transform.position = spawnPos;
                var rb = existingPlayer.GetComponent<Rigidbody>();
                if (rb != null) rb.velocity = Vector3.zero;
                
                Debug.Log($"[HybridLevelGenerator] Existing Player found. Moved to safe tile: {spawnPos}");
            }
            else
            {
                Debug.Log($"[HybridLevelGenerator] Spawning New Player at safe tile: {spawnTile} (World: {spawnPos})");
                SpawnAtTile(spawnTile, playerPrefab, "Player", yOffset: 1.5f);
            }

            // 2. Exit Spawn (Last Room Center)
            RoomData endRoom = _grid.Rooms[_grid.Rooms.Count - 1];
            GameObject exitObj = SpawnAtTile(endRoom.Center, settings.exitPrefab, "ExitPortal", yOffset: 0.1f);
            if (exitObj != null && exitObj.GetComponent<ExitPortal>() == null)
            {
                exitObj.AddComponent<ExitPortal>();
            }

            // 3. Enemies
            var availableEnemies = enemyDatabase != null ? enemyDatabase.GetEnemiesForFloor(settings.floorIndex) : null;

            for (int i = 0; i < _grid.Rooms.Count; i++)
            {
                RoomData room = _grid.Rooms[i];
                if (i == 0) continue; // Skip enemy spawn in start room

                if (availableEnemies != null && availableEnemies.Count > 0)
                {
                    int budget = settings.baseEnemyBudget + (i * settings.budgetMultiplierPerRoom);
                    SpawnEnemiesInRoom(room, availableEnemies, budget);
                }
            }
        }

        private void SpawnEnemiesInRoom(RoomData room, List<EnemyEntry> pool, int budget)
        {
            int currentBudget = budget;
            int maxAttempts = 20;
            int attempts = 0;

            List<Vector2Int> freeTiles = new List<Vector2Int>(room.Tiles);
            freeTiles.Remove(room.Center);

            while (currentBudget > 0 && freeTiles.Count > 0 && attempts < maxAttempts)
            {
                attempts++;
                var enemyEntry = pool[Random.Range(0, pool.Count)];

                if (enemyEntry.powerLevel <= currentBudget)
                {
                    int tileIndex = Random.Range(0, freeTiles.Count);
                    Vector2Int spawnPos = freeTiles[tileIndex];
                    
                    GameObject enemyObj = SpawnAtTile(spawnPos, enemyEntry.prefab, $"Enemy_{enemyEntry.prefab.name}", yOffset: 0.5f);
                    
                    // NEW: Assign home room to AI
                    if (enemyObj != null)
                    {
                        var ai = enemyObj.GetComponent<Remoria.Enemy.EnemyAI>();
                        if (ai != null) ai.SetHomeRoom(room);
                    }
                    
                    currentBudget -= enemyEntry.powerLevel;
                    freeTiles.RemoveAt(tileIndex);
                }
            }
        }

        private GameObject SpawnAtTile(Vector2Int gridPos, GameObject prefab, string name, float yOffset = 0f)
        {
            if (prefab == null) return null;

            Vector3 worldPos = new Vector3(gridPos.x * TileSize, yOffset, gridPos.y * TileSize);
            GameObject go = Instantiate(prefab, worldPos, Quaternion.identity);
            go.name = name;
            _spawnedObjects.Add(go);
            return go;
        }
    }
}
