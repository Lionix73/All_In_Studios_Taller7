using UnityEngine;
using UnityEngine.Events;

public class OnTriggerEvents : MonoBehaviour
{
    public UnityEvent TriggerEnter;
    public UnityEvent TriggerExit;

    private void OnTriggerEnter(Collider other)
    {
        TriggerEnter?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        TriggerExit?.Invoke();
    }
}
