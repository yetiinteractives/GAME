using UnityEngine;

public class EnemyShotKnockback : MonoBehaviour
{
    Animator animator;
    int knockbackLayerIndex;

    public static EnemyShotKnockback Instance { get; private set; }

    [SerializeField] private float knockbackDuration = 3f;
    [SerializeField] private float blendSpeed = 8f;
    [SerializeField] private float targetWeight = 0.9f;

    float knockbackTimer;
    bool isKnockbackActive;

    float currentWeight;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        knockbackLayerIndex = animator.GetLayerIndex("Knockback Layer");
        currentWeight = animator.GetLayerWeight(knockbackLayerIndex);
    }

    private void Update()
    {
        HandleKnockbackTimer();
        HandleLayerBlending();
    }

    public void TriggerKnockback()
    {
        
        knockbackTimer = knockbackDuration;
        isKnockbackActive = true;

        animator.ResetTrigger("Knockback");
        animator.SetTrigger("Knockback");
    }

    void HandleKnockbackTimer()
    {
        if (!isKnockbackActive)
            return;

        knockbackTimer -= Time.deltaTime;

        if (knockbackTimer <= 0f)
        {
            isKnockbackActive = false;
        }
    }

    void HandleLayerBlending()
    {
        float desiredWeight = isKnockbackActive ? targetWeight : 0f;

        currentWeight = Mathf.Lerp(currentWeight, desiredWeight, Time.deltaTime * blendSpeed);

        animator.SetLayerWeight(knockbackLayerIndex, currentWeight);
    }
}