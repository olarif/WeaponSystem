[System.Flags]
public enum FireTrigger
{
    None      = 0,
    OnPress   = 1 << 0,
    OnHold    = 1 << 1,
    OnRelease = 1 << 2,

    // convenience
    PressOrHold = OnPress | OnHold,
    All         = OnPress | OnHold | OnRelease
}