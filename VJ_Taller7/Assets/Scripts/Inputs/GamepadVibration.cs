using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadVibration : MonoBehaviour
{
    [Header("Vibration Settings")]
    [SerializeField][Range(0, 1)] private float DamageVibrationDuration = 0.3f;

    private Health _health;
    private GunManager _gunManager;
    private bool _autoGunShooting;

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

        GunScriptableObject currentGun = _gunManager.CurrentGun;

        if (currentGun.bulletsLeft < 1) return;

        if (currentGun.ShootConfig.IsAutomatic)
        {
            VibrateOnShootingAuto(currentGun);
        }
        else
        {
            StartCoroutine(Vibrate(currentGun.ShootingVibrationIntensity, currentGun.VibrationDuration));
        }
    }

    #region -----Automatic Guns-----
    private void VibrateOnShootingAuto(GunScriptableObject currentGun)
    {
        _autoGunShooting = true;
        StartCoroutine(VibrateAutomaticWeapon(currentGun));
    }

    private IEnumerator VibrateAutomaticWeapon(GunScriptableObject currentGun)
    {
        float delay = currentGun.ShootConfig.FireRate;

        while (_autoGunShooting)
        {
            StartCoroutine(Vibrate(currentGun.ShootingVibrationIntensity, currentGun.VibrationDuration));
            yield return new WaitForSeconds(delay);
        }
    }

    private void StopShooting()
    {
        _autoGunShooting = false;
    }
    #endregion
    #endregion

    private IEnumerator Vibrate(float strength, float duration)
    {
        Gamepad.current?.SetMotorSpeeds(strength, strength);
        yield return new WaitForSeconds(duration);
        Gamepad.current?.SetMotorSpeeds(0f, 0f);
    }
}
