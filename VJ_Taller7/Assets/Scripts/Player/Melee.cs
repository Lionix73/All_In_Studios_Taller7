using UnityEngine;

public class Melee : MonoBehaviour
{
    [SerializeField] private int damage;
    private BoxCollider gunCollider;

    public void Start()
    {
        gunCollider = GetComponent<BoxCollider>();
    }

    public void ActivateMelee()
    {
        gunCollider.enabled = true;
    }

    public void DeactivateMelee()
    {
        gunCollider.enabled = false;
    }

    public void OnTriggerEnter(Collider obj)
    {
        if(obj.gameObject.GetComponent<Enemy>() != null)
        {
            obj.gameObject.GetComponent<Enemy>().TakeDamage(damage);
        }
    }
}
