# Health System Summary - Remaria Roguelike

This document provides a detailed overview of how health, damage, and recovery systems work in the project, including recent changes to ensure player persistence and recovery.

## 1. Core Component: `Health.cs`
The `Health` component is a reusable script attached to the Player, Enemies, and any object that can be damaged.

### Key Features:
- **State Management**: Tracks `CurrentHealth` and `MaxHealth`.
- **Event-Driven**: Uses C# Events to notify other systems (like UI) when health changes or death occurs.
    - `OnHealthChanged(float current, float max)`: Fired on damage, healing, or resets.
    - `OnDied()`: Fired once when health reaches zero.
- **Methods**:
    - `TakeDamage(float amount)`: Reduces health, clamps at 0, and triggers death logic.
    - `Heal(float amount)`: Increases health, clamps at `MaxHealth`.
    - `ResetHealth()`: Instantly restores health to its current `MaxHealth`.
    - `SetMaxHealth(float newMax, bool healDifference)`: Updates the health ceiling. Used for upgrades.

## 2. Meta-Progression & Scaling: `StatApplier.cs`
The player's health is not static; it scales with meta-upgrades purchased in the Hub.

- **Upgrade Logic**: When a level starts, `StatApplier` reads the `healthUpgradeLevel` from `SaveManager`.
- **Formula**: `NewMaxHP = BaseMaxHP + (healthUpgradeLevel * hpPerLevel)`.
- **Application**: It calls `health.SetMaxHealth()` to update the player's total capacity.

## 3. Death Flow: `PlayerDeath.cs`
When the player's health reaches zero, the `PlayerDeath` script handles the transition.

1. **Detection**: Listens to the `OnDied` event from the `Health` component.
2. **State Change**: Sets `GameManager` state to `GameOver`.
3. **Currency**: Calls `CurrencyManager.Instance.FinalizeRun()` to save shards collected during the run.
4. **UI**: The `UIManager` reacts to the `GameOver` state by showing the Game Over screen.

## 4. Recovery & Persistence (Recent Changes)
To ensure the player can start a new run with full health after dying, specific reset logic has been added to the transition points.

### Recovery Points:
- **Return to Hub (`GameOverUI.cs`)**:
    - When the player clicks "Return to Hub" on the Game Over screen, the system finds the player object and calls `ResetHealth()`.
    - This ensures that upon returning to the Hub scene, the player is at 100% HP.
- **Starting a New Run (`LevelManager.cs`)**:
    - In `StartNewRun()`, the player's health is reset again.
    - This acts as a safety measure to ensure that even if the player somehow took damage in the Hub, they enter Floor 1 at full health.

## 5. Integration with UI
- **`HealthBarUI.cs`**: Subscribes to `OnHealthChanged` to update the slider/image fill amount. Because it uses events, it updates automatically whenever `ResetHealth()` or `TakeDamage()` is called.
- **`LevelTransitionUI.cs`**: Handles the black screen fade during scene changes, during which the health reset occurs "behind the scenes".

## 6. Technical Implementation Details
- **Namespace**: `Remoria.Core`
- **Interface**: Implements `IDamageable`, allowing any projectile or melee attack to interact with it via a common interface.
- **Persistence**: Because the Player object is often persistent across scenes (`DontDestroyOnLoad`), explicit calls to `ResetHealth()` are necessary to clear the "dead" state.
