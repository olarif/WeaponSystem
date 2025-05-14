using UnityEngine;
using UnityEngine.InputSystem;

public class ActionInput : InputComponent
{
    public InputActionReference inputReference;
    
    private bool _wasTriggered;
    
    public override void Initialize(WeaponContext weapon)
    {
        if (inputReference != null && inputReference.action != null)
        {
            if (!inputReference.action.enabled) { inputReference.action.Enable(); }
            
            inputReference.action.performed += OnActionTriggered;
        } 
        else
        {
            Debug.LogError("Input action reference is not set or action is null.");
        }
    }
    
    private void OnActionTriggered(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _wasTriggered = true;
            Debug.Log("Action Triggered: " + context.action.name);
        }
    }
    
    public override bool CanExecute()
    {
        if (_wasTriggered)
        {
            _wasTriggered = false;
            return true;
        }
        return false;
    }
    
    private void OnDisable()
    {
        if (inputReference != null && inputReference.action != null)
        {
            inputReference.action.performed -= OnActionTriggered;
        }
    }
}