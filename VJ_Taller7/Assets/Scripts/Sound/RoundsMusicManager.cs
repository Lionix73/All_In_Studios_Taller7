using FMODUnity;
using System.Collections;
using UnityEngine;

public class RoundsMusicManager : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter music;
    [SerializeField] private FloatDampener intensityDampener;
    [SerializeField] private FloatDampener stopMusicDampener;
    [SerializeField][Range(0, 1)] private float volumeDecrease = 0.7f;
    [SerializeField][Range(0, 1)] private float delay = 0.5f;
    [SerializeField][Range(0, 5)] private float stopDelay = 3f;

    private Health health;

    private void Start()
    {
        stopMusicDampener.TargetValue = 1f;

        health = GameObject.FindWithTag("Player").GetComponent<Health>();
    }
    private void Update()
    {
        if(health.isDead)
        {
            StopMusic();
        }

        intensityDampener.Update();
        stopMusicDampener.Update();
    }

    public void PlayMusic()
    {
        if (music.IsPlaying()) return; 
        music.Play();
    }

    public void StopMusic()
    {
        music.Stop();

        //stopMusicDampener.TargetValue = 0f;
        //music.SetParameter("Intensity", stopMusicDampener.CurrentValue);

        //StartCoroutine(FadeOutMusic());
    }

    IEnumerator FadeOutMusic()
    {
        yield return new WaitForSeconds(stopDelay);
        music.Stop();
        music.SetParameter("Intensity", 1f);
    }

    public void OnEnemyKilled()
    {
        intensityDampener.TargetValue = volumeDecrease;

        // Bajar intensidad (volumen)
        music.SetParameter("Intensity", volumeDecrease);

        StartCoroutine(RestoreMusicAfterDelay());
    }

    IEnumerator RestoreMusicAfterDelay()
    {
        // Disparar Crash
        music.SetParameter("PlayCrash", 1f);

        yield return new WaitForSeconds(delay);
        
        intensityDampener.TargetValue = 1f;

        // Restaurar valores
        music.SetParameter("Intensity", 1f);
        music.SetParameter("PlayCrash", 0f);
    }
}
