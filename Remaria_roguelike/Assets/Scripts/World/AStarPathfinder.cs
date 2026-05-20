using System.Collections.Generic;
using UnityEngine;

namespace Remoria.World
{
    /// <summary>
    /// Simple A* implementation for connecting rooms on the grid.
    /// </summary>
    public static class AStarPathfinder
    {
        private class Node
        {
            public Vector2Int Position;
            public Node Parent;
            public int G; // Cost from start
            public int H; // Heuristic to end
            public int F => G + H;

            public Node(Vector2Int pos) => Position = pos;
        }

        public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, DungeonGrid grid, int emptyWeight = 5)
        {
            var openSet = new List<Node>();
            var closedSet = new HashSet<Vector2Int>();
            openSet.Add(new Node(start));

            while (openSet.Count > 0)
            {
                // Get node with lowest F cost
                Node current = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].F < current.F || (openSet[i].F == current.F && openSet[i].H < current.H))
                        current = openSet[i];
                }

                openSet.Remove(current);
                closedSet.Add(current.Position);

                if (current.Position == end)
                {
                    return RetracePath(current);
                }

                foreach (Vector2Int neighborPos in GetNeighbors(current.Position))
                {
                    if (closedSet.Contains(neighborPos)) continue;

                    // Calculate movement cost
                    // If it's already a Floor or Corridor, cost is low (1)
                    // If it's Empty, cost is high (emptyWeight) to encourage path merging
                    CellType cell = grid.GetCell(neighborPos);
                    int moveCost = (cell == CellType.Floor || cell == CellType.Corridor) ? 1 : emptyWeight;

                    int newG = current.G + moveCost;
                    Node neighbor = openSet.Find(n => n.Position == neighborPos);

                    if (neighbor == null || newG < neighbor.G)
                    {
                        if (neighbor == null)
                        {
                            neighbor = new Node(neighborPos);
                            openSet.Add(neighbor);
                        }

                        neighbor.Parent = current;
                        neighbor.G = newG;
                        neighbor.H = Mathf.Abs(neighborPos.x - end.x) + Mathf.Abs(neighborPos.y - end.y);
                    }
                }
            }

            return null; // No path found
        }

        private static List<Vector2Int> RetracePath(Node endNode)
        {
            var path = new List<Vector2Int>();
            Node current = endNode;
            while (current != null)
            {
                path.Add(current.Position);
                current = current.Parent;
            }
            path.Reverse();
            return path;
        }

        private static IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos)
        {
            yield return pos + Vector2Int.up;
            yield return pos + Vector2Int.down;
            yield return pos + Vector2Int.left;
            yield return pos + Vector2Int.right;
        }
    }
}
