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
    [SerializeField]
    private Hitbox _hitBox = null;

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
    private Vector3 _shiftVector = Vector3.zero;

    private Item _heldItemMain = null;
    private Item _heldItemOff = null;

    private Rigidbody2D _rb = null;
    private Animator _animator = null;

    private bool _facingRight = true;
    private MovementState _movementState = MovementState.Free;

    private float _latestAttackTimestamp = 0f;
    private float _nextAvailableTimestamp = 0f;
    private float _shiftComplete = 0f;

    private float _inputBufferThreshold = 0.2f;
    private Queue<WeaponCommand> _inputBuffer = new();
    private AttackDataWrapper _currentAttack = null;
    private List<AttackBlock> _pendingBlocks = new();
    private List<AttackBlock> _activeBlocks = new();

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
                _movementVector = Vector3.Lerp(_movementVector, _targetVector * movementSpeed, _accelerationSharpness * Time.deltaTime);
            }
            else
            {
                _movementVector = Vector3.Lerp(_movementVector, _targetVector, _decelerationSharpness * Time.deltaTime);
            }
        }

        if (Time.time > _shiftComplete)
        {
            _shiftVector = Vector3.zero;
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
            _heldItemMain.transform.position = _body.position + _body.right * _offset.x + _body.up * _offset.y;

        if (_heldItemOff != null)
            _heldItemOff.transform.position = _body.position + _body.right * _offset1.x + _body.up * _offset1.y;

        HandleAttackInputs();

        HandleAttacking();

        HandleInputAvailabilityVisualizer();
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _movementVector + _shiftVector;
    }

    private void SetShiftOverTime(Vector3 relativeOffset, float time)
    {
        _shiftComplete = Time.time + time;
        _shiftVector = new Vector3(transform.right.x * relativeOffset.x / time, relativeOffset.y / time, 0f);
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
            _movementState = MovementState.Free;
            //AttackComplete();

            if (_inputBuffer.Count > 0)
            {
                // Do I even need a queue if I ever just use the first one??
                // What if I need quarter circle shoryuken inputs!?!?
                var command = _inputBuffer.Dequeue();
                _inputBuffer.Clear();

                var attackToUse = AttackManager.Instance.GetNextAttack(command);

                SetCurrentAttack(attackToUse, command);
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

    private void SetCurrentAttack(AttackDataSO attackToUse, WeaponCommand command)
    {
        _movementState = attackToUse.MovementState;
        if (_movementState == MovementState.Anchored)
        {
            _movementVector = Vector3.zero;
            _targetVector = Vector3.zero;
            _animator.Play("Player_Idle");
        }
        
        _latestAttackTimestamp = Time.time;
        _nextAvailableTimestamp = Time.time + attackToUse.GetTotalDuration();

        _currentAttack = new AttackDataWrapper(attackToUse, command.Weapon);
        _pendingBlocks = new List<AttackBlock>(attackToUse.Blocks.OrderBy(b => b.StartTime));
        _activeBlocks = new List<AttackBlock>();
    }

    private void TriggerBlock(AttackBlock block)
    {
        switch (block)
        {
            case AnimationBlock anim:
                Debug.Log("Playing animation " + anim.AnimationClip.name);
                if (anim.AnimationType == AnimationType.PlayerAnimation)
                    _animator.Play(anim.AnimationClip.name, 1, 0f);
                else
                    _currentAttack.UsedItem.Animator.Play(anim.AnimationClip.name);
                break;
            case HitboxBlock hitbox:
                Debug.Log("Activating hitbox");
                _hitBox.Activate(hitbox);
                break;
            case VFXBlock vfx:
                Debug.Log("Creating vfx " + vfx.VFX.gameObject.name);
                CreateVFX(vfx);
                break;
            case ShiftBlock shift:
                SetShiftOverTime(shift.PositionRelative, shift.Duration);
                break;
        }
    }

    private void EndBlock(AttackBlock block)
    {
        if (block is HitboxBlock hitbox)
        {
            Debug.Log("Canceling hitbox");
            _hitBox.Cancel();
        }
        else if (block is VFXBlock vfx)
        {
            //DestroyVFX(vfx);
        }
        else if (block is ShiftBlock shift)
        {
            _shiftComplete = Time.time;
            _shiftVector = Vector3.zero;
        }
    }

    private void CreateVFX(VFXBlock vfxBlock)
    {
        var vfx = Instantiate(vfxBlock.VFX, transform);
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
            _progressIndicator.UpdateElement(Time.time - _latestAttackTimestamp, _nextAvailableTimestamp - _latestAttackTimestamp);
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

public class AttackDataWrapper
{
    public AttackDataSO AttackDataSO = null;
    public Item UsedItem = null;

    public AttackDataWrapper(AttackDataSO attackDataSO, Item usedItem)
    {
        AttackDataSO = attackDataSO;
        UsedItem = usedItem;
    }
}
