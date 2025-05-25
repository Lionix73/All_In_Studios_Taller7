using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.UI;

public class ChangeSens : MonoBehaviour
{
    [SerializeField] private bool vertical;
    private CinemachineInputAxisController axisController;
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void ChangeSensGain()
    {
        if(axisController == null)
        {
            try
            {
                axisController = GameObject.FindGameObjectWithTag("FreeLookCamera").GetComponent<CinemachineInputAxisController>();
            }
            catch
            {
                Debug.Log("No se encontro la camara");
                return;
            }
        }

        foreach (InputAxisControllerBase<CinemachineInputAxisController.Reader>.Controller c in axisController.Controllers)
        {
            if (c.Name == "Look Orbit X" && !vertical)
            {
                c.Input.Gain = 1f * slider.value;
            }
            else if (c.Name == "Look Orbit Y" && vertical)
            {
                c.Input.LegacyGain = -200f * slider.value;
                return;
            }
        }
    }
}
