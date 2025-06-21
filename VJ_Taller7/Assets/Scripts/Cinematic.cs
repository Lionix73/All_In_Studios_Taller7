using System.Collections;
using UnityEngine;

public class Cinematic : MonoBehaviour
{
    [SerializeField] private float cinematicDuration;

    void OnEnable()
    {
        gameObject.SetActive(true);
        StartCoroutine(WaitForCinematic());
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
