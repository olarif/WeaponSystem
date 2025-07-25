using UnityEngine;

/// <summary>
/// Base class that handles outline & world-space prompt for any interactable object.
/// </summary>
public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction Prompt")]
    [SerializeField] private string     _buttonPrompt = "E";
    [SerializeField] private Vector3    _promptOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private GameObject _canvasPrefab;

    [Header("Outline Shader")]
    [SerializeField] private Material _outlineMaterial;
    private const float _defaultOutlineScale     = 1f;
    private const float _highlightedOutlineScale = 1.04f;

    private GameObject _promptInstance;
    private Camera     _playerCamera;

    // ──────────────────────────────────────────────────────────────
    #region Setup

    protected virtual void Awake()
    {
        // outline setup
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.materials.Length >= 2)
        {
            _outlineMaterial         = new Material(renderer.materials[1]);
            var mats          = renderer.materials;
            mats[1]                  = _outlineMaterial;
            renderer.materials       = mats;
        }

        // pre-instantiate prompt
        if (_canvasPrefab != null)
        {
            _promptInstance = Instantiate(_canvasPrefab, transform);
            _promptInstance.transform.localPosition = _promptOffset;
            _promptInstance.layer = LayerMask.NameToLayer("Ignore Raycast");

            _promptInstance
                .GetComponent<InteractCanvas>()
                .InitializeCanvas(_buttonPrompt);

            _promptInstance.SetActive(false);    // start hidden
        }
    }

    #endregion
    // ──────────────────────────────────────────────────────────────
    #region Highlight & Prompt

    public virtual void Highlight(bool isHighlighted, Camera playerCamera)
    {
        _playerCamera = playerCamera;

        if (isHighlighted)
            ShowPrompt();
        else
            HidePrompt();

        RenderOutline(isHighlighted);
    }

    private void ShowPrompt()
    {
        if (_promptInstance == null) return;

        _promptInstance.SetActive(true);
        FaceCamera();
    }

    private void HidePrompt()
    {
        if (_promptInstance != null)
            _promptInstance.SetActive(false);
    }

    private void RenderOutline(bool isHighlighted)
    {
        if (_outlineMaterial == null) return;
        _outlineMaterial.SetFloat("_Scale",
                                  isHighlighted ? _highlightedOutlineScale
                                                : _defaultOutlineScale);
    }

    private void FaceCamera()
    {
        if (_promptInstance == null || _playerCamera == null) return;

        Vector3 dir = _promptInstance.transform.position - _playerCamera.transform.position;
        _promptInstance.transform.rotation = Quaternion.LookRotation(dir);
    }

    private void Update()
    {
        if (_promptInstance != null && _promptInstance.activeSelf)
            FaceCamera();
    }

    #endregion
    // ──────────────────────────────────────────────────────────────
    #region Interaction API

    public virtual void Interact(WeaponManager player) { }
    public virtual void Interact() { }

    #endregion
}
