using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class DamageZoneMulti : MonoBehaviour, IDamageableMulti
{
   public EnemyMulti enemyWhoIsFrom;
   [Range(1,5)] public int damageMult;
   private void Awake() {
    enemyWhoIsFrom = GetComponentInParent<EnemyMulti>();
   }

   public void TakeDamage(int amount, ulong clientId)
   {
        TakeDamageRpc(amount, clientId);
   }
    [Rpc(SendTo.Server)]
    public void TakeDamageRpc(int amount, ulong clientId)
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
        enemyWhoIsFrom.TakeDamage(damageToTake, clientId);
    }

    [Rpc(SendTo.Everyone)]
    public void FloatingDamageRpc(int amount)
    {

    }
   public Transform GetTransform(){
    return transform;
   }

}
