using UnityEngine;

namespace Remoria.Player
{
    /// <summary>
    /// Rigidbody-based player movement using the OLD Input Manager.
    /// 
    /// Controls:
    ///   WASD / Arrow Keys → Move
    ///   Space → Jump
    /// 
    /// How it works:
    ///   - Reads input axes ("Horizontal" = A/D, "Vertical" = W/S)
    ///   - Calculates a movement direction relative to the camera
    ///   - Applies velocity to the Rigidbody (physics-based)
    ///   - Rotates the player to face the movement direction smoothly
    ///   - Uses a raycast downward to check if the player is on the ground (for jumping)
    /// 
    /// Required components on the same GameObject:
    ///   - Rigidbody (with Freeze Rotation X, Y, Z checked!)
    ///   - Collider (CapsuleCollider recommended)
    /// 
    /// IMPORTANT: On the Rigidbody, you MUST freeze all rotation axes.
    ///   Otherwise the capsule will tip over like a bowling pin.
    ///   Inspector → Rigidbody → Constraints → Freeze Rotation: ✓X ✓Y ✓Z
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        // ─── Inspector Fields ──────────────────────────────────────────
        // These appear in Unity's Inspector panel so you can tweak them
        // without editing code. Hover over them in Unity to see tooltips.

        [Header("Movement")]
        [Tooltip("How fast the player moves (units per second)")]
        [SerializeField] private float moveSpeed = 6f;

        [Tooltip("How quickly the player turns to face movement direction")]
        [SerializeField] private float rotationSpeed = 10f;

        [Header("Jumping")]
        [Tooltip("How high the player jumps")]
        [SerializeField] private float jumpForce = 8f;

        [Tooltip("How far down to check for ground (should be slightly more than half the capsule height)")]
        [SerializeField] private float groundCheckDistance = 1.1f;

        [Tooltip("Which layers count as 'ground'. Set your floor to a 'Ground' layer.")]
        [SerializeField] private LayerMask groundLayer;

        [Header("Boundaries")]
        [Tooltip("Enable to prevent the player from walking off the ground edge")]
        [SerializeField] private bool useBoundaries = true;

        [Tooltip("Half-size of the play area. Default 48 works for a Plane with Scale 10 (100 units wide, 2 unit margin)")]
        [SerializeField] private float boundaryLimit = 48f;

        // ─── Private References ────────────────────────────────────────
        private Rigidbody _rb;
        private Transform _cameraTransform;
        private bool _isGrounded;

        // ─── Public Properties ─────────────────────────────────────────
        /// <summary>Whether the player is currently giving movement input.</summary>
        public bool IsMoving { get; private set; }

        /// <summary>Can be set to false to disable movement (e.g., during dialogue).</summary>
        public bool CanMove { get; set; } = true;

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void Awake()
        {
            // GetComponent finds a component on the SAME GameObject.
            _rb = GetComponent<Rigidbody>();

            // Safety: freeze rotation so the capsule doesn't topple.
            _rb.freezeRotation = true;
        }

        private void Start()
        {
            // Cache the main camera's transform for direction calculations.
            if (UnityEngine.Camera.main != null)
            {
                _cameraTransform = UnityEngine.Camera.main.transform;
            }
            else
            {
                Debug.LogWarning("[PlayerMovement] No Main Camera found! Make sure your camera has the 'MainCamera' tag.");
            }
        }

        private void Update()
        {
            // Update runs every frame — good for input detection.
            // We check jump here because GetKeyDown only works in Update.
            CheckGround();

            if (!CanMove || !Core.GameManager.Instance.IsPlaying) return;

            if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
            {
                Jump();
            }
        }

        private void FixedUpdate()
        {
            // FixedUpdate runs at a fixed rate (default 50 times/second).
            // Physics operations (Rigidbody velocity) should go here for consistency.
            if (!CanMove || !Core.GameManager.Instance.IsPlaying) return;

            Move();
            ClampToBoundaries();
        }

        // ─── Movement Logic ────────────────────────────────────────────

        private void Move()
        {
            // Read input axes. These return values from -1 to 1.
            // "Horizontal" = A/D or Left/Right arrows.
            // "Vertical" = W/S or Up/Down arrows.
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            // Create a raw input vector.
            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical);

            // If the player isn't pressing any keys, stop.
            if (inputDirection.magnitude < 0.1f)
            {
                IsMoving = false;
                // Keep vertical velocity (gravity/jump) but zero out horizontal.
                _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
                return;
            }

            IsMoving = true;

            // Convert input to camera-relative direction.
            // This means "forward" is where the camera is looking, not world-north.
            Vector3 moveDirection = CalculateCameraRelativeDirection(inputDirection);

            // Apply movement via Rigidbody velocity.
            // We preserve the Y velocity (for gravity and jumping).
            _rb.velocity = new Vector3(
                moveDirection.x * moveSpeed,
                _rb.velocity.y,
                moveDirection.z * moveSpeed
            );

            // Smoothly rotate the player to face the movement direction.
            RotateTowards(moveDirection);
        }

        /// <summary>
        /// Converts a raw input direction to be relative to the camera's facing direction.
        /// Without this, pressing W would always move north regardless of camera angle.
        /// </summary>
        private Vector3 CalculateCameraRelativeDirection(Vector3 inputDirection)
        {
            if (_cameraTransform == null)
            {
                // Fallback: just use world-space direction.
                return inputDirection.normalized;
            }

            // Get the camera's forward and right directions, flattened to the horizontal plane.
            Vector3 cameraForward = _cameraTransform.forward;
            Vector3 cameraRight = _cameraTransform.right;
            cameraForward.y = 0f; // Remove vertical component.
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Combine: input.z (forward/back) * camera's forward + input.x (left/right) * camera's right
            Vector3 worldDirection = cameraForward * inputDirection.z + cameraRight * inputDirection.x;
            return worldDirection.normalized;
        }

        /// <summary>
        /// Smoothly rotates the player to face the given direction.
        /// Uses Quaternion.Slerp for a smooth, natural-looking turn.
        /// </summary>
        private void RotateTowards(Vector3 direction)
        {
            if (direction == Vector3.zero) return;

            // Quaternion.LookRotation creates a rotation that "looks at" the given direction.
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            // Slerp = Spherical Linear Interpolation. Smoothly blends from current to target.
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }

        // ─── Jumping ───────────────────────────────────────────────────

        private void Jump()
        {
            // Apply an upward impulse for jumping.
            // ForceMode.Impulse = instant force (good for jumps).
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        /// <summary>
        /// Shoots a short ray downward to check if the player is on the ground.
        /// This prevents double-jumping (jumping while already in the air).
        /// </summary>
        private void CheckGround()
        {
            // Physics.Raycast shoots an invisible line from a point in a direction.
            // If it hits something on the groundLayer within groundCheckDistance, we're grounded.
            _isGrounded = Physics.Raycast(
                transform.position,
                Vector3.down,
                groundCheckDistance,
                groundLayer
            );
        }

        // ─── Boundary Clamping ─────────────────────────────────────────

        /// <summary>
        /// Prevents the player from walking beyond the ground edges.
        /// Clamps X and Z position within the boundary limit.
        /// If the player somehow falls below Y=0, resets them.
        /// </summary>
        private void ClampToBoundaries()
        {
            if (!useBoundaries) return;

            Vector3 pos = transform.position;
            bool clamped = false;

            if (pos.x > boundaryLimit) { pos.x = boundaryLimit; clamped = true; }
            if (pos.x < -boundaryLimit) { pos.x = -boundaryLimit; clamped = true; }
            if (pos.z > boundaryLimit) { pos.z = boundaryLimit; clamped = true; }
            if (pos.z < -boundaryLimit) { pos.z = -boundaryLimit; clamped = true; }

            // Safety: if the player somehow falls below the ground, reset them.
            if (pos.y < -5f)
            {
                pos = new Vector3(0f, 2f, 0f);
                _rb.velocity = Vector3.zero;
                clamped = true;
            }

            if (clamped)
            {
                transform.position = pos;
                // Zero out velocity in the clamped direction to prevent sliding.
                _rb.velocity = new Vector3(
                    Mathf.Abs(pos.x) >= boundaryLimit ? 0f : _rb.velocity.x,
                    _rb.velocity.y,
                    Mathf.Abs(pos.z) >= boundaryLimit ? 0f : _rb.velocity.z
                );
            }
        }
    }
}
