using UnityEngine;

public class LarvaBrain : UniversalEnemyAi, IDamageable
{
    public enum BossState { Idle, Chase, Attack }
    public BossState state;

    private bool alternateAttack = true;
    private bool attackInProgress;

    [Header("Ragdoll")]
    private Collider mainCollider;
    private Collider[] allColliders;
    private RagdollPhysicsHandler[] ragdollHandlers;

    [Header("Movement")]
    public float walkSpeed = 3f;

    [Header("Detection & Combat")]
    public float lookRadius = 8f;
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float attackCooldown = 1.2f;
    public BoxCollider attackHitBox;

    private float attackTimer;
    private float lookRadiusSqr;
    private float attackRangeSqr;

    // ──────────── Lifecycle ────────────

    protected override void OnEnemyAwake()
    {
        // Cache squared distances to avoid per-frame multiplications
        lookRadiusSqr = lookRadius * lookRadius;
        attackRangeSqr = attackRange * attackRange;

        if (attackHitBox != null)
            attackHitBox.enabled = false;

        mainCollider = GetComponent<Collider>();
        allColliders = GetComponentsInChildren<Collider>(true);
        ragdollHandlers = GetComponentsInChildren<RagdollPhysicsHandler>(true);

        DisableRagdoll();

        state = BossState.Idle;
        PlayStartAnimation();
        PlayIdleAnimation();
    }

    // ──────────── AI ────────────

    protected override void HandleAI()
    {
        if (player == null) return;

        float dist = distanceToPlayer;

        if (dist <= attackRangeSqr)
            state = BossState.Attack;
        else if (dist <= lookRadiusSqr)
            state = BossState.Chase;
        else
            state = BossState.Idle;

        switch (state)
        {
            case BossState.Idle:
                agent.isStopped = true;
                PlayIdleAnimation();
                attackInProgress = false;
                break;

            case BossState.Chase:
                agent.isStopped = false;
                agent.speed = walkSpeed;
                agent.SetDestination(player.position);
                PlayWalkAnimation();
                attackInProgress = false;
                break;

            case BossState.Attack:
                agent.isStopped = true;

                if (!attackInProgress)
                {
                    attackInProgress = true;
                    attackTimer = 0f;

                    if (alternateAttack)
                        PlayAttackAnimation();
                    else
                        PlayAttack2Animation();
                }

                attackTimer += Time.deltaTime;

                if (attackTimer >= attackCooldown)
                {
                    attackInProgress = false;
                    alternateAttack = !alternateAttack;
                }
                break;
        }
    }

    // ──────────── IDamageable ────────────

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
            Die();
    }

    public bool IsDead() => isDead;

    // ──────────── Death ────────────

    protected override void OnEnemyDeath()
    {
        // TODO: loot / item drops
    }

    protected override void HandleDeathVisuals()
    {
        PlayDieAnimation();
    }

    public void OnDeathEnds()
    {
        if (anim != null)
            anim.enabled = false;

        if (agent != null)
            agent.enabled = false;

        if (mainCollider != null)
            mainCollider.enabled = false;

        EnableRagdoll();
    }

    public void ApplyDeathForce(Collider hitCollider, Vector3 hitPoint, Vector3 impulse)
    {
        // TODO: apply impulse to ragdoll bone
    }

    // ──────────── Attack Hitbox ────────────

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

    // ──────────── Ragdoll ────────────

    private void EnableRagdoll()
    {
        for (int i = 0; i < ragdollHandlers.Length; i++)
            ragdollHandlers[i].EnableRagdoll();

        SetCollidersRagdollState(isTrigger: false);
    }

    private void DisableRagdoll()
    {
        for (int i = 0; i < ragdollHandlers.Length; i++)
            ragdollHandlers[i].DisableRagdoll();

        SetCollidersRagdollState(isTrigger: true);

        if (mainCollider != null)
            mainCollider.enabled = true;
    }

    private void SetCollidersRagdollState(bool isTrigger)
    {
        for (int i = 0; i < allColliders.Length; i++)
        {
            Collider col = allColliders[i];
            if (col == null || col == mainCollider) continue;
            col.isTrigger = isTrigger;
            col.enabled = true;
        }
    }
}
