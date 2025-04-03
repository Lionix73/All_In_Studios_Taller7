using System.Collections;
using TMPro;
using UnityEngine;

public class RespawnInteractables : MonoBehaviour
{
    [SerializeField] private float respawnCooldown;
    public GameObject model;
    public void StartCountdown(){
        model.SetActive(false);
        StartCoroutine(Respawning());
    }
    
    private IEnumerator Respawning(){
        yield return new WaitForSeconds(respawnCooldown);

        model.SetActive(true);
    }
}
