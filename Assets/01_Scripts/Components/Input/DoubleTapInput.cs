using UnityEngine;
using UnityEngine.InputSystem;

public class DoubleTapInput : InputComponent
{
    public InputActionReference inputReference;
    
    [Tooltip( "Time in seconds to hold the input before it is considered a tap")]
    public float maxTimeBetweenTaps = 0.3f;
    public int tapsRequired = 2;
    
    private float _lastTapTime;
    private int _tapCount;
    private bool _wasTriggered;
    
    public override void Initialize(WeaponContext weapon)
    {
        _tapCount = 0;
        _lastTapTime = -maxTimeBetweenTaps;
        
        if (inputReference != null && inputReference.action != null)
        {
            if (!inputReference.action.enabled) { inputReference.action.Enable(); }

            inputReference.action.performed += OnActionPerformed;
        } 
        else
        {
            Debug.LogError("Input action reference is not set or action is null.");
        }
    }
    
    private void OnActionPerformed(InputAction.CallbackContext context)
    {
        float timeSinceLastTap = Time.time - _lastTapTime;
        
        if (timeSinceLastTap <= maxTimeBetweenTaps)
        {
            _tapCount++;
            if (_tapCount >= tapsRequired)
            {
                _wasTriggered = true;
                _tapCount = 0;
            }
        }
        else
        {
            _tapCount = 1;
        }
        
        _lastTapTime = Time.time;
    }

    public override bool CanExecute()
    {
        if (_wasTriggered)
        {
            _wasTriggered = false;
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
            inputReference.action.performed -= OnActionPerformed;
        }
    }
}