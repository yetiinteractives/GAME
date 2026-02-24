using UnityEngine;
using UnityEngine.AI;

public class ZombieBrain : UniversalEnemyAi, IDamageable
{
    public enum ZombieState { Idle, Chase, Attack, Rage }
    public ZombieState state;

  
    bool attackInProgress = false;


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
        float sqrDist = distanceToPlayer;
        if (sqrDist <= attackRange*attackRange)
            state = ZombieState.Attack;
        else if (sqrDist <= lookRadius*lookRadius)
        {    
          state = ZombieState.Chase;
            
   

        }
        else
            state = ZombieState.Idle;

        if(state == ZombieState.Chase || state == ZombieState.Attack)
            FacePlayer();


        switch (state)
        {
            case ZombieState.Idle:
                if (isDead) break;
               StopAgent();
                PlayIdleAnimation();
                attackInProgress = false;
                break;

            case ZombieState.Chase:
                if(isDead) break;

                PlayWalkAnimation();
                MoveAgent(WalkSpeed);
                attackInProgress = false;
               if(!agent.hasPath || agent.destination != player.position)
                    agent.SetDestination(player.position);

               
                break;

            case ZombieState.Attack:
                if(isDead) break;
                StopAgent();
                if (!attackInProgress)
                {
                    anim.SetBool("Walk", false);
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
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }
    private void MoveAgent(float speed)
    {
        agent.isStopped = false;
        agent.speed = speed;

    }

     public void TakeDamage(int damage)
    {
         if (isDead) return;
        currentHealth -= damage;
         
        if (currentHealth <= 0)
        {

              Die(); 
        }
        else
        {
            EnemyShotKnockback.Instance.TriggerKnockback();
             

        }

         

         
    }
 

    protected override void OnEnemyDeath()
    {

        //for loot and some items
        deathHandler.PlayRandomDeath();
    }

}
