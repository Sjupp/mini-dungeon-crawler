using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField]
    private Collider2D _hitboxCollider = null;

    private DamageInfo _damageInfo = null;

    public void Activate(HitboxBlock hitbox, DamageInfo damageInfo)
    {
        _damageInfo = damageInfo;

        transform.localPosition = hitbox.Position;
        transform.localScale = hitbox.Scale;

        _hitboxCollider.enabled = true;
    }

    public void Cancel()
    {
        _hitboxCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamagable damagable))
        {
            if (damagable.TryHit(_damageInfo))
            {
                damagable.TakeDamage(_damageInfo);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_hitboxCollider.enabled)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}
