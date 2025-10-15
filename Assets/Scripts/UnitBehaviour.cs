using PrimeTween;
using UnityEngine;

public class UnitBehaviour : MonoBehaviour, IDamagable
{
    private int _health = 100;

    private float _lastHitTimestamp = 0f;
    private float _hitInvincibilityDuration = 0f;

    [SerializeField]
    private SpriteRenderer _unitSprite = null;
    [SerializeField]
    private Transform _visualsTransform = null;
    //[SerializeField]
    //private ShakeSettings _shakeSettings = null;

    public void TakeDamage(int damage)
    {
        _health -= damage;
        OnTakeDamage(damage);
    }

    public bool TryHit()
    {
        var success = Time.time - _lastHitTimestamp > _hitInvincibilityDuration;
        if (success)
            _lastHitTimestamp = Time.time;
        return success;
    }

    private void OnTakeDamage(int damage)
    {
        Tween.Color(_unitSprite, Color.red, Color.white, 0.2f);
        Tween.PunchLocalRotation(_visualsTransform, Vector3.forward * 15f, 0.2f, 10);

        TextManager.Instance.CreateTextAtPosition(damage.ToString(), transform.position + Vector3.up * 1f + Vector3.right * Random.Range(-0.25f, 0.25f));
    }
}