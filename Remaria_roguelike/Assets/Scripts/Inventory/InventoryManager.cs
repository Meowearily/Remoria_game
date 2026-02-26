using System.Collections.Generic;
using UnityEngine;
using Remoria.Core;

namespace Remoria.Inventory
{
    /// <summary>
    /// Manages the player's inventory — a list of item slots.
    /// 
    /// Access from anywhere: InventoryManager.Instance.AddItem(someItemData)
    /// 
    /// Features:
    ///   - Add/remove items
    ///   - Stacking (multiple of the same item in one slot)
    ///   - Event when inventory changes (UI subscribes to this)
    ///   - Check if an item exists
    /// 
    /// How slots work:
    ///   Each slot holds ONE type of item and a count.
    ///   Example: Slot 1 = "Health Potion" x3, Slot 2 = "Iron Sword" x1
    ///   If adding an item that already exists and isn't at max stack,
    ///   the count increases instead of using a new slot.
    /// </summary>
    public class InventoryManager : Singleton<InventoryManager>
    {
        // ─── Inspector Fields ──────────────────────────────────────────
        [Header("Inventory Settings")]
        [Tooltip("Maximum number of inventory slots")]
        [SerializeField] private int maxSlots = 20;

        // ─── Data ──────────────────────────────────────────────────────
        // The inventory is a list of InventorySlot structs.
        private List<InventorySlot> _slots = new List<InventorySlot>();

        // ─── Events ────────────────────────────────────────────────────
        /// <summary>Fired whenever the inventory changes. UI subscribes to this.</summary>
        public event System.Action OnInventoryChanged;

        // ─── Public Properties ─────────────────────────────────────────
        public List<InventorySlot> Slots => _slots;
        public int MaxSlots => maxSlots;

        // ─── Public Methods ────────────────────────────────────────────

        /// <summary>
        /// Try to add an item to the inventory.
        /// Returns true if successful, false if inventory is full.
        /// </summary>
        public bool AddItem(ItemData item, int amount = 1)
        {
            if (item == null) return false;

            // First, try to stack with an existing slot that has the same item.
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].Item == item && _slots[i].Amount < item.maxStack)
                {
                    // Calculate how many we can add to this slot.
                    int spaceInSlot = item.maxStack - _slots[i].Amount;
                    int toAdd = Mathf.Min(amount, spaceInSlot);

                    InventorySlot updatedSlot = _slots[i];
                    updatedSlot.Amount += toAdd;
                    _slots[i] = updatedSlot;

                    amount -= toAdd;

                    if (amount <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        Debug.Log($"[Inventory] Added {item.itemName} (stacked). Total: {updatedSlot.Amount}");
                        return true;
                    }
                }
            }

            // If there's remaining amount, create new slots.
            while (amount > 0 && _slots.Count < maxSlots)
            {
                int toAdd = Mathf.Min(amount, item.maxStack);
                _slots.Add(new InventorySlot(item, toAdd));
                amount -= toAdd;
            }

            OnInventoryChanged?.Invoke();

            if (amount > 0)
            {
                Debug.LogWarning($"[Inventory] Inventory full! Could not add {amount}x {item.itemName}.");
                return false;
            }

            Debug.Log($"[Inventory] Added {item.itemName}.");
            return true;
        }

        /// <summary>
        /// Remove an item from the inventory.
        /// Returns true if the item was found and removed.
        /// </summary>
        public bool RemoveItem(ItemData item, int amount = 1)
        {
            if (item == null) return false;

            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                if (_slots[i].Item == item)
                {
                    if (_slots[i].Amount > amount)
                    {
                        InventorySlot updatedSlot = _slots[i];
                        updatedSlot.Amount -= amount;
                        _slots[i] = updatedSlot;
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                    else if (_slots[i].Amount == amount)
                    {
                        _slots.RemoveAt(i);
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                    else
                    {
                        // This slot doesn't have enough — take what we can and continue.
                        amount -= _slots[i].Amount;
                        _slots.RemoveAt(i);
                    }
                }
            }

            OnInventoryChanged?.Invoke();
            return amount <= 0;
        }

        /// <summary>
        /// Check if the inventory contains at least 'amount' of the given item.
        /// </summary>
        public bool HasItem(ItemData item, int amount = 1)
        {
            int total = 0;
            foreach (var slot in _slots)
            {
                if (slot.Item == item)
                {
                    total += slot.Amount;
                    if (total >= amount) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the total count of a specific item across all slots.
        /// </summary>
        public int GetItemCount(ItemData item)
        {
            int total = 0;
            foreach (var slot in _slots)
            {
                if (slot.Item == item)
                    total += slot.Amount;
            }
            return total;
        }

        /// <summary>
        /// Clear the entire inventory.
        /// </summary>
        public void ClearInventory()
        {
            _slots.Clear();
            OnInventoryChanged?.Invoke();
        }
    }

    /// <summary>
    /// Represents one slot in the inventory.
    /// A struct (not a class) because it's a simple data container.
    /// </summary>
    [System.Serializable]
    public struct InventorySlot
    {
        public ItemData Item;
        public int Amount;

        public InventorySlot(ItemData item, int amount)
        {
            Item = item;
            Amount = amount;
        }

        /// <summary>Is this slot empty?</summary>
        public bool IsEmpty => Item == null || Amount <= 0;
    }
}
