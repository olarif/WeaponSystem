using UnityEngine;
using UnityEngine.InputSystem;

public class TapInput : InputComponent
{
    public InputActionReference inputReference;
    
    public float cooldownTime = 0.2f;
    private float _lastTapTime;

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
        
        _wasTriggered = false;
        _lastTapTime = -cooldownTime;
    }
    
    private void OnActionTriggered(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _wasTriggered = true;
        }
    }
    
    public override bool CanExecute()
    {
        if (_wasTriggered && Time.time - _lastTapTime >= cooldownTime)
        {
            _wasTriggered = false;
            _lastTapTime = Time.time;
            return true;
        }
        return false;
    }
    
    public override bool IsExecuting()
    {
        return CanExecute();
    }

    private void OnDisable()
    {
        if (inputReference != null && inputReference.action != null)
        {
            inputReference.action.performed -= OnActionTriggered;
        }
    }
}