using Unity.Cinemachine;
using UnityEngine;

public class FOVHandler : MonoBehaviour
{
    [SerializeField][Range(30, 100)] private float normalFOV = 65;
    [SerializeField][Range(20, 80)] private float aimFOV = 55;
    [SerializeField] private FloatDampener timeToAim;

    private CinemachineCamera _freeLookCamera;

    public float CameraNormalFOV
    {
        get => normalFOV;
        set => normalFOV = value;
    }

    public float CameraAimFOV
    {
        get => aimFOV;
        set => aimFOV = value;
    }

    private void Start()
    {
        _freeLookCamera = GameObject.FindGameObjectWithTag("FreeLookCamera").GetComponent<CinemachineCamera>();
        timeToAim.TargetValue = normalFOV;
    }

    private void Update()
    {
        timeToAim.Update();
        AdjustimeToAim();
    }

    #region -----FOV Ajustment-----
    public void AdjustimeToAim()
    {
        _freeLookCamera.Lens.FieldOfView = timeToAim.CurrentValue;
    }

    public void AimFOV()
    {
        timeToAim.TargetValue = aimFOV;
    }

    public void NormalFOV()
    {
        timeToAim.TargetValue = normalFOV;
    }

    public void SetAimFOV(float gunAimFOV)
    {
        aimFOV = gunAimFOV;
    }
    #endregion
}
