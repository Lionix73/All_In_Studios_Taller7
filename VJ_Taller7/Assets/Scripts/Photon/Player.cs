using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkCharacterController _cc;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            Debug.Log("Obtencion de datos");
            data.direction.Normalize();
            _cc.Move(5 * data.direction * Runner.DeltaTime);
        }
    }
}