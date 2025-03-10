using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

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

    public NetworkVariable<int> HealthNet = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);



    private int maxHealth;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HealthNet.Value = health;
        maxHealth = HealthNet.Value;

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

        healthBar.SetProgress(HealthNet.Value / maxHealth, 3);
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
        var go = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity, transform);
        go.GetComponent<TextMesh>().text = damage.ToString();
    }
}
