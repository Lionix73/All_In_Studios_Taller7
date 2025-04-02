using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class OutOfBoundsTrigger : MonoBehaviour
{
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private float damageMultiplier = 0.5f; // Porcentaje de daño a aplicar

    private void Start()
    {
        if (spawnPoint == null)
        {
            spawnPoint = GameObject.FindGameObjectWithTag("Respawn");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IDamageable damageable;
            if (other.TryGetComponent<IDamageable>(out damageable))
            {
                int damageToDeal = Mathf.FloorToInt(GameManager.Instance.playerManager.PlayerCurrentHealth * damageMultiplier);

                damageable.TakeDamage(damageToDeal);
            }

            StartCoroutine(RespawnPlayer(other.gameObject));
        }
    }

    private IEnumerator RespawnPlayer(GameObject player)
    {
        player.GetComponent<Rigidbody>().isKinematic = true; // Desactivar la fisica del jugador
        player.GetComponent<Animator>().enabled = false; // Desactivar la animacion del jugador
        player.GetComponent<Animator>().applyRootMotion = false; // Desactivar la root del jugador

        player.transform.position = spawnPoint.transform.position;

        yield return null;

        player.GetComponent<Rigidbody>().isKinematic = false; // Reactivar la fisica del jugador
        player.GetComponent<Animator>().enabled = true; // Reactivar la animacion del jugador
        player.GetComponent<Animator>().applyRootMotion = false; // Desactivar la root del jugador

    }
}
