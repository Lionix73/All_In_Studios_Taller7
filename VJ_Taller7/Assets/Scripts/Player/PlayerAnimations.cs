using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAnimations : MonoBehaviour
{
    [Header("Animator Controllers")]
    [SerializeField] private List<RuntimeAnimatorController> _animators;

    [Header("Animator Variables")]
    [SerializeField] private FloatDampener speedX;
    [SerializeField] private FloatDampener speedY;
    /*[SerializeField] private FloatDampener layersDampener1;
    //[SerializeField] private FloatDampener layersDampener2;
    //private int animationLayerToShow = 0;*/

    #region Private Variables
    private bool isEmoting;
    private GunType lastGun;
    #endregion

    #region Private Components
    private GunManager _gunManager;
    private PlayerController _playerController;
    private Animator _animator;
    private Rig _rig;
    private Health _health;
    #endregion

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _gunManager = transform.root.GetComponentInChildren<GunManager>();
        _rig = GetComponentInChildren<Rig>();
        _animator = GetComponent<Animator>();
        _health = GetComponent<Health>();
    }

    private void Start()
    {
        lastGun = _gunManager.CurrentGun.Type;

        _playerController.JumpingEvent += JumpAnimation;
        _playerController.MeleeAttackEvent += MeleeAnimation;
        _gunManager.ReloadEvent += ReloadAnimation;
        _gunManager.ChangeGun += WeaponChangeAnimation;
        _gunManager.ShootingEvent += ShootAnimation;
        _gunManager.StopShootingFeeback += StopShootingAnimation;
        _health.OnPlayerDamage += HitAnimation;
        _health.OnPlayerDeath += DeathAnimation;
    }

    private void Update()
    {
        //UpdateAnimLayer();
        HandleAnimations();
        HandleEmotes();

        if(lastGun != _gunManager.CurrentGun.Type) SelectGunType();
    }

    private void HandleAnimations()
    {
        speedX.Update();
        speedY.Update();
        /*layersDampener1.Update();
        //layersDampener2.Update();
        //ChangeAnimLayer(SelectAnimLayer());*/

        speedX.TargetValue = _playerController.MovInput.x;
        speedY.TargetValue = _playerController.MovInput.y;

        _animator.SetBool("isGrounded", _playerController.PlayerInGround);
        _animator.SetBool("isMoving", _playerController.PlayerIsMoving);
        _animator.SetBool("isRunning", _playerController.PlayerIsRunning);
        _animator.SetBool("isCrouching", _playerController.PlayerIsCrouching);
        _animator.SetBool("isSliding", _playerController.PlayerIsSliding);
        _animator.SetBool("isAiming", _playerController.PlayerIsAiming);
        isEmoting = _animator.GetBool("isEmoting");

        _animator.SetFloat("SpeedX", speedX.CurrentValue);
        _animator.SetFloat("SpeedY", speedY.CurrentValue);
    }

    private void HandleEmotes()
    {
        if ((Vector3)_playerController.MovInput != Vector3.zero)
        {
            _animator.SetBool("isEmoting", false);
        }

        if (isEmoting)
            _rig.weight = 0f;
        else
            _rig.weight = 1f;
    }

    #region -----Animation Layers Management------ NO ESTA EN USO ACTUALMENTE
    /*
    private void ChangeAnimLayer(int index)
    {
        if (animationLayerToShow == index) return;

        animationLayerToShow = index;

        if (Mathf.Abs(layersDampener1.TargetValue - layersDampener1.CurrentValue) <= 0.05f)
        {
            if (layersDampener1.TargetValue == 0)
            {
                layersDampener1.TargetValue = 1;
                layersDampener2.TargetValue = 0;
            }
            else
            {
                layersDampener1.TargetValue = 0;
                layersDampener2.TargetValue = 1;
            }
        }
    }

    private void UpdateAnimLayer()
    {
        _animator.SetLayerWeight(animationLayerToShow, layersDampener1.TargetValue == 1 ? layersDampener1.CurrentValue : layersDampener2.CurrentValue);
        int layersAmount = _animator.GetLayerIndex("Fire");

        for (int i = 0; i < 2; i++)
        {
            if (i != animationLayerToShow && _animator.GetLayerWeight(i) > 0.05f)
            {
                _animator.SetLayerWeight(i, layersDampener1.TargetValue == 0 ? layersDampener1.CurrentValue : layersDampener2.CurrentValue);
            }

            if (i != animationLayerToShow && _animator.GetLayerWeight(i) < 0.05f && _animator.GetLayerWeight(i) > 0f)
            {
                _animator.SetLayerWeight(i, 0);
            }
        }
    }

    private int SelectAnimLayer()
    {
        switch (_gunManager.CurrentGun.Type)
        {
            case GunType.Rifle:
                return 0;

            case GunType.BasicPistol:
                return 1;

            case GunType.Revolver:
                return 1;

            case GunType.Shotgun:
                return 0;

            case GunType.Sniper:
                return 0;

            case GunType.ShinelessFeather:
                return 0;

            case GunType.GoldenFeather:
                return 0;

            case GunType.GranadeLaucher:
                return 0;

            case GunType.AncientTome:
                return 0;

            case GunType.Crossbow:
                return 0;

            case GunType.MysticCanon:
                return 0;
            
            default: 
                return 0;
        }
    }*/
    #endregion

    #region -----GUNS-----
    private void ShootAnimation()
    {
        if (!_playerController.PlayerCanMove || _gunManager.CurrentGun.bulletsLeft < 1) return;

        if (_gunManager.CurrentGun.ShootConfig.IsAutomatic)
        {
            _animator.SetBool("ShootBurst", true);
        }
        else
        {
            _animator.SetTrigger("ShootOnce");
        }
    }

    private void SelectGunType()
    {
        switch (_gunManager.CurrentGun.Type)
        {
            case GunType.Rifle:
                _animator.runtimeAnimatorController = _animators[1];
                break;

            case GunType.BasicPistol:
                _animator.runtimeAnimatorController = _animators[0];
                break;

            case GunType.Revolver:
                _animator.runtimeAnimatorController = _animators[0];
                break;

            case GunType.Shotgun:
                break;

            case GunType.Sniper:
                break;

            case GunType.ShinelessFeather:
                break;

            case GunType.GoldenFeather:
                break;

            case GunType.GranadeLaucher:
                break;

            case GunType.AncientTome:
                break;

            case GunType.Crossbow:
                break;

            case GunType.MysticCanon:
                break;
        }
    }

    public void WeaponChangeAnimation()
    {
        _animator.SetTrigger("ChangeGun");

        SelectGunType();
    }

    public void ReloadAnimation()
    {
        _animator.SetBool("ShootBurst", false);
        _animator.SetTrigger("Reload");
    }

    private void StopShootingAnimation()
    {
        _animator.SetBool("ShootBurst", false);
    }
    #endregion

    #region -----INPUT ACTIONS-----
    private void JumpAnimation()
    {
        StartCoroutine(Jumping());
    }

    private IEnumerator Jumping()
    {
        _animator.applyRootMotion = false;
        _animator.SetTrigger("Jump");
        _animator.SetBool("isJumping", true);

        yield return new WaitForSeconds(_playerController.JumpDuration);

        _animator.SetBool("isJumping", false);
    }

    private void MeleeAnimation()
    {
        _animator.SetTrigger("Melee");
    }
    #endregion

    #region -----Health-----
    private void HitAnimation()
    {
        _animator.SetTrigger("Hit");
    }

    private void DeathAnimation(GameObject _)
    {
        _animator.SetTrigger("Dead");
    }
    #endregion
}
