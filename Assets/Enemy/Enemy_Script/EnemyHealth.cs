using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            TakeDamage(90);
        }
      

    }

void TakeDamage(int damage)
{
    currentHealth -= damage;
    Debug.Log("Enemy Health:" + currentHealth);
    if (currentHealth <= 0)
    {
        Die();
    }
}
void Die()
{
    Debug.Log("Enemy died");
    Destroy(gameObject);
}
}
