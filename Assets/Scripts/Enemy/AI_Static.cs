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

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 50f;

    private bool wasWatchedLastFrame = true;

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

        agent.updateRotation = false;
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer < detectionRange)
        {
            FacePlayer();
        }

        bool currentlyWatched = IsBeingWatched();

        if (distanceToPlayer < detectionRange)
        {
            if (!currentlyWatched && wasWatchedLastFrame)
            {
                PerformInstantJump(distanceToPlayer);
            }
        }

        wasWatchedLastFrame = currentlyWatched;
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
