using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class MultiAttackRadius : NetworkBehaviour
{
    public SphereCollider sphereCollider;
    protected List<IDamageableMulti> damageables = new List<IDamageableMulti>();
    
    [SerializeField] protected int damage = 10;
    public int Damage { get => damage; set => damage = value; }

    [SerializeField] protected float attackDelay = 1f;
    public float AttackDelay { get => attackDelay; set => attackDelay = value; }

    [SerializeField] private OnRangeBehvaior onRangeBehvaiorMethod = OnRangeBehvaior.Stop;
    public OnRangeBehvaior OnRangeBehvaior { get => onRangeBehvaiorMethod; set => onRangeBehvaiorMethod = value; }

    public PlayerControllerMulti Player { get; set; }

    
    public delegate void AttackEvent(IDamageableMulti target);
    public AttackEvent OnAttack;
    protected Coroutine attackCoroutine;
    protected EnemyMulti enemy;
    protected ThisObjectSounds soundManager;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    
        enemy = GetComponentInParent<EnemyMulti>();
        sphereCollider = GetComponent<SphereCollider>();
        soundManager = GetComponentInParent<ThisObjectSounds>();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if(!IsServer) return;

        if(enemy.IsDead){
            StopAllCoroutines();
            attackCoroutine = null;
            return;
        } 
        Player = other.GetComponent<PlayerControllerMulti>();
        IDamageableMulti damageable = other.GetComponent<IDamageableMulti>();
        if(damageable != null)
        {
            damageables.Add(damageable);

            if(attackCoroutine == null)
            {
                attackCoroutine = StartCoroutine(Attack());
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if(!IsServer) return;

        if(enemy.IsDead){
            StopAllCoroutines();
            attackCoroutine = null;
            return;
        }
        
        Player = null;
        IDamageableMulti damageable = other.GetComponent<IDamageableMulti>();
        if(damageable != null)
        {
            damageables.Remove(damageable);
            if(damageables.Count == 0)
            {
                StopCoroutine(attackCoroutine);
                enemy.Movement.ResumeMovement();
                attackCoroutine = null;
            }
        }
    }

    protected virtual IEnumerator Attack()
    {

        WaitForSeconds wait = new WaitForSeconds(attackDelay);

        yield return wait;

        IDamageableMulti closestDamageable = null;

        //Closest distance to enemy is 100% of their attack radius
        float closestDistance = sphereCollider.radius;
        while(damageables.Count > 0)
        {
            if(enemy.IsDead){
                StopAllCoroutines();
                closestDamageable = null;
                damageables.RemoveAll(DisabledDamageables);
                attackCoroutine = null;
                yield break;
            }

            for(int i = 0; i < damageables.Count; i++)
            {
                Transform damageablesTransform = damageables[i].GetTransform();
                float distance = Vector3.Distance(transform.position, damageablesTransform.position);

                //Debug.Log("Distance: " + distance + " - ClosestDistance: " + closestDistance);

                if(distance < closestDistance)
                {
                    //Debug.Log("Attacking");


                    if(onRangeBehvaiorMethod == OnRangeBehvaior.Stop)
                    {
                        enemy.Movement.StopMovement();
                        closestDamageable = damageables[i];
                    }
                    else if(onRangeBehvaiorMethod == OnRangeBehvaior.Circles)
                    {
                        enemy.Movement.MoveInCircles();
                        closestDamageable = damageables[i];
                    }
                    else if(onRangeBehvaiorMethod == OnRangeBehvaior.Around)
                    {
                        enemy.Movement.MoveAround();
                        closestDamageable = damageables[i];
                    }
                    else if(onRangeBehvaiorMethod == OnRangeBehvaior.Follow)
                    {
                        closestDamageable = damageables[i];
                    }

                    //closestDistance = distance;
                }
            }

            if(closestDamageable != null)
            {
                OnAttack?.Invoke(closestDamageable);
                closestDamageable.TakeDamage(damage,10);
            }

            closestDamageable = null;
            closestDistance = sphereCollider.radius;

            yield return wait;

            damageables.RemoveAll(DisabledDamageables);
        }

        attackCoroutine = null;
    }

    protected bool DisabledDamageables(IDamageableMulti damageable)
    {
        return damageable != null && !damageable.GetTransform().gameObject.activeSelf;
    }
}
