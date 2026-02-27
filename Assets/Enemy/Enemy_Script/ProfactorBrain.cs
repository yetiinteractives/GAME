using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class ProfactorBrain: UniversalEnemyAi, IDamageable
{
        public enum BossState { Idle, Chase, Attack, Spit,SpawnLarva ,Roar }
    public BossState state;
      
    bool attackchanger = true;
    bool attackInProgress = false;
    bool larvaSpawned = false;
    private int attackIndex = 0;
    private bool isAttacking = false;


    [Header("Movement")]
    public float WalkSpeed = 0f;

    [Header("Detection & Combat")]
    public float lookRadius = 40f;
    public float attackRange = 10f;
    public int attackDamage = 100;
    public float attackCooldown = 7f;
    public float playerTOOCloseRange = 7f;
    float attackTimer = 0f;

    [Header("Boss will enter rage mode at 50% health and spit its larva")]
    public GameObject larva;
    public Transform larvaSpawnPoint;

    public bool IsDead()
    {
        return isDead;
    }

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

         if (dist <= attackRange*attackRange){
           if((currentHealth==maxhealth* 0.5f ||dist <= playerTOOCloseRange*playerTOOCloseRange ) && !larvaSpawned){
            larvaSpawned=true;
           state = BossState.SpawnLarva;
           }
           else
            state = BossState.Attack;
        }
        else if (dist <= lookRadius*lookRadius)
            state = BossState.Chase;
        else
            state = BossState.Idle;

        if((state == BossState.Chase || state == BossState.Attack ) && dist> playerTOOCloseRange*playerTOOCloseRange)
             FacePlayer();
            

        


      
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
            if (isDead) break;

            agent.isStopped = true;

             if (!isAttacking)
              {
               StartCoroutine(AttackCycle());
              }
                break;
           
               case BossState.SpawnLarva:
                  if (isDead) break;

                   agent.isStopped = true;

                    if (!larvaSpawned)
              {
                      larvaSpawned = true;
                      StartCoroutine(SpawnLarvaRoutine());
              }
                      break;


        

            case BossState.Roar:
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
    IEnumerator SpawnLarvaRoutine()
{
    anim.SetTrigger("SpawnLarva");

    yield return new WaitForSeconds(1f); // wait for animation timing

    for (int i = 0; i < 10; i++)
    {
        Instantiate(larva, larvaSpawnPoint.position, larvaSpawnPoint.rotation);
    }

    yield return new WaitForSeconds(2f);

    state = BossState.Attack;
}

    IEnumerator AttackCycle()
    {
        isAttacking = true;
            switch (attackIndex)
    {
        case 0:
            PlayAttackAnimation();
            break;

        case 1:
            PlayAttack2Animation();
            break;

        case 2:
            anim.SetTrigger("Spit");
            break;

        case 3:
            anim.SetTrigger("Roar");
            break;
    }

    attackIndex = (attackIndex + 1) % 4;

    yield return new WaitForSeconds(attackCooldown);

    isAttacking = false;
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
       private void FacePlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // keep only horizontal rotation
        if (direction == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);
    }
    protected override void OnEnemyDeath()
    {
       //for loot and some items
    }


    public void ApplyDeathForce(Collider hitCollider, Vector3 hitPoint, Vector3 impulse)
    {
        // override interface
     }
}
