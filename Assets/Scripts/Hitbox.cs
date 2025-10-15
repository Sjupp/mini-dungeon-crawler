using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField]
    private Collider2D _hitboxCollider = null;

    private float _timer = 0;
    private float _windupDuration = 0;
    private float _activeDuration = 0;

    private bool _windup = true;
    private bool _paused = true;


    public void Activate(AttackDataSO attackToUse)
    {
        _timer = 0f;
        _windupDuration = attackToUse.AttackTimeline.Windup;
        _activeDuration = attackToUse.AttackTimeline.Duration;

        _paused = false;
        _windup = true;

        transform.localPosition = attackToUse.HitboxPosition;
        transform.localScale = attackToUse.HitboxScale;

        _hitboxCollider.enabled = false;
    }

    public void Cancel()
    {
        _hitboxCollider.enabled = false;
        _paused = true;
    }

    private void Update()
    {
        if (_paused) return;

        if (_windup)
        {
            if (_timer < _windupDuration)
            {
                _timer += Time.deltaTime;
                if (_timer >= _windupDuration)
                {
                    _hitboxCollider.enabled = true;
                    _timer = 0;
                    _windup = false;
                }
            }
        }
        else
        {
            if (_timer < _activeDuration)
            {
                _timer += Time.deltaTime;
                if (_timer >= _activeDuration)
                {
                    _hitboxCollider.enabled = false;
                    _timer = 0;
                    _paused = true;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamagable damagable))
        {
            if (damagable.TryHit())
            {
                damagable.TakeDamage(5);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!_paused && !_windup)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}
