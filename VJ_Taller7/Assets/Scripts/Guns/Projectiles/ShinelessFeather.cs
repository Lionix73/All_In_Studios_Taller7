using UnityEngine;

public class ShinelessFeather : GoldenFeathers
{
    public delegate void OnfailedShootEvent(); // Este evento no se usa?
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

        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            soundManager.PlaySound("featherHitEnemy");
            InvokeBulletEnd(other);
            if (!isReturning) Return();
        }
        else
        {
            FindFirstObjectByType<PlayerSoundsManager>().brokenFeather = true;
            soundManager.PlaySound("featherBreak");
        }

        //if (!isReturning) Return(); // me estaba dando un error y como se supone que solo regresa si golpea un enemigo, lo movi arriba. Att: Chanti
    }

    public override void OnCollisionExit(Collision other)
    {
        
    }
}
