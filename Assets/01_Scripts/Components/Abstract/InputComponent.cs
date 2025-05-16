using System;

public abstract class InputComponent : WeaponComponent
{
    public event Action Pressed, Held, Released;
    protected bool _wasDown;
    
    protected void RaisePressed() { Pressed?.Invoke(); }
    protected void RaiseHeld() { Held?.Invoke(); }
    protected void RaiseReleased() { Released?.Invoke(); }

    public void Poll()
    {
        bool down = CanExecute();
        if (down && !_wasDown)
        {
            RaisePressed();
            RaiseHeld();
        }
        else if (down)
        {
            RaiseHeld();
        }
        else if (_wasDown)
        {
            RaiseReleased();
        }
        _wasDown = down;
    }
    
    public abstract bool CanExecute();

    public virtual void EnableInput() { }
    
    public virtual void DisableInput() { }
}