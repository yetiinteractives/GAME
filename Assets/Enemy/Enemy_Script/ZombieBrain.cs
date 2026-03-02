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
    [Header("Vision")]
    public float fieldOfView = 120f;
    public float eyeHeight = 1.6f;
    [SerializeField] private LayerMask visionMask = ~0;

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float investigateRunMultiplier = 1.5f;

    [Header("Detection & Combat")]
    public float lookRadius = 8f;
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float attackCooldown = 2f;
    public BoxCollider attackHitBox;

    [Header("Hearing")]
    [SerializeField] private float hearingSensitivity = 1f;
    [SerializeField] private float minAudibleLoudness = 0.05f;
    [SerializeField] private bool useSoundOcclusion = true;
    [SerializeField] private LayerMask soundOcclusionMask = ~0;
    [SerializeField] private float occludedLoudnessMultiplier = 0.5f;

    [Header("Hearing Ranges (base meters at loudness = 1)")]
    [SerializeField] private float distractionHearingRange = 6f;
    [SerializeField] private float footstepHearingRange = 10f;
    [SerializeField] private float objectBreakHearingRange = 14f;
    [SerializeField] private float reloadHearingRange = 12f;
    [SerializeField] private float gunshotHearingRange = 30f;
    [SerializeField] private float explosionHearingRange = 40f;

    [Header("Investigation")]
    public float investigateLingerTime = 3f;
    public float investigateArrivalThreshold = 1.5f;

    [Header("Desync (applied once at spawn)")]
    [SerializeField] private float speedVariation = 0.1f;
    [SerializeField] private float maxAttackDelay = 0.3f;
    [SerializeField] private float pathSpread = 3f;

    public enum ZombieState { Idle, Investigate, Chase, Attack }
    public ZombieState state;

    private float minDot;
    private bool canSeePlayer;

    private float speedMultiplier;
    private float attackWindUpDelay;
    private Vector3 pathOffset;

    private bool attackInProgress;
    private float attackTimer;
    private bool attackWindUpDone;

    private Vector3 investigateTarget;
    private SoundType investigateSoundType;
    private float investigateTimer;
    private bool hasInvestigateTarget;

    private EnemyShotKnockback knockback;
    private Collider mainCollider;
    private Collider[] allColliders;
    private RagdollPhysicsHandler[] ragdollHandlers;

    // Bullet pipeline deferral (IDamageable ApplyDeathForce)
    private bool pendingDeathImpulse;
    private Collider pendingHitCollider;
    private Vector3 pendingHitPoint;
    private Vector3 pendingImpulse;

    
    private bool pendingExplosionForce;
    private Vector3 pendingExplosionOrigin;
    private float pendingExplosionRadius;
    private float pendingExplosionForceValue;
    private float pendingExplosionUpward;
    private ForceMode pendingExplosionMode = ForceMode.Impulse;

    private float lookRadiusSqr;
    private float attackRangeSqr;
    private float arrivalThresholdSqr;

    private bool registeredWithTickManager;
    private float selfTickTimer;
    private const float SELF_TICK_INTERVAL = 0.2f;

    public bool IsDead() => isDead;

    protected override void OnEnemyAwake()
    {
        if (attackHitBox != null) attackHitBox.enabled = false;

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

        if (anim != null)
        {
            float animOffset = Random.Range(0f, 1f);
            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
            anim.Play(info.fullPathHash, 0, animOffset);
            anim.speed = Random.Range(0.97f, 1.03f);
        }
    }

    private void OnEnable() => TryRegisterWithManagers();

    private void OnDisable()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.Unregister(this);
        if (AITickManager.Instance != null) AITickManager.Instance.Unregister(this);
        registeredWithTickManager = false;
    }

    private void TryRegisterWithManagers()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.Register(this);

        if (AITickManager.Instance != null)
        {
            AITickManager.Instance.Register(this);
            registeredWithTickManager = true;
        }
    }

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

    private void EvaluateState()
    {
        canSeePlayer = CheckVision();
        float sqrDist = (player.position - transform.position).sqrMagnitude;

        ZombieState newState;
        if (sqrDist <= attackRangeSqr) newState = ZombieState.Attack;
        else if (canSeePlayer) newState = ZombieState.Chase;
        else if (hasInvestigateTarget) newState = ZombieState.Investigate;
        else newState = ZombieState.Idle;

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
        float fade = Mathf.Clamp01((sqrDist - attackRangeSqr) / Mathf.Max(0.0001f, (lookRadiusSqr - attackRangeSqr)));
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

    private bool CheckVision()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 direction = player.position - origin;

        float sqrDistance = direction.sqrMagnitude;
        if (sqrDistance > lookRadiusSqr) return false;

        float distance = Mathf.Sqrt(sqrDistance);
        direction /= Mathf.Max(distance, 0.0001f);

        if (Vector3.Dot(transform.forward, direction) < minDot) return false;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, lookRadius, visionMask, QueryTriggerInteraction.Ignore))
            return hit.transform == player || hit.transform.IsChildOf(player);

        return false;
    }

    public void HearSound(SoundStimulus stimulus)
    {
        if (isDead || canSeePlayer) return;
        if (!CanHearStimulus(stimulus)) return;

        bool shouldOverride = !hasInvestigateTarget ||
                              GetSoundPriority(stimulus.Type) > GetSoundPriority(investigateSoundType);

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




    public void QueueExplosionForce(Vector3 origin, float radius, float force, float upward, ForceMode mode)
    {
        pendingExplosionForce = true;
        pendingExplosionOrigin = origin;
        pendingExplosionRadius = radius;
        pendingExplosionForceValue = force;
        pendingExplosionUpward = upward;
        pendingExplosionMode = mode;
    }

    // ADD this private method:
    private void ApplyQueuedExplosionForce()
    {
        if (!pendingExplosionForce) return;
        pendingExplosionForce = false;

        for (int i = 0; i < ragdollHandlers.Length; i++)
        {
            var handler = ragdollHandlers[i];
            if (handler == null) continue;

            Rigidbody rb = handler.Rigidbody;
            if (rb == null || rb.isKinematic) continue;

            rb.AddExplosionForce(
                pendingExplosionForceValue,
                pendingExplosionOrigin,
                pendingExplosionRadius,
                pendingExplosionUpward,
                pendingExplosionMode
            );

            rb.WakeUp();
        }
    }
    // ===== IDamageable contract (UNCHANGED) =====
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
            Die();
        else
            knockback?.TriggerKnockback();
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

    /// <summary>
    /// Grenade-only helper (not interface): returns all ragdoll RBs ready for AddExplosionForce.
    /// </summary>
    public Rigidbody[] GetExplosionRagdollBodies()
    {
        if (!isDead)
        {
            currentHealth = 0f;
            Die();
        }

        EnableRagdoll();

        if (ragdollHandlers == null || ragdollHandlers.Length == 0)
            return System.Array.Empty<Rigidbody>();

        int count = 0;
        for (int i = 0; i < ragdollHandlers.Length; i++)
        {
            if (ragdollHandlers[i] != null && ragdollHandlers[i].Rigidbody != null)
                count++;
        }

        Rigidbody[] result = new Rigidbody[count];
        int idx = 0;

        for (int i = 0; i < ragdollHandlers.Length; i++)
        {
            var h = ragdollHandlers[i];
            if (h == null || h.Rigidbody == null) continue;

            Rigidbody rb = h.Rigidbody;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
            rb.detectCollisions = true;
            rb.WakeUp();

            result[idx++] = rb;
        }

        return result;
    }

    protected override void HandleDeathVisuals()
    {
        if (anim != null)
            anim.enabled = false;

        if (mainCollider != null)
            mainCollider.enabled = false;

        EnableRagdoll();

        // NEW: apply queued grenade blast right after ragdoll activation
        ApplyQueuedExplosionForce();

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