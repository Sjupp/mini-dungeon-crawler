using TMPro;
using UnityEngine;
using PrimeTween;

public class AnimatedTextElement : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _label = null;
    [SerializeField]
    private TweenSettings _tweenSettings;
    [SerializeField]
    private float _floatHeight = 1f;

    public void AssignText(string text)
    {
        _label.text = text;
        Animate();
    }

    private void Animate()
    {
        Tween.Position(transform, transform.position, transform.position + Vector3.up * _floatHeight, _tweenSettings).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}