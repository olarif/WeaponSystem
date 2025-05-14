using System;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.InputSystem;

public class HoldInput : InputComponent
{
    public InputActionReference inputReference;

    [Tooltip( "Time in seconds to hold the input before it is considered a valid action.")]
    public float holdTime = 0.5f;
    
    private float _holdStartTime; 
    private bool _isHolding;
    private bool _wasTriggered;

    public override void Initialize(WeaponContext weapon)
    {
        _isHolding = false;
        
        if (inputReference != null && inputReference.action != null)
        {
            if (!inputReference.action.enabled) { inputReference.action.Enable(); }
            
            inputReference.action.performed += OnActionStarted;
            inputReference.action.canceled += OnActionCanceled;
        } 
        else
        {
            Debug.LogError("Input action reference is not set or action is null.");
        }
    }
    
    private void OnActionStarted(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _holdStartTime = Time.time;
            _isHolding = true;
        }
    }
    
    public override bool IsExecuting()
    {
        return CanExecute();
    }
    
    private void OnActionCanceled(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            _isHolding = false;
            if (context.time - _holdStartTime >= holdTime)
            {
                _wasTriggered = true;
            }
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
            inputReference.action.performed -= OnActionStarted;
            inputReference.action.canceled -= OnActionCanceled;
        }
    }
}