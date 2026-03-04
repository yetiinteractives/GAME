using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class GrenadeExplosion : MonoBehaviour
{
    [SerializeField] private float fuseTime = 2f;
    [SerializeField] private float radius = 3f;
    [SerializeField] private float killDamage = 99999f;
    [SerializeField] private LayerMask damageMask = ~0;

    [SerializeField] private float explosionForce = 30f;
    [SerializeField] private float upwardsModifier = 2f;
    [SerializeField] private ForceMode forceMode = ForceMode.Impulse;

    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionFxLifetime = 3f;

    private bool exploded;
  

    

    private void OnCollisionEnter(Collision collision)
    {
        if (exploded) return;

        StartCoroutine(FuseRoutine());
    }

    

    private IEnumerator FuseRoutine()
    {
        yield return new WaitForSeconds(fuseTime - .125f);
        if (explosionPrefab != null)
        {
            var fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(fx, explosionFxLifetime);
        }
        yield return new WaitForSeconds(.125f);
        Explode();
    }

    public void Explode()
    {
        if (exploded) return;
        exploded = true;

        Collider[] hits = Physics.OverlapSphere(transform.position, radius, damageMask, QueryTriggerInteraction.Collide);

        HashSet<IDamageable> unique = new HashSet<IDamageable>();

        for (int i = 0; i < hits.Length; i++)
        {
            IDamageable d = hits[i].GetComponentInParent<IDamageable>();
            if (d == null || !unique.Add(d)) continue;

            Component comp = d as Component;
            if (comp != null)
            {
                ZombieBrain zb = comp.GetComponent<ZombieBrain>();
                if (zb != null)
                {
                    // Queue blast first, then kill.
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