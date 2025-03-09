using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class GunManagerCollider : MonoBehaviour
{
    [SerializeField] private GunManager gunManager;
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Pickeable")){
            if (other.TryGetComponent<GunPickeable>(out GunPickeable component)){}
            
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Pickeable")){
            if (other.TryGetComponent<GunPickeable>(out GunPickeable component)){ gunManager.ExitPickeableGun();}
        }
    }
}
