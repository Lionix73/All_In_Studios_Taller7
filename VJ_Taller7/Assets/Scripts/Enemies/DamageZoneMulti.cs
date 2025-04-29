using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class DamageZoneMulti : MonoBehaviour, IDamageable
{
   public EnemyMulti enemyWhoIsFrom;
   [Range(1,5)] public int damageMult;
   private void Awake() {
    enemyWhoIsFrom = GetComponentInParent<EnemyMulti>();
   }

   public void TakeDamage(int amount)
   {
        TakeDamageRpc(amount);
   }
    [Rpc(SendTo.Server)]
    public void TakeDamageRpc(int amount)
    {
        int damageToTake = amount * damageMult;

        if (enemyWhoIsFrom == null) return;

        if (damageMult <= 1)
        {
            //ShowFloatingText(amount, enemyWhoIsFrom.floatingTextPrefab);
            enemyWhoIsFrom.ShowFloatingTextRpc(damageToTake);
        }
        else
        {
            //enemyWhoIsFrom.ShowFloatingText(amount, enemyWhoIsFrom.floatingTextCriticPrefab);
            enemyWhoIsFrom.ShowFloatingTextCriticRpc(damageToTake);
        }
        enemyWhoIsFrom.TakeDamage(damageToTake);
    }

    [Rpc(SendTo.Everyone)]
    public void FloatingDamageRpc(int amount)
    {

    }
   public Transform GetTransform(){
    return transform;
   }

}
