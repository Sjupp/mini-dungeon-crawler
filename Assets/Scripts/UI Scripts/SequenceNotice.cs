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
    private RectTransform _titleHolder = null;
    [SerializeField]
    private RectTransform _iconsHolder = null;
}
