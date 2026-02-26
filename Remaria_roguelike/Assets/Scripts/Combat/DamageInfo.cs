namespace Remoria.Combat
{
    /// <summary>
    /// Holds information about a damage event.
    /// 
    /// A "struct" is like a class but lighter-weight. It's stored directly
    /// in memory (not as a reference). Good for small data bundles like this.
    /// 
    /// Currently simple, but designed for expansion:
    ///   - Add DamageType.Fire, DamageType.Poison, etc.
    ///   - Add status effects (burning, frozen)
    ///   - Add critical hit info
    /// </summary>
    public struct DamageInfo
    {
        /// <summary>How much damage to deal.</summary>
        public float Amount;

        /// <summary>What caused the damage (for kill credit, combat log, etc.).</summary>
        public UnityEngine.GameObject Source;

        /// <summary>What kind of damage this is.</summary>
        public DamageType Type;

        /// <summary>
        /// Convenience constructor.
        /// </summary>
        public DamageInfo(float amount, UnityEngine.GameObject source, DamageType type)
        {
            Amount = amount;
            Source = source;
            Type = type;
        }
    }

    /// <summary>
    /// Types of damage. Expand this as you add new mechanics.
    /// </summary>
    public enum DamageType
    {
        Melee,      // Sword, axe, punch
        Ranged,     // Arrow, bullet, thrown
        Magic,      // Spells, elemental
        Fire,       // Burning (future expansion)
        Poison,     // Damage over time (future expansion)
        Fall        // Fall damage (future expansion)
    }
}
