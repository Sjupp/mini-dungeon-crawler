using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SequenceNotice : MonoBehaviour
{
    [SerializeField]
    private Sprite _arrowIcon = null;
    [SerializeField]
    private GameObject _iconPrefab = null;
    [SerializeField]
    private TMP_Text _label = null;
    [SerializeField]
    private CanvasGroup _canvasGroup = null;
    [SerializeField]
    private RectTransform _titleBoundary = null;
    [SerializeField]
    private RectTransform _iconsBoundary = null;
    [SerializeField]
    private RectTransform _iconsContainer = null;

    public void Init(AttackSequenceSO attackSequenceSO)
    {
        _label.text = attackSequenceSO.name;

        for (int i = 0; i < attackSequenceSO.CommandSequence.Count; i++)
        {
            if (i != 0)
            {
                var arrow = Instantiate(_iconPrefab, Vector3.zero, Quaternion.Euler(0f, 0f, -90f), _iconsContainer);
                arrow.GetComponent<Image>().sprite = _arrowIcon;
            }

            var icon = Instantiate(_iconPrefab, _iconsContainer);
            icon.GetComponent<Image>().sprite = IconHelper.Instance.GetIconByWeaponType(attackSequenceSO.CommandSequence[i].WeaponCommand.WeaponType);
        }
    }
}
