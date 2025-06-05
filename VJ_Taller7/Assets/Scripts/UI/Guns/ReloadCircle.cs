using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ReloadCircle : MonoBehaviour
{
    private Image _reloadCircle;
    private GunManager _gunManager;

    private void Awake()
    {
        _reloadCircle = GetComponent<Image>();
    }

    private void Start()
    {
        _gunManager = FindFirstObjectByType<GunManager>();
        _gunManager.ReloadEvent += ReloadCircleUI;
    }

    private void ReloadCircleUI()
    {
        StartCoroutine(DecreaseCircle(_gunManager.CurrentGun.ReloadTime));
    }

    public IEnumerator DecreaseCircle(float reloadTime)
    {
        if (reloadTime > 0)
        {
            _reloadCircle.fillAmount = 1;

            float epsilon = 0f;

            while (epsilon < reloadTime)
            {
                epsilon += Time.deltaTime;
                float t = Mathf.Clamp01(epsilon / reloadTime);

                _reloadCircle.fillAmount = Mathf.Lerp(1, 0, t);
                yield return null;
            }
        }

        _reloadCircle.fillAmount = 0;
        yield return null;
    }
}
