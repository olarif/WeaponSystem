using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private float _interactRange = 2f;
    
    [SerializeField] private InputActionReference _interactAction;
    
    private IInteractable _currentInteractable;
    
    private void OnEnable()
    {
        if (_interactAction != null)
        {
            _interactAction.action.performed += OnInteract;
            _interactAction.action.Enable();
        }
        else
        {
            Debug.LogError("Interact action not assigned in the Inspector!");
        }
    }
    
    private void OnDisable()
    {
        if (_interactAction != null)
        {
            _interactAction.action.performed -= OnInteract;
            _interactAction.action.Disable();
        }
        
        if (_currentInteractable != null)
        {
            _currentInteractable.Highlight(false, null);
            _currentInteractable = null;
        }
    }
    
    private void Update()
    {
        // Check for interactable objects
        CheckForInteractable();
    }
    
    private void OnInteract(InputAction.CallbackContext context)
    {
        if (_currentInteractable != null)
        {
            _currentInteractable.Interact( GetComponent<WeaponManager>());
            Debug.DrawRay(_playerCamera.transform.position, _playerCamera.transform.forward * _interactRange, Color.green, 0.5f);
        }
    }
    
    private void CheckForInteractable()
    {
        if (_playerCamera == null) return;
        
        // Cast ray from camera center
        Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, _interactRange);
        
        // Debug ray
        Debug.DrawRay(ray.origin, ray.direction * _interactRange, hitSomething ? Color.green : Color.red);
        
        // Handle case where we hit something
        if (hitSomething)
        {
            // Try to get interactable component
            IInteractable hitInteractable = hit.collider.GetComponent<IInteractable>();
            
            // If we hit an interactable and it's different from our current one
            if (hitInteractable != null && _currentInteractable != hitInteractable)
            {
                // Unhighlight old object if we had one
                if (_currentInteractable != null)
                    _currentInteractable.Highlight(false, null);
                
                // Set and highlight new object
                _currentInteractable = hitInteractable;
                _currentInteractable.Highlight(true, _playerCamera);
            }
            // If we didn't hit an interactable, clear current
            else if (hitInteractable == null && _currentInteractable != null)
            {
                _currentInteractable.Highlight(false, null);
                _currentInteractable = null;
            }
        }
        // If we didn't hit anything but had an interactable, clear it
        else if (_currentInteractable != null)
        {
            _currentInteractable.Highlight(false, null);
            _currentInteractable = null;
        }
    }
}