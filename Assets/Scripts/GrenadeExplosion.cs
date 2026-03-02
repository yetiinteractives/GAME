using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeExplosion : MonoBehaviour
{
    [SerializeField] private float fuseTime = 2f;
    [SerializeField] private float radius = 7f;
    [SerializeField] private float killDamage = 99999f;
    [SerializeField] private LayerMask hitMask = ~0;

    [SerializeField] private float explosionForce = 2200f; // start higher
    [SerializeField] private float upwardsModifier = 0.6f;
    [SerializeField] private ForceMode forceMode = ForceMode.Impulse;

    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float fxLifetime = 3f;

    private bool exploded;

    private void Start() => StartCoroutine(Fuse());

    private IEnumerator Fuse()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    public void Explode()
    {
        if (exploded) return;
        exploded = true;

        Vector3 origin = transform.position;

        if (explosionPrefab != null)
        {
            var fx = Instantiate(explosionPrefab, origin, Quaternion.identity);
            Destroy(fx, fxLifetime);
        }

        Collider[] hits = Physics.OverlapSphere(origin, radius, hitMask, QueryTriggerInteraction.Ignore);

        HashSet<UniversalEnemyAi> unique = new HashSet<UniversalEnemyAi>();
        List<ZombieBrain> zombies = new List<ZombieBrain>();

        for (int i = 0; i < hits.Length; i++)
        {
            var enemy = hits[i].GetComponentInParent<UniversalEnemyAi>();
            if (enemy == null || !unique.Add(enemy)) continue;

            if (enemy is IDamageable d)
                d.TakeDamage(killDamage);

            if (enemy is ZombieBrain z)
                zombies.Add(z);
        }

        StartCoroutine(BlastZombiesDeferred(origin, zombies));
        Destroy(gameObject);
    }

    private IEnumerator BlastZombiesDeferred(Vector3 origin, List<ZombieBrain> zombies)
    {
        // wait 2 fixed ticks for death->ragdoll->physics sync
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        for (int i = 0; i < zombies.Count; i++)
        {
            if (zombies[i] == null) continue;
            zombies[i].ApplyExplosionRagdollForce(origin, radius, explosionForce, upwardsModifier, forceMode);
        }
    }
}