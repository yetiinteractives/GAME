using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("UI")]
    [SerializeField] private Image healthBar;        
    [SerializeField] private Image healthBarTrail;   

    [Header("Trail Colors ")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private Color healColor = Color.green;

    [Header("Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float trailSpeed = 0.5f;

    private float currentHealth;
    public float CurrentHealth => currentHealth;

    private bool isDead = false;

    public static event Action OnPlayerDie;

    private Coroutine trailCoroutine;

    private void Start()
    {
        currentHealth = maxHealth;
        healthBar.fillAmount = 1f;
        healthBarTrail.fillAmount = 1f;
    }

    
    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);

        // Health bar updates instantly
        healthBar.fillAmount = currentHealth / maxHealth;

        healthBarTrail.color = damageColor;

        StartTrail(DamageTrail());

        if (currentHealth <= 0f)
            OnPlayerDie?.Invoke();
    }

   
    public void HealPlayer(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);

        // Trail updates instantly
        healthBarTrail.fillAmount = currentHealth / maxHealth;

        healthBarTrail.color = healColor;

        StartTrail(HealTrail());
    }

  
    private void StartTrail(IEnumerator routine)
    {
        if (trailCoroutine != null)
            StopCoroutine(trailCoroutine);

        trailCoroutine = StartCoroutine(routine);
    }

   
    private IEnumerator DamageTrail()
    {
        while (healthBarTrail.fillAmount > healthBar.fillAmount)
        {
            healthBarTrail.fillAmount = Mathf.MoveTowards(
                healthBarTrail.fillAmount,
                healthBar.fillAmount,
                trailSpeed * Time.deltaTime
            );
            yield return null;
        }
    }

    private IEnumerator HealTrail()
    {
        while (healthBar.fillAmount < healthBarTrail.fillAmount)
        {
            healthBar.fillAmount = Mathf.MoveTowards(
                healthBar.fillAmount,
                healthBarTrail.fillAmount,
                trailSpeed * Time.deltaTime
            );
            yield return null;
        }
    }



    //Interface implementation

    
    public bool IsDead() 
    { 
        return isDead;
    }

    public void ApplyDeathForce(Collider hitCollider, Vector3 hitPoint, Vector3 impulse)
    {

    }
}
