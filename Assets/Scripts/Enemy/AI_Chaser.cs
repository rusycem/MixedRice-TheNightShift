using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AI_Chaser : MonoBehaviour
{
    public enum EnemyState
    {
        Idle = 0,
        Walk = 1,
        Chase = 2
    }

    [Header("Components")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Animator anim;

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float idleWaitTime = 3f;
    [SerializeField] private float lostPlayerIdleTime = 2f;

    private bool isWaiting = false;
    private int currentPointIndex = -1;

    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 10f;
    private bool isChasing = false;

    [Header("Events")]
    public GameEvent onPlayerDied;

    private MaskManager playerMask;

    void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!anim) anim = GetComponent<Animator>();

        if (playerTarget)
            playerMask = playerTarget.GetComponent<MaskManager>();

        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (!playerTarget) return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);
        bool canDetectPlayer = (playerMask == null || !playerMask.isMaskOn);

        if (distance < detectionRange && canDetectPlayer)
        {
            if (!isChasing)
            {
                StopAllCoroutines();
                isWaiting = false;
                isChasing = true;
            }

            ChasePlayer();
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                StartCoroutine(LostPlayerDelay());
            }

            Patrol();
        }
    }

    void SetState(EnemyState state)
    {
        anim.SetInteger("AnimState", (int)state);
    }

    void ChasePlayer()
    {
        agent.isStopped = false;
        agent.speed = 6f;
        agent.destination = playerTarget.position;

        SetState(EnemyState.Chase);
    }

    void Patrol()
    {
        if (isWaiting) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(WaitAtPoint());
        }
        else
        {
            SetState(EnemyState.Walk);
        }
    }

    IEnumerator WaitAtPoint()
    {
        isWaiting = true;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        SetState(EnemyState.Idle);

        yield return new WaitForSeconds(idleWaitTime);

        isWaiting = false;
        GoToNextPatrolPoint();
    }

    IEnumerator LostPlayerDelay()
    {
        isWaiting = true;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        SetState(EnemyState.Idle);

        yield return new WaitForSeconds(lostPlayerIdleTime);

        isWaiting = false;
        agent.speed = 3f;
        GoToNextPatrolPoint();
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        int newIndex = currentPointIndex;
        while (patrolPoints.Length > 1 && newIndex == currentPointIndex)
        {
            newIndex = Random.Range(0, patrolPoints.Length);
        }

        currentPointIndex = newIndex;

        agent.isStopped = false;
        agent.speed = 3f;
        agent.destination = patrolPoints[currentPointIndex].position;

        SetState(EnemyState.Walk);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bool playerIsMasked = playerMask != null && playerMask.isMaskOn;

            if (!playerIsMasked && onPlayerDied != null)
            {
                onPlayerDied.Raise();
            }
        }
    }
}
