using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls weapon input bindings, model attachment, and firing phases.
/// Supports charge-mode and standard input modes.
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Weapon Data")]
    [SerializeField] private WeaponDataSO data;
    private WeaponContext ctx;

    // Subscription info for each input binding
    private struct BindingSubscription
    {
        public InputAction action;
        public Action<InputAction.CallbackContext> onStart;
        public Action<InputAction.CallbackContext> onPerform;
        public Action<InputAction.CallbackContext> onCancel;
        public Coroutine chargeCoroutine;
        public List<Coroutine> tickCoroutines;
    }

    private readonly List<BindingSubscription> _subscriptions = new();

    [HideInInspector]
    public List<GameObject> _models = new();

    /// <summary>
    /// Sets up the weapon controller with a fresh copy of the data and the shared context.
    /// </summary>
    public void Initialize(WeaponDataSO sourceData, WeaponContext context)
    {
        ctx = context;
        data = Instantiate(sourceData);
        ctx.WeaponController = this;

        AttachModel();

        // Hook each input binding defined in the data
        foreach (var ib in data.inputBindings)
            HookBinding(ib);
    }

    /// <summary>
    /// Instantiates the correct model(s) based on configured hand, and gathers fire points.
    /// </summary>
    private void AttachModel()
    {
        ctx.FirePoints.Clear();

        // Remove previous models
        foreach (var mdl in _models)
            if (mdl != null)
                Destroy(mdl);
        _models.Clear();

        // Helper to spawn a model for one hand
        void Spawn(GameObject prefab, Transform parent)
        {
            if (prefab == null || parent == null)
                return;

            var model = Instantiate(prefab, parent);
            model.transform.localPosition = data.modelPositionOffset;
            model.transform.localRotation = Quaternion.Euler(data.modelRotationOffset);
            _models.Add(model);

            // Find the child fire point
            var firePoint = model.GetComponentsInChildren<Transform>(true)
                                  .FirstOrDefault(t => t.name == "FirePoint");
            if (firePoint != null)
                ctx.FirePoints.Add(firePoint);
        }

        // Spawn model(s) according to defaultHand setting
        switch (data.defaultHand)
        {
            case Hand.Right:
                Spawn(data.rightHandModel, ctx.rightHand);
                break;
            case Hand.Left:
                Spawn(data.leftHandModel, ctx.leftHand);
                break;
            case Hand.Both:
                Spawn(data.rightHandModel, ctx.rightHand);
                Spawn(data.leftHandModel, ctx.leftHand);
                break;
        }
    }

    /// <summary>
    /// Binds an InputBindingData to Unity InputActions and sets up callbacks.
    /// Supports both Charge and standard modes.
    /// </summary>
    private void HookBinding(InputBindingData ib)
    {
        var action = ib.actionRef.action;
        action.Enable();

        // Choose the correct UI for charge mode
        var ui = ib.hand == Hand.Left ? ctx.leftChargeUI : ctx.rightChargeUI;
        float chargeStartTime = 0f;
        var tickCoroutines = new List<Coroutine>();

        var sub = new BindingSubscription
        {
            action = action,
            tickCoroutines = tickCoroutines
        };

        if (ib.bindingMode == BindingMode.Charge)
        {
            // CHARGE MODE: start and cancel
            sub.onStart = _ =>
            {
                chargeStartTime = Time.time;
                ui.Reset(); ui.Show();
                sub.chargeCoroutine = StartCoroutine(UpdateChargeUI(ib, ui));
            };

            sub.onCancel = _ =>
            {
                if (sub.chargeCoroutine != null)
                    StopCoroutine(sub.chargeCoroutine);

                ui.Hide();

                // Only perform if held long enough
                if (Time.time - chargeStartTime >= ib.holdTime)
                    FirePhase(ib, TriggerPhase.OnPerform);
            };

            action.started += sub.onStart;
            action.canceled += sub.onCancel;
        }
        else
        {
            // STANDARD MODES: OnStart, OnPerform, OnCancel
            sub.onStart = _ =>
            {
                // First tick immediately
                foreach (var ab in ib.bindings.Where(b => b.triggerPhase == TriggerPhase.OnTick))
                {
                    ab.action.Execute(ctx, ib, ab);
                    tickCoroutines.Add(StartCoroutine(TickLoop(ib, ab)));
                }
                FirePhase(ib, TriggerPhase.OnStart);
            };

            sub.onPerform = _ =>
            {
                // Cooldown guard
                if (Time.time - ib.lastPerformTime < ib.cooldown)
                    return;

                ib.lastPerformTime = Time.time;
                FirePhase(ib, TriggerPhase.OnPerform);
            };

            sub.onCancel = _ =>
            {
                // Stop ticking coroutines
                foreach (var co in tickCoroutines)
                    if (co != null)
                        StopCoroutine(co);
                tickCoroutines.Clear();

                FirePhase(ib, TriggerPhase.OnCancel);
            };

            action.started += sub.onStart;
            action.performed += sub.onPerform;
            action.canceled += sub.onCancel;
        }

        _subscriptions.Add(sub);
    }

    /// <summary>
    /// Fires all bound actions in a given phase.
    /// </summary>
    private void FirePhase(InputBindingData ib, TriggerPhase phase)
    {
        foreach (var ab in ib.bindings.Where(b => b.triggerPhase == phase))
            ab.action.Execute(ctx, ib, ab);
    }

    /// <summary>
    /// Loops OnTick bindings at their specified tickRate.
    /// </summary>
    private IEnumerator TickLoop(InputBindingData ib, ActionBindingData ab)
    {
        if (ab.tickRate <= 0f)
            yield break;

        while (true)
        {
            yield return new WaitForSeconds(ab.tickRate);
            ab.action.Execute(ctx, ib, ab);
        }
    }

    /// <summary>
    /// Updates the charge UI fill percentage until full.
    /// </summary>
    private IEnumerator UpdateChargeUI(InputBindingData ib, ChargeUI ui)
    {
        float elapsed = 0f;
        while (elapsed < ib.holdTime)
        {
            elapsed += Time.deltaTime;
            ui.SetPercent(elapsed / ib.holdTime);
            yield return null;
        }
        ui.SetPercent(1f);
    }

    /// <summary>
    /// Unsubscribe all input actions and clean up models/coroutines.
    /// </summary>
    private void OnDestroy()
    {
        foreach (var sub in _subscriptions)
        {
            if (sub.action == null)
                continue;

            // Unbind callbacks
            if (sub.onStart   != null) sub.action.started   -= sub.onStart;
            if (sub.onPerform != null) sub.action.performed -= sub.onPerform;
            if (sub.onCancel  != null) sub.action.canceled  -= sub.onCancel;
            sub.action.Disable();

            // Stop any running coroutines
            if (sub.chargeCoroutine != null)
                StopCoroutine(sub.chargeCoroutine);

            if (sub.tickCoroutines != null)
            {
                foreach (var co in sub.tickCoroutines)
                    if (co != null)
                        StopCoroutine(co);
            }
        }
        _subscriptions.Clear();

        // Destroy spawned models
        foreach (var mdl in _models)
            if (mdl != null)
                Destroy(mdl);
        _models.Clear();
    }
}
