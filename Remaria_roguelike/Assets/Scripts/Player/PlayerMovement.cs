using UnityEngine;

namespace Remoria.Player
{
    /// <summary>
    /// Rigidbody-based player movement with smooth deceleration and dash.
    /// 
    /// Controls:
    ///   WASD / Arrow Keys → Move
    ///   Space → Dash forward (quick burst of speed with cooldown)
    /// 
    /// How it works:
    ///   - Uses GetAxis (smoothed) for gradual acceleration/deceleration
    ///   - When the player releases keys, velocity decays smoothly (no sudden stop)
    ///   - Dash adds a burst of speed in the facing direction
    ///   - Camera-relative movement: "forward" follows the camera angle
    /// 
    /// Required components:
    ///   - Rigidbody (Freeze Rotation X, Y, Z!)
    ///   - Collider (CapsuleCollider recommended)
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        // ─── Inspector Fields ──────────────────────────────────────────

        [Header("Movement")]
        [Tooltip("How fast the player moves (units per second)")]
        [SerializeField] private float moveSpeed = 6f;

        [Tooltip("How quickly the player turns to face movement direction")]
        [SerializeField] private float rotationSpeed = 10f;

        [Tooltip("How quickly the player decelerates when no input is given (higher = faster stop)")]
        [Range(1f, 20f)]
        [SerializeField] private float deceleration = 6f;

        [Header("Dash")]
        [Tooltip("How far/fast the dash moves the player")]
        [SerializeField] private float dashForce = 15f;

        [Tooltip("How long the dash lasts (seconds)")]
        [SerializeField] private float dashDuration = 0.15f;

        [Tooltip("Cooldown between dashes (seconds)")]
        [SerializeField] private float dashCooldown = 1f;

        [Header("Ground Check")]
        [Tooltip("How far down to check for ground")]
        [SerializeField] private float groundCheckDistance = 1.1f;

        [Tooltip("Which layers count as 'ground'")]
        [SerializeField] private LayerMask groundLayer;

        [Header("Boundaries")]
        [Tooltip("Enable to prevent the player from walking off the ground edge")]
        [SerializeField] private bool useBoundaries = false;

        [Tooltip("Half-size of the play area")]
        [SerializeField] private float boundaryLimit = 1000f;

        // ─── Private References ────────────────────────────────────────
        private Rigidbody _rb;
        private Transform _cameraTransform;
        private Animator _animator;  // Drives animation states.
        private bool _isGrounded;
        private float _logTimer = 0f; // Timer for position logging

        // Animator parameter name hashes (cached for performance).
        private static readonly int AnimSpeed = Animator.StringToHash("Speed");
        private static readonly int AnimDash = Animator.StringToHash("Dash");

        // Dash state
        private bool _isDashing = false;
        private float _dashTimeRemaining = 0f;
        private float _lastDashTime = -999f;
        private Vector3 _dashDirection;

        // ─── Public Properties ─────────────────────────────────────────
        /// <summary>Whether the player is currently giving movement input.</summary>
        public bool IsMoving { get; private set; }

        /// <summary>Can be set to false to disable movement (e.g., during dialogue).</summary>
        public bool CanMove { get; set; } = true;

        /// <summary>Whether the player is currently dashing.</summary>
        public bool IsDashing => _isDashing;

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;

            // Search children for Animator (the model is usually a child object).
            _animator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            if (UnityEngine.Camera.main != null)
            {
                _cameraTransform = UnityEngine.Camera.main.transform;
            }

            // Force boundaries off for procedural levels to avoid Inspector overrides
            useBoundaries = false;
        }

        private void Update()
        {
            CheckGround();

            if (!CanMove || !Core.GameManager.Instance.IsPlaying) return;

            // Space = Dash (replaces jump).
            if (Input.GetKeyDown(KeyCode.Space) && !_isDashing)
            {
                TryDash();
            }
        }

        private void FixedUpdate()
        {
            if (!CanMove || !Core.GameManager.Instance.IsPlaying) return;

            if (_isDashing)
            {
                PerformDash();
            }
            else
            {
                Move();
            }

            ClampToBoundaries();

            // Coordinate logging
            _logTimer += Time.fixedDeltaTime;
            if (_logTimer >= 2f)
            {
                _logTimer = 0f;
                Debug.Log($"[PlayerLocation] X: {transform.position.x:F2}, Y: {transform.position.y:F2}, Z: {transform.position.z:F2}");
            }

            // Drive the Animator's Speed parameter.
            // Uses horizontal velocity magnitude (ignoring Y) so jumping/gravity doesn't affect it.
            if (_animator != null)
            {
                Vector3 horizontalVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
                _animator.SetFloat(AnimSpeed, horizontalVel.magnitude);
            }
        }

        // ─── Movement Logic ────────────────────────────────────────────

        private void Move()
        {
            // Use GetAxis (NOT GetAxisRaw) for smoothed input.
            // GetAxis gradually ramps from 0→1 and 1→0, giving smooth acceleration/deceleration.
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical);

            if (inputDirection.magnitude < 0.05f)
            {
                // No input: smoothly decelerate horizontal velocity instead of stopping instantly.
                IsMoving = false;
                Vector3 currentVel = _rb.velocity;
                Vector3 dampedVel = new Vector3(
                    Mathf.Lerp(currentVel.x, 0f, deceleration * Time.fixedDeltaTime),
                    currentVel.y,
                    Mathf.Lerp(currentVel.z, 0f, deceleration * Time.fixedDeltaTime)
                );
                _rb.velocity = dampedVel;
                return;
            }

            IsMoving = true;

            // Clamp input magnitude to 1 (prevent diagonal speed boost).
            if (inputDirection.magnitude > 1f)
                inputDirection.Normalize();

            Vector3 moveDirection = CalculateCameraRelativeDirection(inputDirection);

            _rb.velocity = new Vector3(
                moveDirection.x * moveSpeed,
                _rb.velocity.y,
                moveDirection.z * moveSpeed
            );

            // Smoothly rotate the player to face movement direction.
            RotateTowards(moveDirection);
        }

        /// <summary>
        /// Converts raw input to camera-relative direction.
        /// </summary>
        private Vector3 CalculateCameraRelativeDirection(Vector3 inputDirection)
        {
            if (_cameraTransform == null)
                return inputDirection.normalized;

            Vector3 cameraForward = _cameraTransform.forward;
            Vector3 cameraRight = _cameraTransform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 worldDirection = cameraForward * inputDirection.z + cameraRight * inputDirection.x;
            return worldDirection.normalized;
        }

        /// <summary>
        /// Smoothly rotates the player to face the given direction.
        /// </summary>
        private void RotateTowards(Vector3 direction)
        {
            if (direction == Vector3.zero) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }

        // ─── Dash ──────────────────────────────────────────────────────

        private void TryDash()
        {
            if (Time.time - _lastDashTime < dashCooldown) return;

            _lastDashTime = Time.time;
            _isDashing = true;
            _dashTimeRemaining = dashDuration;

            // Dash in the direction the player is facing.
            // If the player is moving, dash in the movement direction instead.
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 input = new Vector3(h, 0f, v);

            if (input.magnitude > 0.1f)
            {
                _dashDirection = CalculateCameraRelativeDirection(input);
            }
            else
            {
                _dashDirection = transform.forward;
            }

            // Trigger dash animation.
            if (_animator != null)
            {
                _animator.SetTrigger(AnimDash);
            }

            Debug.Log("[PlayerMovement] Dash!");
        }

        private void PerformDash()
        {
            _dashTimeRemaining -= Time.fixedDeltaTime;

            if (_dashTimeRemaining <= 0f)
            {
                _isDashing = false;
                return;
            }

            // Apply dash velocity (keep Y for gravity).
            _rb.velocity = new Vector3(
                _dashDirection.x * dashForce,
                _rb.velocity.y,
                _dashDirection.z * dashForce
            );
        }

        // ─── Ground Check ──────────────────────────────────────────────

        private void CheckGround()
        {
            _isGrounded = Physics.Raycast(
                transform.position,
                Vector3.down,
                groundCheckDistance,
                groundLayer
            );
        }

        // ─── Boundary Clamping ─────────────────────────────────────────

        private void ClampToBoundaries()
        {
            if (!useBoundaries) return;

            Vector3 pos = transform.position;
            bool clamped = false;

            if (pos.x > boundaryLimit) { pos.x = boundaryLimit; clamped = true; }
            if (pos.x < -boundaryLimit) { pos.x = -boundaryLimit; clamped = true; }
            if (pos.z > boundaryLimit) { pos.z = boundaryLimit; clamped = true; }
            if (pos.z < -boundaryLimit) { pos.z = -boundaryLimit; clamped = true; }

            if (pos.y < -5f)
            {
                pos = new Vector3(0f, 2f, 0f);
                _rb.velocity = Vector3.zero;
                clamped = true;
            }

            if (clamped)
            {
                transform.position = pos;
                _rb.velocity = new Vector3(
                    Mathf.Abs(pos.x) >= boundaryLimit ? 0f : _rb.velocity.x,
                    _rb.velocity.y,
                    Mathf.Abs(pos.z) >= boundaryLimit ? 0f : _rb.velocity.z
                );
            }
        }
    }
}
