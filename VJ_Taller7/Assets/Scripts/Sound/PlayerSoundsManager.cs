using System.Collections;
using UnityEngine;

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
        _gunManager.ReloadEvent += StopShooting;
        _gunManager.ChangeGun += WeaponChangeSound;
        _gunManager.ShootingEvent += OnShootingEvent;
        _gunManager.StopShootingFeeback += StopShooting;
        _gunManager.buyWeaponEvent += BuyWeapon;
        _gunManager.buyItemEvent += BuyItem;
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
    private bool autoGunShooting;

    private void OnShootingEvent()
    {
        if (!firerateAllowShoot || !_playerController.PlayerCanMove || _gunManager.CurrentGun.realoading) return;
        
        if (_gunManager.CurrentGun.bulletsLeft < 1)
        {
            soundManager.PlaySound("EmptyMAG");
            return;
        }

        if(_gunManager.CurrentGun.ShootConfig.IsAutomatic)
        {
            SelectAutomaticWeaponSound();
        }
        else
        {
            SelectNonAutomaticWeaponSound();
        }
    }

    #region -----Automatic Guns-----
    private void SelectAutomaticWeaponSound()
    {
        autoGunShooting = true;

        switch (_gunManager.CurrentGun.Type)
        {
            case GunType.Rifle:
                StartCoroutine(PlayAutoWeaponSound("rifleFire"));
                break;
        }
        StartCoroutine(FirerateDelay());
    }

    private IEnumerator PlayAutoWeaponSound(string soundName)
    {
        float delay = _gunManager.CurrentGun.ShootConfig.FireRate;

        while (autoGunShooting)
        {
            soundManager.PlaySound(soundName);
            yield return new WaitForSeconds(delay);
        }
    }
    #endregion

    #region -----Non Automatic Guns-----
    private void SelectNonAutomaticWeaponSound()
    {
        switch (_gunManager.CurrentGun.Type)
        {
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
                if (!brokenFeather)
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
        StartCoroutine(FirerateDelay());
    }
    #endregion

    // Limita la emision de sonidos al firerate del arma
    private IEnumerator FirerateDelay()
    {
        firerateAllowShoot = false;

        float delay = _gunManager.CurrentGun.ShootConfig.FireRate;
        yield return new WaitForSeconds(delay);

        firerateAllowShoot = true;
    }

    private void StopShooting()
    {
        autoGunShooting = false;
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
    public void WeaponChangeSound()
    {
        soundManager.PlaySound("ChangeGun");
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

    #region -----Buy Things-----
    private void BuyWeapon(bool enoughScore)
    {
        if(enoughScore)
            soundManager.PlaySound("BuyItem");
        else
            soundManager.PlaySound("CantBuyItem");
    }

    private void BuyItem(PickeableType type)
    {
        switch (type)
        {
            case PickeableType.Ammo:
                soundManager.PlaySound("AmmoPickable");
                break;
            case PickeableType.Healing:
                soundManager.PlaySound("HalthPickable");
                break;
        }
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