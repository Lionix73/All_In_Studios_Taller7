using FMODUnity;
using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    private StudioEventEmitter emitter;
    private string path_GangamStyle = "event:/Music/GangnamStyle";

    private void Awake()
    {
        emitter = GetComponent<StudioEventEmitter>();
    }

    public void PlayMusic()
    {
        //emitter.EventReference = EventReference.Find(path_GangamStyle);

        if (emitter.IsPlaying() == false)
        {
            emitter.Play();
        }
    }

    public void StopMusic()
    {
        emitter.Stop();
    }
}
