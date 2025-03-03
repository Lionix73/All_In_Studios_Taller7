using Fusion;
using Projectiles.ProjectileDataBuffer_Hitscan;
using UnityEngine;

public class HealthPhoton : NetworkBehaviour
{

    public bool IsAlive => CurrentHealth > 0f;
    public float MaxHealth => _maxHealth;
    
    [Networked]
    public float CurrentHealth { get; set; }


    [SerializeField]
    private float _maxHealth = 100f;
    [SerializeField] public GameObject floatingTextPrefab;
    [SerializeField] public ProgressBar healthBar;
    private float _damage;
    private float _hitPos;

    public override void Spawned()
    {
        SetHealth(_maxHealth);
        //UIManager.Singleton.GetPlayerHealth(currentHealth, maxHealth);
    }


    private float AddHealth(float amount)
    {
        float previousHealth = CurrentHealth;
        SetHealth(CurrentHealth + amount);
        return CurrentHealth - previousHealth;
    }

    private float RemoveHealth(float amount)
    {
        float previousHealth = CurrentHealth;
        SetHealth(CurrentHealth - amount);
        return previousHealth - CurrentHealth;
    }

    private void SetHealth(float health)
    {
        CurrentHealth = Mathf.Clamp(health, 0, _maxHealth);
        //healthBar.SetProgress(CurrentHealth / MaxHealth);

    }


    public void TakeDamage(float damage)
    {
        RemoveHealth(damage);
        
      // ShowFloatingText(projectileData.Damage, projectileData.HitPosition);

    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void ShowFloatingText(float damage, Vector3 hitPosition)
    {
        var go = Instantiate(floatingTextPrefab, hitPosition, Quaternion.identity);
        go.GetComponent<TextMesh>().text = damage.ToString();
        if (Runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Multiple)
        {
            Runner.MoveToRunnerScene(go);
            Runner.AddVisibilityNodes(go.gameObject);
        }
    }

    public void SetUpHealthBar(Canvas canvas, Camera mainCamera)
    {
        healthBar.transform.SetParent(canvas.transform);

        if (healthBar.TryGetComponent<FaceCamera>(out FaceCamera faceCamera))
        {
            faceCamera.Camera = mainCamera;
        }
    }

}
