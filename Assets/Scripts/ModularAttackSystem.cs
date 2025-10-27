using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModularAttackSystem
{
    // Attack data
    private AttackDataWrapper _currentAttack = null;
    private List<AttackBlock> _pendingBlocks = new();
    private List<AttackBlock> _activeBlocks = new();
    private float _latestAttackTimestamp = 0f;
    private float _nextAvailableTimestamp = 0f;
    private AlternatingHitboxes _alternatingHitboxes = null;

    private IModularAttackSystemUser _user;

    public bool CanAttack => Time.time > _nextAvailableTimestamp;
    public float LatestAttackTimestamp => _latestAttackTimestamp;
    public float NextAvailableTimestamp => _nextAvailableTimestamp;

    public ModularAttackSystem(IModularAttackSystemUser user)
    {
        _user = user;
        _alternatingHitboxes = new();
    }

    public void Tick()
    {
        if (Time.time > _nextAvailableTimestamp && _user.MovementState != MovementState.Free)
        {
            _user.SetMovementState(MovementState.Free);
        }

        HandleAttacking();
    }

    public void SetCurrentAttack(AttackDataSO attackToUse, Item weaponRef)
    {
        _user.SetMovementState(attackToUse.MovementState);

        _latestAttackTimestamp = Time.time;
        _nextAvailableTimestamp = Time.time + attackToUse.GetTotalDuration();

        _currentAttack = new AttackDataWrapper(attackToUse, weaponRef);
        _pendingBlocks = new List<AttackBlock>(attackToUse.Blocks.OrderBy(b => b.StartTime));
        _activeBlocks = new List<AttackBlock>();
    }

    private void HandleAttacking()
    {
        if (_currentAttack == null)
            return;

        float elapsed = Time.time - _latestAttackTimestamp;

        // Trigger blocks when start time reached
        for (int i = _pendingBlocks.Count - 1; i >= 0; i--)
        {
            var block = _pendingBlocks[i];
            if (elapsed >= block.StartTime)
            {
                TriggerBlock(block);
                _activeBlocks.Add(block);
                _pendingBlocks.RemoveAt(i);
            }
        }

        // Handle block durations (e.g., disabling hitboxes)
        for (int i = _activeBlocks.Count - 1; i >= 0; i--)
        {
            var block = _activeBlocks[i];
            if (elapsed >= block.StartTime + block.Duration)
            {
                EndBlock(block);
                _activeBlocks.RemoveAt(i);
            }
        }

        // Optionally clear _currentAttack once all blocks finished
        if (_pendingBlocks.Count == 0 && _activeBlocks.Count == 0)
        {
            _currentAttack = null;
        }
    }

    private void TriggerBlock(AttackBlock block)
    {
        switch (block)
        {
            case AnimationBlock anim:
                if (anim.AnimationType == AnimationType.PlayerAnimation)
                {
                    if (_user.Animator != null)
                    {
                        _user.Animator.Play(anim.AnimationClip.name, 1, 0f);
                    }
                }
                else
                {
                    _currentAttack.UsedItem.PlayItemAnimation(anim.AnimationClip.name);
                }
                break;
            case HitboxBlock hitbox:
                _alternatingHitboxes.ActivateHitbox(
                    hitbox,
                    _user.GenerateDamageInfo(_currentAttack),
                    _user.Transform);
                break;
            case VFXBlock vfx:
                CreateVFX(vfx);
                break;
            case ShiftBlock shift:
                _user.ShiftBegin(shift.PositionRelative, shift.Duration);
                break;
        }
    }

    private void EndBlock(AttackBlock block)
    {
        if (block is HitboxBlock hitbox)
        {
            _alternatingHitboxes.Cancel(hitbox);
        }
        else if (block is VFXBlock vfx)
        {
            //DestroyVFX(vfx);
        }
        else if (block is ShiftBlock shift)
        {
            _user.ShiftCancel();
        }
    }

    private void CreateVFX(VFXBlock vfxBlock)
    {
        var vfx = GameObject.Instantiate(vfxBlock.VFX, _user.Transform);
        vfx.transform.SetLocalPositionAndRotation(vfxBlock.VFXPosition, Quaternion.Euler(0f, 0f, vfxBlock.VFXRotationZ));
        if (vfxBlock.VFXScale != Vector3.one)
        {
            vfx.transform.localScale = vfxBlock.VFXScale; // scaling pixel art vfx usually looks bad
        }
        if (vfxBlock.StartTime != 0f)
        {
            var variable = vfx.main;
            variable.startDelay = vfxBlock.StartTime;
        }
    }
}
