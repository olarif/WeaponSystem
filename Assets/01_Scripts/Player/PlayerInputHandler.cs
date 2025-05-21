using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Action Asset")] 
    private PlayerInput _actions;

    public PlayerInput Actions => _actions;

    [Header("Action Name References")]
    [SerializeField] private string movement = "Movement";
    [SerializeField] private string rotation = "Rotation";
    [SerializeField] private string jump = "Jump";
    [SerializeField] private string sprint = "Sprint";
    
    private InputAction MovementAction => _actions.FindAction(movement, true);
    private InputAction RotationAction => _actions.FindAction(rotation, true);
    private InputAction JumpAction => _actions.FindAction(jump, true);
    private InputAction SprintAction => _actions.FindAction(sprint, true);
    
    public Vector2 MovementInput { get; private set; }
    public Vector2 RotationInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool SprintInput { get; private set; }

    private void Awake()
    {
        _actions = new PlayerInput();
    }
    
    private void OnEnable() => _actions.Enable();
    
    private void OnDisable() => _actions.Disable();

    private void Update()
    {
        MovementInput = MovementAction.ReadValue<Vector2>();
        RotationInput = RotationAction.ReadValue<Vector2>();
        JumpInput = JumpAction.triggered;

        SprintInput = SprintAction.ReadValue<float>() > 0.5f;
    }

    
    
    
}
