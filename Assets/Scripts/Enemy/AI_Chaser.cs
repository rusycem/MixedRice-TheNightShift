using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AI_Chaser : MonoBehaviour
{
    public enum EnemyState { Idle = 0, Walk = 1, Chase = 2 }

    [Header("Components")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Animator anim;

    [Header("Detection Settings")]
    public bool useRadiusDetection = true;
    public float detectionRadius = 10f; // USED FOR HEARING

    public bool useConeVision = true;
    public float visionRadius = 15f;    // USED FOR SIGHT
    [Range(0, 360)] public float viewAngle = 90f;
    public LayerMask obstacleMask;

    [Header("Detection Alert")]
    public AudioSource alertAudio;
    public GameObject alertUI;
    public float alertDuration = 2f;
    public float chaseStartDelay = 1.0f; // Time to wait before running
    public float chaseSpeed = 4.0f;      // Slower than before (was 6)

    [Header("Footsteps")]
    public AudioSource footstepSource;
    public float footstepInterval = 0.5f;
    private float nextStepTime;

    [Header("Jumpscare Settings")]
    public GameObject jumpscarePanel;
    public AudioSource jumpscareAudio;
    [Tooltip("How long the AI screams in your face before teleporting away")]
    public float jumpscareDuration = 2.0f; // Renamed from recoveryTime for clarity

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float idleWaitTime = 3f;
    [SerializeField] private float lostPlayerIdleTime = 2f;

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

        // --- FIX: FORCE 3D SOUND SETTINGS ---
        if (alertAudio) alertAudio.spatialBlend = 1.0f;     // 1.0 = Fully 3D
        if (footstepSource) footstepSource.spatialBlend = 1.0f;

        // --- FIX: SAFETY INITIALIZATION ---
        // 1. Prevent animation from overriding movement
        if (anim) anim.applyRootMotion = false;

        // 2. Snap to NavMesh immediately to prevent "Missing Mesh" / Sinking
        // This looks 5 units down/up to find the floor and snaps the agent there.
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
        else
        {
            Debug.LogError($"AI {name} is NOT placed on or near a NavMesh! It will not move.");
        }

        GoToNextPatrolPoint();
    }

    void Update()
    {
        HandleFootsteps();

        // If attacking, freeze logic.
        if (isAttacking) return;

        bool playerDetected = false;

        // Only run detection if we actually have a target
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
            if (!isAttacking)
            {
                // Only start the chase routine if we aren't already alerted/chasing
                if (!hasAlerted)
                {
                    StopAllCoroutines();
                    isWaiting = false;
                    StartCoroutine(ChasePlayerRoutine());
                }
                else
                {
                    // Already chasing, just update destination
                    agent.destination = playerTarget.position;
                }
            }
        }
        else
        {
            // --- FIX: Patrol runs even if playerTarget is null or hidden ---
            Patrol();
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

    void HandleFootsteps()
    {
        if (agent.velocity.magnitude > 0.1f && !isWaiting && !isAttacking)
        {
            if (Time.time >= nextStepTime)
            {
                if (footstepSource) footstepSource.Play();
                nextStepTime = Time.time + footstepInterval;
            }
        }
    }

    // ================= MOVEMENT LOGIC =================
    IEnumerator ChasePlayerRoutine()
    {
        hasAlerted = true;

        // 1. Trigger Alert Visuals/Audio
        if (alertAudio) alertAudio.Play();
        if (alertUI)
        {
            alertUI.SetActive(true);
            StartCoroutine(HideAlertUI());
        }

        // 2. Hesitate! (Give player time to react)
        agent.isStopped = true;
        SetState(EnemyState.Idle);

        yield return new WaitForSeconds(chaseStartDelay);

        // 3. Start Running
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
        // --- FIX: Force UI off immediately if we lost the player ---
        if (hasAlerted)
        {
            hasAlerted = false;
            if (alertUI) alertUI.SetActive(false); // Immediate cleanup
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

        // --- FIX: Ensure Agent is valid before setting destination ---
        if (!agent.isOnNavMesh)
        {
            // Try one last time to snap it
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas))
                agent.Warp(hit.position);
            else
                return; // Cannot move if not on NavMesh
        }

        // Random patrol logic
        int newIndex = currentPointIndex;
        while (patrolPoints.Length > 1 && newIndex == currentPointIndex)
            newIndex = Random.Range(0, patrolPoints.Length);

        currentPointIndex = newIndex;
        agent.isStopped = false;
        agent.speed = 3f;
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
            if (maskCheck != null && maskCheck.isMaskOn) return; // Ignore if mask on

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

        // SNAP PLAYER ROTATION & FIX AI POSITION
        if (playerTarget != null)
        {
            // 1. Calculate ideal scare position (1.5m in front of player)
            Vector3 forwardPoint = playerTarget.position + (playerTarget.forward * 1.5f);

            // 2. Validate with NavMesh so we don't spawn inside a wall/void
            NavMeshHit hit;
            Vector3 finalPos = forwardPoint;

            // Search for valid floor within 2 units. NavMesh.AllAreas ensures we find ANY walkable spot.
            if (NavMesh.SamplePosition(forwardPoint, out hit, 2.0f, NavMesh.AllAreas))
            {
                finalPos = hit.position;
            }
            else
            {
                // Fallback: If wall blocked it, spawn directly at player position (collision will push out)
                // This prevents appearing in 'void'
                finalPos = playerTarget.position;
            }

            agent.Warp(finalPos);
            transform.LookAt(playerTarget); // Force AI to face player

            // 3. Force Player to look at AI (Removed y=0 so camera can pitch up/down)
            Vector3 directionToEnemy = transform.position + Vector3.up * 1.5f - playerTarget.position; // Look at head height

            Quaternion lookRotation = Quaternion.LookRotation(directionToEnemy);
            playerTarget.rotation = lookRotation;
        }

        if (jumpscarePanel) jumpscarePanel.SetActive(true);
        if (jumpscareAudio) jumpscareAudio.Play();

        // Wait for the exposed duration (prevent multiple scares)
        yield return new WaitForSeconds(jumpscareDuration);

        if (jumpscarePanel) jumpscarePanel.SetActive(false);
        TeleportToFarthestPoint();
    }

    void TeleportToFarthestPoint()
    {
        if (patrolPoints.Length == 0) return;

        Transform bestPoint = null;
        float maxDistance = 0f;

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
            // --- FIX: Verify the destination is valid before warping ---
            NavMeshHit hit;
            if (NavMesh.SamplePosition(bestPoint.position, out hit, 5.0f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                Debug.Log($"AI retreated to {bestPoint.name} (Valid NavMesh Spot)");
            }
            else
            {
                // If the patrol point was placed inside a wall/void, warp to the unsafe point anyway as a fallback
                // or consider picking a different point.
                agent.Warp(bestPoint.position);
                Debug.LogWarning($"AI retreated to {bestPoint.name} but NavMesh spot wasn't found nearby!");
            }
        }

        StartCoroutine(TemporaryBlindness());
    }

    IEnumerator TemporaryBlindness()
    {
        // 1. Reset States
        isAttacking = false;
        agent.isStopped = false;

        // 2. DISABLE SENSES (Store original values)
        float originalHearing = detectionRadius;
        float originalVision = visionRadius;

        // --- THE FIX: Set correct variables to 0 ---
        detectionRadius = 0f;
        visionRadius = 0f;

        // 3. Start walking immediately
        SetState(EnemyState.Walk);
        GoToNextPatrolPoint();

        // 4. Wait
        yield return new WaitForSeconds(3.0f);

        // 5. RESTORE SENSES
        detectionRadius = originalHearing;
        visionRadius = originalVision;
    }

    // ================= GIZMOS =================
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