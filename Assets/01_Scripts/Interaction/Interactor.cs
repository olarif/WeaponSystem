using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Casts a ray from the centre of the player camera, highlights / unhighlights
/// objects that implement IInteractable and passes Interact() calls to them.
/// </summary>
public class Interactor : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private float  _interactRange   = 2f;
    [Tooltip("Which layers count as interactables (exclude UI / prompt).")]
    [SerializeField] private LayerMask _interactableMask = ~0;

    [Header("Input")]
    [SerializeField] private InputActionReference _interactAction;

    private IInteractable _currentInteractable;
    private WeaponManager _weaponManager;
    
    private void Awake()
    {
        _weaponManager = GetComponent<WeaponManager>();
    }

    private void OnEnable()
    {
        if (_interactAction == null)
        {
            Debug.LogError("Interact action not assigned in the Inspector!");
            return;
        }

        _interactAction.action.performed += OnInteract;
        _interactAction.action.Enable();
    }

    private void OnDisable()
    {
        if (_interactAction != null)
        {
            _interactAction.action.performed -= OnInteract;
            _interactAction.action.Disable();
        }

        ClearCurrent();
    }

    
    //Raycast scan throttle to 20Hz (50ms) 
    private float _nextScan;
    private const float _scanInterval = 0.05f;

    private void Update()
    {
        if (Time.time >= _nextScan)
        {
            _nextScan = Time.time + _scanInterval;
            CheckForInteractable();
        }
    }

    private void OnInteract(InputAction.CallbackContext _)
    {
        _currentInteractable?.Interact(_weaponManager);
    }

    private void CheckForInteractable()
    {
        if (_playerCamera == null) return;

        // Cast ray from camera centre
        Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, _interactRange, _interactableMask);

        // Debug ray
        Debug.DrawRay(ray.origin,
                      ray.direction * _interactRange,
                      hitSomething ? Color.green : Color.red,
                      0.05f);

        if (!hitSomething)
        {
            ClearCurrent();
            return;
        }

        // Try exact object, otherwise climb the hierarchy
        IInteractable hitInteractable =
            hit.collider.GetComponent<IInteractable>() ??
            hit.collider.GetComponentInParent<IInteractable>();

        if (hitInteractable == _currentInteractable) return;       // nothing changed

        // switch highlight
        _currentInteractable?.Highlight(false, null);
        _currentInteractable = hitInteractable;
        _currentInteractable?.Highlight(true, _playerCamera);
    }

    private void ClearCurrent()
    {
        _currentInteractable?.Highlight(false, null);
        _currentInteractable = null;
    }
}
