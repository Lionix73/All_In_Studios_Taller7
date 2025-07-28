using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ThisObjectSounds : MonoBehaviour
{
    public List<StudioEventEmitter> sounds;

    #region -----Private Variables-----
    private int sound_index;
    private Queue<string> soundQueue = new Queue<string>();
    private StudioEventEmitter currentPlaying = null;
    private bool isChecking = false;
    #endregion

    private void Start()
    {
        StudioEventEmitter[] objs = GetComponentsInChildren<StudioEventEmitter>();

        sounds = objs.ToList();
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

    #region -----Play Sounds-----
    public void PlaySound(string s_name1)
    {
        sound_index = GetSoundIndex(s_name1);
        if (sound_index != -1)
        {
            sounds[sound_index].EventDescription.isOneshot(out bool oneshot);

            if (oneshot)
            {
                sounds[sound_index].Play();
            }
            else if (!oneshot && !sounds[sound_index].IsPlaying())
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
    #endregion

    #region -----Stop Sounds-----
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
            s.Stop();
        }
    }

    public void PauseAllSound()
    {
        RuntimeManager.PauseAllEvents(true);
    }
    #endregion

    #region -----Dialogues-----
    public void QueueSound(string s_name)
    {
        soundQueue.Enqueue(s_name);
        TryPlayNext();
    }

    private void TryPlayNext()
    {
        if (currentPlaying == null || !currentPlaying.IsPlaying())
        {
            if (soundQueue.Count > 0)
            {
                string nextSound = soundQueue.Dequeue();
                int index = GetSoundIndex(nextSound);

                if (index != -1)
                {
                    currentPlaying = sounds[index];
                    currentPlaying.Play();

                    if (!isChecking)
                        StartCoroutine(CheckWhenDone());
                }
            }
        }
    }

    private IEnumerator CheckWhenDone()
    {
        isChecking = true;

        while (currentPlaying != null && currentPlaying.IsPlaying())
        {
            yield return null;
        }

        currentPlaying = null;
        isChecking = false;
        TryPlayNext();
    }
    #endregion
}
