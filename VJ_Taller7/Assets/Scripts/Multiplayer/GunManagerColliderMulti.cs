using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class GunManagerColliderMulti : MonoBehaviour
{
    [SerializeField] private GunManagerMulti2 gunManager;


    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Pickeable")){
            if (other.TryGetComponent<GunPickeableMulti>(out GunPickeableMulti component)){}
            
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Pickeable")){
            if (other.TryGetComponent < GunPickeableMulti>(out GunPickeableMulti component)){ gunManager.ExitPickeableGun();}
        }
    }
}
