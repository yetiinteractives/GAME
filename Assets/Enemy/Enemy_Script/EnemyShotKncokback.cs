using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyShotKnockback : MonoBehaviour
{
    private Animator animator;
    private int knockbackLayerIndex;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDuration = 0.4f;
    [SerializeField] private float blendSpeed = 8f;
    [SerializeField] private float targetWeight = 1f;

    private float knockbackTimer;
    private bool isKnockbackActive;
    private float currentWeight;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        //  knockbackLayerIndex = animator.GetLayerIndex("Knockback Layer");

        if (knockbackLayerIndex == -1)
        {
            Debug.LogError("Knockback Layer not found in Animator!");
        }
    }

    private void Update()
    {
        HandleKnockbackTimer();
        HandleLayerBlending();
    }

    public void TriggerKnockback()
    {
        if (knockbackLayerIndex == -1) return;

        knockbackTimer = knockbackDuration;
        isKnockbackActive = true;

        animator.ResetTrigger("Knockback");
        animator.SetTrigger("Knockback");
    }

    private void HandleKnockbackTimer()
    {
        if (!isKnockbackActive) return;

        knockbackTimer -= Time.deltaTime;

        if (knockbackTimer <= 0f)
        {
            isKnockbackActive = false;
        }
    }

    private void HandleLayerBlending()
    {
        if (knockbackLayerIndex == -1) return;

        float desiredWeight = isKnockbackActive ? targetWeight : 0f;

        currentWeight = Mathf.Lerp(currentWeight, desiredWeight, Time.deltaTime * blendSpeed);

        animator.SetLayerWeight(knockbackLayerIndex, currentWeight);
    }
}   