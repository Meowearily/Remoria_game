# Remaria Roguelike - Project Overview

Remaria Roguelike is a Unity-based 3D action roguelike featuring procedural level generation, rigidbody-based player movement, and an event-driven combat system.

## 🏗 Architecture & Core Systems

The project follows a modular architecture organized under the `Remoria` namespace.

### Core (`Remoria.Core`)
- **`GameManager`**: A singleton that manages the global `GameState` (Playing, Paused, Dialogue, GameOver). It controls time scale and cursor visibility.
- **`Health`**: An event-driven health system. It implements `IDamageable` and fires `OnHealthChanged` and `OnDied` events.
- **`Singleton<T>`**: A base class for persistent singleton systems.

### World & Generation (`Remoria.World`)
- **`LevelGenerator`**: The core of the procedural generation. It snaps room prefabs together using `RoomConnector` points.
- **`RoomTemplate`**: Defines the layout and metadata for individual rooms.
- **`RoomDatabase`**: A ScriptableObject containing all available room templates for the generator.

### Player (`Remoria.Player`)
- **`PlayerMovement`**: Rigidbody-based movement with dash mechanics. It accounts for camera orientation.
- **`PlayerCombat`**: Handles melee (sphere cast), ranged (projectiles), and a lock-on targeting system.
- **`PlayerInteraction`**: Manages raycast-based interactions with the environment.

### Combat (`Remoria.Combat`)
- **`Projectile`**: A physics-based projectile system that interacts with `IDamageable` entities.

---

## 🛠 Building and Running

### Prerequisites
- **Unity 2022.3+** (Recommended)
- **Visual Studio / VS Code** for C# scripting.

### Setup
1. Open the project folder in Unity Hub.
2. Ensure all packages are resolved via `Packages/manifest.json`.
3. Open `Assets/Scenes/SampleScene.unity`.
4. Press **Play** in the Unity Editor.

### Controls
- **WASD / Arrows**: Move
- **Space**: Dash
- **Left Mouse**: Melee Attack
- **Right Mouse**: Ranged Attack
- **Middle Mouse**: Toggle Lock-On
- **E**: Interact
- **Esc**: Pause / Menu

---

## 📝 Development Conventions

### Namespaces
Always wrap scripts in the `Remoria.<Module>` namespace (e.g., `Remoria.Core`, `Remoria.Player`).

### Coding Style
- **Inspector Usability**: Use `[SerializeField]`, `[Header]`, and `[Tooltip]` for all public/serialized private fields.
- **Documentation**: Use XML documentation tags (`/// <summary>`) for classes and public methods.
- **Private Fields**: Prefix private runtime state with an underscore (e.g., `_currentHealth`).
- **State Management**: Always check `GameManager.Instance.IsPlaying` before processing input or logic that should be paused.
- **Events**: Prefer C# Actions/Events for decoupled communication (e.g., `Health.OnHealthChanged`).

### Component Pattern
- Use `RequireComponent` when a script depends on another component (like `Rigidbody` or `Animator`).
- Cache component references in `Awake` or `Start`.
- Use `Animator.StringToHash` for performance when setting animation parameters.

---

## 📂 Directory Structure
- `Assets/Scripts/Core`: Fundamental systems and interfaces.
- `Assets/Scripts/World`: Procedural generation and level logic.
- `Assets/Scripts/Player`: Player-specific mechanics.
- `Assets/Scripts/Combat`: Projectiles and common combat logic.
- `Assets/Prefabs`: Reusable GameObject templates (Player, Enemies, Rooms).
- `Assets/ScriptableObjects`: Data-driven assets (Room Databases, Items).
