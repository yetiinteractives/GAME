using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;






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

    private PersistentSceneEntity persistentEntity;

    protected bool isDead = false;

    protected virtual void Awake()
    {
        currentHealth = maxhealth;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        if(player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        persistentEntity = GetComponent<PersistentSceneEntity>();
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
            // Safe NavMesh shutdown — prevents "Stop can only be called on
            // an active agent that has been placed on a NavMesh" exception,
            // which would interrupt the death → ragdoll → explosion chain.
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            if (agent.enabled)
                agent.enabled = false;
        }

        HandleDeathVisuals();
        OnEnemyDeath();
        OnEnemyDeathEvent?.Invoke();

        // inside Die()
        if (persistentEntity != null)
            persistentEntity.MarkRemoved(false); 

        Destroy(gameObject, deathDecayTime);
    }

    protected virtual void HandleDeathVisuals()
    {
        PlayDieAnimation();
    }

    protected abstract void OnEnemyDeath();
    protected abstract void HandleAI();

    protected float distanceToPlayer =>
        player != null ? (player.position - transform.position).sqrMagnitude : float.MaxValue;

    // -------- Cached Animator Parameter Hashes --------
    protected static readonly int AnimGetOutOfGround = Animator.StringToHash("GetOutOfGround");
    protected static readonly int AnimIdle = Animator.StringToHash("Idle");
    protected static readonly int AnimWalk = Animator.StringToHash("Walk");
    protected static readonly int AnimCharge = Animator.StringToHash("Charge");
    protected static readonly int AnimAttack1 = Animator.StringToHash("Attack1");
    protected static readonly int AnimAttack2 = Animator.StringToHash("Attack2");
    protected static readonly int AnimRoar = Animator.StringToHash("Roar");
    protected static readonly int AnimGetHitBack = Animator.StringToHash("GetHitBack");
    protected static readonly int AnimKnockback = Animator.StringToHash("Knockback");
    protected static readonly int AnimDeath = Animator.StringToHash("Death");

    // -------- Animation Controls --------

    protected void PlayStartAnimation()
    {
        if (anim != null) anim.SetTrigger(AnimGetOutOfGround);
    }

    protected void PlayIdleAnimation()
    {
        if (anim != null)
        {
            anim.SetBool(AnimIdle, true);
            anim.SetBool(AnimWalk, false);
        }
    }

    protected void PlayWalkAnimation()
    {
        if (anim != null)
        {
            anim.SetBool(AnimWalk, true);
            anim.SetBool(AnimIdle, false);
        }
    }

    protected void PlayChargeAnimation()
    {
        if (anim != null)
        {
            anim.SetBool(AnimIdle, false);
            anim.SetBool(AnimWalk, false);
            anim.SetBool(AnimCharge, true);
        }
    }

    protected void PlayAttackAnimation()
    {
        if (anim != null) anim.SetTrigger(AnimAttack1);
    }

    protected void PlayAttack2Animation()
    {
        if (anim != null) anim.SetTrigger(AnimAttack2);
    }

    protected void PlayRageAnimation()
    {
        if (anim != null) anim.SetTrigger(AnimRoar);
    }

    protected void PlayHitAnimation()
    {
        if (anim != null) anim.SetTrigger(AnimGetHitBack);
    }

    protected void PlayKnockbackAnimation()
    {
        if (anim != null) anim.SetTrigger(AnimKnockback);
    }

    protected void PlayDieAnimation()
    {
        if (anim != null) anim.SetTrigger(AnimDeath);
    }

    protected virtual void OnEnable()
    {
        PlayerHealth.OnPlayerDie += HandlePlayerDeath;
    }

    protected virtual void OnDisable()
    {
        PlayerHealth.OnPlayerDie -= HandlePlayerDeath;
    }

    protected virtual void HandlePlayerDeath()
    {
        // Stop all enemy actions when player dies
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        PlayIdleAnimation();
    }
}