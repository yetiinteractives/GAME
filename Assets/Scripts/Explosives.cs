using System.Collections.Generic;
using System.Net.Sockets;
using Unity.VisualScripting;
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
    [SerializeField] private AudioClip explosionSound;
    private AudioSource audioSource;

    private MeshRenderer meshRenderer;

    private bool exploded;


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        meshRenderer = GetComponent<MeshRenderer>();
    }
    public void Explode()
    {
        if (exploded) return;
        exploded = true;

        Vector3 pos = transform.position;

        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, pos + Vector3.up, Quaternion.identity);
            Destroy(fx, explosionFxLifetime);
        }

        if(audioSource != null && explosionSound!= null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        CinemachineShake.Instance.Shake(5f, 1f);

        SoundEmitter.EmitSoundAt(pos, SoundType.Explosion, 100f , gameObject);

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

        meshRenderer.enabled = false;

        Destroy(gameObject, 2.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}