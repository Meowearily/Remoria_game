using UnityEngine;
using Remoria.Interaction;
using System.Collections;

namespace Remoria.World
{
    /// <summary>
    /// A door that can be opened by interacting with it.
    /// When opened, it slides downwards into the floor.
    /// </summary>
    public class Door : MonoBehaviour, IInteractable
    {
        [Header("Settings")]
        [SerializeField] private float openDistance = 3f;
        [SerializeField] private float openSpeed = 2f;
        
        [Header("Fog of War")]
        [SerializeField] private RoomVisibility targetRoom;

        private bool _isOpen = false;
        private Vector3 _closedPosition;
        private Vector3 _openPosition;

        public void SetTargetRoom(RoomVisibility room) => targetRoom = room;

        public string InteractionPrompt => _isOpen ? "" : "Press E to Open Door";

        private void Start()
        {
            _closedPosition = transform.position;
            _openPosition = _closedPosition + Vector3.down * openDistance;
        }

        public void Interact(GameObject interactor)
        {
            if (_isOpen) return;

            Open();
        }

        public void Open()
        {
            if (_isOpen) return;
            _isOpen = true;
            
            // Reveal room content
            if (targetRoom != null) targetRoom.Reveal();

            StartCoroutine(OpenRoutine());
            Debug.Log("[Door] Door is opening...");
        }

        private IEnumerator OpenRoutine()
        {
            float elapsed = 0;
            float duration = openDistance / openSpeed;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(_closedPosition, _openPosition, elapsed / duration);
                yield return null;
            }

            transform.position = _openPosition;
            
            // Door is fully open, destroy it to clear the path
            Destroy(gameObject);
        }
    }
}
