using UnityEngine;

public interface IDamagable
{
    bool TryHit(DamageInfo damageInfo);
    void TakeDamage(DamageInfo damageInfo);
}

public interface IModularAttackSystemUser
{
    Transform Transform { get; }
    Animator Animator { get; }
    MovementState MovementState { get; }
    DamageInfo GenerateDamageInfo(AttackDataWrapper data);
    void ShiftBegin(Vector3 positionRelative, float duration);
    void ShiftCancel();
    void SetMovementState(MovementState state);
}