# Door & Room Visibility System Summary

## Overview
The door system now serves a dual purpose: acting as a physical barrier and managing the **"Fog of War"** (Room Visibility). Contents of a room (enemies and items) remain hidden and inactive until the player interacts with the door leading into that room.

## Key Components

### 1. Door Script (`Door.cs`) - UPDATED
- **Fog of War Integration**: Added a reference to a `RoomVisibility` component.
- **Reveal Logic**: When the `Open()` method is triggered (via interaction), it calls `targetRoom.Reveal()` before starting its opening animation.
- **Automatic Linking**: During level generation, each door is automatically assigned the `RoomVisibility` manager of the room it leads into.

### 2. Room Visibility Manager (`RoomVisibility.cs`) - NEW
- **Content Tracking**: Maintains a list of all GameObjects (enemies, items) belonging to a specific room.
- **State Management**:
    - **Hidden**: Objects are set to `SetActive(false)` immediately upon spawning.
    - **Revealed**: When triggered by a door, all tracked objects are set to `SetActive(true)`.
- **Performance**: Hiding objects also disables their AI and physics processing, improving performance in large levels.

### 3. Procedural Distribution (`HybridLevelGenerator.cs`) - UPDATED
- **Visibility Initialization**: Now creates a `RoomVisibility` object for every generated room during Phase 1.
- **Smart Registration**: The `SpawnAtTile` method now checks if a spawned object is an "Enemy" or "Item" and automatically registers it with the correct room manager.
- **Start Room**: The generator automatically reveals the first room (player spawn) so the starting area is always visible.

## How to Set Up
1. **No Manual Setup Required**: The system is fully automated through the `HybridLevelGenerator`.
2. **Door Prefab**: Ensure your Door prefab has the `Door.cs` script. The generator will handle the `Target Room` assignment at runtime.
3. **Identification**: The system identifies what to hide based on the object's name (starts with "Enemy" or "Item").

## Current Behavior
- **Walls & Floors**: Remain visible at all times so the player can see the layout of the dungeon.
- **Enemies & Items**: Are completely invisible and non-functional until the door to their room is opened.
- **Interaction**: Pressing **E** on a door opens it and "wakes up" the room.

## Future Expansion Ideas
- **Dynamic Fog**: Adding a black overlay or particle "shroud" over unvisited rooms that disappears when the door opens.
- **Re-locking**: Doors that close and hide content again if the player leaves (optional).
- **Audio**: Playing a "discovery" sting or sound effect when a room is revealed.
