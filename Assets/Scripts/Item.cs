using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    private ItemDataSO _itemData;
    [SerializeField]
    private Animator _animator = null;
    [SerializeField]
    private SpriteRenderer _spriteRenderer = null;

    public Animator Animator { get => _animator; set => _animator = value; }
    public ItemDataSO ItemData { get => _itemData; set => _itemData = value; }
    public WeaponType Type => _itemData.WeaponType;

    public void Init(ItemDataSO itemData)
    {
        _itemData = itemData;
        
        _spriteRenderer.sprite = itemData.Sprite;

        PlayItemAnimation(itemData.IdleAnimationName);

        gameObject.name = "item_" + itemData.ItemName;
    }

    public void PlayItemAnimation(string animationClipName)
    {
        if (!string.IsNullOrEmpty(animationClipName))
        {
            _animator.Play(animationClipName, 0, 0f);
        }
    }
}

public enum WeaponType
{
    Any,
    Sword,
    Shield,
    Spear,
    Hammer,
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
    public Item Weapon;
    public WeaponType WeaponType;
    public InputType InputType;
    public float Timestamp;

    public WeaponCommand(Item weapon, WeaponType weaponType, InputType inputType, float time)
    {
        Weapon = weapon;
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

public class InputData
{
    public WeaponType WeaponType;
    public InputType InputType;
}