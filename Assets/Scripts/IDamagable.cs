using UnityEngine;

public interface IDamagable
{
    bool TryHit(DamageInfo damageInfo);
    void TakeDamage(DamageInfo damageInfo);
}
