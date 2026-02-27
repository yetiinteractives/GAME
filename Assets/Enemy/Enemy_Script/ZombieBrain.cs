using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ZombieBrain : UniversalEnemyAi, IDamageable
{
    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;
    private Collider mainCollider;

    private RagdollPhysicsHandler[] ragdollHandlers;

    [Header("Movement")]
    public float WalkSpeed = 3f;

    [Header("Detection & Combat")]
    public float lookRadius = 8f;
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float attackCooldown = 2f;

    [Header("Hit Reaction (Partial Ragdoll)")]
    [SerializeField] private float hitReactionDuration = 0.22f;
    [SerializeField] private float hitAgentStopTime = 0.15f; // pause nav briefly so it doesn't fight
    [SerializeField] private float partialMaxBodies = 1;      

    private float attackTimer = 0f;
    private bool attackInProgress = false;

    private EnemyShotKnockback knockback;

    // Death force cache
    private bool pendingDeathForce;
    private Collider pendingHitCollider;
    private Vector3 pendingHitPoint;
    private Vector3 pendingImpulse;

    // Avoid stacking many coroutines per zombie
    private Coroutine hitRoutine;
    private int activePartialBodies = 0;

    public enum ZombieState { Idle, Chase, Attack }
    public ZombieState state;

    public bool IsDead() => isDead;

    protected override void OnEnemyAwake()
    {
        state = ZombieState.Idle;

        mainCollider = GetComponent<Collider>();
        ragdollBodies = GetComponentsInChildren<Rigidbody>(true);
        ragdollColliders = GetComponentsInChildren<Collider>(true);
        ragdollHandlers = GetComponentsInChildren<RagdollPhysicsHandler>(true);

        knockback = GetComponent<EnemyShotKnockback>();


        SetAlivePhysicsState();
    }

    private void SetAlivePhysicsState()
    {
        // Disable physics simulation for bones
        foreach (var h in ragdollHandlers)
            h.DisableRagdoll();

        
        foreach (var col in ragdollColliders)
        {
            if (col == null) continue;
            if (col == mainCollider) continue;

            col.enabled = true;
            col.isTrigger = true;
        }

        if (mainCollider != null)
        {
            mainCollider.enabled = true;
            
        }
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
                    if (anim != null) anim.SetBool("Walk", false);
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

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
            Die();
        else
            knockback?.TriggerKnockback();
    }

    // Non-lethal reaction
    public void ApplyHitReaction(Collider hitCollider, Vector3 hitPoint, Vector3 impulse, float duration)
    {
        if (isDead) return;
        if (hitCollider == null) return;

        // Pick limb rigidbody
        Rigidbody limbRb = hitCollider.attachedRigidbody;
        if (limbRb == null) return;

        // Find handler for this rigidbody
        RagdollPhysicsHandler handler = limbRb.GetComponent<RagdollPhysicsHandler>();
        if (handler == null) return;

        // keep it stable: only allow N partial bodies at a time
        if (activePartialBodies >= partialMaxBodies)
            return;

        if (hitRoutine != null)
            StopCoroutine(hitRoutine);

        hitRoutine = StartCoroutine(HitReactionRoutine(handler, hitPoint, impulse, duration));
    }

    private IEnumerator HitReactionRoutine(RagdollPhysicsHandler handler, Vector3 hitPoint, Vector3 impulse, float duration)
    {
        activePartialBodies++;

        // Pause nav briefly to reduce fighting/jitter
        float originalSpeed = agent != null ? agent.speed : 0f;
        bool hadAgent = agent != null && agent.enabled;

        if (hadAgent)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero; // clears current movement impulse
        }

        // Temporarily enable physics on that limb only
        handler.EnablePartial();
        handler.Rigidbody.AddForceAtPosition(impulse, hitPoint, ForceMode.Impulse);
        handler.Rigidbody.WakeUp();

        // Keep it physical for a short time
        yield return new WaitForSeconds(duration);

        // Return limb to animation control
        handler.DisablePartial();

        // Resume nav
        if (hadAgent)
        {
            // small extra delay can help stability if you see jitter
            if (hitAgentStopTime > duration)
                yield return new WaitForSeconds(hitAgentStopTime - duration);

            agent.isStopped = false;
            agent.speed = originalSpeed;
        }

        activePartialBodies--;
        hitRoutine = null;
    }

    // Lethal force (ragdoll)
    public void ApplyDeathForce(Collider hitCollider, Vector3 hitPoint, Vector3 impulse)
    {
        if (!isDead)
        {
            pendingDeathForce = true;
            pendingHitCollider = hitCollider;
            pendingHitPoint = hitPoint;
            pendingImpulse = impulse;
            return;
        }

        ApplyDeathForceInternal(hitCollider, hitPoint, impulse);
    }

    protected override void HandleDeathVisuals()
    {
        if (hitRoutine != null)
        {
            StopCoroutine(hitRoutine);
            hitRoutine = null;
        }

        if (anim != null)
            anim.enabled = false;

        if (mainCollider != null)
            mainCollider.enabled = false;

        // Full ragdoll:
        foreach (var col in ragdollColliders)
        {
            if (col == null) continue;
            if (col == mainCollider) continue;

            col.enabled = true;
            col.isTrigger = false;

        }

        foreach (var h in ragdollHandlers)
            h.EnableRagdoll();

        if (pendingDeathForce)
        {
            pendingDeathForce = false;
            ApplyDeathForceInternal(pendingHitCollider, pendingHitPoint, pendingImpulse);
        }
    }

    protected override void OnEnemyDeath()
    {
        enabled = false;
    }

    private void ApplyDeathForceInternal(Collider hitCollider, Vector3 hitPoint, Vector3 impulse)
    {
        Rigidbody target = hitCollider != null ? hitCollider.attachedRigidbody : null;
        if (target == null)
            target = GetNearestActiveRagdollBody(hitPoint);

        if (target == null) return;

        target.AddForceAtPosition(impulse, hitPoint, ForceMode.Impulse);
        target.WakeUp();
    }

    private Rigidbody GetNearestActiveRagdollBody(Vector3 hitPoint)
    {
        Rigidbody nearest = null;
        float bestSqr = float.MaxValue;

        foreach (var h in ragdollHandlers)
        {
            if (h == null) continue;
            var rb = h.Rigidbody;
            if (rb == null) continue;
            if (rb.isKinematic) continue;

            float sqr = (rb.worldCenterOfMass - hitPoint).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                nearest = rb;
            }
        }

        return nearest;
    }
}