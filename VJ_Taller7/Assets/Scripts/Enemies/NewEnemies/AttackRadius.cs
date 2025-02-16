using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class AttackRadius : MonoBehaviour
{
    public SphereCollider sphereCollider;
    protected List<IDamageable> damageables = new List<IDamageable>();
    
    [SerializeField] protected int damage = 10;
    public int Damage { get => damage; set => damage = value; }

    [SerializeField] protected float attackDelay = 1f;
    public float AttackDelay { get => attackDelay; set => attackDelay = value; }
    
    public delegate void AttackEvent(IDamageable target);
    public AttackEvent OnAttack;
    protected Coroutine attackCoroutine;

    protected virtual void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
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
        IDamageable damageable = other.GetComponent<IDamageable>();
        if(damageable != null)
        {
            damageables.Remove(damageable);
            if(damageables.Count == 0)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
        }
    }

    protected virtual IEnumerator Attack()
    {
        WaitForSeconds wait = new WaitForSeconds(attackDelay);

        yield return wait;

        IDamageable closestDamageable = null;
        float closestDistance = 2f;

        while(damageables.Count > 0)
        {
            for(int i = 0; i < damageables.Count; i++)
            {
                Transform damageablesTransform = damageables[i].GetTransform();
                float distance = Vector3.Distance(transform.position, damageablesTransform.position);

                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDamageable = damageables[i];
                }
            }

            if(closestDamageable != null)
            {
                OnAttack?.Invoke(closestDamageable);
                closestDamageable.TakeDamage(damage);
            }

            closestDamageable = null;
            closestDistance = 2f;

            yield return wait;

            damageables.RemoveAll(DisabledDamageables);
        }

        attackCoroutine = null;
    }

    protected bool DisabledDamageables(IDamageable damageable)
    {
        return damageable != null && !damageable.GetTransform().gameObject.activeSelf;
    }
}
