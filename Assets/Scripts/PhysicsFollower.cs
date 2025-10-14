using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Idle,
    Moving,
    Rolling,
    Something,
}

public class PhysicsFollower : MonoBehaviour
{
    public Action<bool> ChangedDirection = null;

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


    private void Awake()
    {
        _moveAction = InputSystem.actions.FindAction("Player/Move");
        _jumpAction = InputSystem.actions.FindAction("Player/Jump");

        _moveAction.Enable();
        _jumpAction.Enable();

        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
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

        if (_targetVector.sqrMagnitude > 0)
        {
            _movementVector = Vector3.Lerp(_movementVector, _targetVector * _moveSpeed, _accelerationSharpness * Time.deltaTime);
        }
        else
        {
            _movementVector = Vector3.Lerp(_movementVector, _targetVector, _decelerationSharpness * Time.deltaTime);
        }

        if (_movementVector.x > 0)
        {
            if (!_facingRight)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                _facingRight = true;
                ChangedDirection?.Invoke(_facingRight);
            }
        }
        else if (_movementVector.x < 0)
        {
            if (_facingRight)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                _facingRight = false;
                ChangedDirection?.Invoke(_facingRight);
            }
        }
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
}
