using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : NetworkBehaviour, IInputProvider
{
    [Header("Input Action Asset")] 
    private PlayerInput _actions;
    
    [Header("Input Settings")]
    [SerializeField] private bool invertY = false;
    [SerializeField] private float mouseSensitivityMultiplier = 1.0f;
    
    [Header("Action Name References")]
    [SerializeField] private string movementAction = "Movement";
    [SerializeField] private string rotationAction = "Rotation";
    [SerializeField] private string jumpAction = "Jump";
    [SerializeField] private string sprintAction = "Sprint";
    [SerializeField] private string crouchAction = "Crouch";
    [SerializeField] private string dashAction = "Dash";
    
    // Input properties
    public Vector2 MovementInput { get; private set; }
    public Vector2 RotationInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool SprintInput { get; private set; }
    public bool CrouchInput { get; private set; }
    public bool DashInput { get; private set; }
    
    // Cached Input Actions
    private InputAction _movementInputAction;
    private InputAction _rotationInputAction;
    private InputAction _jumpInputAction;
    private InputAction _sprintInputAction;
    private InputAction _crouchInputAction;
    private InputAction _dashInputAction;
    
    public bool IsInputEnabled { get; private set; } = true;
    
    private void Awake()
    {
        InitializeInputSystem();
    }

    private void InitializeInputSystem()
    {
        _actions = new PlayerInput();
        CacheInputActions();
    }
    
    private void CacheInputActions()
    {
        _movementInputAction = _actions.FindAction(movementAction, true);
        _rotationInputAction = _actions.FindAction(rotationAction, true);
        _jumpInputAction = _actions.FindAction(jumpAction, true);
        _sprintInputAction = _actions.FindAction(sprintAction, true);
        _crouchInputAction = _actions.FindAction(crouchAction, true);
        _dashInputAction = _actions.FindAction(dashAction, true);
        
        ValidateInputActions();
    }
    
    private void ValidateInputActions()
    {
        if (_movementInputAction == null) Debug.LogError($"Movement action '{movementAction}' not found!");
        if (_rotationInputAction == null) Debug.LogError($"Rotation action '{rotationAction}' not found!");
        if (_jumpInputAction == null) Debug.LogError($"Jump action '{jumpAction}' not found!");
        if (_sprintInputAction == null) Debug.LogError($"Sprint action '{sprintAction}' not found!");
        if (_crouchInputAction == null) Debug.LogError($"Crouch action '{crouchAction}' not found!");
        if (_dashInputAction == null) Debug.LogError($"Dash action '{dashAction}' not found!");
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        
        // Only enable input for the owner (local player)
        if (IsOwner)
        {
            EnableInput();
            Debug.Log($"Input enabled for local player {gameObject.name}");
        }
        else
        {
            DisableInput();
            Debug.Log($"Input disabled for remote player {gameObject.name}");
        }
    }
    
    private void OnEnable()
    {
        _actions?.Enable();
        IsInputEnabled = true;
    }
    
    private void OnDisable()
    {
        _actions?.Disable();
        IsInputEnabled = false;
    }
    
    private void Update()
    {
        if (!IsInputEnabled) return;
        UpdateInputValues();
    }

    private void UpdateInputValues()
    {
        MovementInput = _movementInputAction?.ReadValue<Vector2>() ?? Vector2.zero;
        
        Vector2 rawRotation = _rotationInputAction?.ReadValue<Vector2>() ?? Vector2.zero;
        RotationInput = new Vector2(
            rawRotation.x * mouseSensitivityMultiplier,
            rawRotation.y * mouseSensitivityMultiplier * (invertY ? -1f : 1f)
        );
        
        JumpPressed = _jumpInputAction?.triggered == true;
        JumpHeld = _jumpInputAction?.ReadValue<float>() > 0.5f;
        SprintInput = _sprintInputAction?.ReadValue<float>() > 0.5f;
        CrouchInput = _crouchInputAction?.ReadValue<float>() > 0.5f;
        DashInput = _dashInputAction?.triggered == true;
    }
    
    public void EnableInput()
    {
        IsInputEnabled = true;
        _actions?.Enable();
    }
    
    public void DisableInput()
    {
        IsInputEnabled = false;
        ResetInputValues();
    }
    
    private void ResetInputValues()
    {
        MovementInput = Vector2.zero;
        RotationInput = Vector2.zero;
        JumpPressed = false;
        JumpHeld = false;
        SprintInput = false;
        CrouchInput = false;
        DashInput = false;
    }
}