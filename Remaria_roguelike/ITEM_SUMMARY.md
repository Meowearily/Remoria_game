# Item Spawning System Summary

## Overview
The item spawning system has been fully integrated into the procedural level generation of Remaria Roguelike. Items are now distributed across rooms based on complexity and a defined loot budget.

## Key Components

### 1. Weighted Spawning (`ItemDatabase.cs`)
- Improved the `ItemDatabase` to support weighted random selection. 
- Higher weight values increase the probability of an item appearing, allowing for rarity control (e.g., common potions vs. rare equipment).

### 2. Universal Pickup Logic (`ItemPickup.cs`)
- Instead of multiple scripts, we use a single, universal `ItemPickup.cs`.
- **Automatic Detection**: The script checks the `ItemType` of the assigned `ItemData`.
- **Immediate Consumption**: If the item is a `Consumable` (like a Healing Potion), it applies its effects (e.g., `healAmount`) immediately upon interaction.
- **Inventory Support**: If the item is a weapon, armor, or misc item, it is added to the player's inventory.
- **Visuals**: Includes a built-in bobbing and rotating animation for all items.

### 3. Procedural Distribution (`HybridLevelGenerator.cs`)
- Added `SpawnItemsInRoom` method to the level generator.
- **Budget-Based**: Each room calculates an item budget based on the floor index and room number.
- **Random Placement**: Items spawn on random floor tiles within a room, excluding the center (usually reserved for player/exit).
- **Collision Safety**: Items are spawned slightly above the floor (`yOffset: 0.5f`) and are configured to not block movement for the player or enemies.

## How to Add New Items
1. Create a 3D model/prefab for the item.
2. Add a `Collider` set to **Is Trigger**.
3. Attach the universal `ItemPickup` script.
4. Create an `ItemData` asset:
   - Set `Item Type` to `Consumable` (for immediate use) or other for inventory.
   - Set `Heal Amount` (if it's a potion).
5. Create an `ItemEntry` in the `ItemDatabase` ScriptableObject and assign the prefab and weight.

## Configuration (LevelSettings)
- `lootChance`: Probability (0-1) that a room will contain items.
- `baseItemBudget`: Minimum loot "value" per room.
- `itemBudgetMultiplierPerRoom`: Increases loot quantity/quality in later rooms.
