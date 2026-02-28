using UnityEngine;

/// <summary>
/// Zombie AI with split update:
///   • SlowAITick()  — called by AITickManager ~5×/sec, staggered across frames.
///                     Handles vision, distance checks, state decisions.
///   • FrameUpdate() — called every frame from Update().
///                     Handles smooth rotation, attack timing, investigation timers.
///   • NavMeshAgent  — path-following is automatic and smooth between slow ticks.
/// </summary>
public class ZombieBrain : UniversalEnemyAi, IDamageable, ISoundListener, ITickableAI
{
    // ──────────── Vision ────────────

    [Header("Vision")]
    public float fieldOfView = 120f;
    public float eyeHeight = 1.6f;

    [Tooltip("Layers that block vision raycasts (exclude zombie / ragdoll layers).")]
    [SerializeField] private LayerMask visionMask = ~0;

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

    // ──────────── Desynchronization ────────────

    [Header("Desync (applied once at spawn)")]
    [Tooltip("Max ± fraction applied to walkSpeed and agent.acceleration. 0.1 = ±10%.")]
    [SerializeField] private float speedVariation = 0.1f;

    [Tooltip("Max random delay (seconds) added before each attack animation.")]
    [SerializeField] private float maxAttackDelay = 0.3f;

    // Per-instance random values (computed once, zero per-frame cost)
    private float speedMultiplier;     // [1-variation .. 1+variation]
    private float attackWindUpDelay;   // [0 .. maxAttackDelay]
    private float animOffsetNormalized; // [0..1] random phase for animation desync

    // ──────────── State ────────────

    public enum ZombieState { Idle, Investigate, Chase, Attack }
    public ZombieState state;

    private bool attackInProgress;
    private float attackTimer;
    private bool attackWindUpDone;  // tracks whether the wind-up delay has elapsed

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

    // Squared-distance caches (computed once in Awake, zero per-frame cost)
    private float lookRadiusSqr;
    private float attackRangeSqr;
    private float arrivalThresholdSqr;

    public bool IsDead() => isDead;

    // ════════════════════════════════════════════════
    //  LIFECYCLE
    // ════════════════════════════════════════════════

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
        ApplyDesync();
    }

    /// <summary>
    /// Called once at spawn. Rolls per-instance random values for speed,
    /// attack delay, animation offset, and agent acceleration.
    /// All randomness is baked into fields — zero per-frame cost.
    /// </summary>
    private void ApplyDesync()
    {
        // ── Speed & acceleration variation ──
        speedMultiplier = Random.Range(1f - speedVariation, 1f + speedVariation);

        if (agent != null)
            agent.acceleration *= Random.Range(1f - speedVariation, 1f + speedVariation);

        // ── Attack wind-up delay (unique per zombie) ──
        attackWindUpDelay = Random.Range(0f, maxAttackDelay);

        // ── Animation phase offset ──
        // Play the current state at a random normalizedTime so walk/idle
        // cycles are not aligned across zombies sharing the same controller.
        animOffsetNormalized = Random.Range(0f, 1f);

        if (anim != null)
        {
            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
            anim.Play(info.fullPathHash, 0, animOffsetNormalized);

            // Slight animator speed jitter (±3%) makes even looping anims
            // drift apart over time without being visually noticeable.
            anim.speed = Random.Range(0.97f, 1.03f);
        }
    }

    private void OnEnable()
    {
        SoundManager.Instance?.Register(this);
        AITickManager.Instance?.Register(this);
    }

    private void OnDisable()
    {
        SoundManager.Instance?.Unregister(this);
        AITickManager.Instance?.Unregister(this);
    }

    // ════════════════════════════════════════════════
    //  UPDATE SPLIT
    // ════════════════════════════════════════════════

    /// <summary>
    /// Override base Update so HandleAI() is NOT called every frame.
    /// Only the cheap FrameUpdate runs per-frame.
    /// </summary>
    protected override void Update()
    {
        if (isDead) return;
        FrameUpdate();
    }

    /// <summary>
    /// Required by base class — intentionally empty.
    /// Logic is split between SlowAITick (decisions) and FrameUpdate (execution).
    /// </summary>
    protected override void HandleAI() { }

    /// <summary>
    /// Called by AITickManager ~5× per second, staggered so only a few zombies
    /// tick each frame. Contains ALL expensive work: vision raycast, distance
    /// calculations, state transitions, NavMesh destination updates.
    /// </summary>
    public void SlowAITick()
    {
        if (isDead || player == null || agent == null) return;
        EvaluateState();
    }

    // ════════════════════════════════════════════════
    //  SLOW TICK — DECISION MAKING  (~5×/sec)
    // ════════════════════════════════════════════════

    private void EvaluateState()
    {
        canSeePlayer = CheckVision();

        float sqrDist = (player.position - transform.position).sqrMagnitude;

        // Priority: Attack > Chase > Investigate > Idle
        ZombieState newState;

        if (sqrDist <= attackRangeSqr)
            newState = ZombieState.Attack;
        else if (canSeePlayer)
            newState = ZombieState.Chase;
        else if (hasInvestigateTarget)
            newState = ZombieState.Investigate;
        else
            newState = ZombieState.Idle;

        // Apply state change (setup happens once on transition)
        if (newState != state)
        {
            state = newState;
            OnStateEnter(newState);
        }

        // Refresh NavMesh destinations for moving states
        if (state == ZombieState.Chase)
            agent.SetDestination(player.position);
        else if (state == ZombieState.Investigate)
            agent.SetDestination(investigateTarget);
    }

    /// <summary>
    /// One-time setup when transitioning into a new state.
    /// Sets agent speed, stops/starts movement, and picks the right animation.
    /// </summary>
    private void OnStateEnter(ZombieState newState)
    {
        attackInProgress = false;
        attackWindUpDone = false;

        switch (newState)
        {
            case ZombieState.Idle:
                StopAgent();
                PlayIdleAnimation();
                break;

            case ZombieState.Investigate:
                investigateTimer = 0f;
                MoveAgent(GetInvestigateSpeed() * speedMultiplier);
                PlayWalkAnimation();
                break;

            case ZombieState.Chase:
                hasInvestigateTarget = false;
                MoveAgent(walkSpeed * speedMultiplier);
                PlayWalkAnimation();
                break;

            case ZombieState.Attack:
                hasInvestigateTarget = false;
                StopAgent();
                if (anim != null) anim.SetBool("Walk", false);
                break;
        }
    }

    // ════════════════════════════════════════════════
    //  FAST UPDATE — EXECUTION  (every frame)
    // ════════════════════════════════════════════════

    /// <summary>
    /// Runs every frame. Only cheap operations:
    /// smooth rotation, attack cooldown timer, investigation linger timer.
    /// NavMeshAgent movement is automatic — no per-frame cost here.
    /// </summary>
    private void FrameUpdate()
    {
        switch (state)
        {
            case ZombieState.Attack:
                FacePlayer();
                UpdateAttack();
                break;

            case ZombieState.Investigate:
                UpdateInvestigate();
                break;

            // Chase & Idle: NavMeshAgent + animations are already set by OnStateEnter.
            // Nothing expensive to do per-frame.
        }
    }

    private void UpdateAttack()
    {
        if (!attackInProgress)
        {
            attackInProgress = true;
            attackWindUpDone = false;
            attackTimer = 0f;
        }

        attackTimer += Time.deltaTime;

        // Random wind-up delay before the animation fires
        // (prevents all zombies attacking on the exact same frame)
        if (!attackWindUpDone && attackTimer >= attackWindUpDelay)
        {
            attackWindUpDone = true;
            PlayAttackAnimation();
        }

        if (attackTimer >= attackCooldown + attackWindUpDelay)
            attackInProgress = false;
    }

    private void UpdateInvestigate()
    {
        float sqrToTarget = (transform.position - investigateTarget).sqrMagnitude;

        if (sqrToTarget <= arrivalThresholdSqr)
        {
            // Arrived — linger at location
            StopAgent();
            PlayIdleAnimation();

            investigateTimer += Time.deltaTime;
            if (investigateTimer >= investigateLingerTime)
            {
                hasInvestigateTarget = false;
                state = ZombieState.Idle;
                PlayIdleAnimation();
            }
        }
    }

    // ════════════════════════════════════════════════
    //  VISION  (called inside SlowAITick only)
    // ════════════════════════════════════════════════

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

    // ════════════════════════════════════════════════
    //  ISoundListener
    // ════════════════════════════════════════════════

    public void HearSound(SoundStimulus stimulus)
    {
        if (isDead) return;
        if (canSeePlayer) return;

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

    private float GetInvestigateSpeed()
    {
        switch (investigateSoundType)
        {
            case SoundType.Gunshot:     return walkSpeed * investigateRunMultiplier;
            case SoundType.Explosion:   return walkSpeed * investigateRunMultiplier * 1.2f;
            case SoundType.Footstep:
            case SoundType.Reload:      return walkSpeed * 0.7f;
            case SoundType.Distraction: return walkSpeed * 0.5f;
            default:                    return walkSpeed;
        }
    }

    // ════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════

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

    // ════════════════════════════════════════════════
    //  IDamageable
    // ════════════════════════════════════════════════

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
            Die();
        else
            knockback?.TriggerKnockback();
    }

    // ════════════════════════════════════════════════
    //  DEATH & RAGDOLL
    // ════════════════════════════════════════════════

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

    // ════════════════════════════════════════════════
    //  ATTACK HITBOX
    // ════════════════════════════════════════════════

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

    // ════════════════════════════════════════════════
    //  GIZMOS
    // ════════════════════════════════════════════════

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lookRadius);

        Vector3 left = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, fieldOfView / 2, 0) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + left * lookRadius);
        Gizmos.DrawLine(transform.position, transform.position + right * lookRadius);

        if (hasInvestigateTarget)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(investigateTarget, 0.5f);
            Gizmos.DrawLine(transform.position, investigateTarget);
        }
    }
}