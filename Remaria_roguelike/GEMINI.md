# Remaria Roguelike: Project Documentation & Guidelines

This project is a 3D Action Roguelike developed in Unity, featuring procedural generation, a state-driven AI system, and modular core architecture.

## 🏗 Project Architecture

The codebase is organized into modules under the `Remoria` namespace:

- **`Remoria.Core`**: Fundamental systems like `GameManager` (state control), `LevelManager` (progression), and the universal `Health` system.
- **`Remoria.World`**: Procedural generation logic. Uses a hybrid approach combining random room placement and A* pathfinding for corridors (`HybridLevelGenerator`).
- **`Remoria.Player`**: Physical movement (`PlayerMovement`), combat mechanics (`PlayerCombat` with Lock-On), and interaction systems.
- **`Remoria.Enemy`**: AI behavior driven by a Finite State Machine (FSM) in `EnemyAI`.
- **`Remoria.Combat`**: Projectile logic and damage data structures.
- **`Remoria.Dialogue` / `Remoria.NPC`**: ScriptableObject-driven dialogue system and NPC controllers.
- **`Remoria.UI`**: Management of HUD, menus, and interaction prompts.

## 🛠 Building and Running

1.  **Unity Version**: Open the project using Unity Editor (compatible with 2022.3 LTS).
2.  **Scene**: The main entry point is `Assets/Scenes/SampleScene.unity`.
3.  **Build**: Use `File -> Build Settings`. Target platform is Standalone (PC/Mac/Linux).
4.  **Testing**: Play directly in the Editor. Ensure `GameManager` and `LevelManager` are present in the scene or spawned.

## 📜 Development Conventions

### Coding Style
- **Namespace**: All scripts should reside within the `Remoria` namespace or its sub-namespaces (e.g., `Remoria.Core`).
- **Naming**:
  - `PascalCase` for classes, methods, and public properties.
  - `_camelCase` for private fields (e.g., `private Rigidbody _rb;`).
  - Use `[SerializeField]` for private fields that need to be exposed to the Inspector.
- **Singletons**: Use the `Singleton<T>` base class for global managers. Access via `ManagerName.Instance`.

### Practices
- **Logging**: Use prefixed debug logs for better traceability (e.g., `Debug.Log("[GameManager] State changed to Playing");`).
- **Interfaces**: 
  - Use `IDamageable` for any object that can take damage.
  - Use `IInteractable` for world objects the player can interact with.
- **Data-Driven Design**: Prefer `ScriptableObjects` for configuration (enemies, level settings, dialogues).
- **Physics**: Player and Enemy movement is `Rigidbody`-based. Avoid direct `transform.Translate` for moving entities.

## 📂 Key Files & Directories

- `Assets/Scripts/`: Main source code.
- `Assets/Prefabs/`: Game entities (Player, Enemies, UI).
- `Assets/ScriptableObjects/`: Game data and configurations.
- `PROJECT_OVERVIEW_DETAILED.md`: Deep dive into system mechanics.
- `PROJECT_FILE_EXPLANATION.md`: Map of all key files in the project.

## 🎮 Controls (Default)
- **WASD / Arrows**: Move.
- **Space**: Dash.
- **Left Click**: Melee Attack.
- **Right Click**: Ranged Attack.
- **Middle Click (Wheel)**: Lock-On Target.
- **E**: Interact.
- **Esc**: Pause / Menu.
