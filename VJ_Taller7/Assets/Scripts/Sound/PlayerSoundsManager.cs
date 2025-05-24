using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSoundsManager : MonoBehaviour
{
    private ThisObjectSounds soundManager;
    private GunManager gunManager;
    private PlayerController pControl;

    private void Awake()
    {
        soundManager = GetComponent<ThisObjectSounds>();
        gunManager = FindAnyObjectByType<GunManager>();
        pControl = GetComponent<PlayerController>();
    }

    private void Update()
    {
        MovSounds();
    }

    private void MovSounds()
    {
        if (!pControl.PlayerCanMove)
        {
            soundManager.StopSound("Walk", "Run");
            return;
        }

        if(pControl.PlayerIsMoving)
        {
            StopEmoteMusic();
            soundManager.StopSound("Idle");

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
            soundManager.PlaySound("Idle");
            soundManager.StopSound("Walk", "Run");
        }
    }

    #region -----SHOOTING SOUNDS-----

    public bool brokenFeather;
    private bool firerateAllowShoot = true;

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (!gunManager.canShoot || !firerateAllowShoot || !pControl.PlayerCanMove || gunManager.CurrentGun.bulletsLeft < 1) return;

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
                    soundManager.PlaySound("GLFire");
                    break;
                case GunType.AncientTome:
                    break;
                case GunType.Crossbow:
                    soundManager.PlaySound("CrossbowFire");
                    break;
                case GunType.MysticCanon:
                    break;
            }
        }

        if (context.canceled)
        {
            soundManager.StopSound("rifleFire");
        }

        StartCoroutine(FirerateDelay());
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

    #region -----EMOTE SOUNDS-----

    private string activeEmoteMusic;

    public void EmoteMusic(string musicName)
    {
        StopEmoteMusic();
        activeEmoteMusic = musicName;

        if(pControl.PlayerCanMove)
        {
            soundManager.PlaySound(musicName);
        }
    }

    public void StopEmoteMusic()
    {
        soundManager.StopSound(activeEmoteMusic);
    }

    #endregion
}
