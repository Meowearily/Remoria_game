using UnityEngine;

namespace Remoria.Enemy
{
    /// <summary>
    /// ScriptableObject that defines an enemy TYPE (like a blueprint).
    /// 
    /// What is a ScriptableObject?
    ///   It's a data container that lives as a file in your project.
    ///   You create them via right-click in the Project panel:
    ///   Create → RPG → Enemy Stats
    /// 
    /// Why use it?
    ///   - Create "Goblin", "Skeleton", "Dragon" as separate files
    ///   - Each file has different health, damage, speed values
    ///   - Drag the file onto an Enemy prefab to configure it
    ///   - Change values without editing any code
    ///   - Multiple enemies can share the same stats file
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemyStats", menuName = "RPG/Enemy Stats")]
    public class EnemyStats : ScriptableObject
    {
        [Header("Identity")]
        public string enemyName = "Enemy";

        [Header("Health")]
        [Tooltip("Maximum hit points")]
        public float maxHealth = 50f;

        [Header("Combat")]
        [Tooltip("Damage dealt per attack")]
        public float attackDamage = 10f;

        [Tooltip("Seconds between attacks")]
        public float attackCooldown = 1.5f;

        [Tooltip("How close the enemy must be to attack")]
        public float attackRange = 2f;

        [Header("Movement")]
        [Tooltip("Walking speed during patrol")]
        public float patrolSpeed = 2f;

        [Tooltip("Running speed when chasing the player")]
        public float chaseSpeed = 4f;

        [Header("Meta Progression")]
        [Tooltip("How much currency this enemy awards on death")]
        public int currencyValue = 10;

        [Header("Detection")]
        [Tooltip("How far the enemy can 'see' the player")]
        public float detectionRange = 10f;

        [Tooltip("How far the enemy will chase before giving up and returning")]
        public float chaseDesistRange = 15f;
    }
}
