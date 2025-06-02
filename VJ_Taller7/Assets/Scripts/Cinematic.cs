using FMODUnity;
using System.Collections;
using UnityEngine;

public class Cinematic : MonoBehaviour
{
    [SerializeField] private float cinematicDuration;
    [SerializeField] private bool startWithCinematic;
    void OnEnable()
    {
        gameObject.SetActive(true);
        StartCoroutine(WaitForCinematic());
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndCinematic();
        }

    }
    private IEnumerator WaitForCinematic(){
        yield return new WaitForSeconds(cinematicDuration);
        EndCinematic();
    }

    public void EndCinematic(){
        StopAllCoroutines();
        UIManager.Singleton.SelectedScene("BetaFinalMap");
        UIManager.Singleton.StartGameUI();
        gameObject.SetActive(false);
    }
}
