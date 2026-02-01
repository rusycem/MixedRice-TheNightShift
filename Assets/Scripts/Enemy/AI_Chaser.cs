using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AI_Chaser : MonoBehaviour
{
    public enum EnemyState { Idle = 0, Walk = 1, Chase = 2 }

    [Header("Components")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Animator anim;

    [Header("Footsteps (RandomSoundPlayer)")]
    public RandomSoundPlayer stepPlayer; // Drag the RandomSoundPlayer component here
    public float walkStepInterval = 0.6f;
    public float runStepInterval = 0.35f;
    private float nextStepTime;

    [Header("Detection Settings")]
    public bool useRadiusDetection = true;
    public float detectionRadius = 10f;

    public bool useConeVision = true;
    public float visionRadius = 15f;
    [Range(0, 360)] public float viewAngle = 90f;
    public LayerMask obstacleMask;

    [Header("Detection Alert")]
    public AudioSource alertAudio;
    public GameObject alertUI;
    public float alertDuration = 2f;
    public float chaseStartDelay = 1.0f;
    public float chaseSpeed = 4.0f;

    [Header("Jumpscare Settings")]
    public GameObject jumpscarePanel;
    public AudioSource jumpscareAudio;
    public float jumpscareDuration = 2.0f;

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float idleWaitTime = 3f;

    private bool isWaiting = false;
    private int currentPointIndex = -1;
    private bool isAttacking = false;
    private MaskManager playerMask;
    private bool hasAlerted = false;

    // Events
    public GameEvent onPlayerDied;

    void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!anim) anim = GetComponent<Animator>();
        if (playerTarget) playerMask = playerTarget.GetComponentInParent<MaskManager>();
        if (obstacleMask == 0) obstacleMask = LayerMask.GetMask("Default");

        // Ensure alert audio is 3D
        if (alertAudio) alertAudio.spatialBlend = 1.0f;

        if (anim) anim.applyRootMotion = false;

        // Snap to NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        GoToNextPatrolPoint();
    }

    void Update()
    {
        HandleFootsteps();

        if (isAttacking) return;

        bool playerDetected = false;

        if (playerTarget != null)
        {
            bool isMaskSafe = (playerMask != null && playerMask.isMaskOn);
            if (!isMaskSafe)
            {
                if (useRadiusDetection && CheckHearing()) playerDetected = true;
                else if (useConeVision && CheckSight()) playerDetected = true;
            }
        }

        if (playerDetected)
        {
            if (!hasAlerted)
            {
                StopAllCoroutines();
                isWaiting = false;
                StartCoroutine(ChasePlayerRoutine());
            }
            else
            {
                agent.destination = playerTarget.position;
            }
        }
        else
        {
            Patrol();
        }
    }

    void HandleFootsteps()
    {
        // 1. Only play steps if moving and not in a sequence
        if (agent.velocity.magnitude > 0.2f && !isWaiting && !isAttacking)
        {
            // 2. Determine interval based on current speed
            // If speed is high (chasing), use run interval, otherwise use walk interval
            float currentInterval = (agent.speed > 3.5f) ? runStepInterval : walkStepInterval;

            // 3. Play sound using the RandomSoundPlayer script
            if (Time.time >= nextStepTime)
            {
                if (stepPlayer != null)
                {
                    stepPlayer.Play();
                }

                nextStepTime = Time.time + currentInterval;
            }
        }
    }

    // ================= SENSORY LOGIC =================
    bool CheckHearing()
    {
        return Vector3.Distance(transform.position, playerTarget.position) < detectionRadius;
    }

    bool CheckSight()
    {
        Vector3 dirToPlayer = (playerTarget.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distToPlayer > visionRadius) return false;

        if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
        {
            Vector3 eyePos = transform.position + Vector3.up * 1.6f;
            Vector3 targetPos = playerTarget.position + Vector3.up * 1.0f;

            if (!Physics.Linecast(eyePos, targetPos, obstacleMask)) return true;
        }
        return false;
    }

    // ================= MOVEMENT LOGIC =================
    IEnumerator ChasePlayerRoutine()
    {
        hasAlerted = true;

        if (alertAudio) alertAudio.Play();
        if (alertUI)
        {
            alertUI.SetActive(true);
            StartCoroutine(HideAlertUI());
        }

        agent.isStopped = true;
        SetState(EnemyState.Idle);

        yield return new WaitForSeconds(chaseStartDelay);

        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.destination = playerTarget.position;
        SetState(EnemyState.Chase);
    }

    IEnumerator HideAlertUI()
    {
        yield return new WaitForSeconds(alertDuration);
        if (alertUI) alertUI.SetActive(false);
    }

    void Patrol()
    {
        if (hasAlerted)
        {
            hasAlerted = false;
            if (alertUI) alertUI.SetActive(false);
        }

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

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        if (!agent.isOnNavMesh) return;

        int newIndex = currentPointIndex;
        while (patrolPoints.Length > 1 && newIndex == currentPointIndex)
            newIndex = Random.Range(0, patrolPoints.Length);

        currentPointIndex = newIndex;
        agent.isStopped = false;
        agent.speed = 3f; // Walking speed
        agent.destination = patrolPoints[currentPointIndex].position;
        SetState(EnemyState.Walk);
    }

    // ================= COLLISION & JUMPSCARE =================
    private void OnTriggerEnter(Collider other)
    {
        if (isAttacking) return;

        if (other.CompareTag("Player"))
        {
            MaskManager maskCheck = other.GetComponentInParent<MaskManager>();
            if (maskCheck != null && maskCheck.isMaskOn) return;

            PlayerHealth healthScript = other.GetComponentInParent<PlayerHealth>();
            if (healthScript != null)
            {
                healthScript.TakeDamage(1);
                StartCoroutine(JumpscareSequence());
            }
        }
    }

    IEnumerator JumpscareSequence()
    {
        isAttacking = true;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        SetState(EnemyState.Idle);

        if (playerTarget != null)
        {
            Vector3 forwardPoint = playerTarget.position + (playerTarget.forward * 1.5f);
            NavMeshHit hit;
            Vector3 finalPos = forwardPoint;

            if (NavMesh.SamplePosition(forwardPoint, out hit, 2.0f, NavMesh.AllAreas))
                finalPos = hit.position;
            else
                finalPos = playerTarget.position;

            agent.Warp(finalPos);
            transform.LookAt(playerTarget);

            Vector3 directionToEnemy = transform.position + Vector3.up * 1.5f - playerTarget.position;
            playerTarget.rotation = Quaternion.LookRotation(directionToEnemy);
        }

        if (jumpscarePanel) jumpscarePanel.SetActive(true);
        if (jumpscareAudio) jumpscareAudio.Play();

        yield return new WaitForSeconds(jumpscareDuration);

        if (jumpscarePanel) jumpscarePanel.SetActive(false);
        TeleportToFarthestPoint();
    }

    void TeleportToFarthestPoint()
    {
        if (patrolPoints.Length == 0) return;

        Transform bestPoint = null;
        float maxDist = 0f;

        foreach (Transform point in patrolPoints)
        {
            float dist = Vector3.Distance(playerTarget.position, point.position);
            if (dist > maxDist)
            {
                maxDist = dist;
                bestPoint = point;
            }
        }

        if (bestPoint != null)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(bestPoint.position, out hit, 5.0f, NavMesh.AllAreas))
                agent.Warp(hit.position);
            else
                agent.Warp(bestPoint.position);
        }

        StartCoroutine(TemporaryBlindness());
    }

    IEnumerator TemporaryBlindness()
    {
        isAttacking = false;
        agent.isStopped = false;
        float originalHearing = detectionRadius;
        float originalVision = visionRadius;

        detectionRadius = 0f;
        visionRadius = 0f;

        SetState(EnemyState.Walk);
        GoToNextPatrolPoint();

        yield return new WaitForSeconds(3.0f);

        detectionRadius = originalHearing;
        visionRadius = originalVision;
    }

    void OnDrawGizmosSelected()
    {
        if (useRadiusDetection)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }

        if (useConeVision)
        {
            Gizmos.color = Color.yellow;
            Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
            Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);
            Gizmos.DrawLine(transform.position, transform.position + viewAngleA * visionRadius);
            Gizmos.DrawLine(transform.position, transform.position + viewAngleB * visionRadius);
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    void SetState(EnemyState state) { anim.SetInteger("AnimState", (int)state); }
}