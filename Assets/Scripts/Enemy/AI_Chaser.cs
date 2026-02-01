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

    [Header("Jumpscare Settings")]
    public GameObject jumpscarePanel; // Drag your UI Panel with the scary face here
    public AudioSource jumpscareAudio; // Drag an AudioSource with the scream sound
    public float recoveryTime = 2.0f; // How long before AI chases again

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float idleWaitTime = 3f;
    [SerializeField] private float lostPlayerIdleTime = 2f;

    private bool isWaiting = false;
    private int currentPointIndex = -1;
    private bool isAttacking = false; // Prevents rapid-fire hits

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
        Debug.Log($"AI touched object named: {other.name}");
        if (isAttacking) return; // Don't attack if already in the middle of a jumpscare

        if (other.CompareTag("Player"))
        {
            PlayerHealth healthScript = other.GetComponent<PlayerHealth>();

            if (healthScript != null)
            {
                // 1. Deal Damage (Logic handled in PlayerHealth.cs)
                healthScript.TakeDamage(1);

                // 2. Trigger the Sequence
                StartCoroutine(JumpscareSequence());
            }
        }
    }

    IEnumerator JumpscareSequence()
    {
        isAttacking = true;

        // STOP the AI immediately
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        SetState(EnemyState.Idle);

        // --- VISUALS ---
        if (jumpscarePanel) jumpscarePanel.SetActive(true);
        if (jumpscareAudio) jumpscareAudio.Play();

        // Wait for the scare to finish (e.g., 2 seconds)
        yield return new WaitForSeconds(recoveryTime);

        if (jumpscarePanel) jumpscarePanel.SetActive(false);

        // OPTION 1: TELEPORT AWAY (Give player breathing room)
        // ==========
        TeleportToFarthestPoint();
        // OPTION 2: FREEZE IN PLACE (Uncomment to use this instead)
        // ===============
        // // Just wait a bit longer while player runs, then resume
        // yield return new WaitForSeconds(3.0f); 
        // isAttacking = false;
        // agent.isStopped = false;
        // =======
    }

    void TeleportToFarthestPoint()
    {
        if (patrolPoints.Length == 0) return;

        Transform bestPoint = null;
        float maxDistance = 0f;

        // Loop through all points to find the one furthest from the player
        foreach (Transform point in patrolPoints)
        {
            float dist = Vector3.Distance(playerTarget.position, point.position);

            if (dist > maxDistance)
            {
                maxDistance = dist;
                bestPoint = point;
            }
        }

        if (bestPoint != null)
        {
            agent.Warp(bestPoint.position);
            Debug.Log($"AI retreated to {bestPoint.name} ({maxDistance}m away)");
        }

        // --- CRITICAL SAFETY STEP ---
        // Force the AI to be "blind" for 2 seconds after teleporting.
        // This prevents it from instantly spotting you if the map is small.
        StartCoroutine(TemporaryBlindness());
    }

    IEnumerator TemporaryBlindness()
    {
        // 1. Reset variables
        isAttacking = false;
        agent.isStopped = false;

        // 2. Reduce detection range to 0 temporarily
        float originalRange = detectionRange;
        detectionRange = 0f;

        // 3. Start walking to the next point immediately so it doesn't look frozen
        SetState(EnemyState.Walk);
        GoToNextPatrolPoint();

        // 4. Wait, then restore vision
        yield return new WaitForSeconds(3.0f);
        detectionRange = originalRange;
    }
}
