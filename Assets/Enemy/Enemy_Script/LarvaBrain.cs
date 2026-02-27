
using UnityEngine;
using UnityEngine.AI;

public class LarvaBrain : UniversalEnemyAi,IDamageable
{
    public enum BossState { Idle, Chase, Attack, Rage }
    public BossState state;

    bool attackchanger = true;
    bool attackInProgress = false;
    bool phase2;

    [Header("Movement")]
    public float WalkSpeed = 3f;

    [Header("Detection & Combat")]
    public float lookRadius = 8f;
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float attackCooldown = 1.2f;
    float attackTimer = 1f;
    public BoxCollider attackHitBox;
    protected override void OnEnemyAwake()
    {
        if(attackHitBox != null)
            attackHitBox.enabled = false;


        state = BossState.Idle;
        PlayStartAnimation();
        PlayIdleAnimation();
    }

    protected override void HandleAI()
    {
        if (player == null) return;
        float dist = distanceToPlayer;
        if (dist <= attackRange*attackRange)
            state = BossState.Attack;
        else if (dist <= lookRadius*lookRadius)
            state = BossState.Chase;
        else
            state = BossState.Idle;

      
        switch (state)
        {
            case BossState.Idle:
            if(isDead) break;
                agent.isStopped = true;
                PlayIdleAnimation();
                attackInProgress = false; // reset attack when leaving attack range
                break;

            case BossState.Chase:
            if(isDead) break;
                agent.isStopped = false;
                agent.speed = WalkSpeed;
                if (!agent.hasPath || agent.destination != player.position)
                    agent.SetDestination(player.position);

                PlayWalkAnimation();
                attackInProgress = false;
                break;

            case BossState.Attack:
            if(isDead) break;
                agent.isStopped = true;
                if (!attackInProgress)
                {
                    attackInProgress = true;
                    attackTimer = 0f;
                    if (attackchanger)
                        PlayAttackAnimation();
                    else
                        PlayAttack2Animation();
                }

                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    attackInProgress = false;
                    attackchanger = !attackchanger; // switch attack for next time
                }
                break;

            case BossState.Rage:
            if(isDead) break;
                agent.isStopped = true;
                PlayRageAnimation();
                attackInProgress = false;
                break;
        }
    }
    

    public void TakeDamage(float damage)
    {
         if (isDead) return;
        currentHealth -= damage;
         
        if (currentHealth <= 0)
        {
              Die(); 
        }
  
         
    }

    public bool IsDead()
    {
        return isDead;
    }

    /* protected override void OnDamageTaken(int damage)
     {
         if (!phase2 && currentHealth <= maxhealth * 0.5f)
         {
             phase2 = true;
             state = BossState.Rage;
         }


     }
     */
    protected override void OnEnemyDeath()
    {
       //for loot and some items
    }
    public void OnDeathEnds()
    {
               if (anim != null)
            anim.enabled = false;
    }


    public void ApplyDeathForce(Collider hitCollider, Vector3 hitPoint, Vector3 impulse)
    {
        // override interface
    }
        protected override void HandleDeathVisuals()
    {
        PlayDieAnimation();


        

    }

    //------------atacking logic yeta-----------------/

    public void CreateHitBox()
    {
        if (attackHitBox != null)
            attackHitBox.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && attackHitBox != null && attackHitBox.enabled)
        {
         
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }
    public void DestroyHitBox()
    {
        if (attackHitBox != null)
            attackHitBox.enabled = false;
    }

}
