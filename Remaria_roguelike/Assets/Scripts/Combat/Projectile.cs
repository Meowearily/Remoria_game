using UnityEngine;
using Remoria.Core;

namespace Remoria.Combat
{
    /// <summary>
    /// A projectile that flies forward, damages the first IDamageable it hits,
    /// and destroys itself.
    /// 
    /// This is instantiated by PlayerCombat (ranged attack) or could be used
    /// by enemies too (just call Initialize with different parameters).
    /// 
    /// The projectile uses Rigidbody for movement so it interacts with physics.
    /// 
    /// Setup for the Projectile PREFAB:
    ///   1. Create a Sphere (or any shape) in Unity.
    ///   2. Add a Rigidbody (uncheck "Use Gravity").
    ///   3. Add a SphereCollider, check "Is Trigger".
    ///   4. Add this script.
    ///   5. Drag it to the Prefabs folder to make it a prefab.
    ///   6. Delete it from the scene (it will be spawned by code).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        // ─── Inspector Fields ──────────────────────────────────────────
        [Header("Projectile Settings")]
        [Tooltip("How long before the projectile auto-destroys (seconds)")]
        [SerializeField] private float lifetime = 5f;

        // ─── Runtime State ─────────────────────────────────────────────
        private float _damage;
        private float _speed;
        private Vector3 _direction;
        private Rigidbody _rb;
        private bool _hasHit = false;

        // ─── Initialization ────────────────────────────────────────────

        /// <summary>
        /// Called by the script that fires the projectile.
        /// Sets damage, speed, and direction.
        /// </summary>
        public void Initialize(float damage, float speed, Vector3 direction)
        {
            _damage = damage;
            _speed = speed;
            _direction = direction.normalized;
        }

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;   // Projectiles fly straight.
            _rb.isKinematic = false;

            // Auto-destroy after lifetime expires (cleanup, prevents memory leaks).
            Destroy(gameObject, lifetime);
        }

        private void FixedUpdate()
        {
            if (_hasHit) return;

            // Move the projectile forward.
            _rb.velocity = _direction * _speed;
        }

        /// <summary>
        /// Called when the projectile's trigger collider touches another collider.
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (_hasHit) return;

            // Don't hit the player (the one who fired it).
            if (other.CompareTag("Player")) return;

            // Try to damage what we hit.
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null && !damageable.IsDead)
            {
                damageable.TakeDamage(_damage);
                Debug.Log($"[Projectile] Hit {other.gameObject.name} for {_damage} damage!");
            }

            // Destroy the projectile on any hit (even non-damageable walls).
            _hasHit = true;
            Destroy(gameObject);
        }
    }
}
