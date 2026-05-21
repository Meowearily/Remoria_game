using UnityEngine;
using System.Collections.Generic;

namespace Remoria.World
{
    /// <summary>
    /// Manages the visibility of objects within a specific room.
    /// Hides enemies and items until the room is "revealed" (e.g., door opened).
    /// </summary>
    public class RoomVisibility : MonoBehaviour
    {
        private List<GameObject> _roomContent = new List<GameObject>();
        private bool _isRevealed = false;

        public void RegisterContent(GameObject obj)
        {
            _roomContent.Add(obj);
            // Hide immediately if room not revealed yet
            if (!_isRevealed)
            {
                obj.SetActive(false);
            }
        }

        public void Reveal()
        {
            if (_isRevealed) return;
            _isRevealed = true;

            foreach (var obj in _roomContent)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
            
            Debug.Log($"[RoomVisibility] Room content revealed! Count: {_roomContent.Count}");
        }
    }
}
