using UnityEngine;
using Unity.Cinemachine;

public class ChangeSens : MonoBehaviour
{
    private CinemachineInputAxisController axisController;

    private void Awake()
    {
        axisController = GetComponent<CinemachineInputAxisController>();
    }

    public void ChangeSensGain(float gainMultiplier)
    {
        foreach (InputAxisControllerBase<CinemachineInputAxisController.Reader>.Controller c in axisController.Controllers)
        {
            if (c.Name == "Look Orbit X")
            {
                c.Input.Gain = c.Input.Gain * gainMultiplier;
            }

            if (c.Name == "Look Orbit Y")
            {
                c.Input.Gain = c.Input.Gain * gainMultiplier * -1f;
            }
        }
    }
}
