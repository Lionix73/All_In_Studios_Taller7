using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MultiShinelessFeather : MultiGoldenFeathers
{
    public delegate void OnfailedShootEvent();
    public event OnfailedShootEvent OnShootMiss;

    public override void OnCollisionEnter(Collision other)
    {
        InvokeCollisionEvent(other);

        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            soundManager.PlaySound("featherHitEnemy");
            InvokeBulletEnd(other);
            if (!isReturning) Return(GOownerId);
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
