using UnityEngine;
using Remoria.Core;
using Remoria.Combat;

namespace Remoria.Player
{
    /// <summary>
    /// Handles melee, ranged attacks, and enemy lock-on targeting.
    /// 
    /// Controls:
    ///   Left Mouse Button   → Melee attack
    ///   Right Mouse Button  → Ranged attack (fires a projectile)
    ///   Middle Mouse Button  → Lock-on to enemy under cursor (click again to unlock)
    /// 
    /// Lock-on:
    ///   When locked on, the player automatically rotates to face the target enemy.
    ///   Attacks are directed toward the locked target.
    ///   A small indicator appears above the locked enemy.
    ///   Press middle mouse again (or target dies) to unlock.
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        // ─── Melee Settings ────────────────────────────────────────────
        [Header("Melee Attack")]
        [Tooltip("Damage dealt per melee hit")]
        [SerializeField] private float meleeDamage = 20f;

        [Tooltip("How far in front of the player the melee hitbox reaches")]
        [SerializeField] private float meleeRange = 2f;

        [Tooltip("Radius of the melee hit sphere")]
        [SerializeField] private float meleeRadius = 1f;

        [Tooltip("Seconds between melee attacks")]
        [SerializeField] private float meleeCooldown = 0.5f;

        [Tooltip("Which layers can be hit by melee (set to 'Enemy' layer)")]
        [SerializeField] private LayerMask meleeHitLayers;

        // ─── Ranged Settings ───────────────────────────────────────────
        [Header("Ranged Attack")]
        [Tooltip("The projectile prefab to instantiate")]
        [SerializeField] private GameObject projectilePrefab;

        [Tooltip("Where the projectile spawns")]
        [SerializeField] private Transform projectileSpawnPoint;

        [Tooltip("Damage dealt by each projectile")]
        [SerializeField] private float rangedDamage = 15f;

        [Tooltip("How fast the projectile flies")]
        [SerializeField] private float projectileSpeed = 20f;

        [Tooltip("Seconds between ranged attacks")]
        [SerializeField] private float rangedCooldown = 0.8f;

        // ─── Lock-On Settings ──────────────────────────────────────────
        [Header("Lock-On Targeting")]
        [Tooltip("Maximum distance to lock onto an enemy")]
        [SerializeField] private float lockOnMaxDistance = 50f;

        [Tooltip("Which layers can be locked onto (set to 'Enemy' or 'Default')")]
        [SerializeField] private LayerMask lockOnLayers;

        [Tooltip("How quickly the player rotates toward the locked target")]
        [SerializeField] private float lockOnRotationSpeed = 12f;

        // ─── Runtime State ─────────────────────────────────────────────
        private float _lastMeleeTime = -999f;
        private float _lastRangedTime = -999f;
        private Animator _animator;

        // Animator parameter hashes (cached for performance).
        private static readonly int AnimMeleeAttack = Animator.StringToHash("MeleeAttack");
        private static readonly int AnimRangedAttack = Animator.StringToHash("RangedAttack");

        // Lock-on
        private Transform _lockOnTarget;
        private GameObject _lockOnIndicator; // Visual marker above locked enemy.

        // ─── Public Properties ─────────────────────────────────────────
        /// <summary>The currently locked-on target (null if none).</summary>
        public Transform LockOnTarget => _lockOnTarget;

        /// <summary>Whether the player is locked onto a target.</summary>
        public bool HasLockOn => _lockOnTarget != null;

        /// <summary>Add bonus damage to melee and ranged attacks.</summary>
        public void AddBonusDamage(float meleeBonus, float rangedBonus)
        {
            meleeDamage += meleeBonus;
            rangedDamage += rangedBonus;
            Debug.Log($"[PlayerCombat] Bonus Damage applied! Melee: {meleeDamage}, Ranged: {rangedDamage}");
        }

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void Start()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            if (!GameManager.Instance.IsPlaying) return;

            // Left Mouse Button = Melee.
            if (Input.GetMouseButtonDown(0))
            {
                TryMeleeAttack();
            }

            // Right Mouse Button = Ranged.
            if (Input.GetMouseButtonDown(1))
            {
                TryRangedAttack();
            }

            // Middle Mouse Button = Lock-On Toggle.
            if (Input.GetMouseButtonDown(2))
            {
                ToggleLockOn();
            }

            // If locked on, rotate toward the target.
            if (_lockOnTarget != null)
            {
                // Check if target was destroyed.
                Health targetHealth = _lockOnTarget.GetComponent<Health>();
                if (targetHealth != null && targetHealth.IsDead)
                {
                    ClearLockOn();
                }
                else
                {
                    RotateTowardTarget();
                }
            }
        }

        // ─── Lock-On System ────────────────────────────────────────────

        private void ToggleLockOn()
        {
            if (_lockOnTarget != null)
            {
                // Already locked on — unlock.
                ClearLockOn();
                Debug.Log("[PlayerCombat] Lock-on cleared.");
                return;
            }

            // Raycast from the mouse position into the 3D world.
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, lockOnMaxDistance, lockOnLayers))
            {
                // Check if what we hit has a Health component (i.e., it's an enemy).
                Health health = hit.collider.GetComponent<Health>();
                if (health != null && !health.IsDead)
                {
                    _lockOnTarget = hit.collider.transform;
                    CreateLockOnIndicator();
                    Debug.Log($"[PlayerCombat] Locked onto {_lockOnTarget.name}!");
                }
            }
        }

        private void ClearLockOn()
        {
            _lockOnTarget = null;

            if (_lockOnIndicator != null)
            {
                Destroy(_lockOnIndicator);
                _lockOnIndicator = null;
            }
        }

        /// <summary>
        /// Smoothly rotates the player to face the locked target.
        /// </summary>
        private void RotateTowardTarget()
        {
            Vector3 direction = _lockOnTarget.position - transform.position;
            direction.y = 0f; // Keep rotation horizontal.

            if (direction.sqrMagnitude < 0.01f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                lockOnRotationSpeed * Time.deltaTime
            );
        }

        /// <summary>
        /// Creates a simple diamond-shaped indicator above the locked enemy.
        /// </summary>
        private void CreateLockOnIndicator()
        {
            // Clean up old indicator.
            if (_lockOnIndicator != null)
                Destroy(_lockOnIndicator);

            // Create a small diamond shape above the enemy using a scaled cube.
            _lockOnIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _lockOnIndicator.name = "LockOnIndicator";
            _lockOnIndicator.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            // Remove collider so it doesn't interfere with gameplay.
            Collider col = _lockOnIndicator.GetComponent<Collider>();
            if (col != null) Destroy(col);

            // Make it bright yellow and rotate it 45 degrees to look like a diamond.
            Renderer rend = _lockOnIndicator.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = new Material(Shader.Find("Sprites/Default"));
                rend.material.color = Color.yellow;
            }

            _lockOnIndicator.transform.rotation = Quaternion.Euler(0f, 45f, 45f);

            // Parent to the enemy so it follows automatically.
            _lockOnIndicator.transform.SetParent(_lockOnTarget);
            _lockOnIndicator.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        }

        // ─── Melee Attack ──────────────────────────────────────────────

        private void TryMeleeAttack()
        {
            if (Time.time - _lastMeleeTime < meleeCooldown) return;

            _lastMeleeTime = Time.time;

            // Trigger melee animation.
            if (_animator != null)
            {
                _animator.SetTrigger(AnimMeleeAttack);
            }

            // Attack direction: toward locked target if locked on, otherwise forward.
            Vector3 attackDirection = transform.forward;
            if (_lockOnTarget != null)
            {
                Vector3 toTarget = _lockOnTarget.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0.01f)
                    attackDirection = toTarget.normalized;
            }

            Vector3 attackCenter = transform.position + attackDirection * meleeRange + Vector3.up;

            Collider[] hits = Physics.OverlapSphere(attackCenter, meleeRadius, meleeHitLayers);

            foreach (Collider hit in hits)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable != null && !damageable.IsDead)
                {
                    damageable.TakeDamage(meleeDamage);
                    Debug.Log($"[PlayerCombat] Melee hit {hit.gameObject.name} for {meleeDamage} damage!");
                }
            }

            Debug.Log($"[PlayerCombat] Melee attack! Hit {hits.Length} targets.");
        }

        // ─── Ranged Attack ─────────────────────────────────────────────

        private void TryRangedAttack()
        {
            if (Time.time - _lastRangedTime < rangedCooldown) return;

            if (projectilePrefab == null)
            {
                Debug.LogWarning("[PlayerCombat] No projectile prefab assigned!");
                return;
            }

            _lastRangedTime = Time.time;

            // Trigger ranged animation.
            if (_animator != null)
            {
                _animator.SetTrigger(AnimRangedAttack);
            }

            Vector3 spawnPos = projectileSpawnPoint != null
                ? projectileSpawnPoint.position
                : transform.position + transform.forward * 1.5f + Vector3.up;

            // Fire direction: toward locked target if locked on, otherwise forward.
            Vector3 fireDirection = transform.forward;
            if (_lockOnTarget != null)
            {
                Vector3 toTarget = _lockOnTarget.position - spawnPos;
                if (toTarget.sqrMagnitude > 0.01f)
                    fireDirection = toTarget.normalized;
            }

            Quaternion spawnRotation = Quaternion.LookRotation(fireDirection);
            GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, spawnRotation);

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(rangedDamage, projectileSpeed, fireDirection);
            }

            Debug.Log("[PlayerCombat] Ranged attack fired!");
        }

        // ─── Cleanup ───────────────────────────────────────────────────

        private void OnDestroy()
        {
            ClearLockOn();
        }

        // ─── Gizmos ────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 attackCenter = transform.position + transform.forward * meleeRange + Vector3.up;
            Gizmos.DrawWireSphere(attackCenter, meleeRadius);

            // Draw line to lock-on target.
            if (_lockOnTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position + Vector3.up, _lockOnTarget.position + Vector3.up);
            }
        }
    }
}
