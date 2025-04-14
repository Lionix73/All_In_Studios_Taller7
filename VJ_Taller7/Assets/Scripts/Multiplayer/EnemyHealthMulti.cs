using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
public class EnemyHealthMulti : NetworkBehaviour
{
    [Header("Enemy UI")]
    [SerializeField] GameObject floatingTextPrefab;
    [SerializeField] private ProgressBar healthBar;
    public ProgressBar HealthBar
    {
        get => healthBar;
        set => healthBar = value;
    }

    [Header("Enemy Health")]
    [SerializeField] private int health = 100;

    [SerializeField] private NetworkVariable<int> HealthNet = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);



    private int maxHealth;
    public NetworkVariable<int> MaxHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        HealthNet.Value = health;
        maxHealth = HealthNet.Value;
        MaxHealth.Value = maxHealth;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Rpc(SendTo.Server)]
    public void TakeDamageRpc(int damage)
    {
        health -= damage;
        HealthNet.Value = health;

        TakeDamageUIRpc(damage);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void TakeDamageUIRpc(int damage)
    {
        if (floatingTextPrefab != null)
        {
            ShowFloatingText(damage);
        }

        healthBar.SetProgress(HealthNet.Value / MaxHealth.Value, 3);
    }
    public void SetUpHealthBar(Canvas canvas, Camera mainCamera)
    {
        healthBar.transform.SetParent(canvas.transform);
        healthBar.gameObject.SetActive(true);

        if (healthBar.TryGetComponent<FaceCamera>(out FaceCamera faceCamera))
        {
            faceCamera.Camera = mainCamera;
        }

        healthBar.SetProgress(HealthNet.Value / maxHealth, 3);
    }
    private void ShowFloatingText(float damage)
    {
        var go = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity);
        go.GetComponentInChildren<TextMeshPro>().text = damage.ToString();
    }
}
