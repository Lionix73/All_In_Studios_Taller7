using FMODUnity;
using UnityEngine;

public class MusicEmitter : MonoBehaviour
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
        emitter.Event = path_GangamStyle;

        if(emitter.IsPlaying() == false)
        {
            emitter.Play();
        }
    }

    public void StopMusic()
    {
        emitter.Stop();
    }
}
