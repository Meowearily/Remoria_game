using UnityEngine;
using Remoria.Core;
using Remoria.World;
using System.Collections.Generic;

namespace Remoria.Enemy
{
    /// <summary>
    /// Updated AI controller that can autonomously find patrol points within a room.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyAI : MonoBehaviour
    {
        public enum AIState { Idle, Patrol, Chase, Attack, Die }

        [Header("Stats")]
        [SerializeField] private EnemyStats stats;

        [Header("Patrol Settings")]
        [SerializeField] private float waypointReachDistance = 0.5f;
        [SerializeField] private float waypointWaitTime = 2f;
        [SerializeField] private int maxPatrolPoints = 3;

        // ─── Runtime State ─────────────────────────────────────────────
        public AIState CurrentState { get; private set; } = AIState.Idle;

        // ─── Public Properties ─────────────────────────────────────────
        public EnemyStats Stats => stats;

        private Rigidbody _rb;
        private Transform _player;
        private Health _health;
        private bool _isDead = false;
        
        private RoomData _homeRoom;
        private Vector3 _spawnPosition;
        private List<Vector3> _patrolPoints = new List<Vector3>();
        private int _currentPatrolIndex = 0;
        private float _waitTimer = 0f;
        private float _lastAttackTime = -999f;
        private const float TileSize = 3f;

        private Vector3 _wanderTarget;
        private bool _hasWanderTarget = false;

        private float _stuckTimer = 0f;
        private Vector3 _lastPosition;

        public void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;
            _health = GetComponent<Health>();
            _spawnPosition = transform.position;
            _lastPosition = transform.position;
        }

        public void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) _player = playerObj.transform;

            if (_health != null) _health.OnDied += OnDeath;

            _spawnPosition = transform.position;
            _lastPosition = transform.position;
            
            // Fix: Only set Idle if we haven't been assigned a Patrol state by the generator
            if (CurrentState == AIState.Idle && (_patrolPoints == null || _patrolPoints.Count == 0))
            {
                CurrentState = AIState.Idle;
            }
            else if (_patrolPoints != null && _patrolPoints.Count > 0)
            {
                TransitionTo(AIState.Patrol);
            }
            
            Debug.Log($"[EnemyAI] {gameObject.name} initialized at {_spawnPosition}. State: {CurrentState}");
        }

        /// <summary>
        /// Assigns the room this enemy belongs to and generates random patrol points.
        /// </summary>
        public void SetHomeRoom(RoomData room)
        {
            _homeRoom = room;
            _patrolPoints.Clear();

            if (_homeRoom != null && _homeRoom.Tiles.Count > 0)
            {
                // Pick random unique tiles from the room
                List<Vector2Int> tiles = new List<Vector2Int>(_homeRoom.Tiles);
                for (int i = 0; i < maxPatrolPoints && tiles.Count > 0; i++)
                {
                    int index = Random.Range(0, tiles.Count);
                    Vector2Int tile = tiles[index];
                    _patrolPoints.Add(new Vector3(tile.x * TileSize, transform.position.y, tile.y * TileSize));
                    tiles.RemoveAt(index);
                }
            }

            if (_patrolPoints.Count > 0)
            {
                TransitionTo(AIState.Patrol);
            }
        }

        private void Update()
        {
            if (_isDead || !GameManager.Instance.IsPlaying) return;

            // Stuck detection: if we are trying to move but position hasn't changed much
            CheckIfStuck();

            switch (CurrentState)
            {
                case AIState.Idle: UpdateIdle(); break;
                case AIState.Patrol: UpdatePatrol(); break;
                case AIState.Chase: UpdateChase(); break;
                case AIState.Attack: UpdateAttack(); break;
            }
        }

        private void CheckIfStuck()
        {
            if (CurrentState == AIState.Die || CurrentState == AIState.Attack) 
            {
                _stuckTimer = 0f;
                return;
            }

            // If we have velocity but aren't moving much
            float distMoved = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_lastPosition.x, _lastPosition.z));
            if (distMoved < 0.01f && _rb.velocity.magnitude > 0.1f)
            {
                _stuckTimer += Time.deltaTime;
            }
            else
            {
                _stuckTimer = 0f;
            }

            if (_stuckTimer > 1.5f)
            {
                _stuckTimer = 0f;
                HandleStuck();
            }

            _lastPosition = transform.position;
        }

        private void HandleStuck()
        {
            Debug.Log($"[EnemyAI] {gameObject.name} is stuck! Forcing state update.");
            if (CurrentState == AIState.Patrol && _patrolPoints.Count > 0)
            {
                _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Count;
            }
            else if (CurrentState == AIState.Idle)
            {
                _hasWanderTarget = false;
            }
            
            // Apply a small "nudge" to break collision
            _rb.AddForce(Random.onUnitSphere * 5f, ForceMode.Impulse);
        }

        private void UpdateIdle()
        {
            if (CanSeePlayer())
            {
                TransitionTo(AIState.Chase);
                return;
            }

            // Wander behavior even in Idle
            if (!_hasWanderTarget)
            {
                _waitTimer += Time.deltaTime;
                if (_waitTimer >= waypointWaitTime)
                {
                    _waitTimer = 0f;
                    // Pick a random point within 2 meters of the spawn position or current position
                    Vector2 randomCircle = Random.insideUnitCircle * 2f;
                    _wanderTarget = new Vector3(_spawnPosition.x + randomCircle.x, transform.position.y, _spawnPosition.z + randomCircle.y);
                    _hasWanderTarget = true;
                }
                
                // Keep looking around or stay still
                _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
            }
            else
            {
                float distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_wanderTarget.x, _wanderTarget.z));
                if (distance <= waypointReachDistance)
                {
                    _hasWanderTarget = false;
                    _waitTimer = 0f;
                }
                else
                {
                    MoveToward(_wanderTarget, stats.patrolSpeed * 0.5f);
                }
            }
        }

        private void UpdatePatrol()
        {
            if (CanSeePlayer())
            {
                TransitionTo(AIState.Chase);
                return;
            }

            if (_patrolPoints.Count == 0)
            {
                TransitionTo(AIState.Idle);
                return;
            }

            Vector3 target = _patrolPoints[_currentPatrolIndex];
            float distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(target.x, target.z));

            if (distance <= waypointReachDistance)
            {
                _waitTimer += Time.deltaTime;
                _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);

                if (_waitTimer >= waypointWaitTime)
                {
                    _waitTimer = 0f;
                    _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Count;
                }
            }
            else
            {
                MoveToward(target, stats.patrolSpeed);
            }
        }

        private void UpdateChase()
        {
            if (_player == null) { TransitionTo(AIState.Idle); return; }

            float distance = Vector3.Distance(transform.position, _player.position);

            if (distance > stats.chaseDesistRange)
            {
                TransitionTo(_patrolPoints.Count > 0 ? AIState.Patrol : AIState.Idle);
                return;
            }

            if (distance <= stats.attackRange)
            {
                TransitionTo(AIState.Attack);
                return;
            }

            MoveToward(_player.position, stats.chaseSpeed);
        }

        private void UpdateAttack()
        {
            if (_player == null) { TransitionTo(AIState.Idle); return; }

            float distance = Vector3.Distance(transform.position, _player.position);

            if (distance > stats.attackRange * 1.2f)
            {
                TransitionTo(AIState.Chase);
                return;
            }

            LookAt(_player.position);
            _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);

            if (Time.time - _lastAttackTime >= stats.attackCooldown)
            {
                PerformAttack();
            }
        }

        private void PerformAttack()
        {
            _lastAttackTime = Time.time;
            IDamageable playerDamageable = _player.GetComponent<IDamageable>();
            if (playerDamageable != null && !playerDamageable.IsDead)
            {
                playerDamageable.TakeDamage(stats.attackDamage);
            }
        }

        private void OnDeath()
        {
            _isDead = true;
            CurrentState = AIState.Die;
            _rb.velocity = Vector3.zero;
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            Destroy(gameObject, 3f);
        }

        private void MoveToward(Vector3 targetPosition, float speed)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0f;
            _rb.velocity = new Vector3(direction.x * speed, _rb.velocity.y, direction.z * speed);
            LookAt(targetPosition);
        }

        private void LookAt(Vector3 targetPosition)
        {
            Vector3 lookDirection = targetPosition - transform.position;
            lookDirection.y = 0f;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
            }
        }

        private bool CanSeePlayer()
        {
            if (_player == null) return false;
            return Vector3.Distance(transform.position, _player.position) <= stats.detectionRange;
        }

        private void TransitionTo(AIState newState)
        {
            if (CurrentState == newState) return;
            CurrentState = newState;
        }

        private void OnDrawGizmosSelected()
        {
            if (stats == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, stats.detectionRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stats.attackRange);
            
            if (_patrolPoints != null)
            {
                Gizmos.color = Color.blue;
                foreach (var p in _patrolPoints) Gizmos.DrawSphere(p, 0.2f);
            }
        }
    }
}
