using UnityEngine;
using UnityEngine.InputSystem;

public class ClickInput : InputComponent
{
    public InputActionReference inputAction;
    
    private bool _consumed;
    
    public override void Initialize(WeaponContext context)
    {
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
        if (_consumed)
        {
            _consumed = false;
            return true;
        }
        
        return false;
    }
}