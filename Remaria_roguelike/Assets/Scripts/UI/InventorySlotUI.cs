using UnityEngine;
using UnityEngine.UI;
using Remoria.Inventory;

namespace Remoria.UI
{
    /// <summary>
    /// One slot in the inventory grid. Shows an item icon and stack count.
    /// 
    /// This is a PREFAB — you create one, save it as a prefab, and InventoryUI
    /// spawns copies of it dynamically.
    /// 
    /// Setup:
    ///   1. Create a UI → Image (background, e.g., dark gray).
    ///   2. Set size to 80x80.
    ///   3. Add a child Image (for the item icon), stretch to fill.
    ///   4. Add a child Text (for the stack count), anchor bottom-right.
    ///   5. Attach this script to the root.
    ///   6. Drag references in the Inspector.
    ///   7. Save as prefab in Prefabs/UI/ folder.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
    {
        // ─── Inspector Fields ──────────────────────────────────────────
        [Header("References")]
        [Tooltip("Image displaying the item icon")]
        [SerializeField] private Image iconImage;

        [Tooltip("Text displaying the stack count")]
        [SerializeField] private TMPro.TextMeshProUGUI countText;

        [Tooltip("Background image for highlight effects")]
        [SerializeField] private Image backgroundImage;

        [Header("Colors")]
        [Tooltip("Normal background color")]
        [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        [Tooltip("Color when empty")]
        [SerializeField] private Color emptyColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);

        // ─── Runtime ───────────────────────────────────────────────────
        private ItemData _itemData;

        // ─── Public Methods ────────────────────────────────────────────

        /// <summary>
        /// Set this slot to display an item.
        /// </summary>
        public void SetSlot(ItemData item, int amount)
        {
            _itemData = item;

            if (iconImage != null)
            {
                iconImage.sprite = item.icon;
                iconImage.enabled = item.icon != null; // Hide if no icon.
                iconImage.color = Color.white;
            }

            if (countText != null)
            {
                // Only show count if stackable and more than 1.
                countText.text = amount > 1 ? amount.ToString() : "";
                countText.enabled = amount > 1;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = normalColor;
            }
        }

        /// <summary>
        /// Clear this slot (empty slot).
        /// </summary>
        public void ClearSlot()
        {
            _itemData = null;

            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            if (countText != null)
            {
                countText.text = "";
                countText.enabled = false;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = emptyColor;
            }
        }
    }
}
