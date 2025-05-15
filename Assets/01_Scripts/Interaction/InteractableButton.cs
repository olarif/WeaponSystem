using UnityEngine;
using System.Collections;

public class InteractableButton : BaseInteractable
{
    [Header("Button Settings")]
    [SerializeField] private bool _isPressed = false;
    [SerializeField] private bool _isToggle = false;
    [SerializeField] private bool _isTimed = false;
    [SerializeField] private float _timeToPress = 2f;
    
    public override void Interact()
    {
        if (_isPressed)
        {
            if (_isToggle)
            {
                _isPressed = false;
                Debug.Log("Button released");
            }
            else
            {
                Debug.Log("Button already pressed");
            }
        }
    }
}
