using FMODUnity;
using System.Collections;
using UnityEngine;

public class RoundsMusicManager : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private StudioEventEmitter music;
    
    [Header("Dampeners")]
    [SerializeField][Tooltip("Smooth Time is how long it takes to the music to stop completly")]
    private FloatDampener stopMusicDampener;
    
    [Header("Crash Effect")]
    [SerializeField][Tooltip("How much to reduce music volume, 1 = Max volume, 0 = No music")]
    [Range(0, 1)] private float volumeDecrease = 0.9f;
    [SerializeField][Tooltip("How long the music will be with a reduced volume in seconds")]
    [Range(0, 1)] private float LowIntensityTime = 0.2f;

    private Health health;

    private void Start()
    {
        //GameObject.Find("Menu").GetComponentInChildren<ThisObjectSounds>().StopAllSounds();
        stopMusicDampener.TargetValue = 1f;

        health = GameObject.FindWithTag("Player").GetComponent<Health>();
    }
    private void Update()
    {
        if(health.isDead)
        {
            StopMusic();
        }

        stopMusicDampener.Update();
    }

    public void PlayMusic()
    {
        if (music.IsPlaying()) return; 
        music.Play();
    }

    #region Stop Music
    public void StopMusic()
    {
        StartCoroutine(FadeOutMusic());
    }

    private IEnumerator FadeOutMusic()
    {
        stopMusicDampener.TargetValue = 0f;
        float t = 0f;
        while (t < stopMusicDampener.SmoothTime)
        {
            t += Time.deltaTime;
            music.SetParameter("Intensity", stopMusicDampener.CurrentValue);
            yield return null;
        }
        music.Stop();
        stopMusicDampener.TargetValue = 1f;
        music.SetParameter("Intensity", 1f);
    }
    #endregion

    #region Crash Effect
    public void OnEnemyKilled()
    {
        // Bajar intensidad (volumen)
        StartCoroutine(LowIntensity());
    }

    private IEnumerator LowIntensity()
    {
        float delay = LowIntensityTime / 2;

        float t = 0f;
        while (t < delay)
        {
            t += Time.deltaTime;
            float v = Mathf.Lerp(1f, volumeDecrease, t / delay);
            music.SetParameter("Intensity", v);
            yield return null;
        }
        music.SetParameter("Intensity", volumeDecrease);
        music.SetParameter("PlayCrash", 1f);

        t = 0f;
        while (t < delay)
        {
            t += Time.deltaTime;
            float v = Mathf.Lerp(volumeDecrease, 1f, t / delay);
            music.SetParameter("Intensity", v);
            yield return null;
        }
        music.SetParameter("Intensity", 1f);
        music.SetParameter("PlayCrash", 0f);
    }
    #endregion
}
