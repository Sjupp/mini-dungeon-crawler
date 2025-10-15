using UnityEngine;

public class TextManager : MonoBehaviour
{
    public static TextManager Instance = null;

    [SerializeField]
    private float _zDepth = -5f;
    [SerializeField]
    private AnimatedTextElement _prefab = null;

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

    public void CreateTextAtPosition(string text, Vector3 worldPosition, float zDepthOverride = 0f)
    {
        worldPosition.z = zDepthOverride != 0f ? zDepthOverride : _zDepth;
        var textObj = Instantiate(_prefab, worldPosition, Quaternion.identity);
        textObj.AssignText(text);
    }
}
