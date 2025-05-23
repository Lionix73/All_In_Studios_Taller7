using UnityEngine;
using FMODUnity;

public class ShopDialoguesManager : MonoBehaviour
{
    [Header("Dialogues")]
    [SerializeField] private StudioEventEmitter BussinessManDialogues;
    [SerializeField] private StudioEventEmitter TheWomenOfBussinessManDialogues;

    [Header("Cooldown")]
    [Tooltip("cooldown before play another dialogue")]
    [SerializeField][Range(0, 60)] private float cooldownTime = 3f; // Tiempo en segundos antes de poder reproducir otro audio

    private float lastTriggerTime = -Mathf.Infinity;

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time >= lastTriggerTime + cooldownTime)
        {
            PlayRandomEvent();
            lastTriggerTime = Time.time;
        }
    }

    private void PlayRandomEvent()
    {
        StudioEventEmitter selectedEvent = Random.value > 0.5f ? BussinessManDialogues : TheWomenOfBussinessManDialogues;
        
        selectedEvent.Play();
    }
}
