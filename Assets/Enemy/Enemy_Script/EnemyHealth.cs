using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour , IDamageable
{
    public event Action  OnEnemyDeath; // Event to notify when the enemy dies

    [Header("Enemy Health Settings")]
    public int enemy_maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = enemy_maxHealth;

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            TakeDamage(20);
        }
      

    }

public void TakeDamage(int damage)  //override from interface.
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
    OnEnemyDeath?.Invoke();
    Debug.Log("Enemy died");
    Destroy(gameObject);
}
}
