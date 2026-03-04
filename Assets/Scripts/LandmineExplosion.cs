using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandmineExplosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private float killDamage = 99999f;
    [SerializeField] private LayerMask damageMask = ~0;

    [Header("Force Settings")]
    [SerializeField] private float explosionForce = 30f;
    [SerializeField] private float upwardsModifier = 2f;
    [SerializeField] private ForceMode forceMode = ForceMode.Impulse;

    [Header("FX")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionFxLifetime = 3f;

    private bool exploded;

    private void OnTriggerEnter(Collider other)
    {
        if (exploded) return;

       

        
        if (other.GetComponentInParent<IDamageable>() == null)
            return;

        Explode();
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;

        // Spawn FX
        if (explosionPrefab != null)
        {
            var fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(fx, explosionFxLifetime);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, radius, damageMask, QueryTriggerInteraction.Collide);

        HashSet<IDamageable> unique = new HashSet<IDamageable>();

        foreach (Collider hit in hits)
        {
            // Apply force
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, radius, upwardsModifier, forceMode);
            }

            // Damage
            IDamageable d = hit.GetComponentInParent<IDamageable>();
            if (d == null || !unique.Add(d)) continue;

            Component comp = d as Component;
            if (comp != null)
            {
                ZombieBrain zb = comp.GetComponent<ZombieBrain>();
                if (zb != null)
                {
                    zb.QueueExplosionForce(transform.position, radius, explosionForce, upwardsModifier, forceMode);
                }
            }

            d.TakeDamage(killDamage);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}