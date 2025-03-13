using System.Collections;
using UnityEngine;

public class AreaDamage : MonoBehaviour
{
    [SerializeField] private int damage = 5;
    public int Damage { get => damage; set => damage = value; }

    [SerializeField] private float tickRate;
    public float TickRate { get => tickRate; set => tickRate = value; }

    private IDamageable damageable;

    private void OnTriggerEnter(Collider other)
    {
        if(damageable == null)
        {
            IDamageable damageableTemp = other.GetComponent<IDamageable>();

            if(damageableTemp != null)
            {
                damageable = damageableTemp;
                StartCoroutine(DealDamage());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(damageable == null)
        {
            IDamageable damageableTemp = other.GetComponent<IDamageable>();

            if(damageableTemp != null)
            {
                damageable = null;
            }
        }
    }

    private IEnumerator DealDamage(){
        WaitForSeconds wait = new WaitForSeconds(tickRate);

        while(damageable != null)
        {
            damageable.TakeDamage(damage);
            yield return wait;
        }
    }

    private void OnDisable()
    {
        damageable = null;
    }
}
