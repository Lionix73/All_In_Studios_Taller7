using System.Collections;
using UnityEngine;

public class Cinematic : MonoBehaviour
{
    [SerializeField] private float cinematicDuration;
    [SerializeField] private bool startWithCinematic;

    void Start(){
        if (startWithCinematic) {
            gameObject.SetActive(true);
            StartCoroutine(WaitForCinematic());
        }
        else gameObject.SetActive(false);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) EndCinematic();
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
