using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GrenadeExplosion : MonoBehaviour , IExplodable
{
  


    [SerializeField] private float fuseTime = 2f;
    [SerializeField] private float radius = 3f;
    [SerializeField] private float killDamage = 99999f;
    [SerializeField] private LayerMask damageMask = ~0;

    [SerializeField] private float explosionForce = 30f;
    [SerializeField] private float upwardsModifier = 2f;
    [SerializeField] private ForceMode forceMode = ForceMode.Impulse;

    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioSource grenadeAudioSource;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private float explosionFxLifetime = 3f;


    [SerializeField] private GameObject[] gernadeModels; 

    private bool hasCollided = false;

    private bool exploded;

    private void Start()
    {
        grenadeAudioSource = GetComponent<AudioSource>();
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (exploded) return;

        Explode();
        hasCollided = true;
    }
    
    public void Explode()
    {
        if (hasCollided) return;   
         StartCoroutine(FuseRoutine());
    }


    
    private IEnumerator FuseRoutine()
    {
        yield return new WaitForSeconds(fuseTime - .125f);
        if (explosionPrefab != null)
        {
            var fx = Instantiate(explosionPrefab, transform.position + Vector3.up, Quaternion.identity);
            Destroy(fx, explosionFxLifetime);
        }
        if(grenadeAudioSource != null && explosionSound != null)
        {
            grenadeAudioSource.PlayOneShot(explosionSound);
        }
        CinemachineShake.Instance.Shake(8f, 1.25f);

        yield return new WaitForSeconds(.125f);
        ExplodePhysics();
    }

    public void ExplodePhysics()
    {
        if (exploded) return;
        exploded = true;

        Collider[] hits = Physics.OverlapSphere(transform.position, radius, damageMask, QueryTriggerInteraction.Collide);
        foreach (Collider hit in hits)
        {
            IExplodable explodable = hit.GetComponent<IExplodable>();
            if (explodable != null)
            {
                explodable.Explode();
            }


            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if(rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, radius, upwardsModifier, forceMode);
            }
        }


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

        
        for(int i = 0; i < gernadeModels.Length;i++)
        {
            gernadeModels[i].SetActive(false);
        }

        Destroy(gameObject,5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }


    
}