using UnityEngine;

public interface IInputProvider
{
    Vector2 MovementInput { get; }
    Vector2 RotationInput { get; }
    bool JumpInput { get; }
    bool SprintInput { get; }
    bool CrouchInput { get; }
    bool DashInput { get; }
    bool IsInputEnabled { get; }
}