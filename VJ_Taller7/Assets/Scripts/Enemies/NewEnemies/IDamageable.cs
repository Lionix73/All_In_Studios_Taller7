using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int damage);
    Transform GetTransform();
}

public interface IDamageableMulti
{
    void TakeDamage(int damage, ulong attackerId);
    Transform GetTransform();
}
