using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AI_Chaser : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Animator anim;

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float idleWaitTime = 3f;
    private bool isWaiting = false;
    private int currentPointIndex = -1;

    [Header("AI State")]
    public bool isChasing = false;
    [SerializeField] private float detectionRange = 10f;

    [Header("Events")]
    public GameEvent onPlayerDied;

    private MaskManager playerMask;

    void Start()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }

        if (playerTarget != null)
        {
            playerMask = playerTarget.GetComponent<MaskManager>();
        }
        
        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        bool canDetectPlayer = (playerMask == null || !playerMask.isMaskOn);

        if (distanceToPlayer < detectionRange && canDetectPlayer)
        {
            if (isWaiting) 
            {
                StopAllCoroutines();
                isWaiting = false;
            }
            
            isChasing = true;
            ChasePlayer();
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                agent.speed = 3f;
                GoToNextPatrolPoint(); 
            }
            Patrol();
        }
    }

    void ChasePlayer()
    {
        agent.isStopped = false;
        agent.speed = 6f;
        agent.destination = playerTarget.position;

        if (!anim.GetBool("isChasing"))
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalking", false);
            anim.SetBool("isChasing", true);
        }
    }

    void Patrol()
    {
        if (isWaiting) return;

        if (agent.isStopped) agent.isStopped = false;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(WaitAtPoint());
        }
    }

    IEnumerator WaitAtPoint()
    {
        isWaiting = true;
        agent.isStopped = true;

        anim.SetBool("isWalking", false);
        anim.SetBool("isChasing", false);
        anim.SetBool("isIdle", true);

        yield return new WaitForSeconds(idleWaitTime);

        isWaiting = false;
        GoToNextPatrolPoint();
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        if (patrolPoints.Length > 1)
        {
            int newIndex = currentPointIndex;
            while (newIndex == currentPointIndex)
            {
                newIndex = Random.Range(0, patrolPoints.Length);
            }
            currentPointIndex = newIndex;
        }
        else
        {
            currentPointIndex = 0;
        }

        agent.isStopped = false; 
        agent.destination = patrolPoints[currentPointIndex].position;

        anim.SetBool("isIdle", false);
        anim.SetBool("isChasing", false);
        anim.SetBool("isWalking", true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            bool playerIsMasked = (playerMask != null && playerMask.isMaskOn);

            if (!playerIsMasked && onPlayerDied != null)
            {
                onPlayerDied.Raise();
            }
            else if (playerIsMasked)
            {
                Debug.Log("Enemy bumped into masked player - ignoring.");
            }
        }
    }
}