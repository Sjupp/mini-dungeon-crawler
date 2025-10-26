using UnityEngine;

public class UnitTargetingVolume : MonoBehaviour
{
    [SerializeField]
    private UnitBehaviour _unit = null;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerBehaviour playerBehaviour))
        {
            _unit.AddTarget(playerBehaviour.transform);
        }
    }
}
