using System.Collections.Generic;
using UnityEngine;
using PrimeTween;
public class InputVisualizer : MonoBehaviour
{
    // TODO, make pool
    //[SerializeField]
    //private List<InputElement> _elementsPool = null;

    [SerializeField]
    private List<Sprite> _weaponTypeIcons = new();

    [SerializeField]
    private float _elementSpacing = 10f;
    [SerializeField]
    private int _maxElements = 3;

    [SerializeField]
    private InputElement _elementPrefab = null;

    private List<InputElement> _activeElements = new();

    private void Start()
    {
        AttackManager.Instance.Attack -= OnAttack;
        AttackManager.Instance.Attack += OnAttack;
    }

    private void OnAttack(WeaponCommand inputType, AttackDataSO attackData, int commandHistoryCount, List<AttackSequenceSO> finishedSequences)
    {
        if (_activeElements.Count < _maxElements)
        {
            var createdElement = Instantiate(_elementPrefab,
                                  transform.position + Vector3.right * ((_elementPrefab.Width + _elementSpacing) * _activeElements.Count),
                                  Quaternion.identity,
                                  transform);

            createdElement.Init(GetIconByWeaponType(inputType.WeaponType), attackData.name);
            _activeElements.Add(createdElement);
        }
        else
        {
            var createdElement = Instantiate(_elementPrefab,
                      transform.position + Vector3.right * ((_elementPrefab.Width + _elementSpacing) * (_maxElements - 1)),
                      Quaternion.identity,
                      transform);

            createdElement.Init(GetIconByWeaponType(inputType.WeaponType), attackData.name);
            _activeElements.Add(createdElement);

            for (int i = 0; i < _activeElements.Count - 1; i++)
            {
                if (i == 0)
                {
                    var leftmostElement = _activeElements[0];
                    //_activeElements.RemoveAt(0);
                    Tween.Position(leftmostElement.transform, leftmostElement.transform.position + Vector3.left * (_elementPrefab.Width + _elementSpacing), 0.1f).OnComplete(() =>
                    {
                        Destroy(leftmostElement.gameObject);
                    });
                }
                else
                {
                    Tween.Position(_activeElements[i].transform, _activeElements[i].transform.position + Vector3.left * (_elementPrefab.Width + _elementSpacing), 0.1f);
                }
            }
            _activeElements.RemoveAt(0);
        }
    }

    private Sprite GetIconByWeaponType(WeaponType type)
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
