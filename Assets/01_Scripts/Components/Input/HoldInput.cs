using UnityEngine;
using UnityEngine.InputSystem;

public class HoldInput : InputComponent
{
    [Tooltip("Drag in your InputActions → Hold action here")]
    public InputActionReference inputAction;
    
    public override void Initialize(WeaponContext ctx)
    {
        base.Initialize(ctx);
    }

    public override bool CanExecute()
    {
        return inputAction.action.ReadValue<float>() > 0.5f;
    }
    
    public override void EnableInput()
    {
        if (inputAction == null)
        {
            Debug.LogError($"[{name}] HoldInput: missing InputActionReference!");
            return;
        }

        inputAction.action.Enable();
    }

    public override void DisableInput()
    {
        if (inputAction != null)
            inputAction.action.Disable();
    }

    public override void Cleanup()
    {
        DisableInput();
    }
}