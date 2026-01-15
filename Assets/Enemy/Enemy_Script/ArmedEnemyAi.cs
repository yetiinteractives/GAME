using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class ArmedEnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack }

    [Header("References")]
    public Transform[] patrolPoints;
    public Transform player;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float waypointTolerance = 0.5f;

    [Header("Detection & Combat")]
    public EnemyShooting enemyShooting;
    public float stoppingDistance = 5f;

    public float lookRadius = 8f;
    public float attackRange = 10f;
    public int attackDamage = 10;
    public float attackCooldown = 1.2f;

    private int currentPatrolIndex = 0;
    private float lastAttackTime = -999f;
    private State state = State.Patrol;

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
            state = State.Chase;
    }

    void Update()
    {
        if (player == null)
        {
            Patrol();
            return;
        }

        float dist = Vector3.Distance(player.position, transform.position);

        // state transitions ko lagi
        if (dist <= stoppingDistance) state = State.Attack;
        else if (dist <= attackRange) state = State.Attack;
        else if (dist <= lookRadius) state = State.Chase;
        else state = (patrolPoints != null && patrolPoints.Length > 0) ? State.Patrol : State.Chase;

        //current state ko lagi behaviour
        switch (state)
        {
            case State.Patrol: Patrol(); break;
            case State.Chase: Chase(); break;
            case State.Attack: Attack(); break;
        }
    }

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Transform wp = patrolPoints[currentPatrolIndex];
        Vector3 targetPos = new Vector3(wp.position.x, transform.position.y, wp.position.z);
        MoveTowards(targetPos, patrolSpeed);

        if (Vector3.Distance(transform.position, targetPos) <= waypointTolerance)
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void Chase()
    {
        Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > stoppingDistance)
        {
            MoveTowards(targetPos, chaseSpeed);
        }
        RotateTowards(player.position);
    }

    void Attack()
    {
        if (player == null || enemyShooting == null) return;
        RotateTowards(player.position);
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;

            Debug.Log("Enemy shoots!");
            enemyShooting.Shoot(player);
        }
    }

    void MoveTowards(Vector3 targetPos, float speed)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
    }

    void RotateTowards(Vector3 worldPoint)
    {
        Vector3 lookDir = (new Vector3(worldPoint.x, transform.position.y, worldPoint.z) - transform.position).normalized;
        if (lookDir.sqrMagnitude == 0) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), 10f * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
