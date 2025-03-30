using UnityEditor;
using UnityEngine;

public class DamageZone : MonoBehaviour, IDamageable
{
   public Enemy enemyWhoIsFrom;
   [Range(1,5)] public int damageMult;

   private void Awake() {
    enemyWhoIsFrom = GetComponentInParent<Enemy>();
   }

   public void TakeDamage(int amount){
    int damageToTake = amount * damageMult;
    
    if (enemyWhoIsFrom==null)return;
    enemyWhoIsFrom.TakeDamage(damageToTake);
        
        if(damageMult <= 1)
        {
            enemyWhoIsFrom.ShowFloatingText(damageToTake, enemyWhoIsFrom.floatingTextPrefab);
        }
        else
        {
            enemyWhoIsFrom.ShowFloatingText(damageToTake, enemyWhoIsFrom.floatingTextCriticPrefab);
        }
   }
   public Transform GetTransform(){
    return transform;
   }

}
