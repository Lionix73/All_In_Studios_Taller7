using System.Collections;
using UnityEngine;

public class MultiHomingBullet : MultiBulletEnemy
{
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private AnimationCurve noiseCurve;
    [SerializeField] private float yOffSet = 0.2f;
    [SerializeField] private Vector2 MinNoise = new Vector2(-1f, -0.25f);
    [SerializeField] private Vector2 MaxNoise = new Vector2(2f, 0.8f);

    private Coroutine homingCoroutine;

    public override void Spawn(Vector3 forward, int damage, Transform target)
    {
        this.damage = damage;
        this.target = target;

        if(homingCoroutine != null){
            StopCoroutine(homingCoroutine);
        }

        homingCoroutine = StartCoroutine(FindTarget());
    }

    private IEnumerator FindTarget(){
        Vector3 startPos = transform.position;
        Vector2 noise = new Vector2(Random.Range(MinNoise.x, MaxNoise.x), Random.Range(MinNoise.y, MaxNoise.y));

        Vector3 randomOffset = new Vector3(
            Random.Range(-2f, 2f), // Adjust these values to control horizontal inaccuracy
            Random.Range(-2f, 2f), // Adjust these values to control vertical inaccuracy
            Random.Range(-2, 2f)  // Adjust these values to control depth inaccuracy
        );

        Vector3 targetWithOffset = target.position + new Vector3(0, yOffSet, 0) + randomOffset;

        Vector3 bulletDirectionVector = targetWithOffset - startPos;
        Vector3 horizontalNoiseVector = Vector3.Cross(bulletDirectionVector, Vector3.up).normalized;
        float noisePosition = 0;

        float time = 0;

        while (time < 1){
            noisePosition = noiseCurve.Evaluate(time);
            transform.position = Vector3.Lerp(startPos, target.position + new Vector3(0, yOffSet, 0), curve.Evaluate(time)) + new Vector3(horizontalNoiseVector.x * noisePosition * noise.x, noisePosition * noise.y, noisePosition * horizontalNoiseVector.z * noise.x);
            transform.LookAt(target.position + new Vector3(0, yOffSet, 0));

            time += Time.deltaTime * moveSpeed;

            yield return null;
        }
    }
}
