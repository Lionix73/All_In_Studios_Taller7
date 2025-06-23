using UnityEngine;

public class LoopMusicHandler : MonoBehaviour
{
    [SerializeField] private string soundName;
    [SerializeField] private ThisObjectSounds soundManager;

    private void OnEnable()
    {
        Invoke("ReproduceMusic", 0.5f);
    }

    private void ReproduceMusic()
    {
        soundManager.PlaySound(soundName);
    }
}
