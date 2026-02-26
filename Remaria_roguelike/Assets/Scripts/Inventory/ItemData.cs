using UnityEngine;

namespace Remoria.Inventory
{
    /// <summary>
    /// ScriptableObject defining an item type.
    /// 
    /// To create a new item:
    ///   1. Right-click in the Project panel.
    ///   2. Create → RPG → Item
    ///   3. Fill in the name, description, icon, type, etc.
    ///   4. Use it with ItemPickup (world object) and InventoryManager.
    /// 
    /// ScriptableObjects are DATA — they describe WHAT an item is.
    /// The actual behavior (pickup, equip, use) is handled by other scripts.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "RPG/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Display name shown in inventory and tooltips")]
        public string itemName = "New Item";

        [Tooltip("Description shown in tooltips")]
        [TextArea(2, 4)]
        public string description = "An item.";

        [Tooltip("Icon shown in the inventory grid. Can be null for now.")]
        public Sprite icon;

        [Header("Classification")]
        [Tooltip("What category this item belongs to")]
        public ItemType itemType = ItemType.Misc;

        [Header("Stacking")]
        [Tooltip("Maximum number of this item per inventory slot (1 = unstackable)")]
        public int maxStack = 1;

        [Header("Stats (for weapons/armor)")]
        [Tooltip("Damage bonus if this is a weapon")]
        public float damageBonus = 0f;

        [Tooltip("Defense bonus if this is armor")]
        public float defenseBonus = 0f;

        [Tooltip("Health restored if this is a consumable")]
        public float healAmount = 0f;
    }

    /// <summary>
    /// Item categories. Add more as you expand the game.
    /// </summary>
    public enum ItemType
    {
        Weapon,      // Swords, axes, bows
        Armor,       // Helmets, chestplates, shields
        Consumable,  // Potions, food
        Quest,       // Key items that can't be dropped
        Misc         // Crafting materials, junk
    }
}
