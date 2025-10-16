using System.Collections.Generic;
using UnityEngine;
using PrimeTween;
public class InputVisualizer : MonoBehaviour
{
    // TODO, make pool
    //[SerializeField]
    //private List<InputElement> _elementsPool = null;

    [SerializeField]
    private float _elementSpacing = 10f;
    [SerializeField]
    private int _maxElements = 3;

    [SerializeField]
    private InputElement _elementPrefab = null;
    [SerializeField]
    private SequenceNotice _sequenceNoticePrefab = null;

    private List<InputElement> _activeElements = new();
    private List<SequenceNotice> _activeSequences = new();

    [SerializeField]
    private float _inactivityThreshold = 3f;
    private float _timer = 0f;

    private void Start()
    {
        AttackManager.Instance.Attack -= OnAttack;
        AttackManager.Instance.Attack += OnAttack;
    }

    private void Update()
    {
        if (_timer > 0f)
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0f)
            {
                foreach (var item in _activeElements)
                {
                    item.FadeOut(0.3f);
                }
                _activeElements.Clear();
            }
        }
    }

    private void OnAttack(WeaponCommand inputType, AttackDataSO attackData, int commandHistoryCount, List<AttackSequenceSO> finishedSequences)
    {
        UpdateCommandFeed(inputType, attackData);

        UpdateSequenceFeed(finishedSequences);
    }

    private void UpdateSequenceFeed(List<AttackSequenceSO> finishedSequences)
    {
        if (_activeSequences.Count > 0)
        {
            for (int i = 0; i < _activeSequences.Count; i++)
            {
                var seq = _activeSequences[i];
                Destroy(seq.gameObject);
            }
            _activeSequences.Clear();
        }

        if (finishedSequences != null)
        {
            for (int i = 0; i < finishedSequences.Count; i++)
            {
                var createdElement = Instantiate(_sequenceNoticePrefab,
                          transform.position + (50f * Vector3.up) + ((165f + 20) * i * Vector3.up),
                          Quaternion.identity,
                          transform);
                createdElement.Init(finishedSequences[i]);
                _activeSequences.Add(createdElement);
            }
        }
    }

    private void UpdateCommandFeed(WeaponCommand inputType, AttackDataSO attackData)
    {
        _timer = _inactivityThreshold;

        if (_activeElements.Count < _maxElements)
        {
            var createdElement = Instantiate(_elementPrefab,
                                  transform.position + Vector3.right * ((_elementPrefab.Width + _elementSpacing) * _activeElements.Count),
                                  Quaternion.identity,
                                  transform);

            createdElement.Init(IconHelper.Instance.GetIconByWeaponType(inputType.WeaponType), attackData.name);
            _activeElements.Add(createdElement);
        }
        else
        {
            var createdElement = Instantiate(_elementPrefab,
                      transform.position + Vector3.right * ((_elementPrefab.Width + _elementSpacing) * (_maxElements - 1)),
                      Quaternion.identity,
                      transform);

            createdElement.Init(IconHelper.Instance.GetIconByWeaponType(inputType.WeaponType), attackData.name);
            _activeElements.Add(createdElement);

            for (int i = 0; i < _activeElements.Count - 1; i++)
            {
                if (i == 0)
                {
                    var leftmostElement = _activeElements[0];
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
}
