using UnityEngine;
using UnityEngine.AI;

public class ZombieBrain : UniversalEnemyAi
{
    public enum ZombieState { Idle, Chase, Attack, Rage }
    public ZombieState state;

  
    bool attackInProgress = false;
    bool StartChase = false;
    

    [Header("Movement")]
    public float WalkSpeed = 3f;

    [Header("Detection & Combat")]
    public float lookRadius = 8f;
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float attackCooldown = 2;
    public float rageCooldown = 2;
    float attackTimer = 0f;
    float rageTimer = 0f;
    protected override void OnEnemyAwake()
    {
        state = ZombieState.Idle;
       

    }

    protected override void HandleAI()
    {
        if (player == null) return;
            float dist = distanceToPlayer;
        if (dist <= attackRange)
            state = ZombieState.Attack;
        else if (dist <= lookRadius)
        {    
           
            if (StartChase) {
                state = ZombieState.Chase;
            }
            else
            {
                state = ZombieState.Rage;
            }
                

        }
        else
            state = ZombieState.Idle;


        switch (state)
        {
            case ZombieState.Idle:
                agent.isStopped = true;
                PlayIdleAnimation();
                attackInProgress = false;
                break;

            case ZombieState.Chase:
                anim.SetBool("Idle", false);
                agent.isStopped = false;
                    PlayWalkAnimation();
                agent.speed = WalkSpeed;
                agent.SetDestination(player.position);

                attackInProgress = false;
                break;

            case ZombieState.Attack:
                agent.isStopped = true;
                agent.speed = 0;
                anim.SetBool("Walk", false);
                if (!attackInProgress)
                {
                    attackInProgress = true;
                    
                    attackTimer = 0f;

                    PlayAttackAnimation();
                }

                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    attackInProgress = false;
                    
                }
                break;

            case ZombieState.Rage:
                agent.isStopped = true;
                agent.speed = 0;
                PlayRageAnimation();
                attackInProgress = false;
                rageTimer += Time.deltaTime;
                if(rageTimer >= rageCooldown)
                {
                    StartChase = true;
                }
                
                break;
        }
    }



    protected override void OnDamageTaken(int damage)
    {



    }

    protected override void OnEnemyDeath()
    {
        //for loot and some items
    }

}
