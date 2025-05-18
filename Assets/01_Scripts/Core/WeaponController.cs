using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Data & References")]
    public WeaponDataSO weaponData;
    public Transform    firePoint;
    public LineRenderer lineRenderer;
    public bool         isEquipped;

    private WeaponContext            _ctx;
    private List<InputComponent>     _inputClones;
    private List<ExecuteComponent>   _execClones;
    private List<OnHitComponent>     _hitClones;
    private List<WeaponComponent>    _componentClones;

    void Awake()
    {
        // Build context for components
        _ctx = new WeaponContext
        {
            FirePoint       = firePoint,
            LineRenderer    = lineRenderer,
            PlayerCamera    = Camera.main,
            OnHitComponents = new List<OnHitComponent>(),
            Player          = FindObjectOfType<Player>()
        };

        // Clone ScriptableObjects so each weapon has its own instance
        _inputClones = weaponData.inputComponents
            .Cast<InputComponent>()
            .Select(Instantiate)
            .ToList();

        _execClones = weaponData.executeComponents
            .Cast<ExecuteComponent>()
            .Select(Instantiate)
            .ToList();

        _hitClones = weaponData.onHitComponents
            .Cast<OnHitComponent>()
            .Select(Instantiate)
            .ToList();

        // Populate OnHit list in context
        foreach (var hitSo in _hitClones)
            _ctx.OnHitComponents.Add(hitSo);

        // Combine all execute- and on-hit components for event subscription
        _componentClones = new List<WeaponComponent>();
        _componentClones.AddRange(_execClones.Cast<WeaponComponent>());
        _componentClones.AddRange(_hitClones.Cast<WeaponComponent>());

        // Initialize all components with the shared context
        foreach (var inp in _inputClones)  inp.Initialize(_ctx);
        foreach (var ex  in _execClones)    ex.Initialize(_ctx);
        foreach (var hit in _hitClones)     hit.Initialize(_ctx);
    }

    void Update()
    {
        if (!isEquipped) return;

        // Poll all inputs
        foreach (var inp in _inputClones)
            inp.Poll();

        // Drive time-based logic in execute components
        float dt = Time.deltaTime;
        foreach (var exec in _execClones)
            exec.Tick(dt);
    }

    public void EquipWeapon()
    {
        if (isEquipped) return;
        isEquipped = true;

        foreach (var inp in _inputClones)
        {
            inp.EnableInput();

            // Subscribe Press handlers
            foreach (var h in _componentClones.OfType<IPressHandler>())
                inp.Pressed += h.OnPress;

            // Subscribe Hold handlers
            foreach (var h in _componentClones.OfType<IHoldHandler>())
                inp.Held += h.OnHold;

            // Subscribe Release handlers
            foreach (var h in _componentClones.OfType<IReleaseHandler>())
                inp.Released += h.OnRelease;
        }
    }

    public void UnEquipWeapon()
    {
        if (!isEquipped) return;
        isEquipped = false;

        foreach (var inp in _inputClones)
        {
            inp.DisableInput();

            // Unsubscribe Press handlers
            foreach (var h in _componentClones.OfType<IPressHandler>())
                inp.Pressed -= h.OnPress;

            // Unsubscribe Hold handlers
            foreach (var h in _componentClones.OfType<IHoldHandler>())
                inp.Held -= h.OnHold;

            // Unsubscribe Release handlers
            foreach (var h in _componentClones.OfType<IReleaseHandler>())
                inp.Released -= h.OnRelease;

            inp.Cleanup();
        }

        // Disable any ongoing visuals
        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }
}
