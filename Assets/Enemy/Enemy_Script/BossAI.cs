
using UnityEngine;
using UnityEngine.AI;

public class BossAI : UniversalEnemyAi
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
    protected override void OnEnemyAwake()
    {
        state = BossState.Idle;
        PlayStartAnimation();
        PlayIdleAnimation();
    }

    protected override void HandleAI()
    {
        if (player == null) return;
        float dist = distanceToPlayer;
        if (dist <= attackRange)
            state = BossState.Attack;
        else if (dist <= lookRadius)
            state = BossState.Chase;
        else
            state = BossState.Idle;

      
        switch (state)
        {
            case BossState.Idle:
                agent.isStopped = true;
                PlayIdleAnimation();
                attackInProgress = false; // reset attack when leaving attack range
                break;

            case BossState.Chase:
                agent.isStopped = false;
                agent.speed = WalkSpeed;
                agent.SetDestination(player.position);
                if (currentHealth < 50)
                { 
                PlayChargeAnimation();  
                }
                else
                { 
                PlayWalkAnimation();
                }
                attackInProgress = false;
                break;

            case BossState.Attack:
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
                agent.isStopped = true;
                PlayRageAnimation();
                attackInProgress = false;
                break;
        }
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

}
