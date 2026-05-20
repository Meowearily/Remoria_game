using System.Collections.Generic;
using UnityEngine;

namespace Remoria.World
{
    /// <summary>
    /// Types of cells that can exist in our grid.
    /// </summary>
    public enum CellType
    {
        Empty,
        Floor,
        Wall,
        Corridor,
        RoomCenter // Helper for pathfinding
    }

    /// <summary>
    /// Represents a logical room area for content placement later.
    /// </summary>
    public class RoomData
    {
        public RectInt Bounds;
        
        // Corrected Center calculation using FloorToInt to handle negative coordinates properly
        public Vector2Int Center => new Vector2Int(
            Mathf.FloorToInt(Bounds.xMin + Bounds.width / 2f), 
            Mathf.FloorToInt(Bounds.yMin + Bounds.height / 2f)
        );

        public List<Vector2Int> Tiles = new List<Vector2Int>();

        public RoomData(RectInt bounds)
        {
            Bounds = bounds;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Tiles.Add(new Vector2Int(x, y));
                }
            }
        }
    }

    /// <summary>
    /// The logical representation of the level.
    /// Manages the state of every 3x3 tile.
    /// </summary>
    public class DungeonGrid
    {
        private Dictionary<Vector2Int, CellType> _grid = new Dictionary<Vector2Int, CellType>();
        private List<RoomData> _rooms = new List<RoomData>();

        public Dictionary<Vector2Int, CellType> Cells => _grid;
        public List<RoomData> Rooms => _rooms;

        public void SetCell(Vector2Int pos, CellType type)
        {
            _grid[pos] = type;
        }

        public CellType GetCell(Vector2Int pos)
        {
            return _grid.ContainsKey(pos) ? _grid[pos] : CellType.Empty;
        }

        public void AddRoom(RoomData room)
        {
            _rooms.Add(room);
            foreach (var tile in room.Tiles)
            {
                SetCell(tile, CellType.Floor);
            }
        }

        /// <summary>
        /// Checks if a rectangle overlaps with any existing floors (including padding).
        /// </summary>
        public bool IsAreaAvailable(RectInt area, int padding = 1)
        {
            for (int x = area.xMin - padding; x < area.xMax + padding; x++)
            {
                for (int y = area.yMin - padding; y < area.yMax + padding; y++)
                {
                    if (GetCell(new Vector2Int(x, y)) != CellType.Empty)
                        return false;
                }
            }
            return true;
        }

        public void Clear()
        {
            _grid.Clear();
            _rooms.Clear();
        }
    }
}
