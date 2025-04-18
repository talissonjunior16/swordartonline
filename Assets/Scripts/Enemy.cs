using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NetworkObject))]
public class Enemy : NetworkBehaviour
{
    public float patrolRadius = 10f;
    public float detectionRadius = 8f;
    public float patrolWaitTime = 3f;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private Transform targetPlayer;
    private float patrolTimer;
    private Animator animator;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) return; // AI logic only runs on the server

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        agent.stoppingDistance = 2.5f;

        startPosition = transform.position;

        GoToNewPatrolPoint();
    }

    private void Update()
    {
        if (!IsServer) return;

        if (targetPlayer == null)
        {
            var player = FindClosestPlayer();
            if (player != null)
                targetPlayer = player.transform;
            else
                return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);
        bool playerInRange = distanceToPlayer <= detectionRadius;

        if (playerInRange)
        {
            agent.speed = 4f;
            agent.SetDestination(targetPlayer.position);
            UpdateAnimation(agent.velocity.magnitude, 1f);
        }
        else
        {
            agent.speed = 1.5f;
            UpdateAnimation(agent.velocity.magnitude, 0.5f);

            patrolTimer += Time.deltaTime;
            if (agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (patrolTimer >= patrolWaitTime)
                {
                    GoToNewPatrolPoint();
                    patrolTimer = 0f;
                }
            }
        }
    }

    private void GoToNewPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += startPosition;
        randomDirection.y = 0f;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void UpdateAnimation(float moveSpeed, float animSpeed)
    {
        if (animator != null)
        {
            animator.SetFloat("LocomotionSpeed", moveSpeed); // will sync via NetworkAnimator
            animator.speed = animSpeed;
        }
    }

    private Transform FindClosestPlayer()
    {
        float closestDist = float.MaxValue;
        Character closest = null;

        foreach (var character in FindObjectsByType<Character>(FindObjectsSortMode.None))
        {
            float dist = Vector3.Distance(transform.position, character.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = character;
            }
        }

        return closest?.transform;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
