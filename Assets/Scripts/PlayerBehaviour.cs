using PrimeTween;
using System;
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
    // PhysicsFollower
    [SerializeField]
    private float _moveSpeed = 0f;
    [SerializeField]
    private float _accelerationSharpness = 1f;
    [SerializeField]
    private float _decelerationSharpness = 1f;
    private Rigidbody2D _rb = null;

    Vector3 _movementVector = Vector3.zero;
    Vector3 _targetVector = Vector3.zero;

    private InputAction _moveAction;
    private InputAction _jumpAction;

    [SerializeField]
    private Animator _animator = null;
    private int _playerVelocityStringHash = -1;

    private bool _facingRight = true;
    private Vector2 _inputVector = Vector2.zero;

    private MovementState _movementState = MovementState.Free;
    public Animator Animator => _animator;

    // Hands
    private Item _heldItemMain = null;
    private Item _heldItemOff = null;

    [SerializeField]
    private Transform _body = null;

    [SerializeField]
    private Vector3 _offset = Vector3.zero;
    [SerializeField]
    private Vector3 _offset1 = Vector3.zero;

    [SerializeField]
    private ParticleSystem _vfxCloud = null;

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
            KeyboardInput();
        }
        else
        {
            MouseInput();
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

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (_heldItemMain != null)
            {
                Attack(_heldItemMain, InputType.Tap);
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (_heldItemOff != null)
            {
                Attack(_heldItemOff, InputType.Tap);
            }
        }
    }

    private void Attack(Item item, InputType inputType)
    {
        WeaponCommand command = new WeaponCommand(
            item.ItemData.WeaponType,
            inputType,
            Time.time
            );

        var attackToUse = AttackManager.Instance.GetNextAttack(command);

        var hej = attackToUse.AttackTimeline.Total;

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
                item.Animator.Play(attackToUse.AnimationName, 0, 0f);
            });

            _animator.transform.GetChild(0).GetComponent<Hitbox>().Activate(attackToUse);

            if (attackToUse.VFX != null)
            {
                CreateVFX(attackToUse);
            }
        }
        else if (attackToUse.name == "Shield_Retreat")
        {
            item.Animator.Play(attackToUse.AnimationName, 0, 0f);
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
            item.Animator.Play(attackToUse.AnimationName, 0, 0f);
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

            item.Animator.Play(attackToUse.AnimationName, 0, 0f);
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

    private void KeyboardInput()
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

    private void MouseInput()
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

    private void FixedUpdate()
    {
        _rb.linearVelocity = _movementVector;
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
