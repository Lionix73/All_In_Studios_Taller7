using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShinelessFeather : GoldenFeathers
{
    public delegate void OnfailedShootEvent();
    public event OnfailedShootEvent OnShootMiss;

    public override void Spawn(Vector3 SpawnForce)
    {
        SpawnLocation = transform.position;
        transform.forward = SpawnForce.normalized;
        Rigidbody.AddForce(SpawnForce);
        isReturning = false;
        isFlying = true;
    }
    public override void OnCollisionEnter(Collision other)
    {
        InvokeCollisionEvent(other);

        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")) InvokeBulletEnd(other);
        
        if (!isReturning) Return();
    }

    public override void OnCollisionExit(Collision other)
    {
        
    }
}
