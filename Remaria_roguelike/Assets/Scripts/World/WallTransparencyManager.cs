using UnityEngine;
using System.Collections.Generic;

namespace Remoria.World
{
    public class WallTransparencyManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private float fadeSpeed = 5f;
        [SerializeField] private float targetAlpha = 0.3f;

        private List<Material> _currentTransparentMaterials = new List<Material>();
        private Dictionary<Material, float> _materialAlphas = new Dictionary<Material, float>();
        private HashSet<Material> _hittingThisFrame = new HashSet<Material>();

        private void Update()
        {
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTransform = player.transform;
                else return;
            }

            _hittingThisFrame.Clear();

            // Raycast from camera to player
            Vector3 direction = playerTransform.position - transform.position;
            float distance = direction.magnitude;
            RaycastHit[] hits = Physics.RaycastAll(transform.position, direction.normalized, distance, wallLayer);

            foreach (var hit in hits)
            {
                Renderer renderer = hit.collider.GetComponent<Renderer>();
                if (renderer != null)
                {
                    foreach (Material mat in renderer.materials)
                    {
                        if (mat.HasProperty("_Alpha"))
                        {
                            _hittingThisFrame.Add(mat);
                            if (!_materialAlphas.ContainsKey(mat))
                            {
                                _materialAlphas[mat] = 1.0f;
                            }
                        }
                    }
                }
            }

            // Update all tracked materials
            List<Material> keys = new List<Material>(_materialAlphas.Keys);
            foreach (Material mat in keys)
            {
                float current = _materialAlphas[mat];
                float target = _hittingThisFrame.Contains(mat) ? targetAlpha : 1.0f;

                // Move alpha towards target
                _materialAlphas[mat] = Mathf.MoveTowards(current, target, fadeSpeed * Time.deltaTime);
                mat.SetFloat("_Alpha", _materialAlphas[mat]);

                // Cleanup if back to 1.0 and not hitting
                if (_materialAlphas[mat] >= 1.0f && !_hittingThisFrame.Contains(mat))
                {
                    _materialAlphas.Remove(mat);
                }
            }
        }
    }
}
