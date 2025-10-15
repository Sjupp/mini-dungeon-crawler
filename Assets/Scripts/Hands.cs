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

        item.Animator.Play(attackToUse.AnimationName, 0, 0f);
        _owner.transform.GetChild(0).GetComponent<Hitbox>().Activate(attackToUse);
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