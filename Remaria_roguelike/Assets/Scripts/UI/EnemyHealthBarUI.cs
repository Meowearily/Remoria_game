using UnityEngine;
using UnityEngine.UI;
using Remoria.Core;

namespace Remoria.UI
{
    /// <summary>
    /// World-space health bar that floats above an enemy.
    /// 
    /// "World-space" means the health bar exists in 3D space (above the enemy's head),
    /// not flat on the screen like the player's health bar.
    /// The Canvas is set to "World Space" mode so it renders in the 3D scene.
    /// 
    /// The bar always faces the camera (billboarding) so it's readable from any angle.
    /// 
    /// Setup:
    ///   1. Create a Canvas as a child of the Enemy.
    ///   2. Set the Canvas Render Mode to "World Space".
    ///   3. Set the Canvas size to something small (e.g., width=1, height=0.15).
    ///   4. Position it above the enemy's head.
    ///   5. Add background Image + fill Image (like the player health bar).
    ///   6. Attach this script and assign references.
    /// </summary>
    public class EnemyHealthBarUI : MonoBehaviour
    {
        // ─── Inspector Fields ──────────────────────────────────────────
        [Header("References")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Health targetHealth;

        [Header("Visibility")]
        [Tooltip("Only show the health bar when the enemy has taken damage")]
        [SerializeField] private bool hideWhenFull = true;

        [Tooltip("Hide after this many seconds of no damage")]
        [SerializeField] private float hideDelay = 3f;

        // ─── Runtime ───────────────────────────────────────────────────
        private Canvas _canvas;
        private Transform _cameraTransform;
        private float _hideTimer;
        private bool _hasBeenDamaged = false;

        [Header("Positioning")]
        [Tooltip("Height above the enemy's pivot where the bar appears")]
        [SerializeField] private float heightAboveEnemy = 2.2f;

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void Awake()
        {
            _canvas = GetComponentInChildren<Canvas>();

            // Auto-find health on parent if not assigned.
            if (targetHealth == null)
            {
                targetHealth = GetComponentInParent<Health>();
            }

            // Auto-configure the world-space canvas so it doesn't appear huge.
            if (_canvas != null)
            {
                _canvas.renderMode = RenderMode.WorldSpace;

                // Scale down: canvas uses pixels, but world space uses meters.
                // 0.01 means 100 pixels = 1 meter.
                _canvas.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

                // Position above the enemy's head.
                _canvas.transform.localPosition = new Vector3(0f, heightAboveEnemy, 0f);

                // Set a reasonable canvas size (in pixels at 0.01 scale).
                RectTransform rt = _canvas.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.sizeDelta = new Vector2(120f, 20f); // 1.2m wide, 0.2m tall
                }
            }
        }

        private void Start()
        {
            if (UnityEngine.Camera.main != null)
            {
                _cameraTransform = UnityEngine.Camera.main.transform;
            }

            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged += OnHealthChanged;
            }

            // Start hidden if configured.
            if (hideWhenFull && _canvas != null)
            {
                _canvas.enabled = false;
            }
        }

        private void LateUpdate()
        {
            // Billboard: always face the camera.
            if (_cameraTransform != null)
            {
                transform.LookAt(
                    transform.position + _cameraTransform.forward
                );
            }

            // Auto-hide after delay.
            if (_hasBeenDamaged && hideWhenFull)
            {
                _hideTimer -= Time.deltaTime;
                if (_hideTimer <= 0f && _canvas != null)
                {
                    _canvas.enabled = false;
                    _hasBeenDamaged = false;
                }
            }
        }

        private void OnDestroy()
        {
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged -= OnHealthChanged;
            }
        }

        // ─── Event Handler ─────────────────────────────────────────────

        private void OnHealthChanged(float current, float max)
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = max > 0 ? current / max : 0f;
            }

            // Show the health bar when damaged.
            if (_canvas != null)
            {
                _canvas.enabled = true;
            }

            _hasBeenDamaged = true;
            _hideTimer = hideDelay;
        }
    }
}