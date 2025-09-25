using System;
using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStatsSO playerStats;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] public Transform _groundCheck;
    [SerializeField] private GameObject bodyRoot;
    [SerializeField] private GameObject handsRoot;
    
    [Header("Movement Data")]
    [SerializeField] private HorizontalMovementData horizontalMovement = new HorizontalMovementData();
    [SerializeField] private VerticalMovementData verticalMovement = new VerticalMovementData();
    [SerializeField] private RotationData rotationData = new RotationData();
    [SerializeField] private GroundCheckData groundCheckData = new GroundCheckData();
    
    //Components
    private PlayerInputHandler _input;
    private CharacterController _controller;
    private Player _player;
    private StaminaComponent _staminaComponent;
    private PlayerHealthComponent _healthComponent;
    
    //State Machine
    private StateMachine _stateMachine;
    
    //Control
    private bool _hasControl = true;
    private bool _wasGroundedLastFrame = true;
    
    //Events
    public System.Action<bool> OnJump { get; set; }
    public System.Action<bool> OnSprint { get; set; }
    
    //Properties
    public PlayerStatsSO Stats => playerStats;
    public bool IsGrounded => groundCheckData.IsGrounded;
    public bool IsMoving => horizontalMovement.IsMoving;
    
    // Component Access
    public IInputProvider Input => _input;
    public Camera PlayerCamera => _playerCamera;
    public HorizontalMovementData HorizontalMovement => horizontalMovement;
    public VerticalMovementData VerticalMovement => verticalMovement;
    public RotationData RotationData => rotationData;
    public StateMachine StateMachine => _stateMachine;
    public CharacterController Controller => _controller;
    public Player Player => _player;
    public StaminaComponent StaminaComponent => _staminaComponent;
    public PlayerHealthComponent HealthComponent => _healthComponent;

    private void Awake()
    {
        //_runtimeStats = Instantiate(playerStats);
        
        InitializeComponents();
        InitializeData();
        InitializeStateMachine();
        
        _staminaComponent = GetComponent<StaminaComponent>();
        if (_staminaComponent != null)
        {
            _staminaComponent.OnDepleted.AddListener(HandleStaminaDepleted);
        }
        
        _healthComponent = GetComponent<PlayerHealthComponent>();
        if (_healthComponent != null)
        {
            _healthComponent.OnDeath.AddListener(HandleHealthDepleted);
        }
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (IsOwner)
        {
            SetupLocalPlayer();
        }
        else
        {
            SetupRemotePlayer();
        }
    }
    
    private void SetupLocalPlayer()
    {
        if (_playerCamera != null)
        {
            _playerCamera.gameObject.SetActive(true);
            
            // Enable audio listener for local player only
            AudioListener audioListener = _playerCamera.GetComponent<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = true;
            }
            
            int localLayer = LayerMask.NameToLayer("LocalPlayer");
            if (localLayer >= 0)
                _playerCamera.cullingMask &= ~(1 << localLayer);
        }
        else
        {
            Debug.LogError($"No camera found in children of {gameObject.name}");
        }
        
        // Enable input for local player
        if (_input != null)
        {
            _input.EnableInput();
        }
        
        if (bodyRoot != null)
            SetLayerRecursively(bodyRoot, LayerMask.NameToLayer("LocalPlayer"));
        
        Debug.Log($"Local player setup complete for {gameObject.name}");
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void SetupRemotePlayer()
    {
        if (_playerCamera != null)
        {
            _playerCamera.gameObject.SetActive(false);
            
            // Disable audio listener for remote players
            AudioListener audioListener = _playerCamera.GetComponent<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = false;
            }
        }
        
        // Disable input for remote players
        if (_input != null)
        {
            _input.DisableInput();
        }
        
        Debug.Log($"Remote player setup complete for {gameObject.name}");
    }
    
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
    
    private void InitializeComponents()
    {
        _player = GetComponent<Player>();
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputHandler>();

        ValidateComponents();
    }
    
    private void ValidateComponents()
    {
        if (_playerCamera == null) Debug.LogError("PlayerController: Camera not found!");
        if (_input == null) Debug.LogError("PlayerController: PlayerInputHandler component not found!");
        if (_controller == null) Debug.LogError("PlayerController: CharacterController component not found!");
    }

    private void InitializeData()
    {
        horizontalMovement.Initialize(this, _controller, playerStats);
        verticalMovement.Initialize(this);
        rotationData.Initialize(this);
        groundCheckData.Initialize(_groundCheck, playerStats);
    }
    
    private void InitializeStateMachine()
    {
        _stateMachine = new StateMachine(this);
        _stateMachine.OnStateChanged += OnStateChanged;
        _stateMachine.ChangeState(new GroundedState(this));
    }
    
    private void Update()
    {
        if (!_hasControl) return;

        groundCheckData.UpdateGroundCheck();
        verticalMovement.UpdateJumpBuffer();
        horizontalMovement.UpdateExternalForces();
        
        // Static cooldowns
        DashState.UpdateCooldown();
        
        // Handle landing
        if (IsGrounded && !_wasGroundedLastFrame)
        {
            OnLanded();
        }
        
        _stateMachine.Update();
        
        _wasGroundedLastFrame = IsGrounded;
    }

    private void FixedUpdate()
    {
        if (!_hasControl) return;
        _stateMachine.FixedUpdate();
    }
    
    private void OnLanded()
    {
        verticalMovement.OnLanded();
        
        if (playerStats.ResetDashOnLand)
        {
            DashState.ResetDashOnLand();
        }
    }
    
    // Movement Application - calculated by states
    public void ApplyMovement()
    {
        if (!_controller.enabled) return;
        
        Vector3 horizontalVelocity = horizontalMovement.GetTotalHorizontalVelocity();
        Vector3 verticalVelocity = Vector3.up * verticalMovement.YVelocity;
        Vector3 totalMovement = horizontalVelocity + verticalVelocity;
        
        _controller.Move(totalMovement * Time.deltaTime);
    }
    
    // Control Methods
    public void TakeAwayControl(bool resetVelocity = true)
    {
        if (resetVelocity) 
        {
            horizontalMovement.ResetVelocity();
            horizontalMovement.ResetExternalForces();
            verticalMovement.ResetYVelocity();
        }
        _hasControl = false;
    }

    public void ReturnControl()
    {
        _hasControl = true;
    }
    
    public void ResetVelocity()
    {
        horizontalMovement.ResetVelocity();
        horizontalMovement.ResetExternalForces();
        verticalMovement.ResetYVelocity();
    }
    
    public void ApplyForce(Vector3 force)
    {
        // Split force into horizontal and vertical components
        Vector3 horizontalForce = new Vector3(force.x, 0, force.z);
        float verticalForce = force.y;
        
        if (horizontalForce.magnitude > 0)
            horizontalMovement.ApplyForce(horizontalForce);
            
        if (verticalForce != 0)
            verticalMovement.SetYVelocity(verticalForce);
    }

    private void OnStateChanged(State previousState, State newState)
    {
        string prevName = previousState?.GetType().Name ?? "None";
        string newName = newState?.GetType().Name ?? "Unknown";
        //Debug.Log($"State changed from {prevName} to {newName}");
    }

    public void Die()
    {
        Debug.Log("Player died! Resetting game...");
        GameManager.Instance.ResetGame();
    }
    
    private void HandleStaminaDepleted()
    {
        // Handle stamina depletion logic here
        OnSprint?.Invoke(false);
        Debug.Log("Stamina depleted!");
    }
    
    private void HandleHealthDepleted()
    {
        // Handle health depletion logic here
        Debug.Log("Health depleted!");
        Die();
    }
}