using UnityEngine;
using UnityEngine.UI;

public class CrosshairManagerMulti : MonoBehaviour
{
    public Image crosshairImage;
    [SerializeField] private Transform aimTarget;
    private GunManagerMulti2 gunManager;

    private void Awake() {
        gunManager = GetComponent<GunManagerMulti2>();
        //crosshairImage = GameObject.Find("CrossHair").GetComponent<Image>();
    }

    private void Update() {
        UpdateCrosshair();
    }

    /// <summary>
    /// Actualiza la posición del target que tiene rigueado el apuntar del arma, y la posición del crosshair.
    /// </summary>
    private void UpdateCrosshair(){
        Vector3 gunTipPoint = gunManager.weapon.GetRaycastOrigin();
        //Vector3 forward = gunManager.CurrentGun.GetGunForward();
        Vector3 forward = gunManager.camera.transform.forward;

        Vector3 hitPoint = gunTipPoint + forward * gunManager.weapon.TrailConfig.MissDistance;
        if (Physics.Raycast(gunTipPoint,forward, out RaycastHit hit, float.MaxValue, gunManager.weapon.ShootConfig.HitMask))
        {
            hitPoint = hit.point;
        }

        if (aimTarget==null) return;
        aimTarget.transform.position = hitPoint;

        if (crosshairImage == null) return;
        crosshairImage.rectTransform.anchoredPosition = Vector2.zero;
    }

    public void SetCrosshairImage(Sprite image){
        crosshairImage.sprite = image;
    }
}
