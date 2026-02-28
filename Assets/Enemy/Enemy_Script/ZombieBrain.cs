using UnityEngine;

public class ZombieBrain : UniversalEnemyAi, IDamageable, ISoundListener
{
    // ──────────── Vision ────────────

    [Header("Vision")]
    public float fieldOfView = 120f;
    public float eyeHeight = 1.6f;
    public float visionCheckInterval = 0.2f;

    [Tooltip("Layers that block vision raycasts (exclude zombie / ragdoll layers).")]
    [SerializeField] private LayerMask visionMask = ~0;

    private float visionTimer;
    private float minDot;
    private bool canSeePlayer;

    // ──────────── Movement ────────────

    [Header("Movement")]
    public float walkSpeed = 3f;

    [Tooltip("Speed multiplier when rushing toward a gunshot.")]
    public float investigateRunMultiplier = 1.5f;

    // ──────────── Detection & Combat ────────────

    [Header("Detection & Combat")]
    public float lookRadius = 8f;
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float attackCooldown = 2f;
    public BoxCollider attackHitBox;

    // ──────────── Investigation ────────────

    [Header("Investigation")]
    [Tooltip("How long the zombie lingers at the sound location before returning to idle.")]
    public float investigateLingerTime = 3f;

    [Tooltip("How close the zombie must get to the investigation point to consider it 'reached'.")]
    public float investigateArrivalThreshold = 1.5f;

    // ──────────── State ────────────

    public enum ZombieState { Idle, Investigate, Chase, Attack }
    public ZombieState state;

    private bool attackInProgress;
    private float attackTimer;

    // Investigation bookkeeping
    private Vector3 investigateTarget;
    private SoundType investigateSoundType;
    private float investigateTimer;
    private bool hasInvestigateTarget;

    // ──────────── Cached components ────────────

    private EnemyShotKnockback knockback;
    private Collider mainCollider;
    private Collider[] allColliders;
    private RagdollPhysicsHandler[] ragdollHandlers;

    // Death-force deferral
    private bool pendingDeathImpulse;
    private Collider pendingHitCollider;
    private Vector3 pendingHitPoint;
    private Vector3 pendingImpulse;

    // Squared-distance caches
    private float lookRadiusSqr;
    private float attackRangeSqr;
    private float arrivalThresholdSqr;

    public bool IsDead() => isDead;

    // ──────────── Lifecycle ────────────

    protected override void OnEnemyAwake()
    {
        if (attackHitBox != null)
            attackHitBox.enabled = false;

        state = ZombieState.Idle;

        minDot = Mathf.Cos(fieldOfView * 0.5f * Mathf.Deg2Rad);
        lookRadiusSqr = lookRadius * lookRadius;
        attackRangeSqr = attackRange * attackRange;
        arrivalThresholdSqr = investigateArrivalThreshold * investigateArrivalThreshold;

        mainCollider = GetComponent<Collider>();
        allColliders = GetComponentsInChildren<Collider>(true);
        ragdollHandlers = GetComponentsInChildren<RagdollPhysicsHandler>(true);
        knockback = GetComponent<EnemyShotKnockback>();

        DisableRagdoll();
    }

    private void OnEnable()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.Unregister(this);
    }

    // ──────────── Vision ────────────

    private bool CheckVision()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 direction = player.position - origin;

        float sqrDistance = direction.sqrMagnitude;
        if (sqrDistance > lookRadiusSqr)
            return false;

        float distance = Mathf.Sqrt(sqrDistance);
        direction /= distance;

        if (Vector3.Dot(transform.forward, direction) < minDot)
            return false;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, lookRadius, visionMask, QueryTriggerInteraction.Ignore))
            return hit.transform == player || hit.transform.IsChildOf(player);

        return false;
    }

    // ──────────── AI Core ────────────

    protected override void HandleAI()
    {
        if (player == null || agent == null) return;

        // Periodic vision check
        visionTimer += Time.deltaTime;
        if (visionTimer >= visionCheckInterval)
        {
            visionTimer = 0f;
            canSeePlayer = CheckVision();
        }

        // State transitions (highest priority first)
        float sqrDist = (player.position - transform.position).sqrMagnitude;

        if (sqrDist <= attackRangeSqr)
            state = ZombieState.Attack;
        else if (canSeePlayer)
            state = ZombieState.Chase;
        else if (hasInvestigateTarget)
            state = ZombieState.Investigate;
        else if (state != ZombieState.Investigate)
            state = ZombieState.Idle;

        // Execute current state
        switch (state)
        {
            case ZombieState.Idle:
                HandleIdle();
                break;
            case ZombieState.Investigate:
                HandleInvestigate();
                break;
            case ZombieState.Chase:
                HandleChase();
                break;
            case ZombieState.Attack:
                HandleAttack();
                break;
        }
    }

    // ──────────── State Handlers ────────────

    private void HandleIdle()
    {
        StopAgent();
        PlayIdleAnimation();
        attackInProgress = false;
    }

    private void HandleInvestigate()
    {
        attackInProgress = false;

        float speed = walkSpeed;

        // React differently based on what sound was heard
        switch (investigateSoundType)
        {
            case SoundType.Gunshot:
                // Rush toward gunshots at higher speed
                speed = walkSpeed * investigateRunMultiplier;
                break;

            case SoundType.Explosion:
                // Run even faster toward explosions
                speed = walkSpeed * investigateRunMultiplier * 1.2f;
                break;

            case SoundType.Footstep:
            case SoundType.Reload:
                // Walk cautiously
                speed = walkSpeed * 0.7f;
                break;

            case SoundType.Distraction:
                // Slow, curious approach
                speed = walkSpeed * 0.5f;
                break;

            case SoundType.ObjectBreak:
                speed = walkSpeed;
                break;
        }

        MoveAgent(speed);
        agent.SetDestination(investigateTarget);
        PlayWalkAnimation();

        // Check if arrived
        float sqrToTarget = (transform.position - investigateTarget).sqrMagnitude;
        if (sqrToTarget <= arrivalThresholdSqr)
        {
            // Linger at the location, look around
            StopAgent();
            PlayIdleAnimation();

            investigateTimer += Time.deltaTime;
            if (investigateTimer >= investigateLingerTime)
            {
                hasInvestigateTarget = false;
                state = ZombieState.Idle;
            }
        }
        else
        {
            investigateTimer = 0f;
        }
    }

    private void HandleChase()
    {
        hasInvestigateTarget = false; // seeing the player overrides investigation
        attackInProgress = false;

        MoveAgent(walkSpeed);
        agent.SetDestination(player.position);
        PlayWalkAnimation();
    }

    private void HandleAttack()
    {
        hasInvestigateTarget = false;
        FacePlayer();
        StopAgent();

        if (!attackInProgress)
        {
            if (anim != null) anim.SetBool("Walk", false);
            attackInProgress = true;
            PlayAttackAnimation();
            attackTimer = 0f;
        }

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
            attackInProgress = false;
    }

    // ──────────── ISoundListener ────────────

    public void HearSound(SoundStimulus stimulus)
    {
        if (isDead) return;

        // If the zombie can already see the player, ignore sounds
        if (canSeePlayer) return;

        // Accept investigation: louder / more urgent sounds override quieter ones
        bool shouldOverride = !hasInvestigateTarget
            || GetSoundPriority(stimulus.Type) > GetSoundPriority(investigateSoundType);

        if (shouldOverride)
        {
            investigateTarget = stimulus.Position;
            investigateSoundType = stimulus.Type;
            investigateTimer = 0f;
            hasInvestigateTarget = true;
        }
    }

    /// <summary>
    /// Higher value = higher priority. Gunshots and explosions take precedence.
    /// </summary>
    private static int GetSoundPriority(SoundType type)
    {
        switch (type)
        {
            case SoundType.Distraction:  return 1;
            case SoundType.Footstep:     return 2;
            case SoundType.ObjectBreak:  return 3;
            case SoundType.Reload:       return 4;
            case SoundType.Gunshot:      return 5;
            case SoundType.Explosion:    return 6;
            default:                     return 0;
        }
    }

    // ──────────── Helpers ────────────

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

    // ──────────── IDamageable ────────────

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
            Die();
        else
            knockback?.TriggerKnockback();
    }

    // ──────────── Death & Ragdoll ────────────

    protected override void HandleDeathVisuals()
    {
        if (anim != null)
            anim.enabled = false;

        if (mainCollider != null)
            mainCollider.enabled = false;

        EnableRagdoll();

        if (pendingDeathImpulse)
        {
            pendingDeathImpulse = false;
            ApplyDeathForceInternal(pendingHitCollider, pendingHitPoint, pendingImpulse);
        }
    }

    protected override void OnEnemyDeath()
    {
        enabled = false;
    }

    private void EnableRagdoll()
    {
        for (int i = 0; i < ragdollHandlers.Length; i++)
            ragdollHandlers[i].EnableRagdoll();

        SetCollidersRagdollState(isTrigger: false);

        if (attackHitBox != null)
            attackHitBox.enabled = false;
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

    public void ApplyDeathForce(Collider hitCollider, Vector3 hitPoint, Vector3 impulse)
    {
        if (!isDead)
        {
            pendingDeathImpulse = true;
            pendingHitCollider = hitCollider;
            pendingHitPoint = hitPoint;
            pendingImpulse = impulse;
            return;
        }

        ApplyDeathForceInternal(hitCollider, hitPoint, impulse);
    }

    private void ApplyDeathForceInternal(Collider hitCollider, Vector3 hitPoint, Vector3 impulse)
    {
        Rigidbody target = hitCollider != null ? hitCollider.attachedRigidbody : null;

        if (target == null || target.isKinematic)
            target = GetNearestActiveRagdollBody(hitPoint);

        if (target == null) return;

        target.AddForceAtPosition(impulse, hitPoint, ForceMode.Impulse);
        target.WakeUp();
    }

    private Rigidbody GetNearestActiveRagdollBody(Vector3 hitPoint)
    {
        Rigidbody nearest = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < ragdollHandlers.Length; i++)
        {
            var handler = ragdollHandlers[i];
            if (handler == null) continue;

            Rigidbody rb = handler.Rigidbody;
            if (rb == null || rb.isKinematic) continue;

            float sqr = (rb.worldCenterOfMass - hitPoint).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                nearest = rb;
            }
        }

        return nearest;
    }

    // ──────────── Attack Hitbox ────────────

    public void CreateHitBox()
    {
        if (attackHitBox != null)
            attackHitBox.enabled = true;
    }

    public void DisableHitBox()
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

    // ──────────── Gizmos ────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lookRadius);

        Vector3 left = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, fieldOfView / 2, 0) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + left * lookRadius);
        Gizmos.DrawLine(transform.position, transform.position + right * lookRadius);

        // Draw investigate target
        if (hasInvestigateTarget)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(investigateTarget, 0.5f);
            Gizmos.DrawLine(transform.position, investigateTarget);
        }
    }
}