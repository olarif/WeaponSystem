using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private string _buttonPrompt = "E";
    [SerializeField] private Vector3 _promptOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private GameObject _canvas;
    
    [Header("Outline Settings")]
    [SerializeField] private Material _outlineMaterial;
    private float _defaultOutlineScale = 1f;
    private float _highlightedOutlineScale = 1.04f;
    
    private Camera _playerCamera;

    private GameObject _promptInstance;

    protected virtual void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();
        
        if (renderer != null && renderer.materials.Length >= 2)
        {
            // Create a unique instance of the outline material
            _outlineMaterial = new Material(renderer.materials[1]);
        
            // Apply the instance to the renderer
            Material[] materials = renderer.materials;
            materials[1] = _outlineMaterial;
            renderer.materials = materials;
        }
    }

    public virtual void Highlight(bool isHighlighted, Camera playerCamera)
    {
        _playerCamera = playerCamera;
        
        if (isHighlighted)
        {
            ShowInteractionPrompt();
        }
        else
        {
            HideInteractionPrompt();
        }
        
        RenderOutline(isHighlighted);
    }
    
    private void ShowInteractionPrompt()
    {
        if (_promptInstance == null && _canvas != null)
        {
            _promptInstance = Instantiate(_canvas, transform.position + _promptOffset, Quaternion.identity);

            _promptInstance.GetComponent<InteractCanvas>().InitializeCanvas(_buttonPrompt);
        }
        
        if (_promptInstance != null)
        {
            _promptInstance.SetActive(true);
            LookAtCamera();
        }
    }
    
    private void HideInteractionPrompt()
    {
        if (_promptInstance != null)
        {
            //_promptInstance.SetActive(false);
            Destroy(_promptInstance);
        }
    }
    
    private void RenderOutline(bool isHighlighted)
    {
        if (_outlineMaterial != null)
        {
            float scale = isHighlighted ? _highlightedOutlineScale : _defaultOutlineScale;
            _outlineMaterial.SetFloat("_Scale", scale);
        }
    }
    
    private void LookAtCamera()
    {
        if (_promptInstance != null && _playerCamera != null)
        {
            _promptInstance.transform.LookAt(_promptInstance.transform.position + _playerCamera.transform.rotation * Vector3.forward, _playerCamera.transform.rotation * Vector3.up);
        }
    }
    
    private void Update()
    {
        if (_promptInstance != null && _promptInstance.activeSelf)
        {
            LookAtCamera();
        }
    }
    
    private void OnDestroy()
    {
        if (_promptInstance != null)
        {
            Destroy(_promptInstance);
        }
    }

    public virtual void Interact(WeaponManager player){ }
    public virtual void Interact() { }
}