using UnityEngine;

[CreateAssetMenu(fileName = "Item_", menuName = "Create Item")]
public class ItemDataSO : ScriptableObject
{
    public int ItemID;
    public string ItemName;
    public WeaponType WeaponType;
    public string IdleAnimationName;

    public Sprite Sprite;
}
