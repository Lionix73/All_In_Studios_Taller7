using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAnimations : MonoBehaviour
{
    [Header("Animator Controllers")]
    [SerializeField] private List<RuntimeAnimatorController> animators;

    [Header("Animator")]
    [SerializeField] private FloatDampener speedX;
    [SerializeField] private FloatDampener speedY;
    /*[SerializeField] private FloatDampener layersDampener1;
    //[SerializeField] private FloatDampener layersDampener2;
    //private int animationLayerToShow = 0;*/

    private bool isEmoting;
    private GunType lastGun;

    private GunManager _gunManager;
    private PlayerController _playerController;
    private Animator animator;
    private Rig _rig;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _gunManager = transform.root.GetComponentInChildren<GunManager>();
        _rig = GetComponentInChildren<Rig>();
        animator = GetComponent<Animator>();
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

        animator.SetBool("isGrounded", _playerController.PlayerInGround);
        animator.SetBool("isMoving", _playerController.PlayerIsMoving);
        animator.SetBool("isRunning", _playerController.PlayerIsRunning);
        animator.SetBool("isCrouching", _playerController.PlayerIsCrouching);
        animator.SetBool("isSliding", _playerController.PlayerIsSliding);
        animator.SetBool("isAiming", _playerController.PlayerIsAiming);
        isEmoting = animator.GetBool("isEmoting");

        animator.SetFloat("SpeedX", speedX.CurrentValue);
        animator.SetFloat("SpeedY", speedY.CurrentValue);
    }

    private void HandleEmotes()
    {
        if ((Vector3)_playerController.MovInput != Vector3.zero)
        {
            animator.SetBool("isEmoting", false);
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
        animator.SetLayerWeight(animationLayerToShow, layersDampener1.TargetValue == 1 ? layersDampener1.CurrentValue : layersDampener2.CurrentValue);
        int layersAmount = animator.GetLayerIndex("Fire");

        for (int i = 0; i < 2; i++)
        {
            if (i != animationLayerToShow && animator.GetLayerWeight(i) > 0.05f)
            {
                animator.SetLayerWeight(i, layersDampener1.TargetValue == 0 ? layersDampener1.CurrentValue : layersDampener2.CurrentValue);
            }

            if (i != animationLayerToShow && animator.GetLayerWeight(i) < 0.05f && animator.GetLayerWeight(i) > 0f)
            {
                animator.SetLayerWeight(i, 0);
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
            animator.SetBool("ShootBurst", true);
        }
        else
        {
            animator.SetTrigger("ShootOnce");
        }
    }

    private void SelectGunType()
    {
        switch (_gunManager.CurrentGun.Type)
        {
            case GunType.Rifle:
                animator.runtimeAnimatorController = animators[1];
                break;

            case GunType.BasicPistol:
                animator.runtimeAnimatorController = animators[0];
                break;

            case GunType.Revolver:
                animator.runtimeAnimatorController = animators[0];
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
        animator.SetTrigger("ChangeGun");

        SelectGunType();
    }

    public void ReloadAnimation()
    {
        animator.SetBool("ShootBurst", false);
        animator.SetTrigger("Reload");
    }

    private void StopShootingAnimation()
    {
        animator.SetBool("ShootBurst", false);
    }
    #endregion

    #region -----INPUT ACTIONS-----
    private void JumpAnimation()
    {
        StartCoroutine(Jumping());
    }

    private IEnumerator Jumping()
    {
        animator.applyRootMotion = false;
        animator.SetTrigger("Jump");
        animator.SetBool("isJumping", true);

        yield return new WaitForSeconds(_playerController.JumpDuration);

        animator.SetBool("isJumping", false);
    }

    private void MeleeAnimation()
    {
        animator.SetTrigger("Melee");
    }
    #endregion
}
