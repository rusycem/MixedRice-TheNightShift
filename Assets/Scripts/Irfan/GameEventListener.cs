using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    public GameEvent Event; // The SO "Radio Station" to listen to
    public UnityEvent Response; // What happens in the Inspector when heard

    private void OnEnable() => Event.RegisterListener(this);
    private void OnDisable() => Event.UnregisterListener(this);

    public void OnEventRaised()
    {
        Response.Invoke(); // Trigger the functions assigned in the Inspector
    }
}