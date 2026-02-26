using UnityEngine;

namespace Remoria.CameraSystem
{
    /// <summary>
    /// Hades-style isometric follow camera.
    /// 
    /// The camera looks down at the player from a fixed high angle (like in Hades).
    /// It follows the player smoothly but never rotates — the viewing angle stays constant.
    /// 
    /// Key differences from a standard third-person camera:
    ///   - Higher angle (looking down ~55 degrees)
    ///   - Fixed rotation (camera never turns, only translates)
    ///   - Feels top-down but with perspective depth
    /// 
    /// Setup:
    ///   1. Attach to the Main Camera.
    ///   2. Drag the Player into "Target".
    ///   3. Adjust the angle and distance to taste.
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        // ─── Inspector Fields ──────────────────────────────────────────

        [Header("Target")]
        [Tooltip("The player Transform to follow")]
        [SerializeField] private Transform target;

        [Header("Isometric Settings")]
        [Tooltip("Camera angle from horizontal (degrees). 45 = classic isometric, 55 = Hades-like")]
        [Range(30f, 75f)]
        [SerializeField] private float cameraAngle = 55f;

        [Tooltip("Distance from the player")]
        [Range(5f, 30f)]
        [SerializeField] private float distance = 14f;

        [Tooltip("Horizontal rotation around the player (0 = looking north, 45 = diagonal)")]
        [Range(0f, 360f)]
        [SerializeField] private float horizontalAngle = 0f;

        [Header("Smoothing")]
        [Tooltip("How quickly the camera follows. Lower = smoother.")]
        [Range(1f, 20f)]
        [SerializeField] private float followSpeed = 5f;

        [Header("Look Target")]
        [Tooltip("Height above the player's feet to look at")]
        [SerializeField] private float lookAtHeight = 1f;

        // ─── Runtime ───────────────────────────────────────────────────
        private Vector3 _currentVelocity; // Used by SmoothDamp.

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void LateUpdate()
        {
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    target = player.transform;
                else
                    return;
            }

            FollowTarget();
        }

        // ─── Camera Logic ──────────────────────────────────────────────

        private void FollowTarget()
        {
            float angleRad = cameraAngle * Mathf.Deg2Rad;
            float horizontalRad = horizontalAngle * Mathf.Deg2Rad;

            float height = distance * Mathf.Sin(angleRad);
            float horizontalDist = distance * Mathf.Cos(angleRad);

            float offsetX = -Mathf.Sin(horizontalRad) * horizontalDist;
            float offsetZ = -Mathf.Cos(horizontalRad) * horizontalDist;

            Vector3 offset = new Vector3(offsetX, height, offsetZ);
            Vector3 desiredPosition = target.position + offset;

            // Smooth follow — only translates, never rotates.
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref _currentVelocity,
                1f / followSpeed
            );

            // FIXED rotation — always the same angle, no wobble.
            // The camera looks down at cameraAngle degrees, rotated horizontalAngle around Y.
            transform.rotation = Quaternion.Euler(cameraAngle, horizontalAngle, 0f);
        }
    }
}
