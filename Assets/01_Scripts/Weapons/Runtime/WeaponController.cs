using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls weapon input bindings, model attachment, and firing phases.
/// Supports Press, Charge, Continuous, and Release modes.
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Weapon Data")]
    [SerializeField] private WeaponDataSO data;
    private WeaponContext ctx;

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
    [HideInInspector] public List<GameObject> _models = new();

    public void Initialize(WeaponDataSO sourceData, WeaponContext context)
    {
        ctx = context;
        data = Instantiate(sourceData);
        ctx.WeaponController = this;
        AttachModel();
        foreach (var ib in data.inputBindings)
            HookBinding(ib);
    }

    private void AttachModel()
    {
        ctx.FirePoints.Clear();
        foreach (var mdl in _models) if (mdl) Destroy(mdl);
        _models.Clear();

        void Spawn(GameObject prefab, Transform parent)
        {
            if (prefab == null || parent == null) return;
            var model = Instantiate(prefab, parent);
            model.transform.localPosition = data.modelPositionOffset;
            model.transform.localRotation = Quaternion.Euler(data.modelRotationOffset);
            _models.Add(model);
            var fp = model.GetComponentsInChildren<Transform>(true)
                          .FirstOrDefault(t => t.name == "FirePoint");
            if (fp != null) ctx.FirePoints.Add(fp);
        }

        switch (data.defaultHand)
        {
            case Hand.Right: Spawn(data.rightHandModel, ctx.rightHand); break;
            case Hand.Left:  Spawn(data.leftHandModel,  ctx.leftHand);  break;
            case Hand.Both:
                Spawn(data.rightHandModel, ctx.rightHand);
                Spawn(data.leftHandModel,  ctx.leftHand);
                break;
        }
    }

    private void HookBinding(InputBindingData ib)
    {
        var action = ib.actionRef.action;
        action.Enable();

        var sub = new BindingSubscription
        {
            action = action,
            tickCoroutines = new List<Coroutine>()
        };

        float startTime = 0f;
        var ui = ib.hand == Hand.Left ? ctx.leftChargeUI : ctx.rightChargeUI;

        switch (ib.bindingMode)
        {
            case BindingMode.Press:
                sub.onPerform = _ =>
                {
                    if (Time.time - ib.lastPerformTime >= ib.cooldown)
                    {
                        ib.lastPerformTime = Time.time;
                        FirePhase(ib, TriggerPhase.OnPerform);
                    }
                };
                action.performed += sub.onPerform;
                break;

            case BindingMode.Charge:
                sub.onStart = _ =>
                {
                    startTime = Time.time;
                    ui.Reset(); ui.Show();
                    sub.chargeCoroutine = StartCoroutine(UpdateChargeUI(ib, ui));
                };
                sub.onCancel = _ =>
                {
                    if (sub.chargeCoroutine != null) StopCoroutine(sub.chargeCoroutine);
                    ui.Hide();
                    if (Time.time - startTime >= ib.holdTime)
                        FirePhase(ib, TriggerPhase.OnPerform);
                };
                action.started += sub.onStart;
                action.canceled += sub.onCancel;
                break;

            case BindingMode.Continuous:
                sub.onStart = _ =>
                {
                    if (Time.time - ib.lastPerformTime < ib.cooldown) return;
                    ib.lastPerformTime = Time.time;
                    FirePhase(ib, TriggerPhase.OnStart);
                    // start ticks
                    foreach (var ab in ib.bindings.Where(b => b.triggerPhase == TriggerPhase.OnTick))
                        sub.tickCoroutines.Add(StartCoroutine(TickLoop(ib, ab)));
                };
                sub.onCancel = _ =>
                {
                    // stop ticks
                    foreach (var co in sub.tickCoroutines) if (co != null) StopCoroutine(co);
                    sub.tickCoroutines.Clear();
                    FirePhase(ib, TriggerPhase.OnCancel);
                };
                action.started += sub.onStart;
                action.canceled += sub.onCancel;
                break;

            case BindingMode.Release:
                sub.onCancel = _ =>
                {
                    if (Time.time - ib.lastPerformTime >= ib.cooldown)
                    {
                        ib.lastPerformTime = Time.time;
                        FirePhase(ib, TriggerPhase.OnPerform);
                    }
                };
                action.canceled += sub.onCancel;
                break;
        }

        _subscriptions.Add(sub);
    }

    private void FirePhase(InputBindingData ib, TriggerPhase phase)
    {
        foreach (var ab in ib.bindings.Where(b => b.triggerPhase == phase))
            ab.action.Execute(ctx, ib, ab);
    }

    private IEnumerator TickLoop(InputBindingData ib, ActionBindingData ab)
    {
        if (ab.tickRate <= 0f) yield break;
        while (true)
        {
            yield return new WaitForSeconds(ab.tickRate);
            if (Time.time - ib.lastPerformTime >= ib.cooldown)
            {
                ib.lastPerformTime = Time.time;
                ab.action.Execute(ctx, ib, ab);
            }
        }
    }

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

    private void OnDestroy()
    {
        foreach (var sub in _subscriptions)
        {
            if (sub.action == null) continue;
            if (sub.onStart   != null) sub.action.started   -= sub.onStart;
            if (sub.onPerform != null) sub.action.performed -= sub.onPerform;
            if (sub.onCancel  != null) sub.action.canceled  -= sub.onCancel;
            sub.action.Disable();
            if (sub.chargeCoroutine != null) StopCoroutine(sub.chargeCoroutine);
            if (sub.tickCoroutines != null)
                foreach (var co in sub.tickCoroutines) if (co != null) StopCoroutine(co);
        }
        _subscriptions.Clear();

        foreach (var mdl in _models) if (mdl != null) Destroy(mdl);
        _models.Clear();
    }
}
