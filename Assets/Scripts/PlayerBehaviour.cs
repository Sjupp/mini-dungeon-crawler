using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MovementState
{
    Free,
    Slowed,
    Anchored
}

public class PlayerBehaviour : MonoBehaviour
{
    [Header("Debug / Temp")]
    [SerializeField]
    private AnimatedProgressElement _progressIndicator = null;
    [SerializeField]
    private ParticleSystem _vfxCloud = null;

    [Header("Refs")]
    [SerializeField]
    private Transform _body = null;

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
    
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private Vector2 _inputVector = Vector2.zero;
    private Vector3 _movementVector = Vector3.zero;
    private Vector3 _targetVector = Vector3.zero;
    
    private Item _heldItemMain = null;
    private Item _heldItemOff = null;

    private Rigidbody2D _rb = null;
    private Animator _animator = null;

    private bool _facingRight = true;
    private MovementState _movementState = MovementState.Free;
    private float _lastAttackTimestamp = 0f;
    private float _nextAvailableTimestamp = 0f;
    private float _inputBufferThreshold = 0.2f;
    private Queue<WeaponCommand> _inputBuffer = new();

    private void Awake()
    {
        _moveAction = InputSystem.actions.FindAction("Player/Move");
        _jumpAction = InputSystem.actions.FindAction("Player/Jump");

        _moveAction.Enable();
        _jumpAction.Enable();

        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
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
            KeyboardMovementInput();
        }
        else
        {
            MouseMovementInput();
        }


        if (_movementState is MovementState.Free or MovementState.Slowed)
        {
            float movementSpeed = _movementState == MovementState.Free ? _moveSpeed : _moveSpeed * 0.3f;

            if (_targetVector.sqrMagnitude > 0)
            {
                _movementVector = Vector3.Lerp(_movementVector, _targetVector * _moveSpeed, _accelerationSharpness * Time.deltaTime);
            }
            else
            {
                _movementVector = Vector3.Lerp(_movementVector, _targetVector, _decelerationSharpness * Time.deltaTime);
            }
        }

        if (_movementVector.x > 0)
        {
            if (!_facingRight)
            {
                _facingRight = true;
                ChangedDirection(_facingRight);
            }
        }
        else if (_movementVector.x < 0)
        {
            if (_facingRight)
            {

                _facingRight = false;
                ChangedDirection(_facingRight);
            }
        }

        if (_heldItemMain != null)
        {
            _heldItemMain.transform.position = _body.position + _body.right * _offset.x + _body.up * _offset.y;
        }

        if (_heldItemOff != null)
        {
            _heldItemOff.transform.position = _body.position + _body.right * _offset1.x + _body.up * _offset1.y;
        }

        HandleAttackInputs();

        HandleInputAvailabilityVisualizer();
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _movementVector;
    }

    private void HandleAttackInputs()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleInput(true, InputType.Tap);
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            HandleInput(false, InputType.Tap);
        }

        if (Time.time > _nextAvailableTimestamp)
        {
            if (_inputBuffer.Count > 0)
            {
                // Do I even need a queue if I ever just use the first one??
                // What if I need quarter circle shoryuken inputs!?!?
                var command = _inputBuffer.Dequeue();
                _inputBuffer.Clear();

                var attackToUse = AttackManager.Instance.GetNextAttack(command);

                ExecuteAttack(attackToUse, _heldItemMain);
            }
        }

        // ToDo: proper input actions, held input variants
    }

    private void HandleInput(bool mainHand , InputType inputType)
    {
        if (Time.time > _nextAvailableTimestamp - _inputBufferThreshold)
        {
            _inputBuffer.Enqueue(CreateWeaponCommand(mainHand, inputType));
        }
    }

    private WeaponCommand CreateWeaponCommand(bool mainHand, InputType inputType = InputType.Tap)
    {
        var weaponType = WeaponType.Any;
        if (mainHand && _heldItemMain != null)
        {
            weaponType = _heldItemMain.ItemData.WeaponType;
        }
        else if (!mainHand && _heldItemOff != null)
        {
            weaponType = _heldItemMain.ItemData.WeaponType;
        }
        else
        {
            Debug.LogError("No item held in hand");
            return null;
        }

        return new WeaponCommand(
                weaponType,
                inputType,
                Time.time
                );
    }

    private void ExecuteAttack(AttackDataSO attackToUse, Item temporaryParameter)
    {
        _lastAttackTimestamp = Time.time;
        _nextAvailableTimestamp = Time.time + attackToUse.AttackTimeline.Total;

        //_owner.Animator.Play("Player_StepForward", 1, 0f);
        //Tween.Position(transform, transform.position + transform.right * 0.3f, 0.2f);
        //var cloud = Instantiate(_vfxCloud, _owner.transform);
        //cloud.transform.localPosition = Vector3.right * 0.3f;
        //var asd = cloud.main;
        //asd.startDelay = 0.1f;
        //cloud.Play();

        if (attackToUse.name == "Sword_StabSlow")
        {
            _animator.Play("Player_Windup", 1, 0f);

            Tween.Delay(0.5f).OnComplete(() =>
            {
                _animator.Play("Player_StepForward", 1, 0f);
                Tween.Position(transform, transform.position + transform.right * 0.5f, 0.2f);
                temporaryParameter.Animator.Play(attackToUse.AnimationName, 0, 0f);
            });

            _animator.transform.GetChild(0).GetComponent<Hitbox>().Activate(attackToUse);

            if (attackToUse.VFX != null)
            {
                CreateVFX(attackToUse);
            }
        }
        else if (attackToUse.name == "Shield_Retreat")
        {
            temporaryParameter.Animator.Play(attackToUse.AnimationName, 0, 0f);
            _animator.transform.GetChild(0).GetComponent<Hitbox>().Activate(attackToUse);

            Tween.Delay(0.2f).OnComplete(() =>
            {
                _animator.Play("Player_JumpLow", 1, 0f);
                Tween.Position(transform, transform.position + transform.right * -1f, 0.3f);
            });

            if (attackToUse.VFX != null)
            {
                CreateVFX(attackToUse);
            }
        }
        else if (attackToUse.name == "Shield_Uppercut")
        {
            temporaryParameter.Animator.Play(attackToUse.AnimationName, 0, 0f);
            _animator.transform.GetChild(0).GetComponent<Hitbox>().Activate(attackToUse);
            _animator.Play("Player_JumpLow", 1, 0f);

            if (attackToUse.VFX != null)
            {
                CreateVFX(attackToUse);
            }
        }
        else
        {
            _animator.Play("Player_StepForward", 1, 0f);
            Tween.Position(transform, transform.position + transform.right * 0.3f, 0.2f);

            temporaryParameter.Animator.Play(attackToUse.AnimationName, 0, 0f);
            _animator.transform.GetChild(0).GetComponent<Hitbox>().Activate(attackToUse);

            if (attackToUse.VFX != null)
            {
                CreateVFX(attackToUse);
            }
        }
    }

    private void CreateVFX(AttackDataSO attackToUse)
    {
        var vfx = Instantiate(attackToUse.VFX, transform);
        vfx.transform.SetLocalPositionAndRotation(attackToUse.VFXPosition, Quaternion.Euler(0f, 0f, attackToUse.VFXRotationZ));
        if (attackToUse.VFXScale != Vector3.one)
        {
            vfx.transform.localScale = attackToUse.VFXScale; // scaling pixel art vfx usually looks bad
        }
        if (attackToUse.VFXStartDelay != 0f)
        {
            var variable = vfx.main;
            variable.startDelay = attackToUse.VFXStartDelay;
        }
    }

    private void ChangedDirection(bool facingRight)
    {
        transform.rotation = Quaternion.Euler(0f, facingRight ? 0f : 180f, 0f);

        if (_heldItemMain != null)
            _heldItemMain.transform.localRotation = Quaternion.Euler(0f, facingRight ? 0f : 180f, 0f);

        if (_heldItemOff != null)
            _heldItemOff.transform.localRotation = Quaternion.Euler(0f, facingRight ? 0f : 180f, 0f);
    }

    private void KeyboardMovementInput()
    {
        _inputVector = _moveAction.ReadValue<Vector2>();

        _targetVector = _inputVector.normalized;

        if (_inputVector.sqrMagnitude > 0)
        {
            _animator.Play("PlayerRun");
        }
        else
        {
            _animator.Play("PlayerIdle");
        }
    }

    private void MouseMovementInput()
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

    private void HandleInputAvailabilityVisualizer()
    {
        if (Time.time > _nextAvailableTimestamp)
        {
            _progressIndicator.SetElementColor(Color.green);
        }
        else if (Time.time > _nextAvailableTimestamp - _inputBufferThreshold)
        {
            _progressIndicator.SetElementColor(Color.yellow);
        }
        else
        {
            _progressIndicator.SetElementColor(Color.red);
        }

        if (_nextAvailableTimestamp > 0)
            _progressIndicator.UpdateElement(Time.time - _lastAttackTimestamp, _nextAvailableTimestamp - _lastAttackTimestamp);
    }

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
}
