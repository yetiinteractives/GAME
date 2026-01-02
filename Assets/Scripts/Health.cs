using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
   

    private float currentHealth;

    public event System.Action<float> OnHealthChanged; // Current health percentage
    public event System.Action OnDeath;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        // Notify UI or other systems
        OnHealthChanged?.Invoke(currentHealth / maxHealth);

        Debug.Log($"{gameObject.name} took {damage:F1} damage. Health: {currentHealth:F0}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        OnDeath?.Invoke();
        // Add death logic here
    }
}