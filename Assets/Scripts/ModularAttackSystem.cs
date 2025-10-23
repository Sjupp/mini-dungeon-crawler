using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModularAttackSystem : MonoBehaviour
{
    // Attack data
    private AttackDataWrapper _currentAttack = null;
    private List<AttackBlock> _pendingBlocks = new();
    private List<AttackBlock> _activeBlocks = new();
    private float _latestAttackTimestamp = 0f;
    private float _nextAvailableTimestamp = 0f;


    private void Update()
    {
        HandleAttacking();

        // update heldItemPos


    }

    private void SetCurrentAttack(AttackDataSO attackToUse, Item weaponRef)
    {
        //_movementState = attackToUse.MovementState;
        //if (_movementState == MovementState.Anchored)
        //{
        //    _movementVector = Vector3.zero;
        //    _targetVector = Vector3.zero;
        //    //_animator.Play("Player_Idle");
        //}

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
                    //_animator.Play(anim.AnimationClip.name, 1, 0f);
                }
                else
                {
                    _currentAttack.UsedItem.Animator.Play(anim.AnimationClip.name, 0, 0f);
                }
                break;
            case HitboxBlock hitbox:
                //_hitbox.ActivateHitbox(hitbox);
                break;
            case VFXBlock vfx:
                //CreateVFX(vfx);
                break;
            case ShiftBlock shift:
                //SetShiftOverTime(shift.PositionRelative, shift.Duration);
                break;
        }
    }

    private void EndBlock(AttackBlock block)
    {
        if (block is HitboxBlock hitbox)
        {
            //_hitbox.Cancel(hitbox);
        }
        else if (block is VFXBlock vfx)
        {
            //DestroyVFX(vfx);
        }
        else if (block is ShiftBlock shift)
        {
            //_shiftComplete = Time.time;
            //_shiftVector = Vector3.zero;
        }
    }
}
