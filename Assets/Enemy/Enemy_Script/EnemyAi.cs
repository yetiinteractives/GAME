using UnityEngine;                                // (1)
using System.Collections;                         // (2)

public class EnemyAI : MonoBehaviour               // (3)
{
    public enum State { Patrol, Chase, Attack }    // (4)

    [Header("References")]
    public Transform[] patrolPoints;               // (5)
    public Transform player;                       // (6) // optional: auto-find if left empty

    [Header("Movement")]
    public float patrolSpeed = 2f;                 // (7)
    public float chaseSpeed = 4f;                  // (8)
    public float waypointTolerance = 0.5f;         // (9)

    [Header("Detection & Combat")]
    public float lookRadius = 8f;                  // (10)
    public float attackRange = 1.5f;               // (11)
    public int attackDamage = 10;                  // (12)
    public float attackCooldown = 1.2f;            // (13)

    private int currentPatrolIndex = 0;            // (14)
    private float lastAttackTime = -999f;          // (15)
    private State state = State.Patrol;            // (16)

    void Start()                                  // (17)
    {
        if (player == null)                       // (18)
        {
            var p = GameObject.FindGameObjectWithTag("Player"); // (19)
            if (p != null) player = p.transform;  // (20)
        }

        if (patrolPoints == null || patrolPoints.Length == 0) // (21)
            state = State.Chase;                  // (22) // no patrol points -> default to chasing
    }

    void Update()                                 // (23)
    {
        if (player == null)                       // (24)
        {
            Patrol();                             // (25)
            return;                               // (26)
        }

        float dist = Vector3.Distance(player.position, transform.position); // (27)

        // state transitions
        if (dist <= attackRange) state = State.Attack;      // (28)
        else if (dist <= lookRadius) state = State.Chase;   // (29)
        else state = (patrolPoints != null && patrolPoints.Length > 0) ? State.Patrol : State.Chase; // (30)

        // perform behaviour for current state
        switch (state)                                    // (31)
        {
            case State.Patrol: Patrol(); break;           // (32)
            case State.Chase: Chase(); break;             // (33)
            case State.Attack: Attack(); break;           // (34)
        }
    }

    void Patrol()                                     // (35)
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return; // (36)

        Transform wp = patrolPoints[currentPatrolIndex]; // (37)
        Vector3 targetPos = new Vector3(wp.position.x, transform.position.y, wp.position.z); // (38)
        MoveTowards(targetPos, patrolSpeed);            // (39)

        if (Vector3.Distance(transform.position, targetPos) <= waypointTolerance) // (40)
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;  // (41)
    }

    void Chase()                                      // (42)
    {
        Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z); // (43)
        MoveTowards(targetPos, chaseSpeed);            // (44)
        RotateTowards(player.position);                // (45)
    }

    void Attack()                                     // (46)
    {
        RotateTowards(player.position);
         if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
        }
    }

    void MoveTowards(Vector3 targetPos, float speed)  // (52)
    {
        Vector3 dir = (targetPos - transform.position).normalized; // (53)
        transform.position += dir * speed * Time.deltaTime;       // (54)
        if (dir != Vector3.zero)                                  // (55)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
    }

    void RotateTowards(Vector3 worldPoint)            // (56)
    {
        Vector3 lookDir = (new Vector3(worldPoint.x, transform.position.y, worldPoint.z) - transform.position).normalized; // (57)
        if (lookDir.sqrMagnitude == 0) return;          // (58)
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), 10f * Time.deltaTime); // (59)
    }

    void OnDrawGizmosSelected()                       // (60)
    {
        Gizmos.color = Color.yellow;                   // (61)
        Gizmos.DrawWireSphere(transform.position, lookRadius); // (62)
        Gizmos.color = Color.red;                      // (63)
        Gizmos.DrawWireSphere(transform.position, attackRange); // (64)
    }
}
