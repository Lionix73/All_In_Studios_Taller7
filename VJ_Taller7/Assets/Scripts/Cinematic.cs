using FMODUnity;
using System.Collections;
using UnityEngine;

public class Cinematic : MonoBehaviour
{
    [SerializeField] private float cinematicDuration;
    [SerializeField] private bool startWithCinematic;
    [SerializeField] private StudioEventEmitter dialogueSound;

    [SerializeField] private StudioEventEmitter music;
    void Start(){
        if (startWithCinematic) {
            gameObject.SetActive(true);
            dialogueSound.Play();
            StartCoroutine(WaitForCinematic());
        }
        else gameObject.SetActive(false);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            dialogueSound.Stop();
            music.Play();
            EndCinematic();
        }

    }
    private IEnumerator WaitForCinematic(){
        yield return new WaitForSeconds(cinematicDuration);
        EndCinematic();
    }

    public void EndCinematic(){
        StopAllCoroutines();
        gameObject.SetActive(false);
    }
}
