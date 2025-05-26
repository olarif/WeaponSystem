using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Renders and manages a damaging beam from an origin transform using Unity's Particle System.
/// Supports configurable length, width, lifetime, and damage over time.
/// </summary>
public class BeamController : MonoBehaviour
{
    [Header("Particle System Properties")]
    [Tooltip("Width of the beam")]
    public float beamWidth = 0.2f;
    
    [Tooltip("Should the beam use world space simulation?")]
    public bool useWorldSpace = true;

    // Runtime data
    private ParticleSystem _particleSystem;
    private ParticleSystem.MainModule _mainModule;
    private ParticleSystem.ShapeModule _shapeModule;
    private ParticleSystem.VelocityOverLifetimeModule _velocityModule;
    
    private Transform _origin;
    private BeamTargetInfo _targetInfo;
    private float _lifetime;
    private float _damagePerSecond;
    private LayerMask _damageMask;
    private float _endTime;
    private bool _isActive;
    private bool _autoDestroy;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        if (_particleSystem != null)
        {
            _mainModule = _particleSystem.main;
            _shapeModule = _particleSystem.shape;
            _velocityModule = _particleSystem.velocityOverLifetime;
            
            // Configure for beam behavior
            _mainModule.simulationSpace = useWorldSpace ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
            _shapeModule.enabled = true;
            _shapeModule.shapeType = ParticleSystemShapeType.Box;
        }
    }

    /// <summary>
    /// Initialize the beam with all necessary parameters
    /// </summary>
    public void Initialize(Transform origin, BeamTargetInfo targetInfo, float lifetime, float dps, LayerMask damageMask)
    {
        _origin = origin;
        _targetInfo = targetInfo;
        _lifetime = lifetime;
        _damagePerSecond = dps;
        _damageMask = damageMask;
        _autoDestroy = lifetime < Mathf.Infinity;
        _endTime = Time.time + lifetime;
        _isActive = true;

        ConfigureParticleSystem();
        
        if (_particleSystem != null)
        {
            _particleSystem.Play();
        }
    }

    private void Update()
    {
        if (!_isActive) return;

        // Update particle system properties each frame (in case origin moves)
        UpdateParticleSystemProperties();

        // Apply continuous damage
        if (_damagePerSecond > 0f)
        {
            ApplyContinuousDamage();
        }

        // Check for auto-destroy
        if (_autoDestroy && Time.time >= _endTime)
        {
            ForceEnd();
        }
    }

    /// <summary>
    /// Initial configuration of the particle system for beam behavior
    /// </summary>
    private void ConfigureParticleSystem()
    {
        if (_particleSystem == null) return;

        // Main module settings
        _mainModule.startLifetime = 0.5f; // How long particles live
        _mainModule.startSpeed = 0f; // No initial velocity
        _mainModule.startSize = beamWidth;
        _mainModule.startColor = Color.white;
        _mainModule.maxParticles = 1000;
        
        // Set emission rate based on beam length for consistent density
        var emission = _particleSystem.emission;
        emission.rateOverTime = Mathf.Max(50f, _targetInfo.distance * 20f);
        
        UpdateParticleSystemProperties();
    }

    /// <summary>
    /// Updates particle system properties with current beam data
    /// </summary>
    private void UpdateParticleSystemProperties()
    {
        if (_particleSystem == null || _origin == null) return;

        Vector3 startPos = _origin.position;
        Vector3 endPos = _targetInfo.targetPoint;
        Vector3 direction = (endPos - startPos).normalized;
        Vector3 center = (startPos + endPos) * 0.5f;

        // Update shape module for beam
        _shapeModule.position = useWorldSpace ? center : transform.InverseTransformPoint(center);
        _shapeModule.scale = new Vector3(beamWidth, beamWidth, _targetInfo.distance);
        _shapeModule.rotation = useWorldSpace ? 
            Quaternion.LookRotation(direction).eulerAngles : 
            transform.InverseTransformDirection(direction);

        // If using local space, update transform
        if (!useWorldSpace)
        {
            transform.position = startPos;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    /// <summary>
    /// Applies damage over time via raycast
    /// </summary>
    private void ApplyContinuousDamage()
    {
        Vector3 direction = (_targetInfo.targetPoint - _origin.position).normalized;
        
        if (Physics.Raycast(_origin.position, direction, out RaycastHit hit, _targetInfo.distance, _damageMask))
        {
            var damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                float damage = _damagePerSecond * Time.deltaTime;
                damageable.TakeDamage(damage, DamageType.Bleed);
            }
        }
    }

    /// <summary>
    /// Gracefully end the beam with optional extra time
    /// </summary>
    public void EndBeam(float extraTime = 0f)
    {
        if (!_isActive) return;

        _autoDestroy = true;
        _endTime = Time.time + extraTime;
        
        // Stop emitting new particles but let existing ones finish
        if (_particleSystem != null)
        {
            var emission = _particleSystem.emission;
            emission.enabled = false;
        }
    }

    /// <summary>
    /// Immediately end the beam
    /// </summary>
    public void ForceEnd()
    {
        _isActive = false;
        
        if (_particleSystem != null)
        {
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        ForceEnd();
    }
}