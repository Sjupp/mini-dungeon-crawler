using PrimeTween;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MovementState
{
    Free,
    Slowed,
    Anchored
}

public enum Faction
{
    None,
    Player,
    Enemy
}

public class PlayerBehaviour : MonoBehaviour, IDamagable, IModularAttackSystemUser
{
    [Header("Debug / Temp")]
    [SerializeField]
    private AnimatedProgressElement _progressIndicator = null;
    [SerializeField]
    private ParticleSystem _vfxCloud = null;

    [Header("Refs")]
    [SerializeField]
    private Transform _body = null;
    [SerializeField]
    private AlternatingHitboxes _hitbox = null;

    [Header("Settings")]
    [SerializeField]
    private float _moveSpeed = 0f;
    [SerializeField]
    private float _accelerationSharpness = 1f;
    [SerializeField]
    private float _decelerationSharpness = 1f;
    [SerializeField]
    private Vector3 _offset = Vector3.zero;
    [SerializeField]
    private Vector3 _offset1 = Vector3.zero;

    private int _health = 100;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private Vector2 _inputVector = Vector2.zero;
    private Vector3 _movementVector = Vector3.zero;
    private Vector3 _targetVector = Vector3.zero;
    private Vector3 _shiftVector = Vector3.zero;

    private Item _heldItemMain = null;
    private Item _heldItemOff = null;

    private Rigidbody2D _rb = null;
    private Animator _animator = null;
    private SpriteRenderer _spriteRenderer = null;

    private bool _facingRight = true;
    private MovementState _movementState = MovementState.Free;

    private float _shiftCompleteTimestamp = 0f;

    private float _inputBufferThreshold = 0.2f;
    private Queue<WeaponCommand> _inputBuffer = new();
    private AttackDataWrapper _currentAttack = null;

    private float _lastHitTimestamp = 0f;
    private float _hitInvincibilityDuration = 0f;

    private Faction _faction = Faction.Player;

    private ModularAttackSystem _modularAttackSystem = null;

    public Transform Transform => transform;
    public Animator Animator => _animator;
    public MovementState MovementState => _movementState;

    private void Awake()
    {
        _moveAction = InputSystem.actions.FindAction("Player/Move");
        _jumpAction = InputSystem.actions.FindAction("Player/Jump");

        _moveAction.Enable();
        _jumpAction.Enable();

        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = _body.GetComponent<SpriteRenderer>();

        _modularAttackSystem = new(this);
    }

    private void Start()
    {
        _heldItemMain = ItemDatabase.Instance.CreateInstance(0);
        _heldItemOff = ItemDatabase.Instance.CreateInstance(1);
    }

    void Update()
    {
        if (true)
        {
            HandleKeyboardMovementInput();
        }
        else
        {
            HandleMouseMovementInput();
        }


        if (_movementState is MovementState.Free or MovementState.Slowed)
        {
            float movementSpeed = _movementState == MovementState.Free ? _moveSpeed : _moveSpeed * 0.3f;

            if (_targetVector.sqrMagnitude > 0)
            {
                _movementVector = Vector3.Lerp(_movementVector, _targetVector * movementSpeed, _accelerationSharpness * Time.deltaTime);
            }
            else
            {
                _movementVector = Vector3.Lerp(_movementVector, _targetVector, _decelerationSharpness * Time.deltaTime);
            }
        }

        _animator.SetFloat("PlayerVelocity", _movementVector.magnitude);

        if (Time.time > _shiftCompleteTimestamp)
        {
            _shiftVector = Vector3.zero;
        }

        HandleDirectionSwitching();

        if (_heldItemMain != null)
            _heldItemMain.transform.position = _body.position + _body.right * _offset.x + _body.up * _offset.y;

        if (_heldItemOff != null)
            _heldItemOff.transform.position = _body.position + _body.right * _offset1.x + _body.up * _offset1.y;

        _modularAttackSystem.Tick();

        HandleAttackInputs();

        HandleInputAvailabilityVisualizer();
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _movementVector + _shiftVector;
    }

    private void HandleDirectionSwitching()
    {
        if (_movementVector.x > 0 && _movementState == MovementState.Free)
        {
            if (!_facingRight)
            {
                _facingRight = true;
                ChangedDirection(_facingRight);
            }
        }
        else if (_movementVector.x < 0 && _movementState == MovementState.Free)
        {
            if (_facingRight)
            {

                _facingRight = false;
                ChangedDirection(_facingRight);
            }
        }
    }


    private WeaponCommand CreateWeaponCommand(bool mainHand, InputType inputType = InputType.Tap)
    {
        Item weapon = null;
        var weaponType = WeaponType.Any;
        if (mainHand && _heldItemMain != null)
        {
            weapon = _heldItemMain;
            weaponType = _heldItemMain.ItemData.WeaponType;
        }
        else if (!mainHand && _heldItemOff != null)
        {
            weapon = _heldItemOff;
            weaponType = _heldItemOff.ItemData.WeaponType;
        }
        else
        {
            Debug.LogError("No item held in hand");
            return null;
        }

        return new WeaponCommand(
                weapon,
                weaponType,
                inputType,
                Time.time
                );
    }

    private void ChangedDirection(bool facingRight)
    {
        transform.rotation = Quaternion.Euler(0f, facingRight ? 0f : 180f, 0f);

        if (_heldItemMain != null)
            _heldItemMain.transform.localRotation = Quaternion.Euler(0f, facingRight ? 0f : 180f, 0f);

        if (_heldItemOff != null)
            _heldItemOff.transform.localRotation = Quaternion.Euler(0f, facingRight ? 0f : 180f, 0f);
    }

    private void HandleInputAvailabilityVisualizer()
    {
        var next = _modularAttackSystem.NextAvailableTimestamp;
        var latest = _modularAttackSystem.LatestAttackTimestamp;
        if (_modularAttackSystem.CanAttack)
        {
            _progressIndicator.SetElementColor(Color.green);
        }
        else if (Time.time > next - _inputBufferThreshold)
        {
            _progressIndicator.SetElementColor(Color.yellow);
        }
        else
        {
            _progressIndicator.SetElementColor(Color.red);
        }

        if (next > 0)
            _progressIndicator.UpdateElement(Time.time - latest, next - latest);
    }


    #region Input Handling, Movement & Attacking
    private void HandleAttackInputs()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryAddInputToBuffer(true, InputType.Tap);
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            TryAddInputToBuffer(false, InputType.Tap);
        }

        if (_modularAttackSystem.CanAttack)
        {
            if (_inputBuffer.Count > 0)
            {
                // Do I even need a queue if I ever just use the first one??
                // What if I need quarter circle shoryuken inputs!?!?
                var command = _inputBuffer.Dequeue();
                _inputBuffer.Clear();

                var attackToUse = AttackManager.Instance.GetNextAttack(command);

                _modularAttackSystem.SetCurrentAttack(attackToUse, command.Weapon);
            }
        }

        // ToDo: proper input actions, held input variants
    }

    private void TryAddInputToBuffer(bool mainHand, InputType inputType)
    {
        if (Time.time > _modularAttackSystem.NextAvailableTimestamp - _inputBufferThreshold)
        {
            _inputBuffer.Enqueue(CreateWeaponCommand(mainHand, inputType));
        }
    }

    private void HandleKeyboardMovementInput()
    {
        _inputVector = _moveAction.ReadValue<Vector2>();

        _targetVector = _inputVector.normalized;
    }

    private void HandleMouseMovementInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _animator.Play("PlayerRun");
        }

        if (Mouse.current.leftButton.IsPressed())
        {
            var inputValue = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var adjustedForDepth = new Vector3(inputValue.x, inputValue.y, transform.position.z);
            _targetVector = (adjustedForDepth - transform.position).normalized;
        }
        else
        {
            _animator.Play("PlayerIdle");
            _targetVector = Vector3.zero;
        }
    }
    #endregion

    #region IModularAttackSystemUser Methods
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

    public void SetMovementState(MovementState state)
    {
        _movementState = state;

        if (_movementState == MovementState.Anchored)
        {
            _movementVector = Vector3.zero;
            _targetVector = Vector3.zero;
            _animator.Play("Player_Idle");
        }
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (_body != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_body.position + _body.right * _offset.x + _body.up * _offset.y, 0.2f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_body.position + _body.right * _offset1.x + _body.up * _offset1.y, 0.2f);
        }
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

    public void TakeDamage(DamageInfo damageInfo)
    {
        _health -= damageInfo.Damage;
        OnTakeDamage(damageInfo);
    }

    private void OnTakeDamage(DamageInfo damageInfo)
    {
        float punchDirection = 15f;
        if (damageInfo.SourceTransform != null && damageInfo.SourceTransform.position.x < transform.position.x)
        {
            punchDirection *= -1f;
        }

        Tween.Color(_spriteRenderer, Color.red, Color.white, 0.2f);
        Tween.PunchLocalRotation(_body, Vector3.forward * punchDirection, 0.2f, 10, asymmetryFactor: 0.8f);

        TextManager.Instance.CreateTextAtPosition(damageInfo.Damage.ToString(), transform.position + Vector3.up * 1f + Vector3.right * Random.Range(-0.25f, 0.25f));
    }
}