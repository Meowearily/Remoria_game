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
        private Dictionary<RoomData, RoomVisibility> _roomVisibilities = new Dictionary<RoomData, RoomVisibility>();
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

            // Phase 1: Place Rooms & Init Visibility
            GenerateRooms();
            foreach (var room in _grid.Rooms)
            {
                GameObject visObj = new GameObject($"RoomVis_{room.Center}");
                var vis = visObj.AddComponent<RoomVisibility>();
                _roomVisibilities[room] = vis;
                _spawnedObjects.Add(visObj);
            }

            // Phase 2: Connect Rooms with A* Corridors
            GenerateCorridors();

            // Phase 3: Add Walls
            GenerateWalls();

            // Phase 4: Physical Building
            BuildLevel();

            // Phase 5: Distribute Content
            DistributeContent();

            // Special: Reveal start room immediately
            if (_grid.Rooms.Count > 0) _roomVisibilities[_grid.Rooms[0]].Reveal();

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
            _roomVisibilities.Clear();
            _grid.Clear();
        }

        private void GenerateRooms()
        {
            int targetCount = Random.Range(settings.minRooms, settings.maxRooms + 1);
            int attempts = 0;
            int maxAttempts = 500; // Increased attempts to allow for tight packing
            int currentRadius = 10; // Start with a very tight placement radius

            while (_grid.Rooms.Count < targetCount && attempts < maxAttempts)
            {
                attempts++;

                int w = Random.Range(settings.minRoomSize, settings.maxRoomSize + 1);
                int h = Random.Range(settings.minRoomSize, settings.maxRoomSize + 1);
                
                // Random position within the current tight radius
                int x = Random.Range(-currentRadius, currentRadius);
                int y = Random.Range(-currentRadius, currentRadius);

                RectInt area = new RectInt(x, y, w, h);

                if (_grid.IsAreaAvailable(area, padding: 2))
                {
                    _grid.AddRoom(new RoomData(area));
                }
                else
                {
                    // If we fail to find space, expand the search radius slightly
                    if (attempts % 10 == 0)
                    {
                        currentRadius++;
                    }
                }
            }
        }

        private void GenerateCorridors()
        {
            if (_grid.Rooms.Count < 2) return;

            // Connect rooms in sequence to ensure 100% connectivity
            for (int i = 0; i < _grid.Rooms.Count - 1; i++)
            {
                RoomData roomA = _grid.Rooms[i];
                RoomData roomB = _grid.Rooms[i + 1];

                List<Vector2Int> path = AStarPathfinder.FindPath(roomA.Center, roomB.Center, _grid, settings.emptyTileWeight);

                if (path != null)
                {
                    for (int j = 0; j < path.Count; j++)
                    {
                        Vector2Int pos = path[j];
                        CellType currentType = _grid.GetCell(pos);

                        if (currentType == CellType.Empty)
                        {
                            _grid.SetCell(pos, CellType.Corridor);

                            if (settings.doorPrefab != null)
                            {
                                RoomData targetRoom = null;
                                if (j > 0 && _grid.GetCell(path[j - 1]) == CellType.Floor) 
                                    targetRoom = GetRoomAt(path[j - 1]);
                                if (j < path.Count - 1 && _grid.GetCell(path[j + 1]) == CellType.Floor) 
                                    targetRoom = GetRoomAt(path[j + 1]);

                                if (targetRoom != null)
                                {
                                    GameObject doorObj = SpawnAtTile(pos, settings.doorPrefab, "Door", yOffset: 1.5f);
                                    if (doorObj != null)
                                    {
                                        var door = doorObj.GetComponent<Door>();
                                        if (door != null && _roomVisibilities.ContainsKey(targetRoom))
                                        {
                                            door.SetTargetRoom(_roomVisibilities[targetRoom]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private RoomData GetRoomAt(Vector2Int pos)
        {
            foreach (var room in _grid.Rooms)
            {
                if (room.Bounds.Contains(pos)) return room;
            }
            return null;
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
                
                // 1. Handle Floors and Corridors
                if (cell.Value == CellType.Floor || cell.Value == CellType.Corridor)
                {
                    if (settings.floorTilePrefab != null)
                    {
                        GameObject floorGo = Instantiate(settings.floorTilePrefab, worldPos, Quaternion.identity, container.transform);
                        _spawnedObjects.Add(floorGo);
                    }

                    // Check 4 cardinal neighbors to place walls on the edges of this floor tile
                    Vector2Int pos = cell.Key;
                    bool wallN = IsEmpty(pos + Vector2Int.up);    // +Z
                    bool wallS = IsEmpty(pos + Vector2Int.down);  // -Z
                    bool wallE = IsEmpty(pos + Vector2Int.right); // +X
                    bool wallW = IsEmpty(pos + Vector2Int.left);  // -X

                    // Corners (L-shaped) - Check 4 possible corner combinations
                    // North-West Corner
                    if (wallN && wallW) SpawnWall(settings.wallCornerPrefab, worldPos, 0, container.transform);
                    else if (wallN) SpawnStraightWall(worldPos, Vector2Int.up, container.transform);
                    else if (wallW) SpawnStraightWall(worldPos, Vector2Int.left, container.transform);

                    // North-East Corner
                    if (wallN && wallE) SpawnWall(settings.wallCornerPrefab, worldPos, 90, container.transform);
                    else if (wallE) SpawnStraightWall(worldPos, Vector2Int.right, container.transform);

                    // South-East Corner
                    if (wallS && wallE) SpawnWall(settings.wallCornerPrefab, worldPos, 180, container.transform);
                    else if (wallS) SpawnStraightWall(worldPos, Vector2Int.down, container.transform);

                    // South-West Corner
                    if (wallS && wallW) SpawnWall(settings.wallCornerPrefab, worldPos, 270, container.transform);
                }
                
                // 2. Handle Columns for External Corners (Convex corners)
                if (cell.Value == CellType.Empty)
                {
                    Vector2Int pos = cell.Key;
                    bool n = IsFloor(pos + Vector2Int.up);
                    bool s = IsFloor(pos + Vector2Int.down);
                    bool e = IsFloor(pos + Vector2Int.right);
                    bool w = IsFloor(pos + Vector2Int.left);

                    float h = TileSize / 2f;

                    // Если пустая клетка граничит с полом сверху и справа, ставим колонну в их общий угол
                    if (n && e) SpawnColumn(worldPos + new Vector3(h, 0, h), container.transform);
                    // Справа и снизу
                    if (e && s) SpawnColumn(worldPos + new Vector3(h, 0, -h), container.transform);
                    // Снизу и слева
                    if (s && w) SpawnColumn(worldPos + new Vector3(-h, 0, -h), container.transform);
                    // Слева и сверху
                    if (w && n) SpawnColumn(worldPos + new Vector3(-h, 0, h), container.transform);
                }
            }
        }

        private void SpawnColumn(Vector3 pos, Transform parent)
        {
            if (settings.wallColumnPrefab == null) return;
            GameObject colGo = Instantiate(settings.wallColumnPrefab, pos, Quaternion.identity, parent);
            _spawnedObjects.Add(colGo);
        }

        private bool IsEmpty(Vector2Int pos)
        {
            CellType type = _grid.GetCell(pos);
            return type == CellType.Empty || type == CellType.Wall;
        }

        private void SpawnStraightWall(Vector3 tilePos, Vector2Int direction, Transform parent)
        {
            if (settings.wallStraightPrefab == null) return;

            float rotation = 0;
            Vector3 offset = Vector3.zero;
            float halfTile = TileSize / 2f;

            if (direction == Vector2Int.up) { rotation = 0; offset = new Vector3(0, 0, halfTile); }
            else if (direction == Vector2Int.down) { rotation = 180; offset = new Vector3(0, 0, -halfTile); }
            else if (direction == Vector2Int.right) { rotation = 90; offset = new Vector3(halfTile, 0, 0); }
            else if (direction == Vector2Int.left) { rotation = 270; offset = new Vector3(-halfTile, 0, 0); }

            GameObject wallGo = Instantiate(settings.wallStraightPrefab, tilePos + offset, Quaternion.Euler(0, rotation, 0), parent);
            _spawnedObjects.Add(wallGo);
        }

        private void SpawnWall(GameObject prefab, Vector3 pos, float rotation, Transform parent)
        {
            if (prefab == null) return;
            GameObject go = Instantiate(prefab, pos, Quaternion.Euler(0, rotation, 0), parent);
            _spawnedObjects.Add(go);
        }

        private bool ShouldPlaceColumn(Vector2Int pos)
        {
            // A column is needed if this Empty tile touches two Floor tiles that form a convex corner
            bool n = IsFloor(pos + Vector2Int.up);
            bool s = IsFloor(pos + Vector2Int.down);
            bool e = IsFloor(pos + Vector2Int.right);
            bool w = IsFloor(pos + Vector2Int.left);

            // If it touches floor on two adjacent sides, it's an external corner
            return (n && e) || (e && s) || (s && w) || (w && n);
        }

        private bool IsFloor(Vector2Int pos)
        {
            CellType type = _grid.GetCell(pos);
            return type == CellType.Floor || type == CellType.Corridor;
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

            // 3. Enemies & Items
            var availableEnemies = enemyDatabase != null ? enemyDatabase.GetEnemiesForFloor(settings.floorIndex) : null;
            var itemDatabase = settings.itemDatabase;

            for (int i = 0; i < _grid.Rooms.Count; i++)
            {
                RoomData room = _grid.Rooms[i];
                if (i == 0) continue; // Skip content in start room

                // Spawn Enemies
                if (availableEnemies != null && availableEnemies.Count > 0)
                {
                    int enemyBudget = settings.baseEnemyBudget + (i * settings.budgetMultiplierPerRoom);
                    SpawnEnemiesInRoom(room, availableEnemies, enemyBudget);
                }

                // Spawn Items
                if (itemDatabase != null && Random.value <= settings.lootChance)
                {
                    int itemBudget = settings.baseItemBudget + (i * settings.itemBudgetMultiplierPerRoom);
                    SpawnItemsInRoom(room, itemDatabase, itemBudget);
                }
            }
        }

        private void SpawnItemsInRoom(RoomData room, ItemDatabase database, int budget)
        {
            int currentBudget = budget;
            int maxAttempts = 15;
            int attempts = 0;

            // Gather potential free tiles (avoid center)
            List<Vector2Int> freeTiles = new List<Vector2Int>(room.Tiles);
            freeTiles.Remove(room.Center);

            while (currentBudget > 0 && freeTiles.Count > 0 && attempts < maxAttempts)
            {
                attempts++;
                var entry = database.GetRandomItem();
                if (entry == null) break;

                if (entry.weight <= currentBudget)
                {
                    int tileIndex = Random.Range(0, freeTiles.Count);
                    Vector2Int spawnPos = freeTiles[tileIndex];

                    SpawnAtTile(spawnPos, entry.prefab, $"Item_{entry.prefab.name}", yOffset: 0.5f);
                    
                    currentBudget -= entry.weight;
                    freeTiles.RemoveAt(tileIndex);
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

            // NEW: If this is an enemy or item, register it with the room's visibility
            RoomData room = GetRoomAt(gridPos);
            if (room != null && _roomVisibilities.ContainsKey(room))
            {
                // We only hide enemies and items, not portals or doors
                if (name.StartsWith("Enemy") || name.StartsWith("Item"))
                {
                    _roomVisibilities[room].RegisterContent(go);
                }
            }

            return go;
        }
    }
}
