using UnityEngine;
using UnityEngine.AI;

public class AI_Static : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform playerTransform;

    [Header("Movement")]
    [SerializeField] private float jumpSpeed = 50f;
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float stopDistance = 4f;
    [SerializeField] private bool lockRotation = true;

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
        
        agent.stoppingDistance = stopDistance;

        if (lockRotation)
        {
            agent.updateRotation = false;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer < detectionRange)
        {
            if (IsBeingWatched())
            {
                StopMoving();
            }
            else
            {
                if (distanceToPlayer > stopDistance)
                {
                    StartMoving();
                }
                else
                {
                    StopMoving();
                }
            }
        }
        else
        {
            StopMoving();
        }
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

            if (Physics.Raycast(playerCamera.transform.position, directionToEnemy, out hit, detectionRange))
            {
                if (hit.transform == this.transform)
                {
                    return true; 
                }
            }
        }

        return false;
    }

    void StartMoving()
    {
        agent.isStopped = false;
        agent.speed = jumpSpeed;
        agent.acceleration = 1000f;
        agent.destination = playerTransform.position;
    }

    void StopMoving()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }
}
