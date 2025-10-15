using PrimeTween;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Hands : MonoBehaviour
{
    [SerializeField]
    private Transform _body = null;

    [SerializeField]
    private Vector3 _offset = Vector3.zero;
    [SerializeField]
    private Vector3 _offset1 = Vector3.zero;

    [SerializeField]
    private ParticleSystem _vfxCloud = null;

    private Item _heldItemMain = null;
    private Item _heldItemOff = null;

    private PhysicsFollower _owner = null;

    private void Start()
    {
        _heldItemMain = ItemDatabase.Instance.CreateInstance(0);
        _heldItemOff = ItemDatabase.Instance.CreateInstance(1);

        _owner = GetComponent<PhysicsFollower>();
        _owner.ChangedDirection += OnChangedDirection;
    }

    private void OnChangedDirection(bool facingRight)
    {
        if (_heldItemMain != null)
            _heldItemMain.transform.localRotation = Quaternion.Euler(0f, facingRight ? 0f : 180f, 0f);

        if (_heldItemOff != null)
            _heldItemOff.transform.localRotation = Quaternion.Euler(0f, facingRight ? 0f : 180f, 0f);
    }

    private void Update()
    {
        if (_heldItemMain != null)
        {
            _heldItemMain.transform.position = _body.position + _body.right * _offset.x + _body.up * _offset.y;
        }

        if (_heldItemOff != null)
        {
            _heldItemOff.transform.position = _body.position + _body.right * _offset1.x + _body.up * _offset1.y;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (_heldItemMain != null)
            {
                Attack(_heldItemMain, InputType.Tap);
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (_heldItemOff != null)
            {
                Attack(_heldItemOff, InputType.Tap);
            }
        }
    }

    private void Attack(Item item, InputType inputType)
    {
        WeaponCommand command = new WeaponCommand(
            item.ItemData.WeaponType,
            inputType,
            Time.time
            );

        var attackToUse = AttackManager.Instance.GetNextAttack(command);


        //_owner.Animator.Play("Player_StepForward", 1, 0f);
        //Tween.Position(transform, transform.position + transform.right * 0.3f, 0.2f);
        //var cloud = Instantiate(_vfxCloud, _owner.transform);
        //cloud.transform.localPosition = Vector3.right * 0.3f;
        //var asd = cloud.main;
        //asd.startDelay = 0.1f;
        //cloud.Play();

        if (attackToUse.name == "Sword_StabSlow")
        {
            _owner.Animator.Play("Player_Windup", 1, 0f);

            Tween.Delay(0.5f).OnComplete(() =>
            {
                _owner.Animator.Play("Player_StepForward", 1, 0f);
                Tween.Position(transform, transform.position + transform.right * 0.5f, 0.2f);
                item.Animator.Play(attackToUse.AnimationName, 0, 0f);
            });

            _owner.transform.GetChild(0).GetComponent<Hitbox>().Activate(attackToUse);

            if (attackToUse.VFX != null)
            {
                CreateVFX(attackToUse);
            }
        }
        else if (attackToUse.name == "Shield_Retreat")
        {
            item.Animator.Play(attackToUse.AnimationName, 0, 0f);
            _owner.transform.GetChild(0).GetComponent<Hitbox>().Activate(attackToUse);

            Tween.Delay(0.2f).OnComplete(() =>
            {
                _owner.Animator.Play("Player_JumpLow", 1, 0f);
                Tween.Position(transform, transform.position + transform.right * -1f, 0.3f);
            });

            if (attackToUse.VFX != null)
            {
                CreateVFX(attackToUse);
            }
        }
        else if (attackToUse.name == "Shield_Uppercut")
        {
            item.Animator.Play(attackToUse.AnimationName, 0, 0f);
            _owner.transform.GetChild(0).GetComponent<Hitbox>().Activate(attackToUse);
            _owner.Animator.Play("Player_JumpLow", 1, 0f);

            if (attackToUse.VFX != null)
            {
                CreateVFX(attackToUse);
            }
        }
        else
        {
            _owner.Animator.Play("Player_StepForward", 1, 0f);
            Tween.Position(transform, transform.position + transform.right * 0.3f, 0.2f);

            item.Animator.Play(attackToUse.AnimationName, 0, 0f);
            _owner.transform.GetChild(0).GetComponent<Hitbox>().Activate(attackToUse);

            if (attackToUse.VFX != null)
            {
                CreateVFX(attackToUse);
            }
        }
    }

    private void CreateVFX(AttackDataSO attackToUse)
    {
        var vfx = Instantiate(attackToUse.VFX, _owner.transform);
        vfx.transform.SetLocalPositionAndRotation(attackToUse.VFXPosition, Quaternion.Euler(0f, _owner.transform.rotation.eulerAngles.y, attackToUse.VFXRotationZ));
        if (attackToUse.VFXScale != Vector3.one)
        {
            vfx.transform.localScale = attackToUse.VFXScale; // scaling pixel art vfx usually looks bad
        }
        if (attackToUse.VFXStartDelay != 0f)
        {
            var variable = vfx.main;
            variable.startDelay = attackToUse.VFXStartDelay;
        }
    }

    private void OnDrawGizmos()
    {
        if (_body != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_body.position + _body.right * _offset.x + _body.up * _offset.y, 0.2f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_body.position + _body.right * _offset1.x + _body.up * _offset1.y, 0.2f);
        }
    }
}