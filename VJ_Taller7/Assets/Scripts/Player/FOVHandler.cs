using Unity.Cinemachine;
using UnityEngine;

public class FOVHandler : MonoBehaviour
{
    [SerializeField] private float normalFOV = 65;
    [SerializeField] private float aimFOV = 55;
    [SerializeField] private FloatDampener timeToAim;

    private CinemachineCamera _freeLookCamera;

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
