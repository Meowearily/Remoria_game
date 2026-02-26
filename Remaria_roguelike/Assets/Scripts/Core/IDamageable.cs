using UnityEngine;

namespace Remoria.Core
{
    /// <summary>
    /// Interface for anything that can take damage: players, enemies, destructible objects.
    /// 
    /// What is an Interface?
    ///   An interface is a "contract" — it says "any class that implements me
    ///   MUST have these methods." It doesn't contain any actual code, just signatures.
    /// 
    /// Why use it?
    ///   The player's attack doesn't need to know if it hit an Enemy or a Barrel.
    ///   It just asks: "Do you implement IDamageable?" If yes → call TakeDamage().
    ///   This makes your code flexible — add new damageable objects without changing attack code.
    /// 
    /// Usage:
    ///   public class Enemy : MonoBehaviour, IDamageable { ... }
    ///   public class DestructibleBarrel : MonoBehaviour, IDamageable { ... }
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Deal damage to this object.
        /// </summary>
        /// <param name="amount">How much damage to deal.</param>
        void TakeDamage(float amount);

        /// <summary>
        /// Is this object dead (health <= 0)?
        /// </summary>
        bool IsDead { get; }

        /// <summary>
        /// The Transform of this damageable object (for position/distance checks).
        /// </summary>
        Transform transform { get; }
    }
}
