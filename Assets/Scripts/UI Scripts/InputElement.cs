using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class InputElement : MonoBehaviour
{
    public float Width = 240;

    [SerializeField]
    private Image _image = null;
    [SerializeField]
    private TMP_Text _label = null;
    [SerializeField]
    private CanvasGroup _canvasGroup = null;
    [SerializeField]
    private TweenSettings<float> _fadeSettings;
    [SerializeField]
    private ShakeSettings _shakeSettings;

    public void Init(Sprite icon, string name)
    {
        _image.sprite = icon;
        _label.text = name;
        Tween.Alpha(_canvasGroup, _fadeSettings);
        Tween.PunchLocalPosition(transform, _shakeSettings);
    }

    public void FadeOut(float duration)
    {
        Tween.Alpha(_canvasGroup, 0f, duration);
    }
}
