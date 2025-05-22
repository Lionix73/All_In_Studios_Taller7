using UnityEngine;

public class OmnipotentSoundManager : MonoBehaviour
{
    ThisObjectSounds[] sounds;
    RoundsMusicManager roundsMusic;

    public void StopEverySound()
    {
        sounds = FindObjectsByType<ThisObjectSounds>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        roundsMusic = FindFirstObjectByType<RoundsMusicManager>();

        foreach (var sound in sounds)
        {
            sound.StopAllSounds();
        }
        roundsMusic.StopMusic();
    }
}
