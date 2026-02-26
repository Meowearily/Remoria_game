using UnityEngine;

namespace Remoria.World
{
    /// <summary>
    /// Marks a connection point (doorway/exit) in a room prefab.
    /// 
    /// Place this on empty GameObjects at each doorway/exit of a room prefab.
    /// The LevelGenerator uses these points to snap rooms together.
    /// 
    /// How it works:
    ///   - Each connector has a DIRECTION (North, South, East, West).
    ///   - North connects to South, East connects to West (opposites pair).
    ///   - The connector's Transform.position is the exact snap point.
    ///   - When two rooms connect, their connectors line up.
    /// 
    /// Setup:
    ///   1. In your room prefab, create an empty GameObject at each doorway.
    ///   2. Position it exactly at the center of the doorway opening.
    ///   3. Attach this script.
    ///   4. Set the direction (which wall this door is on).
    /// </summary>
    public class RoomConnector : MonoBehaviour
    {
        // ─── Inspector Fields ──────────────────────────────────────────
        [Header("Connection Settings")]
        [Tooltip("Which direction this connector faces (relative to the room)")]
        [SerializeField] private ConnectionDirection direction = ConnectionDirection.North;

        [Tooltip("Is this connector currently connected to another room?")]
        [SerializeField] private bool isConnected = false;

        // ─── Public Properties ─────────────────────────────────────────
        public ConnectionDirection Direction => direction;
        public bool IsConnected { get => isConnected; set => isConnected = value; }

        /// <summary>
        /// Returns the opposite direction. Used for matching connectors.
        /// North ↔ South, East ↔ West.
        /// </summary>
        public ConnectionDirection OppositeDirection
        {
            get
            {
                switch (direction)
                {
                    case ConnectionDirection.North: return ConnectionDirection.South;
                    case ConnectionDirection.South: return ConnectionDirection.North;
                    case ConnectionDirection.East:  return ConnectionDirection.West;
                    case ConnectionDirection.West:  return ConnectionDirection.East;
                    default: return ConnectionDirection.North;
                }
            }
        }

        /// <summary>
        /// Returns the direction as a Vector3 for position calculations.
        /// </summary>
        public Vector3 DirectionVector
        {
            get
            {
                switch (direction)
                {
                    case ConnectionDirection.North: return Vector3.forward;
                    case ConnectionDirection.South: return Vector3.back;
                    case ConnectionDirection.East:  return Vector3.right;
                    case ConnectionDirection.West:  return Vector3.left;
                    default: return Vector3.forward;
                }
            }
        }

        // ─── Gizmos ────────────────────────────────────────────────────
        // Draw an arrow in the Scene view showing the connector direction.

        private void OnDrawGizmos()
        {
            Gizmos.color = isConnected ? Color.green : Color.cyan;
            Gizmos.DrawSphere(transform.position, 0.3f);

            // Draw direction arrow.
            Gizmos.DrawLine(transform.position, transform.position + DirectionVector * 1.5f);
        }
    }

    /// <summary>
    /// Cardinal directions for room connections.
    /// </summary>
    public enum ConnectionDirection
    {
        North,  // +Z
        South,  // -Z
        East,   // +X
        West    // -X
    }
}
