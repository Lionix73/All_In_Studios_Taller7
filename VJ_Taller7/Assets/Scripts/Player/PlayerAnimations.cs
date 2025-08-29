using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAnimations : MonoBehaviour
{
    [Header("Animator Variables")]
    [SerializeField] private FloatDampener speedX;
    [SerializeField] private FloatDampener speedY;

    #region Private Variables
    private bool isEmoting;
    public bool IsEmoting => isEmoting;
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
        HandleAnimations();
        HandleEmotes();

        if (_playerController.PlayerInGround)
            _animator.applyRootMotion = true;
    }

    private void HandleAnimations()
    {
        speedX.Update();
        speedY.Update();

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
        _animator.runtimeAnimatorController = _gunManager.CurrentGun.AnimatorController;
    }

    public void WeaponChangeAnimation()
    {
        SelectGunType();
        _animator.SetTrigger("ChangeGun");
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
