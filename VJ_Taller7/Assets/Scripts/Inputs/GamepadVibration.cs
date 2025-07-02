using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadVibration : MonoBehaviour
{
    [Header("Vibration Settings")]
    public float vibrationDuration = 0.3f;

    private Health health;

    private void Awake()
    {
        health = transform.root.GetComponentInChildren<Health>();
    }

    private void Start()
    {
        health.OnHealthChanged += VibrateOnDamage;
    }

    private void VibrateOnDamage(float a, float b)
    {
        float intensity = Mathf.Clamp01(a / b);
        float vibrationStrength = Mathf.Lerp(1f, 0f, intensity);

        StartCoroutine(VibrateRoutine(vibrationStrength, vibrationDuration));
    }

    private IEnumerator VibrateRoutine(float strength, float duration)
    {
        Gamepad.current?.SetMotorSpeeds(strength, strength);
        yield return new WaitForSeconds(duration);
        Gamepad.current?.SetMotorSpeeds(0f, 0f);
    }

    private void OnDisable()
    {
        Gamepad.current?.SetMotorSpeeds(0f, 0f);
        health.OnHealthChanged -= VibrateOnDamage;
    }
}
