using UnityEngine;

public interface IInputProvider
{
    Vector2 MovementInput { get; }
    Vector2 RotationInput { get; }
    bool JumpPressed { get; }
    bool JumpHeld { get; }
    bool SprintInput { get; }
    bool CrouchInput { get; }
    bool DashInput { get; }
    bool IsInputEnabled { get; }
}