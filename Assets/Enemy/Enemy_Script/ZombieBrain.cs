using UnityEngine;
using UnityEngine.AI;

public class ZombieBrain : UniversalEnemyAi, IDamageable
{
    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;
    private Collider mainCollider;

    public enum ZombieState { Idle, Chase, Attack }
    public ZombieState state;

    bool attackInProgress = false;

    [Header("Movement")]
    public float WalkSpeed = 3f;

    [Header("Detection & Combat")]
    public float lookRadius = 8f;
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float attackCooldown = 2f;

    float attackTimer = 0f;

    private EnemyShotKnockback knockback;

    protected override void OnEnemyAwake()
    {
        state = ZombieState.Idle;

        mainCollider = GetComponent<Collider>();
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        knockback = GetComponent<EnemyShotKnockback>();

        DisableRagdoll();
    }

    protected override void HandleAI()
    {
        if (player == null) return;

        float sqrDist = (player.position - transform.position).sqrMagnitude;

        if (sqrDist <= attackRange * attackRange)
            state = ZombieState.Attack;
        else if (sqrDist <= lookRadius * lookRadius)
            state = ZombieState.Chase;
        else
            state = ZombieState.Idle;

        if (state == ZombieState.Chase || state == ZombieState.Attack)
            FacePlayer();

        switch (state)
        {
            case ZombieState.Idle:
                StopAgent();
                PlayIdleAnimation();
                attackInProgress = false;
                break;

            case ZombieState.Chase:
                PlayWalkAnimation();
                MoveAgent(WalkSpeed);
                attackInProgress = false;

                if (!agent.hasPath || agent.destination != player.position)
                    agent.SetDestination(player.position);
                break;

            case ZombieState.Attack:
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
                    attackInProgress = false;
                break;
        }
    }

    private void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        if (direction == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);
    }

    private void StopAgent()
    {
        agent.isStopped = true;
        agent.ResetPath();
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
            Die();
        else
        {
            if (knockback != null)
            {
                knockback.TriggerKnockback();
            }
        }
    }

    
    protected override void HandleDeathVisuals()
    {
        anim.enabled = false;
        mainCollider.enabled = false;

        EnableRagdoll();
    }

    protected override void OnEnemyDeath()
    {
        enabled = false;
    }

    void EnableRagdoll()
    {
        foreach (Rigidbody rb in ragdollBodies)
            rb.isKinematic = false;

        foreach (Collider col in ragdollColliders)
            col.enabled = true;
    }

    void DisableRagdoll()
    {
        foreach (Rigidbody rb in ragdollBodies)
            rb.isKinematic = true;

        foreach (Collider col in ragdollColliders)
        {
            if (col != mainCollider)
                col.enabled = false;
        }
    }
}