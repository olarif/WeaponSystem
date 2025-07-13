using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour, IInputProvider
{
    [Header("Input Action Asset")] 
    private PlayerInput _actions;
    
    [Header("Input Settings")]
    [SerializeField] private bool invertY = false;
    [SerializeField] private float mouseSensitivityMultiplier = 1.0f;
    
    [Header("Action Name References")]
    [SerializeField] private string movementAction = "Movement";
    [SerializeField] private string rotationAction = "Rotation";
    [SerializeField] private string jumpAction     = "Jump";
    [SerializeField] private string sprintAction   = "Sprint";
    
    //Input properties
    public Vector2 MovementInput { get; private set; }
    public Vector2 RotationInput { get; private set; }
    public bool    JumpInput     { get; private set; }
    public bool    SprintInput   { get; private set; }

    // Cached Input Actions
    private InputAction _movementInputAction;
    private InputAction _rotationInputAction;
    private InputAction _jumpInputAction;
    private InputAction _sprintInputAction;
    
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
        _jumpInputAction     = _actions.FindAction(jumpAction,     true);
        _sprintInputAction   = _actions.FindAction(sprintAction,   true);
        
        ValidateInputActions();
    }
    
    private void ValidateInputActions()
    {
        if (_movementInputAction == null) Debug.LogError($"Movement action '{movementAction}' not found!");
        if (_rotationInputAction == null) Debug.LogError($"Rotation action '{rotationAction}' not found!");
        if (_jumpInputAction     == null) Debug.LogError($"Jump action '{jumpAction}' not found!");
        if (_sprintInputAction   == null) Debug.LogError($"Sprint action '{sprintAction}' not found!");
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
        
        JumpInput = _jumpInputAction?.triggered == true;
        SprintInput = _sprintInputAction?.ReadValue<float>() > 0.5f;
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
        JumpInput = false;
        SprintInput = false;
    }
}
