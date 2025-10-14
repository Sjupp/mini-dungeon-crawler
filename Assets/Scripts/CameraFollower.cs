using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private Transform _followTarget = null;
    [SerializeField] private float _followSharpness = 3f;

    void LateUpdate()
    {
        Vector3 targetVector = new(_followTarget.position.x, _followTarget.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetVector, Time.deltaTime * _followSharpness);
    }
}
