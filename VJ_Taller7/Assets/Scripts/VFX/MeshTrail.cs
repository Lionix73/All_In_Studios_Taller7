using System.Collections;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    [Header("Mesh Settings")]
    [SerializeField] private float activeTime = 0.2f;
    [SerializeField] private float meshRefreshRate = 0.1f;
    [SerializeField] private float destroyTime = 0.5f;
    [SerializeField] private Transform positionToSpawn;

    [Header("Material Settings")]
    [SerializeField] private Material mat;
    [SerializeField] private string propertyName;
    [SerializeField] private float shaderVarRate = 0.01f;
    [SerializeField] private float shaderVarRefresh = 0.1f;

    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    public bool IsTrailActive = false;

    private void Start()
    {
        if(positionToSpawn == null)
        positionToSpawn = transform;
    }

    IEnumerator ActivateTrail(float timeActive)
    {
        while(timeActive > 0)
        {
            timeActive -= meshRefreshRate;

            if(skinnedMeshRenderers == null)
            {
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            }

            for(int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                GameObject obj = new GameObject("Trail");
                obj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer mr = obj.AddComponent<MeshRenderer>();
                MeshFilter mf = obj.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();

                skinnedMeshRenderers[i].BakeMesh(mesh);

                mf.mesh = mesh;
                mr.material = mat;

                StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefresh));

                Destroy(obj, destroyTime);
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }

        IsTrailActive = false;
    }

    IEnumerator AnimateMaterialFloat(Material mat, float goal, float rate, float refreshRate)
    {
        float valueToAnim = mat.GetFloat(propertyName);
        
        while(valueToAnim > goal){
            valueToAnim -= rate;
            mat.SetFloat(propertyName, valueToAnim);
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
