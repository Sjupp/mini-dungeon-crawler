using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedProgressElement : MonoBehaviour
{
    //[SerializeField]
    //private TMP_Text _label = null;
    [SerializeField]
    private Image _progressImage = null;
    [SerializeField]
    private Image _bgImage = null;

    public void UpdateElement(float currentValue, float maxValue)
    {
        _progressImage.fillAmount =  currentValue / maxValue;
    }

    public void SetElementColor(Color color)
    {
        _progressImage.color = color;

        _bgImage.color = color * 0.5f;
    }
}
