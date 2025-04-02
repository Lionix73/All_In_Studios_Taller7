using UnityEngine;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour
{
    public Image crosshairImage;
    [SerializeField] private Transform aimTarget;
    private GunManager gunManager;

    private void Awake() {
        gunManager = GetComponent<GunManager>();
        aimTarget = GameObject.Find("GunAimPoint").transform;
        crosshairImage = GameObject.Find("CrossHair").GetComponent<Image>();
    }

    private void Update() {
        UpdateCrosshair();
    }

    /// <summary>
    /// Actualiza la posición del target que tiene rigueado el apuntar del arma, y la posición del crosshair.
    /// </summary>
    private void UpdateCrosshair(){
        Vector3 gunTipPoint = gunManager.CurrentGun.GetRaycastOrigin();
        //Vector3 forward = gunManager.CurrentGun.GetGunForward();
        Vector3 forward = gunManager.Camera.transform.forward;

        Vector3 hitPoint = gunTipPoint + forward * gunManager.CurrentGun.TrailConfig.MissDistance;
        if (Physics.Raycast(gunTipPoint,forward, out RaycastHit hit, float.MaxValue, gunManager.CurrentGun.ShootConfig.HitMask))
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

    public void AimingZoomIn(){
        crosshairImage.rectTransform.localScale = new Vector3(20,20,1);
    }
    public void AimingZoomOut(){
        crosshairImage.rectTransform.localScale = new Vector3(1,1,1);
    }
}
