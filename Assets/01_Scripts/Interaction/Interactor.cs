using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Interactor : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableMask = -1;
    
    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;
    
    [Header("Performance")]
    [SerializeField] private float raycastInterval = 0.1f; // Slower raycast for better performance
    [SerializeField] private float highlightDelay = 0.1f; // Prevent rapid highlight changes
    
    private PlayerInventory playerInventory;
    private IPickupable currentPickupable;
    private IInteractable currentInteractable;
    
    private Coroutine raycastCoroutine;
    private Coroutine highlightCoroutine;
    
    private void Awake()
    {
        playerInventory = GetComponent<PlayerInventory>();
        if (playerCamera == null)
            playerCamera = Camera.main;
    }
    
    private void OnEnable()
    {
        if (interactAction?.action != null)
        {
            interactAction.action.performed += OnInteract;
            interactAction.action.Enable();
        }
        
        raycastCoroutine = StartCoroutine(RaycastRoutine());
    }
    
    private void OnDisable()
    {
        if (interactAction?.action != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }
        
        if (raycastCoroutine != null)
            StopCoroutine(raycastCoroutine);
            
        if (highlightCoroutine != null)
            StopCoroutine(highlightCoroutine);
            
        ClearCurrentTarget();
    }
    
    private IEnumerator RaycastRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(raycastInterval);
        
        while (true)
        {
            CheckForTarget();
            yield return wait;
        }
    }
    
    private void CheckForTarget()
    {
        if (playerCamera == null) return;
        
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableMask);
        
        // Debug visualization
        Debug.DrawRay(ray.origin, ray.direction * interactRange, 
                     hitSomething ? Color.green : Color.red, raycastInterval);
        
        if (!hitSomething)
        {
            SetNewTarget(null, null);
            return;
        }
        
        // Check for pickupable first, then interactable
        IPickupable pickupable = hit.collider.GetComponentInParent<IPickupable>();
        IInteractable interactable = pickupable == null ? 
            hit.collider.GetComponentInParent<IInteractable>() : null;
        
        SetNewTarget(pickupable, interactable);
    }
    
    private void SetNewTarget(IPickupable newPickupable, IInteractable newInteractable)
    {
        if (newPickupable == currentPickupable && newInteractable == currentInteractable)
            return; // Same target, no change needed
        
        // Use coroutine to prevent rapid highlight changes
        if (highlightCoroutine != null)
            StopCoroutine(highlightCoroutine);
            
        highlightCoroutine = StartCoroutine(ChangeHighlightWithDelay(newPickupable, newInteractable));
    }
    
    private IEnumerator ChangeHighlightWithDelay(IPickupable newPickupable, IInteractable newInteractable)
    {
        yield return new WaitForSeconds(highlightDelay);
        
        // Clear current highlights
        currentPickupable?.Highlight(false);
        currentInteractable?.Highlight(false);
        
        // Set new targets
        currentPickupable = newPickupable;
        currentInteractable = newInteractable;
        
        // Apply new highlights
        currentPickupable?.Highlight(true);
        currentInteractable?.Highlight(true);
    }
    
    private void OnInteract(InputAction.CallbackContext context)
    {
        if (currentPickupable != null)
        {
            if (currentPickupable.CanPickup(playerInventory))
                currentPickupable.OnPickup(playerInventory);
        }
        else if (currentInteractable != null)
        {
            //if (currentInteractable.CanInteract();
             //   currentInteractable.OnInteract();
        }
    }
    
    private void ClearCurrentTarget()
    {
        currentPickupable?.Highlight(false);
        currentInteractable?.Highlight(false);
        currentPickupable = null;
        currentInteractable = null;
    }
}