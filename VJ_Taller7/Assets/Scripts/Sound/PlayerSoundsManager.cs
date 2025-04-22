using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSoundsManager : MonoBehaviour
{
    private SoundManager soundManager;
    private GunManager gunManager;
    private PlayerController playerController;

    private void Awake()
    {
        soundManager = FindAnyObjectByType<SoundManager>();
        gunManager = FindAnyObjectByType<GunManager>();
        playerController = GetComponent<PlayerController>();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (!gunManager.canShoot) return;

        if (context.started)
        {
            switch (gunManager.CurrentGun.Type)
            {
                case GunType.Rifle:
                    soundManager.PlaySound("rifleFire");
                    break;
                case GunType.BasicPistol:
                    soundManager.PlaySound("pistolFire");
                    break;
                case GunType.Revolver:
                    soundManager.PlaySound("revolverFire");
                    break;
                case GunType.Shotgun:
                    soundManager.PlaySound("shotgunFire");
                    break;
                case GunType.Sniper:
                    soundManager.PlaySound("sniperFire");
                    break;
                case GunType.ShinelessFeather:
                    soundManager.PlaySound("featherThrow");
                    break;
                case GunType.GoldenFeather:
                    soundManager.PlaySound("featherThrow");
                    break;
                case GunType.GranadeLaucher:
                    soundManager.PlaySound("featherThrow");
                    break;
                case GunType.AncientTome:
                    soundManager.PlaySound("featherThrow");
                    break;
                case GunType.Crossbow:
                    soundManager.PlaySound("featherThrow");
                    break;
                case GunType.MysticCanon:
                    soundManager.PlaySound("featherThrow");
                    break;
            }
        }

        if (context.canceled)
        {
            soundManager.StopSound("rifleFire");
        }
    }
}
