using UnityEngine;
using UnityEngine.InputSystem;

public class ClickInput : InputComponent
{
    public InputActionReference inputAction;
    
    public float coolDownTime = 0.1f;
    private float _lastFireTime;
    private bool _consumed;
    
    public override void Initialize(WeaponContext context)
    {
        _consumed = false;
        _lastFireTime = Time.time - coolDownTime;

        if (inputAction != null && inputAction.action != null)
        {
            if (!inputAction.action.enabled) { inputAction.action.Enable(); }
            
            inputAction.action.performed += OnActionStarted;
            inputAction.action.canceled += OnActionCanceled;
        }
        else
        {
            Debug.LogError($"Input Action is not set on {name} ClickInputComponent");
        }
    }
    
    private void OnActionStarted(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _consumed = true;
        }
    }
    
    private void OnActionCanceled(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            _consumed = false;
        }
    }
    
    public override bool CanExecute()
    {
        if (_consumed && Time.time - _lastFireTime >= coolDownTime)
        {
            _lastFireTime = Time.time;
            return true;
        }
        
        return false;
    }
}