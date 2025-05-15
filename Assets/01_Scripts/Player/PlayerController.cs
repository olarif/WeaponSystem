using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private Player _player;  
    [SerializeField] private PlayerStatsSO _stats;
    // Player stats
    
    [SerializeField] public Transform _groundCheck;
    
    // Components
    private Camera _playerCamera;
    private PlayerInputHandler _input;
    private CharacterController _controller;
    
    // Movement variables
    private bool _hasControl = true;
    private Vector3 _moveDirection;
    private Vector3 _velocity;
    private float _yVelocity;
    private float _jumpBufferCounter = 0f;
    
    // Rotation variables
    private float _xRotation = 0f;

    public event Action<bool> OnJump;
    public event Action<bool> OnSprint;
    
    public PlayerStatsSO Stats => _stats;
    public bool IsGrounded { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsMoving { get; private set; }

    private void Awake()
    {
        _player = GetComponent<Player>();
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputHandler>();
        _playerCamera = GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!_hasControl) return;

        GatherInput();
        CheckGround();
        HandleJump();
        HandleRotation();
        HandleMovement();
    }

    private void FixedUpdate()
    {
        if (!_hasControl) return;
        
        ApplyGravity();
    }

    private void GatherInput()
    {
        _moveDirection = new Vector3(_input.MovementInput.x, 0, _input.MovementInput.y).normalized;
        IsSprinting = _input.SprintInput;

        if (_input.JumpInput)
        {
            _jumpBufferCounter = _stats.JumpBufferTime;
        }
        else
        {
            _jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void CheckGround()
    {
        IsGrounded = Physics.CheckSphere(_groundCheck.position, _stats.GroundDistance, _stats.GroundLayer);

        // Reset y velocity when grounded and apply downward force
        if (IsGrounded && _yVelocity < 0)
        {
            _yVelocity = -2f;
        }
    }

    private void HandleRotation()
    {
        if (_playerCamera == null) return;
        
        //normalize input and apply sens
        float sensitivity = _stats.RotationSpeed;
        float verticalRotation = _input.RotationInput.y * sensitivity * Time.deltaTime;
        float horizontalRotation = _input.RotationInput.x * sensitivity * Time.deltaTime;
        
        // Handle vertical rotation (pitch)
        _xRotation -= verticalRotation;
        _xRotation = Mathf.Clamp(_xRotation, _stats.MinLookAngle, _stats.MaxLookAngle);
        _playerCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);

        // Handle horizontal rotation (yaw)
        transform.Rotate(Vector3.up * horizontalRotation);
    }

    private void HandleMovement()
    {
        IsMoving = _moveDirection.magnitude > 0;

        float targetSpeed = IsSprinting ? _stats.SprintSpeed : _stats.WalkSpeed;

        // Moving in the direction the camera is facing
        Vector3 move = Quaternion.Euler(0, _playerCamera.transform.eulerAngles.y, 0) * _moveDirection;
        move.y = 0;

        _velocity = Vector3.Lerp(_velocity, move * targetSpeed, Time.deltaTime * _stats.MoveSmoothTime);

        _controller.Move((_velocity + Vector3.up * _yVelocity) * Time.deltaTime);

        OnSprint?.Invoke(IsSprinting);
    }

    private void HandleJump()
    {
        if (IsGrounded)
        {
            if (_jumpBufferCounter > 0f)
            {
                _yVelocity = Mathf.Sqrt(_stats.JumpForce * -2f * _stats.Gravity);
                OnJump?.Invoke(true);
                _jumpBufferCounter = 0f;
            }
            else
            {
                OnJump?.Invoke(false);
            }
        } 
        
        IsJumping = _yVelocity > 0;
    }

    private void ApplyGravity()
    {
        if (!IsGrounded)
        {
            // Apply stronger gravity if player releases jump early
            if (!_input.JumpInput && _yVelocity > 0)
            {
                _yVelocity += _stats.Gravity * 2f * Time.deltaTime;
            }
            else
            {
                _yVelocity += _stats.Gravity * Time.deltaTime;
            }
        }
    }

    public void TakeAwayControl(bool resetVelocity = true)
    {
        if (resetVelocity) _velocity = Vector3.zero;
        _hasControl = false;
    }

    public void ReturnControl()
    {
        _hasControl = true;
    }
}