public enum WeaponInputEvent
{
    Press,       // on performed
    Hold,        // on hold >= holdTime
    Release,     // on canceled
    Continuous,  // on performed while holding
    // TODO: DoubleClick, // on performed twice within doubleClickTime
}