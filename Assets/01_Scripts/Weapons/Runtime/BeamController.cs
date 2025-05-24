using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Renders and manages a damaging beam from an origin transform.
/// Supports configurable length, width, lifetime, and damage over time.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class BeamController : MonoBehaviour
{
    [Header("Beam Settings")]
    [Tooltip("Starting point of the beam.")]
    public Transform origin;

    [Tooltip("Beam length in world units.")]
    public float length = 50f;

    [Tooltip("Beam width multiplier on the LineRenderer.")]
    public float width = 0.05f;

    [Header("Lifetime")]
    [Tooltip("How long the beam stays active before auto‐destroy.")]
    public float lifetime = 0.2f;

    [Tooltip("Automatically destroy this object when lifetime expires.")]
    public bool autoDestroy = true;

    [Header("Damage Over Time")]
    [Tooltip("Damage applied per second while beam hits a valid target.")]
    public float damagePerSecond = 0f;

    [Tooltip("Layers that can be damaged by the beam.")]
    public LayerMask damageMask = ~0;

    [Header("Events")]
    public UnityEvent onBeamStart;
    public UnityEvent onBeamEnd;

    // Internal state
    private LineRenderer _lineRenderer;
    private float _destroyTime;
    private bool _isActive;

    /// <summary>
    /// Cache LineRenderer settings on awake.
    /// </summary>
    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.widthMultiplier = width;

        // Assign a default unlit cyan material if none set
        if (_lineRenderer.material == null)
        {
            var mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = Color.cyan;
            _lineRenderer.material = mat;
        }
    }

    /// <summary>
    /// Activate beam and schedule its end.
    /// </summary>
    private void OnEnable()
    {
        _isActive = true;
        _destroyTime = Time.time + lifetime;
        onBeamStart?.Invoke();
    }

    /// <summary>
    /// Update beam positions, apply damage, and handle auto‐destroy.
    /// </summary>
    private void Update()
    {
        if (!_isActive || origin == null)
            return;

        // Update beam endpoints
        Vector3 startPos = origin.position;
        Vector3 endPos = startPos + origin.forward * length;
        _lineRenderer.SetPosition(0, startPos);
        _lineRenderer.SetPosition(1, endPos);

        // Damage over time via raycast
        if (damagePerSecond > 0f)
        {
            if (Physics.Raycast(startPos, origin.forward, out RaycastHit hit, length, damageMask))
            {
                var target = hit.collider.GetComponent<IDamageable>();
                if (target != null)
                {
                    float dmg = damagePerSecond * Time.deltaTime;
                    target.TakeDamage(dmg, DamageType.Bleed);
                }
            }
        }

        // Check for auto‐destroy
        if (autoDestroy && Time.time >= _destroyTime)
        {
            EndNow();
        }
    }

    /// <summary>
    /// Ends the beam immediately (or after extra life), invokes end event, and destroys object.
    /// </summary>
    /// <param name="extraLife">Additional seconds before destruction (optional).</param>
    public void EndBeam(float extraLife = 0f)
    {
        if (!_isActive)
            return;

        autoDestroy = true;
        _destroyTime = Time.time + extraLife;
    }

    /// <summary>
    /// Handles beam shutdown: invokes end event and destroys the GameObject.
    /// </summary>
    private void EndNow()
    {
        _isActive = false;
        onBeamEnd?.Invoke();
        Destroy(gameObject);
    }
}
