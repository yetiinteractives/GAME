using UnityEngine;

public class HitBoxScript : MonoBehaviour
{
    private ProfactorBrain boss;
    private BoxCollider hitBoxCollider;

    private void Start()
    {
      
        if (boss == null)
            boss = GetComponentInParent<ProfactorBrain>();

      
        hitBoxCollider = GetComponent<BoxCollider>();
        if (hitBoxCollider != null)
            hitBoxCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (boss == null) return;
        if (hitBoxCollider == null || !hitBoxCollider.enabled) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.TakeDamage(boss.attackDamage);
    }
}