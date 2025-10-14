using System.Collections.Generic;
using UnityEngine;

public class IconHelper :MonoBehaviour
{
    public static IconHelper Instance = null;

    [SerializeField]
    private List<Sprite> _weaponTypeIcons = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Sprite GetIconByWeaponType(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Any:
                return _weaponTypeIcons[1];
            case WeaponType.Sword:
                return _weaponTypeIcons[2];
            case WeaponType.Shield:
                return _weaponTypeIcons[3];
            default:
                Debug.LogError("Icon not yet implemented");
                return _weaponTypeIcons[0];
        }
    }
}
