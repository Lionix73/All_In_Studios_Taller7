using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkCharacterController _cc;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpImpulse = 10f;

    [Networked] private NetworkButtons previosButtons { get; set; }
    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetInput data))
        {
            Debug.Log("Obtencion de datos");
            data.Direction.Normalize();
            _cc.Move(5 * data.Direction * Runner.DeltaTime);
            previosButtons = data.Buttons;
        }
    }
}