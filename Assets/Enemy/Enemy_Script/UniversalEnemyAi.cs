using System;
using UnityEngine;
using UnityEngine.AI;
public abstract class UniversalEnemyAi : MonoBehaviour
{
    [SerializeField] protected int deathDecayTime = 5;

     public event Action OnEnemyDeathEvent;
    [Header("core stats")]
    public int maxhealth = 100;
    [HideInInspector] public int currentHealth;

    [Header("Component")]
    protected NavMeshAgent agent;
    protected Animator anim;

    [Header("player refrence")]
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


    protected virtual void Die()
    {
        isDead = true;
        agent.isStopped = true;
        PlayDieAnimation();
        OnEnemyDeathEvent?.Invoke();
        
        Destroy(gameObject, deathDecayTime);

    }
   
    protected abstract void OnEnemyDeath();
    protected abstract void HandleAI();
    protected virtual void Update()
    {
        if(!isDead)
            HandleAI();
    }

    protected float distanceToPlayer =>  (player.position- transform.position).sqrMagnitude;

    //--------Animations control
    protected void PlayStartAnimation() => anim.SetTrigger("GetOutOfGround");
    protected void PlayIdleAnimation()
    {
      
        anim.SetBool("Idle", true);
        anim.SetBool("Walk", false);
    }
    protected void PlayWalkAnimation()
    {
        Debug.Log("walk");
        anim.SetBool("Walk", true);
        anim.SetBool("Idle", false);
    }
    protected void PlayChargeAnimation()
    {
        anim.SetBool("Idle", false);
        anim.SetBool("Walk", false);
        anim.SetBool("Charge", true);

    }
    protected void PlayAttackAnimation() => anim.SetTrigger("Attack1");
    protected void PlayAttack2Animation() => anim.SetTrigger("Attack2");
    protected void PlayRageAnimation() => anim.SetTrigger("Roar");
    protected void PlayHitAnimation() => anim.SetTrigger("GetHitBack");
    protected void PlayDieAnimation() => anim.SetTrigger("Death");




}
