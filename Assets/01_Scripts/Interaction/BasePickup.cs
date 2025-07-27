using UnityEngine;

public abstract class BasePickup : MonoBehaviour, IPickupable
{
    [Header("Pickup Settings")]
    [SerializeField] protected string promptText = "E to pickup";
    [SerializeField] protected Vector3 uiOffset = new Vector3(0, 1f, 0);
    [SerializeField] protected GameObject uiPrefab;
    
    [Header("Highlight Settings")]
    [SerializeField] protected bool useOutline = true;
    [SerializeField] protected Color highlightColor = Color.yellow;
    
    protected InteractionUI uiInstance;
    protected Renderer objectRenderer;
    protected Material[] originalMaterials;
    protected Material highlightMaterial;
    
    protected virtual void Awake()
    {
        SetupUI();
        SetupHighlight();
    }
    
    private void SetupUI()
    {
        if (uiPrefab != null)
        {
            GameObject uiObject = Instantiate(uiPrefab, transform);
            uiObject.transform.localPosition = uiOffset;
            uiInstance = uiObject.GetComponent<InteractionUI>();
        }
    }
    
    private void SetupHighlight()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterials = objectRenderer.materials;
            
            if (useOutline)
            {
                // Create a simple highlight material
                highlightMaterial = new Material(Shader.Find("Standard"));
                highlightMaterial.color = highlightColor;
                highlightMaterial.SetFloat("_Mode", 2); // Transparent mode
                highlightMaterial.SetFloat("_Metallic", 0f);
                highlightMaterial.SetFloat("_Glossiness", 0.5f);
            }
        }
    }
    
    public virtual void Highlight(bool highlighted)
    {
        if (uiInstance != null)
        {
            if (highlighted)
                uiInstance.ShowPrompt(GetPromptText(), Camera.main);
            else
                uiInstance.HidePrompt();
        }
        
        ApplyHighlight(highlighted);
    }
    
    protected virtual void ApplyHighlight(bool highlighted)
    {
        if (!useOutline || objectRenderer == null) return;
        
        if (highlighted)
        {
            // Add highlight effect
            Material[] highlightedMaterials = new Material[originalMaterials.Length + 1];
            for (int i = 0; i < originalMaterials.Length; i++)
                highlightedMaterials[i] = originalMaterials[i];
            highlightedMaterials[originalMaterials.Length] = highlightMaterial;
            
            objectRenderer.materials = highlightedMaterials;
        }
        else
        {
            objectRenderer.materials = originalMaterials;
        }
    }
    
    public abstract string GetPromptText();
    public abstract bool CanPickup(PlayerInventory inventory);
    public abstract void OnPickup(PlayerInventory inventory);
}