using UnityEngine;
using Remoria.Interaction;

namespace Remoria.Inventory
{
    /// <summary>
    /// A world object that the player can pick up.
    /// Implements IInteractable so it works with the PlayerInteraction system.
    /// 
    /// When the player presses E near this object:
    ///   1. The item is added to the InventoryManager.
    ///   2. The pickup object is destroyed (removed from the world).
    /// 
    /// Visual: The object slowly rotates and bobs up and down to catch attention.
    /// 
    /// Setup:
    ///   1. Create a small cube/sphere.
    ///   2. Add a Collider and check "Is Trigger".
    ///   3. Attach this script.
    ///   4. Create an ItemData asset (Create → RPG → Item).
    ///   5. Drag the ItemData into the "Item" field.
    ///   6. Save as a prefab.
    /// </summary>
    public class ItemPickup : MonoBehaviour, IInteractable
    {
        // ─── Inspector Fields ──────────────────────────────────────────
        [Header("Item")]
        [Tooltip("The item data this pickup gives when collected")]
        [SerializeField] private ItemData itemData;

        [Tooltip("How many of this item the pickup gives")]
        [SerializeField] private int amount = 1;

        [Header("Visual Effects")]
        [Tooltip("Rotation speed in degrees per second")]
        [SerializeField] private float rotateSpeed = 90f;

        [Tooltip("How high the item bobs up and down")]
        [SerializeField] private float bobHeight = 0.3f;

        [Tooltip("How fast the item bobs")]
        [SerializeField] private float bobSpeed = 2f;

        // ─── Runtime ───────────────────────────────────────────────────
        private Vector3 _startPosition;

        // ─── IInteractable Implementation ──────────────────────────────

        public string InteractionPrompt
        {
            get
            {
                if (itemData != null)
                    return $"Pick up {itemData.itemName}";
                return "Pick up";
            }
        }

        public void Interact(GameObject interactor)
        {
            if (itemData == null)
            {
                Debug.LogWarning("[ItemPickup] No ItemData assigned to this pickup!");
                return;
            }

            // Try to add to inventory.
            bool success = InventoryManager.Instance.AddItem(itemData, amount);

            if (success)
            {
                Debug.Log($"[ItemPickup] Player picked up {amount}x {itemData.itemName}.");
                // Remove the pickup from the world.
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("[ItemPickup] Inventory full! Can't pick up.");
            }
        }

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void Start()
        {
            _startPosition = transform.position;
        }

        private void Update()
        {
            // Rotate the object.
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

            // Bob up and down using a sine wave.
            // Mathf.Sin returns values from -1 to 1, creating a smooth oscillation.
            float newY = _startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(
                _startPosition.x,
                newY,
                _startPosition.z
            );
        }
    }
}
