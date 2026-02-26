using UnityEngine;

namespace Remoria.Interaction
{
    /// <summary>
    /// Interface for all objects the player can interact with.
    /// NPCs, item pickups, doors, chests — anything the player walks up to and presses E.
    /// 
    /// How to use:
    ///   1. Make your class implement this interface:
    ///      public class TreasureChest : MonoBehaviour, IInteractable { ... }
    ///   2. Implement InteractionPrompt (the text shown to the player, e.g., "Open Chest")
    ///   3. Implement Interact() (what happens when the player presses E)
    ///   4. Make sure the GameObject has a Collider set to "Is Trigger" = true
    ///      so the player's interaction detector can find it.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// The text shown on screen when the player is near this object.
        /// Examples: "Talk", "Pick Up", "Open", "Read"
        /// </summary>
        string InteractionPrompt { get; }

        /// <summary>
        /// Called when the player presses the interaction key (E).
        /// </summary>
        /// <param name="interactor">The GameObject that initiated the interaction (usually the Player).</param>
        void Interact(GameObject interactor);
    }
}
