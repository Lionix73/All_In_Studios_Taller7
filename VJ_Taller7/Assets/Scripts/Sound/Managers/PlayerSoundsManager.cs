using System.Collections;
using FMODUnity;
using UnityEngine;

public class PlayerSoundsManager : MonoBehaviour
{
    #region Private Components
    private ThisObjectSounds _soundManager;
    private GunManager _gunManager;
    private PlayerController _playerController;
    private PlayerAnimations _playerAnimations;
    private Health _health;
    #endregion

    private void Awake()
    {
        _soundManager = GetComponent<ThisObjectSounds>();
        _gunManager = transform.root.GetComponentInChildren<GunManager>();
        _playerController = transform.root.GetComponentInChildren<PlayerController>();
        _playerAnimations = transform.root.GetComponentInChildren<PlayerAnimations>();
        _health = transform.root.GetComponentInChildren<Health>();
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
        _health.OnPlayerDamage += HitSound;
        _health.OnPlayerDeath += DeathSound;
    }

    private void Update()
    {
        MovSounds();
    }

    #region -----Movement-----
    private void MovSounds()
    {
        if (!_playerController.PlayerCanMove)
        {
            _soundManager.StopSound("Walk", "Run");
            return;
        }

        if (_playerController.PlayerIsMoving)
        {
            StopEmoteMusic();
            _soundManager.StopSound("Idle");

            if (_playerController.PlayerIsRunning && _playerController.PlayerInGround)
            {
                _soundManager.PlaySound("Run");
                _soundManager.StopSound("Walk");
            }
            else if (_playerController.PlayerInGround)
            {
                _soundManager.PlaySound("Walk");
                _soundManager.StopSound("Run");
            }
            else
            {
                _soundManager.StopSound("Walk", "Run");
            }
        }
        else
        {
            _soundManager.StopSound("Walk", "Run");

            if (_playerAnimations.IsEmoting)
                _soundManager.StopSound("Idle");
            else
                _soundManager.PlaySound("Idle");
        }
    }
    #endregion

    #region -----SHOOTING-----
    public bool brokenFeather { get; set; }
    private bool autoGunShooting;

    private void OnShootingEvent()
    {
        if (!_playerController.PlayerCanMove) return;

        if (_gunManager.CurrentGun.bulletsLeft < 1)
        {
            RuntimeManager.PlayOneShot(_gunManager.CurrentGun.NoAmmoSound, transform.position);
            return;
        }

        if (_gunManager.CurrentGun.ShootConfig.IsAutomatic)
        {
            SelectAutomaticWeaponSound();
        }
        else
        {
            RuntimeManager.PlayOneShot(_gunManager.CurrentGun.ShootSound, transform.position);
        }
    }

    #region -----Automatic Guns-----
    private void SelectAutomaticWeaponSound()
    {
        autoGunShooting = true;
        StartCoroutine(PlayAutoWeaponSound());
    }

    private IEnumerator PlayAutoWeaponSound()
    {
        float delay = _gunManager.CurrentGun.ShootConfig.FireRate;

        while (autoGunShooting)
        {
            RuntimeManager.PlayOneShot(_gunManager.CurrentGun.ShootSound, transform.position);
            yield return new WaitForSeconds(delay);
        }
    }
    #endregion

    private void StopShooting()
    {
        autoGunShooting = false;
    }
    #endregion

    #region -----RELOAD-----
    private void ReloadingFeedback()
    {
        RuntimeManager.PlayOneShot(_gunManager.CurrentGun.ReloadSound, transform.position);
    }
    #endregion

    #region -----Input Actions-----
    public void WeaponChangeSound()
    {
        _soundManager.PlaySound("ChangeGun");
    }

    private void MeleeSound()
    {
        _soundManager.PlaySound("Melee");
    }

    private void SlideSound()
    {
        _soundManager.PlaySound("Slide");
        StartCoroutine(Sliding());
    }

    private IEnumerator Sliding()
    {
        float timer = 0f;
        while (timer < _playerController.SlideDuration)
        {
            _soundManager.StopSound("Run");
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void JumpSound()
    {
        _soundManager.StopSound("Walk", "Run");
        _soundManager.PlaySound("Jump");
    }
    #endregion

    #region -----Buy Things-----
    private void BuyWeapon(bool enoughScore)
    {
        if (enoughScore)
            _soundManager.PlaySound("BuyItem");
        else
            _soundManager.PlaySound("CantBuyItem");
    }

    private void BuyItem(PickeableType type)
    {
        switch (type)
        {
            case PickeableType.Ammo:
                _soundManager.PlaySound("AmmoPickable");
                break;
            case PickeableType.Healing:
                _soundManager.PlaySound("HalthPickable");
                break;
        }
    }
    #endregion

    #region -----Health-----
    private void HitSound()
    {
        _soundManager.PlaySound("Hit");
    }

    private void DeathSound(GameObject _)
    {
        StartCoroutine(Death());
    }

    private IEnumerator Death()
    {
        _soundManager.StopAllSounds();
        yield return new WaitForSeconds(0.1f);
        _soundManager.PlaySound("Dead");
    }
    #endregion

    #region -----EMOTES-----
    private string activeEmoteMusic;

    public void EmoteMusic(string musicName)
    {
        StopEmoteMusic();
        activeEmoteMusic = musicName;

        if (_playerController.PlayerCanMove)
        {
            _soundManager.PlaySound(musicName);
        }
    }

    public void StopEmoteMusic()
    {
        _soundManager.StopSound(activeEmoteMusic);
    }
    #endregion
}