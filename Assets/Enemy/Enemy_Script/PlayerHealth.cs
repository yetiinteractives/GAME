using UnityEngine;                      // (1)
using System;                           // (2) - for Action events

public class Health : MonoBehaviour     // (3)
{
    [Header("Health Settings")]         // (4)
    public int maxHealth = 100;         // (5)

    [SerializeField]                    // (6)
    private int currentHealth;          // (7)

    public bool isInvincible = false;   // (8)

    // Events other scripts (UI, audio, spawners) can subscribe to
    public event Action<int,int> OnHealthChanged; // (9) current, max
    public event Action OnDied;                   // (10)

    void Awake()                         // (11)
    {
        // initialize current health as early as possible
        currentHealth = maxHealth;       // (12)
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            TakeDamage(20);
        }
        
    }

    // Call this to apply damage
    public void TakeDamage(int amount)   // (13)
    {
        if (amount <= 0) return;         // (14) ignore invalid damage
        if (isInvincible) return;        // (15) ignore when invulnerable

        currentHealth -= amount;         // (16) subtract damage
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // (17) keep in range

        OnHealthChanged?.Invoke(currentHealth, maxHealth); // (18) notify listeners

        if (currentHealth <= 0)          // (19)
            Die();                       // (20)
    }

    // Optional: heal the entity
    public void Heal(int amount)         // (21)
    {
        if (amount <= 0) return;         // (22)
        currentHealth += amount;         // (23)
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // (24)
        OnHealthChanged?.Invoke(currentHealth, maxHealth); // (25)
    }

    void Die()                          // (26)
    {
        OnDied?.Invoke();               // (27) notify listeners before destruction
        Debug.Log($"{gameObject.name} died."); // (28)
        // TODO: play death animation, drop loot, disable components, etc.
        Destroy(gameObject);            // (29) remove object — or use pooling
    }
}
