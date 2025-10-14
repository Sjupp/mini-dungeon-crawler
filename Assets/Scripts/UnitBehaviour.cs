using PrimeTween;
using UnityEngine;

public class UnitBehaviour : MonoBehaviour, IDamagable
{
    private int _health = 100;

    private float _lastHitTimestamp = 0f;
    private float _hitInvincibilityDuration = 0f;

    [SerializeField]
    private SpriteRenderer _unitSprite = null;

    public void TakeDamage(int damage)
    {
        _health -= damage;
        OnTakeDamage();
    }

    public bool TryHit()
    {
        var success = Time.time - _lastHitTimestamp > _hitInvincibilityDuration;
        if (success)
            _lastHitTimestamp = Time.time;
        return success;
    }

    private void OnTakeDamage()
    {
        Tween.Color(_unitSprite, Color.red, Color.white, 0.2f);
    }
}