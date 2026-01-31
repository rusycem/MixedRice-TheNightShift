using UnityEngine;
using UnityEngine.AI;

public class AI_Static : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform playerTransform;

    [Header("Movement")]
    [SerializeField] private float jumpDistance = 4f; // how many distance per move
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float stopDistance = 3f;

    [Header("Mask Stalking")]
    [SerializeField] private float maskedJumpInterval = 2.5f; // How many seconds between creeps when player is masked
    private float maskedTimer = 0f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 50f;

    [Header("Death Settings")]
    [SerializeField] private GameEvent onPlayerDied;
    [SerializeField] private float killRange = 2f;

    private bool wasWatchedLastFrame = true;
    private MaskManager playerMask;

    void Start()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerTransform != null)
        {
            playerMask = playerTransform.GetComponent<MaskManager>();
        }

        agent.updateRotation = false;
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        bool isPlayerMasked = playerMask != null && playerMask.isMaskOn;

        if (distanceToPlayer <= killRange && !isPlayerMasked)
        {
            CheckForKill();
            return;
        }

        if (distanceToPlayer < detectionRange)
        {
            FacePlayer();
        }

        bool physicallyWatched = IsBeingWatched();

        if (distanceToPlayer < detectionRange)
        {
            // Player is NOT wearing the mask
            if (!isPlayerMasked)
            {
                maskedTimer = 0f;

                if (!physicallyWatched && wasWatchedLastFrame)
                {
                    PerformInstantJump(distanceToPlayer);
                }
            }
            // Player IS wearing the mask
            else
            {
                maskedTimer += Time.deltaTime;
                if (maskedTimer >= maskedJumpInterval)
                {
                    PerformInstantJump(distanceToPlayer);
                    maskedTimer = 0f;
                }
            }
        }

        wasWatchedLastFrame = physicallyWatched;
    }

    void CheckForKill()
    {
        PlayerMovement pm = playerTransform.GetComponent<PlayerMovement>();
        if (pm != null && !pm.disableControl)
        {
            onPlayerDied?.Raise();
        }
    }

    void FacePlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void PerformInstantJump(float currentDist)
    {
        if (currentDist <= stopDistance) return;

        float actualJumpDist = Mathf.Min(jumpDistance, currentDist - stopDistance);
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        Vector3 targetPos = transform.position + (directionToPlayer * actualJumpDist);

        agent.Warp(targetPos);
    }

    bool IsBeingWatched()
    {
        Vector3 screenPoint = playerCamera.WorldToViewportPoint(transform.position);
        bool isVisibleOnScreen = screenPoint.z > 0 && 
                                 screenPoint.x > 0 && screenPoint.x < 1 && 
                                 screenPoint.y > 0 && screenPoint.y < 1;

        if (isVisibleOnScreen)
        {
            RaycastHit hit;
            Vector3 directionToEnemy = (transform.position - playerCamera.transform.position).normalized;
            
            if (Physics.Raycast(playerCamera.transform.position, directionToEnemy, out hit, detectionRange + 5f))
            {
                if (hit.transform == this.transform || hit.transform.IsChildOf(this.transform)) 
                {
                    return true; 
                }
            }
        }
        return false;
    }

    
}
