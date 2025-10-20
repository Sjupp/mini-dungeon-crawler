using PrimeTween;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

    private Rigidbody2D _rigidbody = null;
    private Vector3 _movementVector = Vector3.zero;
    private Vector3 _startingPosition = Vector3.zero;
    private Vector3 _targetVector = Vector3.zero;
    private MovementState _movementState = MovementState.Free;

    [SerializeField]
    private float _movementSpeed = 1f;
    [SerializeField]
    private float _accelerationSharpness = 1f;

    // targeting / game ai
    private Transform _target = null;
    private float _pollingFrequency = 1f;
    private float _pollingTimestamp = 0f;
    [SerializeField]
    private float _targetingRange = 3f;
    [SerializeField]
    private float _attackRange = 1f;

    // Attack data
    private AttackDataWrapper _currentAttack = null;
    private List<AttackBlock> _pendingBlocks = new();
    private List<AttackBlock> _activeBlocks = new();
    private float _latestAttackTimestamp = 0f;
    private float _nextAvailableTimestamp = 0f;
    //private float _shiftComplete = 0f;
    [SerializeField]
    private AttackDataSO _attackData = null;
    [SerializeField]
    private ItemDataSO _itemData = null;

    private Item _heldItem = null;
    private bool _facingRight = true;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _startingPosition = transform.position;
    }

    private void Start()
    {
        if (_itemData != null)
        {
            _heldItem = ItemDatabase.Instance.CreateInstance(_itemData);
        }
    }

    private void Update()
    {
        if (_heldItem != null)
        {
            HandleAttacking();

            _heldItem.transform.position = _unitSprite.transform.position + _unitSprite.transform.right * 0.45f + _unitSprite.transform.up * -0.3f;
        }

        if (_target != null)
        {
            if (Time.time > _pollingTimestamp + _pollingFrequency)
            {
                _pollingTimestamp = Time.time;
                if (Vector3.Distance(transform.position, _target.position) > _targetingRange * 1.5f)
                {
                    _target = null;
                    return;
                }
            }

            _targetVector = (_target.position - transform.position).normalized;
        }
        else
        {
            _targetVector = Vector3.zero;
        }

        if (_movementState is MovementState.Free or MovementState.Slowed)
        {
            float movementSpeed = _movementState == MovementState.Free ? _movementSpeed : _movementSpeed * 0.3f;

            if (_targetVector.sqrMagnitude > 0)
            {
                _movementVector = Vector3.Lerp(_movementVector, _targetVector * movementSpeed, _accelerationSharpness * Time.deltaTime);
            }
            else
            {
                _movementVector = Vector3.Lerp(_movementVector, _targetVector, _accelerationSharpness * Time.deltaTime);
            }
        }

        HandleDirectionSwitching();

        if (Time.time > _nextAvailableTimestamp)
        {
            _movementState = MovementState.Free;

            if (_target != null && Vector3.Distance(_target.position, transform.position) < 1.5f)
            {
                SetCurrentAttack(_attackData, _heldItem);
            }
        }
    }

    private void FixedUpdate()
    {
        _rigidbody.linearVelocity = _movementVector;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerBehaviour playerBehaviour))
        {
            _target = playerBehaviour.transform;
        }
    }

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

    private void SetCurrentAttack(AttackDataSO attackToUse, Item weaponRef)
    {
        _movementState = attackToUse.MovementState;
        if (_movementState == MovementState.Anchored)
        {
            _movementVector = Vector3.zero;
            _targetVector = Vector3.zero;
            //_animator.Play("Player_Idle");
        }

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

    private void HandleDirectionSwitching()
    {
        if (_movementVector.x > 0)
        {
            if (!_facingRight)
            {
                _facingRight = true;
                transform.rotation = Quaternion.Euler(0f, _facingRight ? 0f : 180f, 0f);
                _heldItem.transform.localRotation = Quaternion.Euler(0f, _facingRight ? 0f : 180f, 0f);
            }
        }
        else if (_movementVector.x < 0)
        {
            if (_facingRight)
            {
                _facingRight = false;
                transform.rotation = Quaternion.Euler(0f, _facingRight ? 0f : 180f, 0f);
                _heldItem.transform.localRotation = Quaternion.Euler(0f, _facingRight ? 0f : 180f, 0f);
            }
        }
    }
}