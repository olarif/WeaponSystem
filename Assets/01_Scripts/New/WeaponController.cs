using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [SerializeField] WeaponDataSO data;
    WeaponContext ctx;

    // track which bindings we've actually hooked,
    // so we can undo them in OnDestroy
    struct BindingSub
    {
        public InputAction action;
        public Action<InputAction.CallbackContext> onStart;
        public Action<InputAction.CallbackContext> onPerform;
        public Action<InputAction.CallbackContext> onCancel;
        public Coroutine tickCoroutine;
        public Coroutine chargeCoroutine;
    }

    List<BindingSub> _subs = new List<BindingSub>();

    [HideInInspector] public List<GameObject> _models = new();

    public void Initialize(WeaponDataSO sourceData, WeaponContext ctx)
    {
        data = Instantiate(sourceData);
        this.ctx = ctx;
        ctx.WeaponController = this;

        AttachModel();

        // Hook each binding
        foreach (var ib in data.inputBindings)
        {
            HookBinding(ib);
        }
    }

    void HookBinding(InputBindingData ib)
    {
        var act = ib.actionRef.action;
        act.Enable();

        BindingSub sub = new BindingSub { action = act };

        float startTime = 0f;
        ChargeUI ui = ib.hand == Hand.Left ? ctx.leftChargeUI : ctx.rightChargeUI;

        switch (ib.bindingMode)
        {
            case BindingMode.Press:
                sub.onPerform = ctx => {
                    if (Time.time - ib.lastPerformTime >= ib.cooldown)
                    {
                        FirePhase(ib, TriggerPhase.OnPerform);
                        ib.lastPerformTime = Time.time;
                    }
                };
                act.performed += sub.onPerform;
                break;

            case BindingMode.Charge:
                sub.onStart = ctx => {
                    startTime = Time.time;
                    ui.Reset(); ui.Show();
                    sub.chargeCoroutine = StartCoroutine(UpdateChargeUI(ib, ui));
                };
                sub.onCancel = ctx => {
                    if (sub.chargeCoroutine != null) StopCoroutine(sub.chargeCoroutine);
                    ui.Hide();
                    if (Time.time - startTime >= ib.holdTime)
                        FirePhase(ib, TriggerPhase.OnPerform);
                };
                act.started += sub.onStart;
                act.canceled += sub.onCancel;
                break;

            case BindingMode.Continuous:
                sub.onStart = ctx => {
                    sub.tickCoroutine = StartCoroutine(TickLoop(ib, ib.bindings
                        .FirstOrDefault(a => a.triggerPhase == TriggerPhase.OnTick)));
                };
                sub.onCancel = ctx => {
                    if (sub.tickCoroutine != null) StopCoroutine(sub.tickCoroutine);
                };
                act.started += sub.onStart;
                act.canceled += sub.onCancel;
                break;

            case BindingMode.Release:
                sub.onCancel = ctx => FirePhase(ib, TriggerPhase.OnCancel);
                act.canceled += sub.onCancel;
                break;
        }

        _subs.Add(sub);
    }

    private IEnumerator TickLoop(InputBindingData ib, ActionBindingData ab)
    {
        // if someone forgot to set a valid rate, bail out
        if (ab.tickRate <= 0f)
            yield break;

        while (true)
        {
            yield return new WaitForSeconds(ab.tickRate);
            ab.action.Execute(ctx, ib, ab);
        }
    }

    void FirePhase(InputBindingData ib, TriggerPhase phase)
    {
        foreach (var ab in ib.bindings.Where(x => x.triggerPhase == phase))
            ab.action.Execute(ctx, ib, ab);
    }

    IEnumerator UpdateChargeUI(InputBindingData ib, ChargeUI ui)
    {
        float elapsed = 0f;
        while (elapsed < ib.holdTime)
        {
            elapsed += Time.deltaTime;
            ui.SetPercent(elapsed / ib.holdTime);
            yield return null;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe every callback & disable the action
        foreach (var sub in _subs)
        {
            if (sub.action != null)
            {
                sub.action.started   -= sub.onStart;
                sub.action.performed -= sub.onPerform;
                sub.action.canceled  -= sub.onCancel;
                sub.action.Disable();
            }
        }
        _subs.Clear();

        // Clean up model instances
        foreach (var mdl in _models)
            if (mdl != null) Destroy(mdl);
        _models.Clear();
    }
    
    static Transform FindFirePoint(GameObject mdl)
    {
        return mdl.GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(t => t.name == "FirePoint");
    }

    void AttachModel()
    {
        if (data == null) return;
        ctx.FirePoints.Clear();
        _models.Clear();

        void SpawnSide(GameObject prefab, Transform parent)
        {
            if (prefab == null || parent == null) return;
            var mdl = Instantiate(prefab, parent);
            mdl.transform.localPosition = data.modelPositionOffset;
            mdl.transform.localRotation = Quaternion.Euler(data.modelRotationOffset);
            _models.Add(mdl);

            var fp = FindFirePoint(mdl);
            if (fp != null) 
                ctx.FirePoints.Add(fp);
            else 
                Debug.LogWarning($"[{name}] no ‘FirePoint’ anywhere under {mdl.name}", this);
        }

        switch (data.defaultHand)
        {
            case Hand.Right: SpawnSide(data.rightHandModel, ctx.rightHand); break;
            case Hand.Left:  SpawnSide(data.leftHandModel,  ctx.leftHand);  break;
            case Hand.Both:
                SpawnSide(data.rightHandModel, ctx.rightHand);
                SpawnSide(data.leftHandModel,  ctx.leftHand);
                break;
        }
    }
}
