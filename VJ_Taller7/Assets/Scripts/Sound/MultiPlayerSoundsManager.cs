using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiPlayerSoundsManager : NetworkBehaviour
{
    private SoundManager soundManager;
    private GunManagerMulti2 gunManager;
    private PlayerControllerMulti pControl;

    public bool brokenFeather;

    private bool firerateAllowShoot;

    private void Awake()
    {
        soundManager = FindAnyObjectByType<SoundManager>();
        gunManager = FindAnyObjectByType<GunManagerMulti2>();
        pControl = GetComponent<PlayerControllerMulti>();
    }

    private void Update()
    {
        MovSounds();
    }

    private void MovSounds()
    {
        if(!pControl.PlayerCanMove) soundManager.StopSound("Walk", "Run");

        if(pControl.PlayerIsMoving)
        {
            soundManager.StopSound("GangamStyle");

            if (pControl.PlayerRunning && pControl.PlayerInGround)
            {
                soundManager.PlaySound("Run");
                soundManager.StopSound("Walk");
            }
            else if (pControl.PlayerInGround)
            {
                soundManager.PlaySound("Walk");
                soundManager.StopSound("Run");
            }
            else
            {
                soundManager.StopSound("Walk", "Run");
            }
        }
        else
        {
            soundManager.StopSound("Walk", "Run");
        }
    }

    #region Shooting Sounds
    public void OnShoot(InputAction.CallbackContext context)
    {
       // if (!gunManager.canShoot || !firerateAllowShoot) return;

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
                    if(!brokenFeather)
                        soundManager.PlaySound("featherThrow");
                    else
                        soundManager.PlaySound("noFeather");
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

        StartCoroutine(FirerateDelay());
    }

    [Rpc(SendTo.Everyone)]
    public void SoundStateRpc(string soundName, string state)
    {
        if (state == "Play")
        {
            soundManager.PlaySound(soundName);
        }
        else if (state == "Stop")
        {
            soundManager.StopSound(soundName);
        }
    }

    // Limita la emision de sonidos al firerate del arma
    IEnumerator FirerateDelay()
    {
        firerateAllowShoot = false;

        float delay = gunManager.CurrentGun.ShootConfig.FireRate;
        yield return new WaitForSeconds(delay);

        firerateAllowShoot = true;
    }
    #endregion
}
