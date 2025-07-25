using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStatsSO _stats;
    [SerializeField] public Transform _groundCheck;

    [Header("Component Data")]
    [SerializeField] private MovementData    movementData    = new MovementData();
    [SerializeField] private RotationData    rotationData    = new RotationData();
    [SerializeField] private GroundCheckData groundCheckData = new GroundCheckData();
    
    //Components
    private Camera _playerCamera;
    private PlayerInputHandler _input;
    private CharacterController _controller;
    private Player _player;
    
    //State Machine
    private StateMachine _stateMachine;
    
    //Control
    private bool _hasControl = true;
    
    //Events
    public Action<bool> OnJump { get; set; }
    public Action<bool> OnSprint { get; set; }
    
    //Properties
    public PlayerStatsSO Stats => _stats;
    public bool IsGrounded     => groundCheckData.IsGrounded;
    public bool IsMoving       => movementData.IsMoving;
    
    // Component Access
    public IInputProvider Input        => _input;
    public Camera         PlayerCamera => _playerCamera;
    public MovementData   MovementData => movementData;
    public RotationData   RotationData => rotationData;
    public StateMachine   StateMachine => _stateMachine;

    private void Awake()
    {
        InitializeComponents();
        InitializeData();
        InitializeStateMachine();
    }
    
    private void InitializeComponents()
    {
        _player = GetComponent<Player>();
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputHandler>();
        _playerCamera = Camera.main;

        ValidateComponents();
    }
    
    private void ValidateComponents()
    {
        if (_playerCamera == null) { Debug.LogError("PlayerController: Camera not found!"); }
        if (_input == null)        { Debug.LogError("PlayerController: PlayerInputHandler component not found!"); }
        if (_controller == null)   { Debug.LogError("PlayerController: CharacterController component not found!"); }
    }

    private void InitializeData()
    {
        movementData.Initialize(this, _controller, _stats);
        rotationData.Initialize(this);
        groundCheckData.Initialize(_groundCheck, _stats);
    }
    
    private void InitializeStateMachine()
    {
        _stateMachine = new StateMachine(this);
        _stateMachine.OnStateChanged += OnStateChanged;
        
        // Start in grounded state
        _stateMachine.ChangeState(new GroundedState(this));
    }
    
    private void Update()
    {
        if (!_hasControl) return;

        groundCheckData.UpdateGroundCheck();
        
        // Update static cooldowns
        DashState.UpdateCooldown();
        
        // Handle landing resets
        if (IsGrounded && !wasGroundedLastFrame)
        {
            OnLanded();
        }
        
        _stateMachine.Update();
        
        wasGroundedLastFrame = IsGrounded;
    }

    private void FixedUpdate()
    {
        if (!_hasControl) return;
        
        _stateMachine.FixedUpdate();
    }
    
    private bool wasGroundedLastFrame = true;
    
    private void OnLanded()
    {
        // Change this line:
        JumpState.ResetJumpsOnLanding();
    
        if (_stats.ResetDashOnLand)
        {
            DashState.ResetDashOnLand();
        }
    }
    
    private void OnStateChanged(State previousState, State newState)
    {
        string prevName = previousState?.GetType().Name ?? "None";
        string newName = newState?.GetType().Name ?? "Unknown";
        //Debug.Log($"State changed from {prevName} to {newName}");
    }

    public void TakeAwayControl(bool resetVelocity = true)
    {
        if (resetVelocity) 
        {
            movementData.ResetVelocity();
            movementData.ResetYVelocity();
        }
        _hasControl = false;
    }

    public void ReturnControl()
    {
        _hasControl = true;
    }
    
    public void ResetVelocity()
    {
        movementData.ResetVelocity();
        movementData.ResetYVelocity();
    }
    
    public void ApplyForce(Vector3 force)
    {
        movementData.ApplyForce(force);
    }

    public void Die()
    {
        Debug.Log("Player died! Resetting game...");
        GameManager.Instance.ResetGame();
    }
}