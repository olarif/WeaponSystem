using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Data & References")]
    public WeaponDataSO weaponData;
    public Transform     firePoint;
    public LineRenderer  lineRenderer;
    public bool          isEquipped;

    WeaponContext        _ctx;
    List<InputComponent>   _inputClones;
    List<ExecuteComponent> _execClones;
    List<OnHitComponent>   _hitClones;
    
    void Awake()
    {
        _ctx = new WeaponContext {
            FirePoint       = firePoint,
            LineRenderer    = lineRenderer,
            PlayerCamera    = Camera.main,
            OnHitComponents = new List<OnHitComponent>(),
            Player          = FindFirstObjectByType<Player>()
        };

        // Clone each SO so we get a fresh instance per‐weapon
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
        
        // 2) Populate the context’s OnHitComponents list
        foreach (var hitSo in _hitClones)
            _ctx.OnHitComponents.Add(hitSo);

        // 3) Initialize everything with ctx
        foreach (var inp in _inputClones)  inp.Initialize(_ctx);
        foreach (var ex  in _execClones)  ex .Initialize(_ctx);
        foreach (var hit in _hitClones)   hit.Initialize(_ctx);
    }

    void Update()
    {
        if (!isEquipped) return;
        
        foreach (var inp in _inputClones)
            inp.Poll();
    }

    public void EquipWeapon()
    {
        if (isEquipped) return;
        isEquipped = true;
        
        // Enable all input SOs
        foreach (var inp in _inputClones)
            inp.EnableInput();

        // Hook up input → exec/onHit events
        foreach (var inp in _inputClones)
        {
            // Press
            foreach (var h in _execClones.OfType<IPressHandler>())
                inp.Pressed += h.OnPress;
            foreach (var h in _hitClones.OfType<IPressHandler>())
                inp.Pressed += h.OnPress;

            // Hold
            foreach (var h in _execClones.OfType<IHoldHandler>())
                inp.Held += h.OnHold;

            // Release
            foreach (var h in _execClones.OfType<IReleaseHandler>())
                inp.Released += h.OnRelease;
        }
    }

    public void UnEquipWeapon()
    {
        if (!isEquipped) return;
        isEquipped = false;
        
        // Disable all input SOs
        foreach (var inp in _inputClones)
            inp.DisableInput();

        // Unsubscribe all handlers and cleanup
        foreach (var inp in _inputClones)
        {
            foreach (var h in _execClones.OfType<IPressHandler>())
                inp.Pressed -= h.OnPress;
            foreach (var h in _hitClones.OfType<IPressHandler>())
                inp.Pressed -= h.OnPress;

            foreach (var h in _execClones.OfType<IHoldHandler>())
                inp.Held -= h.OnHold;

            foreach (var h in _execClones.OfType<IReleaseHandler>())
                inp.Released -= h.OnRelease;

            inp.Cleanup();
        }

        // disable any ongoing beams or coroutines
        lineRenderer.enabled = false;
    }
}