using UnityEngine;

public class GFeatherSounds : MonoBehaviour
{
    private SoundManager soundManager;
    private GoldenFeathers gFeathers;

    private void Awake()
    {
        soundManager = GetComponent<SoundManager>();
        gFeathers = GetComponent<GoldenFeathers>();
    }
}
