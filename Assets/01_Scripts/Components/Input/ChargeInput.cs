using UnityEngine;
using UnityEngine.InputSystem;

public class ChargeInput : InputComponent
{
    public InputActionReference inputReference;
    [Tooltip( "Time in seconds to hold the input before it is considered a valid action.")]
    public float chargeTime = 0.5f;
    
    private float _holdStartTime; 

    public override void Initialize(WeaponContext weapon)
    {
        base.Initialize(weapon);
    }
    
    private void OnStarted(InputAction.CallbackContext context)
    {
        _holdStartTime = Time.time;
        //RaiseHeld();
    }
    
    private void OnCanceled(InputAction.CallbackContext context)
    {
        if (Time.time - _holdStartTime >= chargeTime)
        {
            RaiseReleased();
        }
    }

    public override bool CanExecute() => false;

    public override void EnableInput()
    {
        if (inputReference == null)
        {
            Debug.LogError($"[{name}] ChargeInput: missing InputActionReference!");
            return;
        }

        inputReference.action.Enable();
        inputReference.action.started += OnStarted;
        inputReference.action.canceled += OnCanceled;
    }
    
    public override void DisableInput()
    {
        if (inputReference != null)
        {
            inputReference.action.started -= OnStarted;
            inputReference.action.canceled -= OnCanceled;
            inputReference.action.Disable();
        }
    }

    public override void Cleanup()
    {
        DisableInput();
    }
}