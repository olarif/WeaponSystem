using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles input bindings for a weapon, cloning InputAction instances per-runner
/// and wiring up WeaponActionData callbacks safely.
/// </summary>
public class WeaponBindingRunner : MonoBehaviour
{
    // Context and binding
    private WeaponContext                      _ctx;
    private WeaponDataSO.InputBinding          _binding;

    // Cloned InputAction instance
    private InputAction                        _actionInstance;

    // Timing and state
    private bool                               _isHolding;
    private float                              _holdStart;
    private float                              _lastFire;
    
    // Charge UI
    private List<ChargeUI>                     _chargeUIs;
    private bool                               _isCharging;

    // ScriptableObject-defined actions for this binding
    private readonly List<WeaponActionData>    _actions = new List<WeaponActionData>();

    // Stored delegates for clean unsubscribe
    private Action<InputAction.CallbackContext> _onPress;
    private Action<InputAction.CallbackContext> _onRelease;
    private Action<InputAction.CallbackContext> _onChargeStart;
    private Action<InputAction.CallbackContext> _onChargeEnd;

    /// <summary>
    /// Initializes this runner with a binding and context, cloning the input action
    /// and wiring up callbacks to WeaponActionData methods.
    /// </summary>
    public void Setup(WeaponDataSO.InputBinding binding, WeaponContext ctx)
    {
        _binding = binding;
        _ctx     = ctx;

        // Clone the shared InputAction so each runner has its own instance
        _actionInstance = binding.inputAction.action.Clone();
        _actionInstance.Enable();

        // Copy actions from the ScriptableObject
        _actions.Clear();
        _actions.AddRange(binding.actions);

        // Prepare Charge UI if needed
        if (binding.eventType == WeaponInputEvent.Charge)
        {
            _chargeUIs = new List<ChargeUI>();
            if (binding.fireHand == WeaponDataSO.Hand.Left ||
                binding.fireHand == WeaponDataSO.Hand.Both)
                if (ctx.leftChargeUI  != null) _chargeUIs.Add(ctx.leftChargeUI);
            if (binding.fireHand == WeaponDataSO.Hand.Right ||
                binding.fireHand == WeaponDataSO.Hand.Both)
                if (ctx.rightChargeUI != null) _chargeUIs.Add(ctx.rightChargeUI);
        }
        
        // Wire up callbacks based on event type
        switch (binding.eventType)
        {
            case WeaponInputEvent.Press:
                _onPress = ctx => {
                    if (Time.time < _lastFire + binding.fireRate) return;
                    _lastFire = Time.time;
                    foreach (var a in _actions)
                        a.OnPress(_ctx, binding);
                };
                _actionInstance.performed += _onPress;
                break;

            case WeaponInputEvent.Release:
                _onRelease = ctx => {
                    if (Time.time < _lastFire + binding.fireRate) return;
                    _lastFire = Time.time;
                    foreach (var a in _actions)
                        a.OnRelease(_ctx, binding);
                };
                _actionInstance.canceled += _onRelease;
                break;

            case WeaponInputEvent.Charge:
                _onChargeStart = ctx2 => {
                    _isCharging = true;               // START CHARGE
                    _holdStart  = Time.time;
                    foreach (var ui in _chargeUIs)
                    {
                        ui.Reset();
                        ui.Show();
                    }
                };
                _onChargeEnd = ctx2 => {
                    // END CHARGE
                    _isCharging = false;
                    foreach (var ui in _chargeUIs)
                        ui.Hide();

                    // auto‐fire if held long enough
                    if (Time.time >= _holdStart + binding.holdTime)
                        foreach (var a in _actions)
                            a.OnPress(_ctx, binding);
                };
                _actionInstance.started  += _onChargeStart;
                _actionInstance.canceled += _onChargeEnd;
                break;

            case WeaponInputEvent.Continuous:
                _actionInstance.started += ctx2 => {
                    _isHolding = true;
                    // SPAWN BEAM HERE
                    foreach (var a in _actions)
                        a.OnPress(_ctx, _binding);
                };
                _actionInstance.canceled += ctx2 => {
                    _isHolding = false;
                    // DESTROY BEAM HERE
                    foreach (var a in _actions)
                        a.OnRelease(_ctx, _binding);
                };
                break;
        }
    }

    /// <summary>
    /// Handles Continuous event firing.
    /// </summary>
    private void Update()
    {
        if (_binding == null) 
            return;

        if (_binding.eventType == WeaponInputEvent.Continuous && _isHolding)
        {
            if (Time.time >= _lastFire + _binding.fireRate)
            {
                _lastFire = Time.time;
                foreach (var a in _actions)
                    a.OnContinuous(_ctx, _binding);
            }
        }
        
        // --- CHARGE UI PERCENT UPDATE ---
        if (_binding.eventType == WeaponInputEvent.Charge && _isCharging)
        {
            float held = Time.time - _holdStart;
            float pct  = Mathf.Clamp01(held / _binding.holdTime);
            foreach (var ui in _chargeUIs)
                ui.SetPercent(pct);
        }

    }

    /// <summary>
    /// Clean up cloned InputAction and unsubscribe callbacks to avoid leaks.
    /// </summary>
    private void OnDestroy()
    {
        if (_actionInstance != null)
        {
            if (_onPress       != null) _actionInstance.performed -= _onPress;
            if (_onRelease     != null) _actionInstance.canceled  -= _onRelease;
            if (_onChargeStart != null) _actionInstance.started   -= _onChargeStart;
            if (_onChargeEnd   != null) _actionInstance.canceled  -= _onChargeEnd;

            _actionInstance.Disable();
            _actionInstance.Dispose();
        }
    }
}
