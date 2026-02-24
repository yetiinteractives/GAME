
using UnityEngine;
using UnityEngine.AI;

public class AlienZombieBrain : UniversalEnemyAi, IDamageable
{
    public enum AlienZombieState { Idle, Chase, Attack}
    public AlienZombieState state;

  
    bool attackInProgress = false;
   
     

    [Header("Movement")]
    public float WalkSpeed = 2f;

    [Header("Detection & Combat")]
    public float teleportRange = 20f;
    public float lookRadius = 30f;
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float attackCooldown = 2;
    float attackTimer = 0f;

    protected override void OnEnemyAwake()
    {
        state = AlienZombieState.Idle;
       

    }

    protected override void HandleAI()
    {
        if (player == null) return;
        float dist = distanceToPlayer;
        if (dist <= attackRange*attackRange)
            state = AlienZombieState.Attack;
        else if (dist <= lookRadius*lookRadius)
        {    
          state = AlienZombieState.Chase;
            
   

        }
        else
            state = AlienZombieState.Idle;

        if(state == AlienZombieState.Chase || state == AlienZombieState.Attack)
            FacePlayer();


        switch (state)
        {
            case AlienZombieState.Idle:
               StopAgent();
                PlayIdleAnimation();
                attackInProgress = false;
                break;

            case AlienZombieState.Chase:

                PlayWalkAnimation();
                MoveAgent(WalkSpeed);
                attackInProgress = false;
                if (!agent.hasPath || agent.destination != player.position)
                    agent.SetDestination(player.position);


               
                break;

            case AlienZombieState.Attack:
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
            Teleport();
        }
         

         
    }
     public void Teleport()
    {
        Vector3 randomDirection = Random.insideUnitSphere * teleportRange;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, teleportRange, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            Debug.Log("EnderSoldier teleported to: " + hit.position);
        }
    }

    protected override void OnEnemyDeath()
    {
        //for loot and some items
    }

}
