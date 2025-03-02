using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class LineOfSightChecker : MonoBehaviour
{
    [SerializeField] private float fov = 90f;
    public float Fov{ get => fov; set => fov = value; }

    [SerializeField] private LayerMask lineOfSightMask;
    public LayerMask LineOfSightMask{ get => lineOfSightMask; set => lineOfSightMask = value; }

    private SphereCollider sphereCollider;
    public SphereCollider SphereCollider{ get => sphereCollider; set => sphereCollider = value; }

    public delegate void GainSightEvent(PlayerController player);
    public GainSightEvent OnGainSight;

    public delegate void LoseSightEvent(PlayerController player);
    public LoseSightEvent OnLoseSight;

    private Coroutine CheckForlineOfSightCoroutine;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player;
        if(other.TryGetComponent<PlayerController>(out player)){
            if(!CheckLineOfSight(player)){
                CheckForlineOfSightCoroutine = StartCoroutine(CheckForLineOfSight(player));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player;
        if(other.TryGetComponent<PlayerController>(out player)){

            OnLoseSight?.Invoke(player);
            if(CheckLineOfSight(player)){
                if(CheckForlineOfSightCoroutine != null){
                    StopCoroutine(CheckForlineOfSightCoroutine);
                }
            }
        }
    }

    private bool CheckLineOfSight(PlayerController player){
        Vector3 direction = (player.transform.position - transform.position).normalized;

        if(Vector3.Dot(transform.forward, direction) >= Mathf.Cos(fov)){
            RaycastHit hit;

            if(Physics.Raycast(transform.position, direction, out hit, sphereCollider.radius, lineOfSightMask)){
                if(hit.transform.GetComponent<PlayerController>() != null){
                    OnGainSight?.Invoke(player);
                    return true;
                }
            }
        }

        return false;
    }

    private IEnumerator CheckForLineOfSight(PlayerController player){
        WaitForSeconds wait = new WaitForSeconds(0.1f);

        while(!CheckLineOfSight(player)){
            yield return wait;
        }
    }
}
