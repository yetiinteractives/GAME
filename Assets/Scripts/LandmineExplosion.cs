using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landmine : MonoBehaviour, IExplodable
{
    [Header("Trigger")]
    [SerializeField] private float triggerDelay = 1.5f;
    [SerializeField] private float setupDelay = 3f;

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
    [SerializeField] private float explosionFxLifetime = 5f;
    [SerializeField] private AudioSource landmineAudioSource;
    [SerializeField] private AudioClip triggerSound;
    [SerializeField] private AudioClip explosionSound;

    private bool exploded;
    private bool triggered;
    private bool isSetupComplete = false;


    private void Start()
    {
        landmineAudioSource = GetComponent<AudioSource>();
        StartCoroutine(SetUpLandmine());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (exploded || triggered || !isSetupComplete) return;

       
        if (other.GetComponentInParent<IDamageable>() == null)
            return;

        triggered = true;
        StartCoroutine(TriggerDelayRoutine());
    }

    private IEnumerator SetUpLandmine()
    {
        yield return new WaitForSeconds(setupDelay);
        isSetupComplete = true;
    }

    public void Explode()
    {
        if (exploded || triggered) return;
        ExplodePhysics();
    }

    private IEnumerator TriggerDelayRoutine()
    {
        if(triggerSound != null)
        {
            landmineAudioSource.PlayOneShot(triggerSound);  
        }
        yield return new WaitForSeconds(triggerDelay);
        ExplodePhysics();
    }

    private void ExplodePhysics()
    {
        if (exploded) return;
        exploded = true;

        if (explosionPrefab != null)
        {
            var fx = Instantiate(explosionPrefab, transform.position + Vector3.up, Quaternion.identity);
            Destroy(fx, explosionFxLifetime);
        }
        if (explosionSound != null)
        {
            landmineAudioSource.PlayOneShot(explosionSound);
        }
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, damageMask, QueryTriggerInteraction.Collide);

        HashSet<IDamageable> unique = new HashSet<IDamageable>();

        foreach (Collider hit in hits)
        {
            IExplodable explodable = hit.GetComponent<IExplodable>();
            if (explodable != null)
            {
                explodable.Explode();
            }

            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, radius, upwardsModifier, forceMode);
            }

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