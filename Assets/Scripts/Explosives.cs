using System.Collections.Generic;
using UnityEngine;

public class Explosives : MonoBehaviour, IExplodable
{
    [Header("Explosion Settings")]
    [SerializeField] private float radius = 4f;
    [SerializeField] private float damage = 200f;
    [SerializeField] private LayerMask damageMask = ~0;

    [Header("Physics")]
    [SerializeField] private float explosionForce = 40f;
    [SerializeField] private float upwardsModifier = 1.5f;
    [SerializeField] private ForceMode forceMode = ForceMode.Impulse;

    [Header("FX")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionFxLifetime = 3f;

    private bool exploded;

    public void Explode()
    {
        if (exploded) return;
        exploded = true;

        Vector3 pos = transform.position;

        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, pos, Quaternion.identity);
            Destroy(fx, explosionFxLifetime);
        }

        Collider[] hits = Physics.OverlapSphere(pos, radius, damageMask, QueryTriggerInteraction.Collide);

        HashSet<IDamageable> damaged = new HashSet<IDamageable>();

        foreach (Collider hit in hits)
        {
            // Chain explosions
            IExplodable explodable = hit.GetComponent<IExplodable>();
            if (explodable != null && explodable != (IExplodable)this)
            {
                explodable.Explode();
            }

            // Physics force
            Rigidbody rb = hit.attachedRigidbody;
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, pos, radius, upwardsModifier, forceMode);
            }

            // Damage
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable != null && damaged.Add(damageable))
            {
                damageable.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}