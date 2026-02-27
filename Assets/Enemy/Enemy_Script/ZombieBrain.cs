using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.AI;

public class ZombieBrain : UniversalEnemyAi, IDamageable
{
    [Header("Vision")]
    public float fieldOfView = 120f;
    public float eyeHeight = 1.6f;
    public float visionCheckInterval = 0.2f;//same as human reaction time
    float visionTimer=0f;
    float minDot;

    bool canSeePlayer = false;

    bool CheckVision()
    {
        if(player == null) return false;

        Vector3 origin = transform.position+ Vector3.up *eyeHeight;
        Vector3 direction = player.position - origin;

        float sqrDistance = direction.sqrMagnitude;
        //distacne check
        if(sqrDistance > lookRadius * lookRadius)
            return false;

        float distance = Mathf.Sqrt(sqrDistance);
        direction /= distance; //normalizee
        
        float dot = Vector3.Dot(transform.forward, direction);
        
        //minDot chai onEnemyAwake vitra xa for optimization changes to avoid calculating cos every frame

        if(dot < minDot)
        return false;

        //line of sight 
        if(Physics.Raycast(origin , direction, out RaycastHit hit, lookRadius))
        {
            if(hit.transform == player)
             return true;
        }
        return false;

    }

    [Header("Movement")]
    public float WalkSpeed = 3f;

    [Header("Detection & Combat")]
    public float lookRadius = 8f;
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float attackCooldown = 2f;

    public enum ZombieState { Idle, Chase, Attack }
    public ZombieState state;

    private bool attackInProgress = false;
    private float attackTimer = 0f;

    private EnemyShotKnockback knockback;

    // Ragdoll system
    private Collider mainCollider;
    private Collider[] allColliders;
    private RagdollPhysicsHandler[] ragdollHandlers;

    // Cache lethal hit so we can apply it after ragdoll is enabled
    private bool pendingDeathImpulse;
    private Collider pendingHitCollider;
    private Vector3 pendingHitPoint;
    private Vector3 pendingImpulse;

    public bool IsDead() => isDead;
   
    protected override void OnEnemyAwake()
    {
        state = ZombieState.Idle;
          
                //angle check
        minDot = Mathf.Cos(fieldOfView * 0.5f * Mathf.Deg2Rad);

        mainCollider = GetComponent<Collider>();
        allColliders = GetComponentsInChildren<Collider>(true);

        ragdollHandlers = GetComponentsInChildren<RagdollPhysicsHandler>(true);
        knockback = GetComponent<EnemyShotKnockback>();

        DisableRagdoll(); // start animated
    }

    protected override void HandleAI()
    {
        if (player == null || agent == null) return;

        float sqrDist = (player.position - transform.position).sqrMagnitude;

        visionTimer += Time.deltaTime;
        if(visionTimer >= visionCheckInterval)
        {
            visionTimer =0f;
            canSeePlayer = CheckVision();

        }

        if (sqrDist <= attackRange * attackRange)
            state = ZombieState.Attack;
        else if (canSeePlayer)
            state = ZombieState.Chase;
        else
            state = ZombieState.Idle;

        if (state == ZombieState.Attack)
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
        agent.isStopped = true;
        agent.ResetPath();
    }

    private void MoveAgent(float speed)
    {
        agent.isStopped = false;
        agent.speed = speed;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            knockback?.TriggerKnockback();
        }
    }

    protected override void HandleDeathVisuals()
    {
        
        if (anim != null)
            anim.enabled = false;

        
        if (mainCollider != null)
            mainCollider.enabled = false;

        EnableRagdoll();

        // Apply cached lethal impulse after ragdoll is live
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

        
        for (int i = 0; i < allColliders.Length; i++)
        {
            Collider col = allColliders[i];
            if (col == null) continue;
            if (col == mainCollider) continue;
            col.enabled = true;
        }
    }

    private void DisableRagdoll()
    {
        
        for (int i = 0; i < ragdollHandlers.Length; i++)
            ragdollHandlers[i].DisableRagdoll();

       
        for (int i = 0; i < allColliders.Length; i++)
        {
            Collider col = allColliders[i];
            if (col == null) continue;
            if (col == mainCollider) continue;
            col.enabled = false;
        }

        if (mainCollider != null)
            mainCollider.enabled = true;
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
        Rigidbody target = null;

       
        if (hitCollider != null)
            target = hitCollider.attachedRigidbody;

        // Fallback: nearest ragdoll rigidbody (if collider had no RB)
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

      void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, lookRadius);

    Vector3 left = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward;
    Vector3 right = Quaternion.Euler(0, fieldOfView / 2, 0) * transform.forward;

    Gizmos.color = Color.red;
    Gizmos.DrawLine(transform.position, transform.position + left * lookRadius);
    Gizmos.DrawLine(transform.position, transform.position + right * lookRadius);
  }
}