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
    [SerializeField] private JumpData        jumpData        = new JumpData();
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
    public bool IsSprinting    => movementData.IsSprinting;
    public bool IsJumping      => jumpData.IsJumping;
    public bool IsMoving       => movementData.IsMoving;
    
    // Component Access
    public IInputProvider Input        => _input;
    public Camera         PlayerCamera => _playerCamera;
    public MovementData   MovementData => movementData;
    public RotationData   RotationData => rotationData;
    public JumpData       JumpData     => jumpData;
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
        jumpData.Initialize(this);
        groundCheckData.Initialize(_groundCheck, _stats);
    }
    
    private void InitializeStateMachine()
    {
        _stateMachine = new StateMachine(this);
        _stateMachine.OnStateChanged += OnStateChanged;
        
        // Initialize with a default idle state
        _stateMachine.ChangeState(new IdleState(this));
    }
    
    private void Update()
    {
        if (!_hasControl) return;

        groundCheckData.UpdateGroundCheck();
        jumpData.UpdateJumpBuffer();
        
        _stateMachine.Update();
    }

    private void FixedUpdate()
    {
        if (!_hasControl) return;
        
        movementData.ApplyGravity();
        
        _stateMachine.FixedUpdate();
    }
    
    private void OnStateChanged(State previousState, State newState)
    {
        string prevName = previousState?.GetType().Name ?? "None";
        string newName = newState?.GetType().Name ?? "Unknown";
        Debug.Log($"State changed from {prevName} to {newName}");
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