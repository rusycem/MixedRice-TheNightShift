using UnityEngine;
using UnityEngine.AI;

public class AI_Chaser : MonoBehaviour
{
    public NavMeshAgent ai;
    public Transform player;
    Vector3 destination;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        destination = player.position;
        ai.destination = destination;
    }
}
