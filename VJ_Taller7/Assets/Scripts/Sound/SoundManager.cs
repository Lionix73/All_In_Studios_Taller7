using FMODUnity;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class SoundManager : MonoBehaviour
{
    public List<StudioEventEmitter> sounds;
        
    private int sound_index;
    private void Start()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Sound");
        objs.ToList();

        foreach(GameObject obj in objs)
        {
            sounds.Add(obj.GetComponent<StudioEventEmitter>());
        }
    }

    public int GetSoundIndex(string s_name)
    {
        for (int i = 0; i < sounds.Count; i++)
        {
            if (sounds[i].gameObject.name == s_name)
            {
                return i;
            }
        }
        return -1;
    }

    public void PlaySound(string s_name1)
    {
        sound_index = GetSoundIndex(s_name1);
        if(sound_index != -1)
        {
            FMOD.RESULT isLoop = sounds[sound_index].EventDescription.isOneshot(out bool oneshot);

            if (!oneshot && !sounds[sound_index].IsPlaying())
            {
                sounds[sound_index].Play();
            }
            else if(oneshot)
            {
                sounds[sound_index].Play();
            }
        }
    }

    public void PlaySound(string s_name1, string s_name2 = "a", string s_name3 = "b", string s_name4 = "c", string s_name5 = "d")
    {
        for (int i = 0; i < sounds.Count; i++)
        {
            if (sounds[i].gameObject.name == s_name1 || sounds[i].gameObject.name == s_name2 || 
                sounds[i].gameObject.name == s_name3 || sounds[i].gameObject.name == s_name4 || 
                sounds[i].gameObject.name == s_name5)
            {
                if (!sounds[i].IsPlaying())
                {
                    sounds[i].Play();
                }
            }
        }
    }

    public void StopSound(string s_name1)
    {
        sound_index = GetSoundIndex(s_name1);
        if (sound_index != -1)
        {
            sounds[sound_index].Stop();
        }
    }

    public void StopSound(string s_name1, string s_name2 = "a", string s_name3 = "b", string s_name4 = "c", string s_name5 = "d")
    {
        for (int i = 0; i < sounds.Count; i++)
        {
            if (sounds[i].gameObject.name == s_name1 || sounds[i].gameObject.name == s_name2 || 
                sounds[i].gameObject.name == s_name3 || sounds[i].gameObject.name == s_name4 || 
                sounds[i].gameObject.name == s_name5)
            {
                sounds[i].Stop();
            }
        }
    }

    public void StopAllSounds()
    {
        foreach (var s in sounds)
        {
            s.GetComponent<StudioEventEmitter>().Stop();
        }
    }
}
