using UnityEngine;
using UnityEngine.AI;

public class BabyAlien : UniversalEnemyAi, IDamageable
{
    public EnemyCombatState enemyCombatState;
    public enum BAState
    {
        IdleWalk,
        Chase,
        Jump,
        Attack
    }
    public BAState State;

    [Header("Patrol")]
    public float patrolRadius = 10f;
    public float patrolPointTolerance = 0.5f;

    [Header("Jump")]
    public float jumpInterval = 4f;
    public float jumpDistance = 3f;
    public float jumpDuration = 0.6f;
    [Header("Movement")]
    public float walkSpeed = 3f;

    [Header("Detection & Combat")]
    public float lookRadius = 8f;
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float attackCooldown = 1.2f;
    public BoxCollider attackHitBox;

    private float lookRadiusSqr;
    private float attackRangeSqr;
    private float patrolRadiusSqr;
    private float jumpTimer;
    private float attackTimer;
    private bool attackInProgress;
    private bool jumpInProgress;
    private Vector3 spawnPosition;
    private Vector3 currentPatrolPoint;

    private EnemySoundController soundController;
    protected override void OnEnemyAwake()
    {
        // Cache squared distances to avoid per-frame multiplications
        lookRadiusSqr = lookRadius * lookRadius;
        attackRangeSqr = attackRange * attackRange;
        patrolRadiusSqr = patrolRadius * patrolRadius;

        if (attackHitBox != null)
            attackHitBox.enabled = false;
        soundController = GetComponent<EnemySoundController>();

        spawnPosition = transform.position;
        currentPatrolPoint = spawnPosition;
        jumpTimer = 0f;
        attackTimer = 0f;
        attackInProgress = false;
        jumpInProgress = false;

        State = BAState.IdleWalk;
        PlayStartAnimation();
        PlayIdleAnimation();
    }

    protected override void HandleAI()
    {
        if (player == null) return;

        float dist = distanceToPlayer;
        jumpTimer += Time.deltaTime;

        if (dist <= attackRangeSqr)
            State = BAState.Attack;
        else if (dist <= lookRadiusSqr)
            State = (jumpTimer >= jumpInterval && !jumpInProgress) ? BAState.Jump : BAState.Chase;
        else
            State = BAState.IdleWalk;

        switch (State)
        {
            case BAState.IdleWalk:
                enemyCombatState.SetCombatState(false);
                HandlePatrol();
                attackInProgress = false;
                jumpInProgress = false;
                break;

            case BAState.Chase:
                enemyCombatState.SetCombatState(true);
                HandleChase();
                attackInProgress = false;
                break;

            case BAState.Jump:
                HandleJump();
                break;

            case BAState.Attack:
                enemyCombatState.SetCombatState(true);
                HandleAttack();
                break;
        }
    }

    private void HandlePatrol()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return;

        agent.isStopped = false;
        agent.speed = walkSpeed;

        if (!agent.hasPath || Vector3.SqrMagnitude(agent.destination - transform.position) <= patrolPointTolerance * patrolPointTolerance)
        {
            if (TryGetPatrolPoint(out Vector3 point))
                currentPatrolPoint = point;

            agent.SetDestination(currentPatrolPoint);
        }

        PlayWalkAnimation();
        if (soundController != null) soundController.PlayIdleSound();
    }

    private void HandleChase()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return;

        agent.isStopped = false;
        agent.speed = walkSpeed;
        if (!agent.hasPath || agent.destination != player.position)
            agent.SetDestination(player.position);

        PlayWalkAnimation();
        if (soundController != null) soundController.PlayChaseSound();
    }

    private void HandleJump()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            State = BAState.Chase;
            return;
        }

        if (!jumpInProgress)
        {
            jumpInProgress = true;
            jumpTimer = 0f;
            PlayJumpAnimation();

            Vector3 jumpTarget = transform.position + transform.forward * jumpDistance;
            if (NavMesh.SamplePosition(jumpTarget, out NavMeshHit hit, jumpDistance, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.speed = jumpDistance / Mathf.Max(0.1f, jumpDuration);
                agent.SetDestination(hit.position);
            }
        }

        if (jumpTimer >= jumpDuration)
        {
            jumpInProgress = false;
            State = BAState.Chase;
        }
    }

    private void HandleAttack()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return;

        agent.isStopped = true;

        if (!attackInProgress)
        {
            attackInProgress = true;
            attackTimer = 0f;
            PlayAttackAnimation();
            if (soundController != null) soundController.PlayAttackSound();
        }

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
            attackInProgress = false;
    }

    private bool TryGetPatrolPoint(out Vector3 point)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            Vector3 candidate = spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                point = hit.position;
                return true;
            }
        }

        point = spawnPosition;
        return false;
    }

    private void PlayJumpAnimation()
    {
        if (anim == null) return;

        anim.SetBool("Idle", false);
        anim.SetBool("Walk", false);
        anim.SetTrigger("Jump");
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (soundController != null) soundController.PlayHurtSound();

        if (currentHealth <= 0f)
            Die();
    }

    public bool IsDead() => isDead;

    protected override void OnEnemyDeath()
    {
    }

    public void ApplyDeathForce(Collider hitCollider, Vector3 hitPoint, Vector3 impulse)
    {
    }
    public void CreateHitBox()
    {
        if (attackHitBox != null)
            attackHitBox.enabled = true;
    }

    public void DestroyHitBox()
    {
        if (attackHitBox != null)
            attackHitBox.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (attackHitBox == null || !attackHitBox.enabled) return;
        if (!other.CompareTag("Player")) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.TakeDamage(attackDamage);
    }
}
