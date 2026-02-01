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

    [Header("Detection Settings")]
    public bool useRadiusDetection = true; // "Hearing" (360 detection through walls?)
    public float detectionRadius = 10f;

    public bool useConeVision = true;      // "Sight" (Limited angle, blocked by walls)
    public float visionRadius = 15f;       // How far it can see
    [Range(0, 360)] public float viewAngle = 90f; // The width of the cone
    public LayerMask obstacleMask;         // What blocks vision (Walls, etc.)

    [Header("Footsteps (Placeholder)")]
    public AudioSource footstepSource;
    public float footstepInterval = 0.5f; // Time between steps
    private float nextStepTime;

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

    // Mask Manager Reference
    private MaskManager playerMask;

    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 10f;
    private bool isChasing = false;

    [Header("Events")]
    public GameEvent onPlayerDied;

    void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!anim) anim = GetComponent<Animator>();
        if (playerTarget) playerMask = playerTarget.GetComponent<MaskManager>();

        // Auto-configure obstacle mask if forgotten (assumes Default layer contains walls)
        if (obstacleMask == 0) obstacleMask = LayerMask.GetMask("Default");

        GoToNextPatrolPoint();
    }

    void Update()
    {
        HandleFootsteps();

        if (!playerTarget || isAttacking) return;

        // --- NEW DETECTION LOGIC ---
        bool playerDetected = false;

        // 1. Check Mask (If mask is ON, AI is blind/deaf to player)
        bool isMaskSafe = (playerMask != null && playerMask.isMaskOn);

        if (!isMaskSafe)
        {
            // Check Hearing (Radius)
            if (useRadiusDetection && CheckHearing())
            {
                playerDetected = true;
            }
            // Check Sight (Cone)
            else if (useConeVision && CheckSight())
            {
                playerDetected = true;
            }
        }

        // --- STATE MACHINE ---
        if (playerDetected)
        {
            if (!isAttacking)
            {
                StopAllCoroutines();
                isWaiting = false;
                ChasePlayer();
            }
        }
        else
        {
            // Player lost or hidden
            Patrol();
        }
    }

    //sensory

    bool CheckHearing()
    {
        // Simple distance check (can hear through walls usually)
        return Vector3.Distance(transform.position, playerTarget.position) < detectionRadius;
    }

    bool CheckSight()
    {
        Vector3 dirToPlayer = (playerTarget.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // 1. Distance Check
        if (distToPlayer > visionRadius) return false;

        // 2. Angle Check (The Cone)
        if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
        {
            // 3. Wall Check (Raycast)
            // We cast from our eyes (up 1.6m) to player's center (up 1m) to avoid floor clipping
            Vector3 eyePos = transform.position + Vector3.up * 1.6f;
            Vector3 targetPos = playerTarget.position + Vector3.up * 1.0f;

            if (!Physics.Linecast(eyePos, targetPos, obstacleMask))
            {
                return true; // Clear line of sight!
            }
        }
        return false;
    }

    void HandleFootsteps()
    {
        // If moving faster than 0.1 velocity
        if (agent.velocity.magnitude > 0.1f && !isWaiting && !isAttacking)
        {
            if (Time.time >= nextStepTime)
            {
                // PLAY SOUND HERE
                if (footstepSource) footstepSource.Play();

                // Debug log to prove it works
                // Debug.Log("Footstep!"); 

                nextStepTime = Time.time + footstepInterval;
            }
        }
    }

    //debug vision
    void OnDrawGizmosSelected()
    {
        // Draw Hearing Radius (Red)
        if (useRadiusDetection)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }

        // Draw Vision Cone (Yellow)
        if (useConeVision)
        {
            Gizmos.color = Color.yellow;
            Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
            Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

            Gizmos.DrawLine(transform.position, transform.position + viewAngleA * visionRadius);
            Gizmos.DrawLine(transform.position, transform.position + viewAngleB * visionRadius);

            // Draw arc (simple line for visualization)
            Gizmos.DrawLine(transform.position + viewAngleA * visionRadius, transform.position + viewAngleB * visionRadius);
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
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

    //JUMPSCARE
    private void OnTriggerEnter(Collider other)
    {
        if (isAttacking) return;

        if (other.CompareTag("Player"))
        {
            // 1. Get the Mask Manager from the object we hit
            // We use GetComponentInParent in case the collider is on a child object
            MaskManager maskCheck = other.GetComponentInParent<MaskManager>();

            // 2. THE FIX: If mask is ON, ignore this collision completely!
            if (maskCheck != null && maskCheck.isMaskOn)
            {
                Debug.Log("AI bumped into player, but ignored them (Mask is ON)");
                return;
            }

            // 3. If no mask (or mask is off), proceed with Jumpscare/Damage
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
