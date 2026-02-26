using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Remoria.Inventory;

namespace Remoria.UI
{
    /// <summary>
    /// Inventory panel UI. Toggled with the I key.
    /// Displays a grid of inventory slots showing item icons and stack counts.
    /// 
    /// Subscribes to InventoryManager.OnInventoryChanged to auto-update.
    /// 
    /// Setup:
    ///   1. Create a Panel inside the Canvas.
    ///   2. Add a GridLayoutGroup component (for automatic grid layout).
    ///      - Cell Size: 80x80, Spacing: 5x5
    ///   3. Attach this script to the Panel.
    ///   4. Create an InventorySlotUI prefab (see InventorySlotUI.cs).
    ///   5. Drag the prefab into "Slot Prefab".
    ///   6. Drag the Grid panel (with GridLayoutGroup) into "Slot Container".
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        // ─── Inspector Fields ──────────────────────────────────────────
        [Header("References")]
        [Tooltip("The prefab for one inventory slot")]
        [SerializeField] private GameObject slotPrefab;

        [Tooltip("The parent transform for slots (should have a GridLayoutGroup)")]
        [SerializeField] private Transform slotContainer;

        // ─── Runtime ───────────────────────────────────────────────────
        private List<InventorySlotUI> _slotUIs = new List<InventorySlotUI>();

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void OnEnable()
        {
            // When the panel is shown, refresh and subscribe.
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged += RefreshUI;
                RefreshUI();
            }
        }

        private void OnDisable()
        {
            // Unsubscribe when hidden.
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
            }
        }

        // ─── Refresh Logic ─────────────────────────────────────────────

        /// <summary>
        /// Rebuild the entire inventory display.
        /// Called whenever the inventory changes.
        /// </summary>
        private void RefreshUI()
        {
            if (slotPrefab == null || slotContainer == null) return;

            // Clear existing slot UIs.
            foreach (var slotUI in _slotUIs)
            {
                if (slotUI != null)
                    Destroy(slotUI.gameObject);
            }
            _slotUIs.Clear();

            // Create a slot UI for each inventory slot.
            List<InventorySlot> slots = InventoryManager.Instance.Slots;
            int maxSlots = InventoryManager.Instance.MaxSlots;

            for (int i = 0; i < maxSlots; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotContainer);
                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();

                if (slotUI != null)
                {
                    if (i < slots.Count && !slots[i].IsEmpty)
                    {
                        slotUI.SetSlot(slots[i].Item, slots[i].Amount);
                    }
                    else
                    {
                        slotUI.ClearSlot();
                    }

                    _slotUIs.Add(slotUI);
                }
            }
        }
    }
}
