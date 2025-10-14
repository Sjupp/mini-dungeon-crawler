using UnityEngine;

public interface IDamagable
{
    bool TryHit();
    void TakeDamage(int damage);
}
