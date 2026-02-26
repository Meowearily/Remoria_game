using UnityEngine;
using Remoria.Core;
using Remoria.Combat;

namespace Remoria.Player
{
    /// <summary>
    /// Handles melee and ranged attacks for the player.
    /// 
    /// Controls:
    ///   Left Mouse Button → Melee attack
    ///   Right Mouse Button → Ranged attack (fires a projectile)
    /// 
    /// How melee works:
    ///   - Uses Physics.OverlapSphere to find all colliders in a sphere
    ///     in front of the player.
    ///   - Checks each hit for IDamageable interface.
    ///   - Deals damage to everything damageable in range.
    /// 
    /// How ranged works:
    ///   - Instantiates a Projectile prefab at a spawn point.
    ///   - The Projectile script handles movement and collision.
    /// 
    /// Required setup:
    ///   - Assign the projectilePrefab in the Inspector.
    ///   - Create an empty child GameObject called "ProjectileSpawnPoint" 
    ///     positioned in front of the player, and assign it.
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

        [Tooltip("Where the projectile spawns (create an empty child object in front of the player)")]
        [SerializeField] private Transform projectileSpawnPoint;

        [Tooltip("Damage dealt by each projectile")]
        [SerializeField] private float rangedDamage = 15f;

        [Tooltip("How fast the projectile flies")]
        [SerializeField] private float projectileSpeed = 20f;

        [Tooltip("Seconds between ranged attacks")]
        [SerializeField] private float rangedCooldown = 0.8f;

        // ─── Runtime State ─────────────────────────────────────────────
        private float _lastMeleeTime = -999f;  // When the last melee attack happened.
        private float _lastRangedTime = -999f;  // When the last ranged attack happened.
        // Starting at -999 ensures the first attack is always ready.

        // ─── Unity Callbacks ───────────────────────────────────────────

        private void Update()
        {
            if (!GameManager.Instance.IsPlaying) return;

            // Left Mouse Button = Melee.
            // GetMouseButtonDown(0) fires once when the button is pressed.
            if (Input.GetMouseButtonDown(0))
            {
                TryMeleeAttack();
            }

            // Right Mouse Button = Ranged.
            if (Input.GetMouseButtonDown(1))
            {
                TryRangedAttack();
            }
        }

        // ─── Melee Attack ──────────────────────────────────────────────

        private void TryMeleeAttack()
        {
            // Check cooldown: has enough time passed since the last attack?
            if (Time.time - _lastMeleeTime < meleeCooldown) return;

            _lastMeleeTime = Time.time;

            // Calculate the center of the hit sphere.
            // transform.position = player's feet.
            // transform.forward = the direction the player is facing.
            Vector3 attackCenter = transform.position + transform.forward * meleeRange + Vector3.up;

            // Find all colliders in the sphere.
            // OverlapSphere is like an "explosion check" — it finds everything within a radius.
            Collider[] hits = Physics.OverlapSphere(attackCenter, meleeRadius, meleeHitLayers);

            foreach (Collider hit in hits)
            {
                // Try to get the IDamageable component from the hit object.
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
            // Check cooldown.
            if (Time.time - _lastRangedTime < rangedCooldown) return;

            // Check if we have a projectile prefab assigned.
            if (projectilePrefab == null)
            {
                Debug.LogWarning("[PlayerCombat] No projectile prefab assigned! Drag one into the Inspector.");
                return;
            }

            _lastRangedTime = Time.time;

            // Determine spawn position. Fall back to a position in front of the player.
            Vector3 spawnPos = projectileSpawnPoint != null
                ? projectileSpawnPoint.position
                : transform.position + transform.forward * 1.5f + Vector3.up;

            // Instantiate the projectile.
            // Instantiate(original, position, rotation) creates a copy of the prefab.
            GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, transform.rotation);

            // Configure the projectile.
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(rangedDamage, projectileSpeed, transform.forward);
            }

            Debug.Log("[PlayerCombat] Ranged attack fired!");
        }

        // ─── Gizmos (Visual Debugging) ─────────────────────────────────
        // Gizmos are visual helpers that appear in the Unity Scene view (not in game).
        // They help you see the melee attack range.

        private void OnDrawGizmosSelected()
        {
            // Draw the melee attack sphere in yellow.
            Gizmos.color = Color.yellow;
            Vector3 attackCenter = transform.position + transform.forward * meleeRange + Vector3.up;
            Gizmos.DrawWireSphere(attackCenter, meleeRadius);
        }
    }
}
