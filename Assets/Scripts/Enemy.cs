using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : NetworkBehaviour
{
    public float patrolRadius = 10f;
    public float detectionRadius = 8f;
    public float patrolWaitTime = 3f;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private Transform player;
    private float patrolTimer;
    private bool playerInRange;
    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = 2.5f;

        startPosition = transform.position;
        player = FindFirstObjectByType<Character>()?.transform;

        // 👇 Get Animator from child
        animator = GetComponentInChildren<Animator>();

        GoToNewPatrolPoint();
    }

    void Update()
    {
        if (!IsServer) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        playerInRange = distanceToPlayer <= detectionRadius;

        if (playerInRange)
        {
            agent.speed = 4f;
            if (animator != null)
            {
                animator.SetFloat("LocomotionSpeed", agent.velocity.magnitude);
                animator.speed = 1f; // Run animation
            }

            agent.SetDestination(player.position);
        }
        else
        {
            agent.speed = 1.5f;
            if (animator != null)
            {
                animator.SetFloat("LocomotionSpeed", agent.velocity.magnitude);
                animator.speed = 0.5f; // Walk animation
            }

            patrolTimer += Time.deltaTime;

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (patrolTimer >= patrolWaitTime)
                {
                    GoToNewPatrolPoint();
                    patrolTimer = 0f;
                }
            }
        }
    }


    void GoToNewPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += startPosition;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}