using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemDataSO ItemData;

    [SerializeField]
    internal Animator _animator = null;
    [SerializeField]
    private SpriteRenderer _spriteRenderer = null;

    public void Init(ItemDataSO itemData)
    {
        this.ItemData = itemData;
        _spriteRenderer.sprite = itemData.Sprite;
        _animator.Play(itemData.IdleAnimationName);
        gameObject.name = "item_" + itemData.ItemName;
     
        // probably obsolete
        _spriteRenderer.flipX = itemData.FlipX;
        _spriteRenderer.sortingOrder = 3;
        _spriteRenderer.transform.SetLocalPositionAndRotation(itemData.Offset, Quaternion.Euler(0f, 0f, itemData.Rotation));
    }

    public void UseItem(InputType inputType)
    {
        WeaponCommand command = new WeaponCommand(
            ItemData.WeaponType,
            inputType,
            Time.time
            );

        var attackToUse = AttackManager.Instance.GetNextAttack(command);

        ExecuteAttack(attackToUse);
    }

    private void ExecuteAttack(AttackDataSO attack)
    {
        _animator.Play(attack.AnimationName, 0, 0f);
    }
}

public enum WeaponType
{
    Any,
    Sword,
    Shield,
}

public enum InputType
{
    Tap,
    Hold,
    Pause // wishlist
}

[System.Serializable]
public class WeaponCommand
{
    public WeaponType WeaponType;
    public InputType InputType;
    public float Timestamp;

    public WeaponCommand(WeaponType weaponType, InputType inputType, float time)
    {
        WeaponType = weaponType;
        InputType = inputType;
        Timestamp = time;
    }

    public bool CompareValues(WeaponCommand other)
    {
        bool success = false;
        success = other.WeaponType == WeaponType && other.InputType == InputType;
        return success;
    }
}