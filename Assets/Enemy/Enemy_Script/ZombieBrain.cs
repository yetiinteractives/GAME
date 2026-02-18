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

    ZombieDeathHandler deathHandler;

    protected override void OnEnemyAwake()
    {
        state = ZombieState.Idle;
        deathHandler = GetComponent<ZombieDeathHandler>();


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

        if(state == ZombieState.Chase || state == ZombieState.Attack)
            FacePlayer();


        switch (state)
        {
            case ZombieState.Idle:
               StopAgent();
                PlayIdleAnimation();
                attackInProgress = false;
                break;

            case ZombieState.Chase:

                MoveAgent(WalkSpeed);
                PlayWalkAnimation();
                attackInProgress = false;
                agent.SetDestination(player.position);

               
                break;

            case ZombieState.Attack:
                StopAgent();

                if (!attackInProgress)
                {
                    attackInProgress = true;
                    PlayAttackAnimation();
                    attackTimer = 0f;

                }

                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    attackInProgress = false;
                    //appy damage to player yeta
                }
                break;

            case ZombieState.Rage:
                StopAgent();
                PlayRageAnimation();
                attackInProgress = false;
                rageTimer += Time.deltaTime;
                if(rageTimer >= rageCooldown)
                {
                    StartChase = true;
                    rageTimer = 0;
                }
                
                break;
        }
    }

    private void FacePlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // keep only horizontal rotation
        if (direction == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);
    }
    private void StopAgent()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }
    private void MoveAgent(float speed)
    {
        agent.isStopped = false;
        agent.speed = speed;

    }


    protected override void OnDamageTaken(int damage)
    {



    }

    protected override void OnEnemyDeath()
    {
        //for loot and some items
        deathHandler.PlayRandomDeath();
    }

}
