using UnityEngine;

[CreateAssetMenu(fileName = "item", menuName = "Create Item")]
public class ItemDataSO : ScriptableObject
{
    public int ItemID;
    public string ItemName;
    public WeaponType WeaponType;
    public string IdleAnimationName;

    public Sprite Sprite;

    // animation controlled now??
    [Space]
    public bool FlipX;
    public Vector3 Offset;
    public float Rotation;
}
