using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private GameObject rayOrigin;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 jumpDirection = GetSurfaceNormal();

                rb.linearVelocity = Vector3.zero; // Resetear la velocidad para evitar acumulaciones raras
                rb.AddForce(jumpForce * jumpDirection, ForceMode.Impulse);
            }
            else
            {
                rb = other.GetComponentInParent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 jumpDirection = GetSurfaceNormal();

                    rb.linearVelocity = Vector3.zero; // Resetear la velocidad para evitar acumulaciones raras
                    rb.AddForce(jumpForce * jumpDirection, ForceMode.Impulse);
                }
            }
        }
    }

    private Vector3 GetSurfaceNormal()
    {
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin.transform.position, -rayOrigin.transform.up, out hit, 2f))
        {
            return hit.normal;
        }

        return transform.up;
    }

    private void Update()
    {
        Debug.DrawLine(transform.position, transform.position + GetSurfaceNormal() * 3, Color.magenta);
    }
}
