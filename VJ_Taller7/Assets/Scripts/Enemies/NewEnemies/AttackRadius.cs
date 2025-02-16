using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class AttackRadius : MonoBehaviour
{
    public SphereCollider sphereCollider;
    private List<IDamageable> damageables = new List<IDamageable>();
    
    [SerializeField] private int damage = 10;
    public int Damage { get => damage; set => damage = value; }

    [SerializeField] private float attackDelay = 1f;
    public float AttackDelay { get => attackDelay; set => attackDelay = value; }
    
    public delegate void AttackEvent(IDamageable target);
    public AttackEvent OnAttack;
    private Coroutine attackCoroutine;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
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

    private void OnTriggerExit(Collider other)
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

    private IEnumerator Attack()
    {
        WaitForSeconds wait = new WaitForSeconds(AttackDelay);

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
                closestDamageable.TakeDamage(Damage);
            }

            closestDamageable = null;
            closestDistance = 2f;

            yield return wait;

            damageables.RemoveAll(DisabledDamageables);
        }

        attackCoroutine = null;
    }

    private bool DisabledDamageables(IDamageable damageable)
    {
        return damageable != null && !damageable.GetTransform().gameObject.activeSelf;
    }
}
