using System;
using UnityEngine;
using UnityEngine.AI;

public abstract class UniversalEnemyAi : MonoBehaviour
{
    [SerializeField] protected int deathDecayTime = 5;

    public event Action OnEnemyDeathEvent;

    [Header("Core Stats")]
    public int maxhealth = 100;
    [HideInInspector] public float currentHealth;

    [Header("Components")]
    protected NavMeshAgent agent;
    protected Animator anim;

    [Header("Player Reference")]
    public Transform player;

    protected bool isDead = false;

    protected virtual void Awake()
    {
        currentHealth = maxhealth;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        OnEnemyAwake();
    }

    protected abstract void OnEnemyAwake();

    protected virtual void Update()
    {
        if (!isDead)
            HandleAI();
    }

    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        HandleDeathVisuals();
        OnEnemyDeath();
        OnEnemyDeathEvent?.Invoke();

        Destroy(gameObject, deathDecayTime);
    }

    protected virtual void HandleDeathVisuals()
    {
        PlayDieAnimation();
    }

    protected abstract void OnEnemyDeath();
    protected abstract void HandleAI();

    protected float distanceToPlayer =>
        (player.position - transform.position).sqrMagnitude;

    // -------- Animation Controls --------

    protected void PlayStartAnimation() =>
        anim.SetTrigger("GetOutOfGround");

    protected void PlayIdleAnimation()
    {
        anim.SetBool("Idle", true);
        anim.SetBool("Walk", false);
    }

    protected void PlayWalkAnimation()
    {
        anim.SetBool("Walk", true);
        anim.SetBool("Idle", false);
    }

    protected void PlayChargeAnimation()
    {
        anim.SetBool("Idle", false);
        anim.SetBool("Walk", false);
        anim.SetBool("Charge", true);
    }

    protected void PlayAttackAnimation() =>
        anim.SetTrigger("Attack1");

    protected void PlayAttack2Animation() =>
        anim.SetTrigger("Attack2");

    protected void PlayRageAnimation() =>
        anim.SetTrigger("Roar");

    protected void PlayHitAnimation() =>
        anim.SetTrigger("GetHitBack");

    protected void PlayDieAnimation() =>
        anim.SetTrigger("Death");
}