using UnityEngine;
using Remoria.Core;

namespace Remoria.Enemy
{
    /// <summary>
    /// State-machine-based AI controller for enemies.
    /// 
    /// States:
    ///   Idle    → Standing still, waiting.
    ///   Patrol  → Walking between waypoints.
    ///   Chase   → Player detected, running toward them.
    ///   Attack  → In attack range, dealing damage on cooldown.
    ///   Die     → Dead, cleanup.
    /// 
    /// How a State Machine works:
    ///   The enemy is always in exactly ONE state.
    ///   Each frame, it runs the logic for that state.
    ///   If conditions change (player gets close), it transitions to a different state.
    ///   This keeps the code organized — no messy if-else chains.
    /// 
    /// Setup:
    ///   - Attach to an enemy GameObject (capsule with Rigidbody).
    ///   - Assign EnemyStats ScriptableObject in the Inspector.
    ///   - Optionally set up patrol waypoints (empty GameObjects).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyAI : MonoBehaviour
    {
        // ─── State Enum ────────────────────────────────────────────────
        public enum AIState
        {
            Idle,
            Patrol,
            Chase,
            Attack,
            Die
        }

        // ─── Inspector Fields ──────────────────────────────────────────
        [Header("Stats (drag an EnemyStats asset here)")]
        [SerializeField] private EnemyStats stats;

        [Header("Patrol Waypoints")]
        [Tooltip("Empty GameObjects marking the patrol path. Leave empty for idle-only enemies.")]
        [SerializeField] private Transform[] patrolWaypoints;

        [Tooltip("How close the enemy must get to a waypoint before moving to the next")]
        [SerializeField] private float waypointReachDistance = 0.5f;

        [Tooltip("Seconds to wait at each waypoint before moving on")]
        [SerializeField] private float waypointWaitTime = 2f;

        // ─── Runtime State ─────────────────────────────────────────────
        public AIState CurrentState { get; private set; } = AIState.Idle;

        private Rigidbody _rb;
        private Transform _player;
        private Health _health;
        private int _currentWaypointIndex = 0;
        private float _waypointWaitTimer = 0f;
        private float _lastAttackTime = -999f;
        private bool _isDead = false;

        // ─── Public Properties ─────────────────────────────────────────
        public EnemyStats Stats => stats;

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;
            _health = GetComponent<Health>();
        }

        private void Start()
        {
            // Find the player by tag.
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _player = playerObj.transform;
            }

            // Subscribe to death event.
            if (_health != null)
            {
                _health.OnDied += OnDeath;
            }

            // Start in patrol if waypoints exist, otherwise idle.
            CurrentState = (patrolWaypoints != null && patrolWaypoints.Length > 0)
                ? AIState.Patrol
                : AIState.Idle;
        }

        private void Update()
        {
            if (_isDead) return;
            if (!GameManager.Instance.IsPlaying) return;

            // Run the logic for the current state.
            switch (CurrentState)
            {
                case AIState.Idle:
                    UpdateIdle();
                    break;
                case AIState.Patrol:
                    UpdatePatrol();
                    break;
                case AIState.Chase:
                    UpdateChase();
                    break;
                case AIState.Attack:
                    UpdateAttack();
                    break;
            }
        }

        // ─── State Logic ───────────────────────────────────────────────

        private void UpdateIdle()
        {
            // Just stand still. Check if the player is within detection range.
            if (CanSeePlayer())
            {
                TransitionTo(AIState.Chase);
            }
        }

        private void UpdatePatrol()
        {
            // Check for player first — chasing takes priority over patrolling.
            if (CanSeePlayer())
            {
                TransitionTo(AIState.Chase);
                return;
            }

            // If we have no waypoints, go idle.
            if (patrolWaypoints == null || patrolWaypoints.Length == 0)
            {
                TransitionTo(AIState.Idle);
                return;
            }

            Transform targetWaypoint = patrolWaypoints[_currentWaypointIndex];
            float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint.position);

            if (distanceToWaypoint <= waypointReachDistance)
            {
                // Reached the waypoint — wait before moving to the next.
                _waypointWaitTimer += Time.deltaTime;
                _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f); // Stop moving.

                if (_waypointWaitTimer >= waypointWaitTime)
                {
                    // Move to the next waypoint (loop around using modulo %).
                    _waypointWaitTimer = 0f;
                    _currentWaypointIndex = (_currentWaypointIndex + 1) % patrolWaypoints.Length;
                }
            }
            else
            {
                // Move toward the waypoint.
                MoveToward(targetWaypoint.position, stats.patrolSpeed);
            }
        }

        private void UpdateChase()
        {
            if (_player == null)
            {
                TransitionTo(AIState.Idle);
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

            // If player is too far, stop chasing and return to patrol.
            if (distanceToPlayer > stats.chaseDesistRange)
            {
                TransitionTo(patrolWaypoints != null && patrolWaypoints.Length > 0
                    ? AIState.Patrol
                    : AIState.Idle);
                return;
            }

            // If close enough to attack, switch to attack state.
            if (distanceToPlayer <= stats.attackRange)
            {
                TransitionTo(AIState.Attack);
                return;
            }

            // Keep chasing the player.
            MoveToward(_player.position, stats.chaseSpeed);
        }

        private void UpdateAttack()
        {
            if (_player == null)
            {
                TransitionTo(AIState.Idle);
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

            // If the player moved out of attack range, chase them.
            if (distanceToPlayer > stats.attackRange * 1.2f)
            {
                TransitionTo(AIState.Chase);
                return;
            }

            // Face the player.
            LookAt(_player.position);

            // Stop moving while attacking.
            _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);

            // Attack on cooldown.
            if (Time.time - _lastAttackTime >= stats.attackCooldown)
            {
                PerformAttack();
            }
        }

        // ─── Actions ───────────────────────────────────────────────────

        private void PerformAttack()
        {
            _lastAttackTime = Time.time;

            // Check if the player is still in range and damageable.
            IDamageable playerDamageable = _player.GetComponent<IDamageable>();
            if (playerDamageable != null && !playerDamageable.IsDead)
            {
                playerDamageable.TakeDamage(stats.attackDamage);
                Debug.Log($"[EnemyAI] {stats.enemyName} attacked player for {stats.attackDamage} damage!");
            }
        }

        private void OnDeath()
        {
            _isDead = true;
            CurrentState = AIState.Die;
            _rb.velocity = Vector3.zero;

            Debug.Log($"[EnemyAI] {stats.enemyName} died.");

            // Disable the collider so the dead enemy isn't interactive.
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Destroy after a delay (so the death can be seen).
            Destroy(gameObject, 3f);
        }

        // ─── Utility Methods ───────────────────────────────────────────

        /// <summary>
        /// Move toward a target position at a given speed.
        /// </summary>
        private void MoveToward(Vector3 targetPosition, float speed)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0f; // Keep movement on the horizontal plane.

            _rb.velocity = new Vector3(
                direction.x * speed,
                _rb.velocity.y, // Preserve gravity
                direction.z * speed
            );

            LookAt(targetPosition);
        }

        /// <summary>
        /// Rotate to face a target position.
        /// </summary>
        private void LookAt(Vector3 targetPosition)
        {
            Vector3 lookDirection = targetPosition - transform.position;
            lookDirection.y = 0f; // Only rotate on the Y axis.

            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
            }
        }

        /// <summary>
        /// Check if the player is within detection range.
        /// </summary>
        private bool CanSeePlayer()
        {
            if (_player == null) return false;
            float distance = Vector3.Distance(transform.position, _player.position);
            return distance <= stats.detectionRange;
        }

        /// <summary>
        /// Transition to a new state.
        /// </summary>
        private void TransitionTo(AIState newState)
        {
            if (CurrentState == newState) return;
            Debug.Log($"[EnemyAI] {stats.enemyName}: {CurrentState} → {newState}");
            CurrentState = newState;
        }

        // ─── Gizmos ────────────────────────────────────────────────────
        // Visual debugging aids visible in Scene view.

        private void OnDrawGizmosSelected()
        {
            if (stats == null) return;

            // Detection range (green).
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, stats.detectionRange);

            // Attack range (red).
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stats.attackRange);

            // Chase desist range (yellow).
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stats.chaseDesistRange);
        }
    }
}
