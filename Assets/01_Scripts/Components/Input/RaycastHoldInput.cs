using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastHoldInput : InputComponent
{
     public InputActionReference inputAction;
     
     private WeaponContext _ctx;
     
     public override bool CanExecute() => inputAction != null && inputAction.action.ReadValue<float>() > 0.5f;
}