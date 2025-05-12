using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RespawnInteractables : MonoBehaviour
{
    [SerializeField] private float respawnCooldown;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Collider colliderInteractable;

    public GameObject model;
    private Material[] materials;

    private void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        colliderInteractable = GetComponentInChildren<Collider>();

        materials = meshRenderer.materials;
        colliderInteractable.enabled = true;

        for(int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat("_DissolveAmount", 0f);
        }
    }

    public void StartCountdown(){
        colliderInteractable.enabled = false;

        for(int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat("_DissolveAmount", 0.7f);
        }

        StartCoroutine(Respawning());
    }
    
    private IEnumerator Respawning(){
        StartCoroutine(DissolveMat());

        yield return new WaitForSeconds(respawnCooldown);

        for(int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat("_DissolveAmount", 0f);
        }

        colliderInteractable.enabled = true;
    }

    private IEnumerator DissolveMat(){
        if(materials.Length > 0)
        {
            float counter = 0;

            while (materials[0].GetFloat("_DissolveAmount") > 0)
            {
                counter += Time.deltaTime / respawnCooldown;

                for(int i = 0; i < materials.Length; i++)
                {
                    materials[i].SetFloat("_DissolveAmount", Mathf.Lerp(0.7f, 0, counter));
                }
                yield return null;
            }
        }
    }
}
