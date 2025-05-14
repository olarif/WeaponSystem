using UnityEngine;
using UnityEngine.InputSystem;

public class AutoFireInput : InputComponent
{
    public InputActionReference inputAction;
    
    public float fireRate = 0.1f;
    
    private float _lastFireTime;
    private bool _isPressed;
    
    public override void Initialize(WeaponContext context)
    {
        _lastFireTime = -fireRate;
        
        if (inputAction != null && inputAction.action != null)
        {
            if (!inputAction.action.enabled) { inputAction.action.Enable(); }
            
            inputAction.action.started += OnActionStarted;
            inputAction.action.canceled += OnActionCanceled;
        }
        else
        {
            Debug.LogError($"Input Action is not set on {name} AutoFireInputComponent");
        }
    }
    
    private void OnActionStarted(InputAction.CallbackContext context)
    {
        _isPressed = true;
    }
    
    private void OnActionCanceled(InputAction.CallbackContext context)
    {
        _isPressed = false;
    }
    
    public override bool CanExecute()
    {
        if (_isPressed && Time.time - _lastFireTime >= fireRate)
        {
            _lastFireTime = Time.time;
            return true;
        }
        return false;
    }
    
    private void OnDisable()
    {
        if (inputAction != null && inputAction.action != null)
        {
            inputAction.action.started -= OnActionStarted;
            inputAction.action.canceled -= OnActionCanceled;
        }
    }
}