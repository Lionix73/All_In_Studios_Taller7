using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairManagerMulti : NetworkBehaviour
{
    public Image crosshairImage;
    [SerializeField] private Transform aimTarget;
    [SerializeField] private GunManagerMulti2 gunManager;


    private void Update() {
        if (!IsOwner) return;
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
        SetTargetTransformRpc(hitPoint);

        if (crosshairImage == null) return;
        crosshairImage.rectTransform.anchoredPosition = Vector2.zero;
    }

    [Rpc(SendTo.Everyone)]
    public void SetTargetTransformRpc(Vector3 hitpoint)
    {
        aimTarget.transform.position =hitpoint;
    }
    public void SetCrosshairImage(Sprite image){
        crosshairImage.sprite = image;
    }
}
