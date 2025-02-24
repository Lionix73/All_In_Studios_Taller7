using FMODUnity;
using UnityEngine;


[System.Serializable]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private GameObject[] sounds;

    public void PlaySound(int i)
    {
        StudioEventEmitter emitter = sounds[i].GetComponent<StudioEventEmitter>();

        if (emitter.IsPlaying() == false)
        {
            emitter.Play();
        }
    }

    public void StopSound(int i)
    {
        StudioEventEmitter emitter = sounds[i].GetComponent<StudioEventEmitter>();
        emitter.Stop();
    }

    public void StopAllSounds()
    {
        foreach (var s in sounds)
        {
            s.GetComponent<StudioEventEmitter>().Stop();
        }
    }
}
