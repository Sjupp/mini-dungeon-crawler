using PrimeTween;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UnitBehaviour : MonoBehaviour, IDamagable, IModularAttackSystemUser
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
    private Vector3 _shiftVector = Vector3.zero;
    private Vector3 _targetVector = Vector3.zero;
    private MovementState _movementState = MovementState.Free;

    [SerializeField]
    private float _movementSpeed = 1f;
    [SerializeField]
    private float _accelerationSharpness = 1f;

    private Transform _target = null;
    private float _pollingFrequency = 1f;
    private float _pollingTimestamp = 0f;
    [SerializeField]
    private float _targetingRange = 3f;
    [SerializeField]
    private float _attackRange = 1f;
  
    private float _shiftCompleteTimestamp = 0f;

    [SerializeField]
    private AttackDataSO _attackData = null;
    [SerializeField]
    private ItemDataSO _itemData = null;

    private Item _heldItem = null;
    private bool _facingRight = true;
    private Faction _faction = Faction.Enemy;

    private ModularAttackSystem _modularAttackSystem = null;

    public Transform Transform => transform;
    public Animator Animator => null;
    public MovementState MovementState => _movementState;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _modularAttackSystem = new(this);
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
            _modularAttackSystem.Tick();

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

        if (Time.time > _shiftCompleteTimestamp)
        {
            _shiftVector = Vector3.zero;
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

        if (_modularAttackSystem.CanAttack)
        {
            if (_target != null && Vector3.Distance(_target.position, transform.position) < 1.5f)
            {
                _modularAttackSystem.SetCurrentAttack(_attackData, _heldItem);
            }
        }
    }

    private void FixedUpdate()
    {
        _rigidbody.linearVelocity = _movementVector + _shiftVector;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerBehaviour playerBehaviour))
        {
            _target = playerBehaviour.transform;
        }
    }

    public void SetMovementState(MovementState state)
    {
        _movementState = state;

        if (_movementState == MovementState.Anchored)
        {
            _movementVector = Vector3.zero;
            _targetVector = Vector3.zero;
            //_animator.Play("Player_Idle");
        }
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        _health -= damageInfo.Damage;
        OnTakeDamage(damageInfo);
    }

    public bool TryHit(DamageInfo damageInfo)
    {
        if (_faction == damageInfo.SourceFaction)
        {
            return false;
        }

        var success = Time.time - _lastHitTimestamp > _hitInvincibilityDuration;
        if (success)
        {
            _lastHitTimestamp = Time.time;
        }

        return success;
    }

    private void OnTakeDamage(DamageInfo damageInfo)
    {
        float punchDirection = 15f;
        if (damageInfo.SourceTransform != null && damageInfo.SourceTransform.position.x < transform.position.x)
        {
            punchDirection *= -1f;
        }

        Tween.Color(_unitSprite, Color.red, Color.white, 0.2f);
        Tween.PunchLocalRotation(_visualsTransform, Vector3.forward * punchDirection, 0.2f, 10, asymmetryFactor: 0.8f);

        TextManager.Instance.CreateTextAtPosition(damageInfo.Damage.ToString(), transform.position + Vector3.up * 1f + Vector3.right * Random.Range(-0.25f, 0.25f));
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

    public DamageInfo GenerateDamageInfo(AttackDataWrapper data)
    {
        return new DamageInfo()
        {
            Damage = data.AttackDataSO.Damage,
            SourceTransform = transform,
            SourceFaction = _faction
        };
    }

    public void ShiftBegin(Vector3 positionRelative, float time)
    {
        _shiftCompleteTimestamp = Time.time + time;
        _shiftVector = new Vector3(transform.right.x * positionRelative.x / time, positionRelative.y / time, 0f);
    }

    public void ShiftCancel()
    {
        _shiftCompleteTimestamp = Time.time;
        _shiftVector = Vector3.zero;
    }
}