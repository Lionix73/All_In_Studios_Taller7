using UnityEngine;

public class VFXDebugger : MonoBehaviour
{
    [SerializeField] private ParticleSystem vfx;

    private void OnTriggerEnter(Collider other)
    {
        vfx.Play();
    }
}
