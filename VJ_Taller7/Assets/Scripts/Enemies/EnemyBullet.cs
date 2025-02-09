using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
   [SerializeField] private float damage = 10.0f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController playerHealth = other.gameObject.GetComponent<PlayerController>();
        if (playerHealth != null)
        {
            //PlayerController.TakeDamage(damage);
            Debug.Log("Bala impacto: " + damage + " de daño.");
        }

        Destroy(gameObject);
    }

    public float Damage { get => damage; set => damage = value; }
}
