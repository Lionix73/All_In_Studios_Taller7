using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSoundsManager : MonoBehaviour
{
    private ThisObjectSounds soundManager;
    private GunManager _gunManager;
    private PlayerController _playerController;

    private void Awake()
    {
        soundManager = GetComponent<ThisObjectSounds>();
        _gunManager = FindAnyObjectByType<GunManager>();
        _playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        _gunManager.ReloadEvent += ReloadingFeedback;
        _playerController.JumpingEvent += JumpSound;
        _playerController.MeleeAttackEvent += MeleeSound;
        _playerController.SlidingEvent += SlideSound;
    }

    private void Update()
    {
        MovSounds();

        if(_gunManager.CurrentGun.bulletsLeft < 1) soundManager.StopSound("rifleFire");
    }

    #region -----Movement-----
    private void MovSounds()
    {
        if (!_playerController.PlayerCanMove)
        {
            soundManager.StopSound("Walk", "Run");
            return;
        }

        if(_playerController.PlayerIsMoving)
        {
            StopEmoteMusic();
            soundManager.StopSound("Idle");

            if (_playerController.PlayerIsRunning && _playerController.PlayerInGround)
            {
                soundManager.PlaySound("Run");
                soundManager.StopSound("Walk");
            }
            else if (_playerController.PlayerInGround)
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
    #endregion

    #region -----SHOOTING-----

    public bool brokenFeather;
    private bool firerateAllowShoot = true;

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (!_gunManager.GunCanShoot || !firerateAllowShoot || !_playerController.PlayerCanMove || _gunManager.CurrentGun.realoading) return;

        if(context.started && _gunManager.CurrentGun.bulletsLeft < 1)
        {
            soundManager.PlaySound("EmptyMAG");
            return;
        }

        if (context.started)
        {
            switch (_gunManager.CurrentGun.Type)
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

        float delay = _gunManager.CurrentGun.ShootConfig.FireRate;
        yield return new WaitForSeconds(delay);

        firerateAllowShoot = true;
    }
    #endregion

    #region -----RELOAD-----
    private void ReloadingFeedback()
    {
        switch (_gunManager.CurrentGun.Type)
        {
            case GunType.Rifle:
                soundManager.PlaySound("rifleReload");
                break;
            case GunType.BasicPistol:
                soundManager.PlaySound("pistolReload");
                break;
            case GunType.Revolver:
                soundManager.PlaySound("revolverReload");
                break;
            case GunType.Shotgun:
                soundManager.PlaySound("shotgunReload");
                break;
            case GunType.Sniper:
                soundManager.PlaySound("sniperReload");
                break;
            case GunType.ShinelessFeather:
                break;
            case GunType.GoldenFeather:
                soundManager.PlaySound("featherReturn");
                break;
            case GunType.GranadeLaucher:
                soundManager.PlaySound("GLReload");
                break;
            case GunType.AncientTome:
                break;
            case GunType.Crossbow:
                soundManager.PlaySound("CrossbowReload");
                break;
            case GunType.MysticCanon:
                break;
        }
    }
    #endregion

    #region -----Input Actions-----
    public void WeaponChangeSound(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            soundManager.PlaySound("ChangeGun");
        }
    }

    private void MeleeSound()
    {
        soundManager.PlaySound("Melee");
    }

    private void SlideSound()
    {
        soundManager.PlaySound("Slide");
        StartCoroutine(Sliding());
    }

    private IEnumerator Sliding()
    {
        float timer = 0f;
        while (timer < _playerController.SlideDuration)
        {
            soundManager.StopSound("Run");
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void JumpSound()
    {
        soundManager.StopSound("Walk", "Run");
        soundManager.PlaySound("Jump");
    }
    #endregion

    #region -----EMOTES-----
    private string activeEmoteMusic;

    public void EmoteMusic(string musicName)
    {
        StopEmoteMusic();
        activeEmoteMusic = musicName;

        if(_playerController.PlayerCanMove)
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