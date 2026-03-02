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

    // ──────────── Hearing ────────────

    [Header("Hearing")]
    [Tooltip("Master multiplier applied to all hearing ranges.")]
    [SerializeField] private float hearingSensitivity = 1f;

    [Tooltip("Minimum loudness required to react to a sound.")]
    [SerializeField] private float minAudibleLoudness = 0.05f;

    [Tooltip("If true, hearing is reduced when line-of-sight to sound is blocked.")]
    [SerializeField] private bool useSoundOcclusion = true;

    [Tooltip("Layers that block sound raycasts.")]
    [SerializeField] private LayerMask soundOcclusionMask = ~0;

    [Tooltip("Multiplier applied to loudness when sound is occluded.")]
    [SerializeField] private float occludedLoudnessMultiplier = 0.5f;

    [Header("Hearing Ranges (base meters at loudness = 1)")]
    [SerializeField] private float distractionHearingRange = 6f;
    [SerializeField] private float footstepHearingRange = 10f;
    [SerializeField] private float objectBreakHearingRange = 14f;
    [SerializeField] private float reloadHearingRange = 12f;
    [SerializeField] private float gunshotHearingRange = 30f;
    [SerializeField] private float explosionHearingRange = 40f;

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

    [Tooltip("Max offset (meters) from the target position. Creates varied approach paths.")]
    [SerializeField] private float pathSpread = 3f;

    // Per-instance random values (computed once, zero per-frame cost)
    private float speedMultiplier;      // [1-variation .. 1+variation]
    private float attackWindUpDelay;    // [0 .. maxAttackDelay]
    private float animOffsetNormalized; // [0..1] random phase for animation desync
    private Vector3 pathOffset;         // unique XZ offset so zombies take different routes

    // ──────────── State ────────────

    public enum ZombieState { Idle, Investigate, Chase, Attack }
    public ZombieState state;

    private bool attackInProgress;
    private float attackTimer;
    private bool attackWindUpDone;

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

    // Death-force deferral (bullet point-force pipeline; required by IDamageable flow)
    private bool pendingDeathImpulse;
    private Collider pendingHitCollider;
    private Vector3 pendingHitPoint;
    private Vector3 pendingImpulse;

    // Squared-distance caches
    private float lookRadiusSqr;
    private float attackRangeSqr;
    private float arrivalThresholdSqr;

    // Self-tick fallback when AITickManager is absent
    private bool registeredWithTickManager;
    private float selfTickTimer;
    private const float SELF_TICK_INTERVAL = 0.2f;

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
        TryRegisterWithManagers();
    }

    private void ApplyDesync()
    {
        speedMultiplier = Random.Range(1f - speedVariation, 1f + speedVariation);

        if (agent != null)
            agent.acceleration *= Random.Range(1f - speedVariation, 1f + speedVariation);

        attackWindUpDelay = Random.Range(0f, maxAttackDelay);

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = Random.Range(0f, pathSpread);
        pathOffset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

        animOffsetNormalized = Random.Range(0f, 1f);

        if (anim != null)
        {
            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
            anim.Play(info.fullPathHash, 0, animOffsetNormalized);
            anim.speed = Random.Range(0.97f, 1.03f);
        }
    }

    private void OnEnable()
    {
        TryRegisterWithManagers();
    }

    private void OnDisable()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.Unregister(this);

        if (AITickManager.Instance != null)
            AITickManager.Instance.Unregister(this);

        registeredWithTickManager = false;
    }

    private void TryRegisterWithManagers()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.Register(this);

        if (AITickManager.Instance != null)
        {
            AITickManager.Instance.Register(this);
            registeredWithTickManager = true;
        }
    }

    // ════════════════════════════════════════════════
    //  UPDATE SPLIT
    // ════════════════════════════════════════════════

    protected override void Update()
    {
        if (isDead) return;

        if (!registeredWithTickManager)
        {
            selfTickTimer += Time.deltaTime;
            if (selfTickTimer >= SELF_TICK_INTERVAL)
            {
                selfTickTimer = 0f;
                SlowAITick();
            }
        }

        FrameUpdate();
    }

    protected override void HandleAI() { }

    public void SlowAITick()
    {
        if (isDead || player == null || agent == null) return;
        EvaluateState();
    }

    // ════════════════════════════════════════════════
    //  SLOW TICK — DECISION MAKING
    // ════════════════════════════════════════════════

    private void EvaluateState()
    {
        canSeePlayer = CheckVision();

        float sqrDist = (player.position - transform.position).sqrMagnitude;

        ZombieState newState;
        if (sqrDist <= attackRangeSqr)
            newState = ZombieState.Attack;
        else if (canSeePlayer)
            newState = ZombieState.Chase;
        else if (hasInvestigateTarget)
            newState = ZombieState.Investigate;
        else
            newState = ZombieState.Idle;

        if (newState != state)
        {
            state = newState;
            OnStateEnter(newState);
        }

        if (state == ZombieState.Chase)
            agent.SetDestination(GetOffsetDestination(player.position));
        else if (state == ZombieState.Investigate)
            agent.SetDestination(investigateTarget);
    }

    private Vector3 GetOffsetDestination(Vector3 target)
    {
        float sqrDist = (target - transform.position).sqrMagnitude;
        float denom = Mathf.Max(0.0001f, (lookRadiusSqr - attackRangeSqr));
        float fade = Mathf.Clamp01((sqrDist - attackRangeSqr) / denom);
        return target + pathOffset * fade;
    }

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
    //  FAST UPDATE — EXECUTION
    // ════════════════════════════════════════════════

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
    //  VISION
    // ══��═════════════════════════════════════════════

    private bool CheckVision()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 direction = player.position - origin;

        float sqrDistance = direction.sqrMagnitude;
        if (sqrDistance > lookRadiusSqr)
            return false;

        float distance = Mathf.Sqrt(sqrDistance);
        direction /= Mathf.Max(distance, 0.0001f);

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
        if (!CanHearStimulus(stimulus)) return;

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

    private bool CanHearStimulus(SoundStimulus stimulus)
    {
        float loudness = Mathf.Max(0f, stimulus.Loudness);
        if (loudness < minAudibleLoudness) return false;

        if (useSoundOcclusion && IsSoundOccluded(stimulus.Position))
            loudness *= occludedLoudnessMultiplier;

        if (loudness < minAudibleLoudness) return false;

        float baseRange = GetBaseHearingRange(stimulus.Type) * hearingSensitivity;
        float effectiveRange = baseRange * loudness;
        if (effectiveRange <= 0f) return false;

        float sqrDist = (stimulus.Position - transform.position).sqrMagnitude;
        return sqrDist <= effectiveRange * effectiveRange;
    }

    private bool IsSoundOccluded(Vector3 soundPosition)
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 toSound = soundPosition - origin;
        float distance = toSound.magnitude;

        if (distance <= 0.01f) return false;

        Vector3 direction = toSound / distance;
        return Physics.Raycast(origin, direction, distance, soundOcclusionMask, QueryTriggerInteraction.Ignore);
    }

    private float GetBaseHearingRange(SoundType type)
    {
        switch (type)
        {
            case SoundType.Distraction: return distractionHearingRange;
            case SoundType.Footstep: return footstepHearingRange;
            case SoundType.ObjectBreak: return objectBreakHearingRange;
            case SoundType.Reload: return reloadHearingRange;
            case SoundType.Gunshot: return gunshotHearingRange;
            case SoundType.Explosion: return explosionHearingRange;
            default: return footstepHearingRange;
        }
    }

    private static int GetSoundPriority(SoundType type)
    {
        switch (type)
        {
            case SoundType.Distraction: return 1;
            case SoundType.Footstep: return 2;
            case SoundType.ObjectBreak: return 3;
            case SoundType.Reload: return 4;
            case SoundType.Gunshot: return 5;
            case SoundType.Explosion: return 6;
            default: return 0;
        }
    }

    private float GetInvestigateSpeed()
    {
        switch (investigateSoundType)
        {
            case SoundType.Gunshot: return walkSpeed * investigateRunMultiplier;
            case SoundType.Explosion: return walkSpeed * investigateRunMultiplier * 1.2f;
            case SoundType.Footstep:
            case SoundType.Reload: return walkSpeed * 0.7f;
            case SoundType.Distraction: return walkSpeed * 0.5f;
            default: return walkSpeed;
        }
    }

    // ════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════

    private void FacePlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;
        if (direction == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);
    }

    private void StopAgent()
    {
        if (agent == null) return;
        agent.isStopped = true;
        agent.ResetPath();
    }

    private void MoveAgent(float speed)
    {
        if (agent == null) return;
        agent.isStopped = false;
        agent.speed = speed;
    }

    // ═══════════════���════════════════════════════════
    //  IDamageable
    // ════════════════════════════════════════════════

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
            Die();
        else
            knockback?.TriggerKnockback();
    }

    // Required by IDamageable: bullet/point-force pipeline
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

    // Grenade-only helper: true explosion force on ragdoll
    public void ApplyExplosionRagdollForce(
    Vector3 explosionOrigin,
    float explosionRadius,
    float explosionForce,
    float upwardsModifier,
    ForceMode mode = ForceMode.Impulse)
    {
        // Ensure dead first so ragdoll gets enabled by death pipeline
        if (!isDead)
        {
            currentHealth = 0f;
            Die();
        }

        // Safety
        EnableRagdoll();

        // Small nudge: if origin is exactly at torso, Unity sometimes gives weak-looking motion.
        // Slightly lower origin usually gives better lift/throw.
        Vector3 adjustedOrigin = explosionOrigin + Vector3.down * 0.15f;

        int appliedCount = 0;

        for (int i = 0; i < ragdollHandlers.Length; i++)
        {
            var handler = ragdollHandlers[i];
            if (handler == null) continue;

            Rigidbody rb = handler.Rigidbody;
            if (rb == null) continue;

            // Force activation even if some script left it kinematic
            if (rb.isKinematic) rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;

            // Reset sleeping / damped state
            rb.WakeUp();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Mass-aware scale so heavy bones still move
            float massScale = Mathf.Clamp(rb.mass, 0.5f, 5f);

            rb.AddExplosionForce(
                explosionForce * massScale,
                adjustedOrigin,
                explosionRadius,
                upwardsModifier,
                mode
            );

            appliedCount++;
        }

#if UNITY_EDITOR
        if (appliedCount == 0)
            Debug.LogWarning($"[ZombieBrain] Explosion force applied to 0 bodies on {name}");
#endif
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

        // Consume deferred bullet impulse, if one was queued pre-death
        if (pendingDeathImpulse)
        {
            pendingDeathImpulse = false;
            ApplyDeathForceInternal(pendingHitCollider, pendingHitPoint, pendingImpulse);
        }

        if (attackHitBox != null)
            attackHitBox.enabled = false;
    }

    protected override void OnEnemyDeath()
    {
        enabled = false;
    }

    private void EnableRagdoll()
    {
        for (int i = 0; i < ragdollHandlers.Length; i++)
            ragdollHandlers[i]?.EnableRagdoll();

        SetCollidersRagdollState(isTrigger: false);

        if (attackHitBox != null)
            attackHitBox.enabled = false;
    }

    private void DisableRagdoll()
    {
        for (int i = 0; i < ragdollHandlers.Length; i++)
            ragdollHandlers[i]?.DisableRagdoll();

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