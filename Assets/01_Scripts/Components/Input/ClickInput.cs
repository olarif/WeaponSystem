using UnityEngine;
using UnityEngine.InputSystem;

public class ClickInput : InputComponent
{
    public InputActionReference inputAction;
    [Tooltip("Minimum time between clicks")]
    public float cooldownTime = 0.1f;
    float _lastFireTime = -Mathf.Infinity;

    public override void Initialize(WeaponContext ctx)
    {
        base.Initialize(ctx);
    }

    private void OnPerformed(InputAction.CallbackContext context)
    {
        if (Time.time - _lastFireTime < cooldownTime) return;
        _lastFireTime = Time.time;

        RaisePressed();
    }
    
    public override bool CanExecute() => false;
    
    public override void EnableInput()
    {
        Debug.Log("Enabling ClickInput");
        
        if (inputAction == null)
        {
            Debug.LogError($"[{name}] ClickInput missing action!");
            return;
        }
        inputAction.action.Enable();
        inputAction.action.performed += OnPerformed;
    }
    
    public override void DisableInput()
    {
        if (inputAction != null)
        {
            inputAction.action.performed -= OnPerformed;
            inputAction.action.Disable();
        }
    }
    
    public override void Cleanup()
    {
        DisableInput();
    }
}