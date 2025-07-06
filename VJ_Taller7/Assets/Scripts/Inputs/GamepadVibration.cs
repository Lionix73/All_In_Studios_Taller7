using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadVibration : MonoBehaviour
{
    [Header("Vibration Settings")]
    [SerializeField][Range(0, 1)] private float DamageVibrationDuration = 0.3f;
    [SerializeField][Range(0, 1)] private float ShootVibrationDuration = 0.1f;

    private Health _health;
    private GunManager _gunManager;
    private bool autoGunShooting;

    private void Awake()
    {
        _health = transform.root.GetComponentInChildren<Health>();
        _gunManager = transform.root.GetComponentInChildren<GunManager>();
    }

    private void OnEnable()
    {
        _health.OnHealthChanged += VibrateOnDamage;
        _gunManager.ShootingEvent += VibrateOnShootingEvent;
        _gunManager.StopShootingFeeback += StopShooting;
        _gunManager.ReloadEvent += StopShooting;
    }

    private void OnDisable()
    {
        Gamepad.current?.SetMotorSpeeds(0f, 0f);

        _health.OnHealthChanged -= VibrateOnDamage;
        _gunManager.ShootingEvent -= VibrateOnShootingEvent;
        _gunManager.StopShootingFeeback -= StopShooting;
        _gunManager.ReloadEvent -= StopShooting;
    }

    private void VibrateOnDamage(float a, float b)
    {
        float intensity = Mathf.Clamp01(a / b);
        float vibrationStrength = Mathf.Lerp(1f, 0f, intensity);

        StartCoroutine(Vibrate(vibrationStrength, DamageVibrationDuration));
    }

    #region -----Vibration Shooting-----
    private void VibrateOnShootingEvent()
    {
        if (_health.isDead) return;

        if (_gunManager.CurrentGun.bulletsLeft < 1)
        {
            StartCoroutine(Vibrate(0.1f, ShootVibrationDuration));
            return;
        }

        if (_gunManager.CurrentGun.ShootConfig.IsAutomatic)
        {
            VibrateOnShootingAuto();
        }
        else
        {
            VibrateOnShootingNonAuto();
        }
    }

    #region -----Automatic Guns-----
    private void VibrateOnShootingAuto()
    {
        autoGunShooting = true;

        switch (_gunManager.CurrentGun.Type)
        {
            case GunType.Rifle:
                StartCoroutine(VibrateAutomaticWeapon(0.2f));
                break;
        }
    }

    private IEnumerator VibrateAutomaticWeapon(float intensity)
    {
        float delay = _gunManager.CurrentGun.ShootConfig.FireRate;

        while (autoGunShooting)
        {
            StartCoroutine(Vibrate(intensity, ShootVibrationDuration));
            yield return new WaitForSeconds(delay);
        }
    }

    private void StopShooting()
    {
        autoGunShooting = false;
    }
    #endregion

    private void VibrateOnShootingNonAuto()
    {
        switch (_gunManager.CurrentGun.Type)
        {
            case GunType.BasicPistol:
                StartCoroutine(Vibrate(0.4f, ShootVibrationDuration));
                break;
            case GunType.Revolver:
                StartCoroutine(Vibrate(0.6f, ShootVibrationDuration));
                break;
            case GunType.Shotgun:
                StartCoroutine(Vibrate(0.7f, ShootVibrationDuration));
                break;
            case GunType.Sniper:
                StartCoroutine(Vibrate(0.8f, ShootVibrationDuration));
                break;
            case GunType.ShinelessFeather:
                StartCoroutine(Vibrate(0.2f, ShootVibrationDuration));
                break;
            case GunType.GoldenFeather:
                StartCoroutine(Vibrate(0.2f, ShootVibrationDuration));
                break;
            case GunType.GranadeLaucher:
                StartCoroutine(Vibrate(0.3f, ShootVibrationDuration));
                break;
            case GunType.Crossbow:
                StartCoroutine(Vibrate(0.3f, ShootVibrationDuration));
                break;
        }
    }
    #endregion

    private IEnumerator Vibrate(float strength, float duration)
    {
        Gamepad.current?.SetMotorSpeeds(strength, strength);
        yield return new WaitForSeconds(duration);
        Gamepad.current?.SetMotorSpeeds(0f, 0f);
    }
}
