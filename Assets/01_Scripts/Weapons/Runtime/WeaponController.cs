using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Data")]
    [SerializeField] private WeaponDataSO data;
    private WeaponContext ctx;
    
    private class BindingState
    {
        public InputAction action;
        public InputBindingData bindingData;
        
        // Event subscriptions
        public Action<InputAction.CallbackContext> onStart;
        public Action<InputAction.CallbackContext> onPerform;
        public Action<InputAction.CallbackContext> onCancel;
        
        // Timing state
        public float lastTriggerTime = float.NegativeInfinity;
        public float inputStartTime;
        public bool isHolding;
        public bool hasTriggeredRelease;
        
        // Coroutine management
        public Coroutine chargeCoroutine;
        public Coroutine continuousCoroutine;
        public readonly List<Coroutine> tickCoroutines = new();
        
        // UI reference
        public ChargeUI chargeUI;
        
        public bool CanTrigger => Time.time - lastTriggerTime >= bindingData.cooldown;
        
        public void MarkTriggered()
        {
            lastTriggerTime = Time.time;
        }
    }

    private readonly List<BindingState> _bindingStates = new();
    [HideInInspector] public List<GameObject> _models = new();

    public void Initialize(WeaponDataSO sourceData, WeaponContext context)
    {
        ctx = context;
        data = Instantiate(sourceData);
        ctx.WeaponController = this;
        
        AttachModel();
        
        foreach (var inputBinding in data.inputBindings)
        {
            SetupBinding(inputBinding);
        }
    }
    
    public void InitializeVisualOnly(WeaponDataSO sourceData, WeaponContext context)
    {
        ctx = context;
        data = Instantiate(sourceData);
        
        AttachModel();
    }

    public void AttachModel()
    {
        ctx.FirePoints.Clear();
        foreach (var mdl in _models) 
            if (mdl) Destroy(mdl);
        _models.Clear();

        void SpawnModel(GameObject prefab, Transform parent)
        {
            if (prefab == null || parent == null) return;
            
            var model = Instantiate(prefab, parent);
            model.transform.localPosition = data.modelPositionOffset;
            model.transform.localRotation = Quaternion.Euler(data.modelRotationOffset);
            _models.Add(model);
            
            var firePoint = model.GetComponentsInChildren<Transform>(true)
                                 .FirstOrDefault(t => t.name == "FirePoint");
            if (firePoint != null) 
                ctx.FirePoints.Add(firePoint);
        }

        switch (data.defaultHand)
        {
            case Hand.Right: 
                SpawnModel(data.rightHandModel, ctx.rightHand); 
                break;
            case Hand.Left:  
                SpawnModel(data.leftHandModel, ctx.leftHand);  
                break;
            case Hand.Both:
                SpawnModel(data.rightHandModel, ctx.rightHand);
                SpawnModel(data.leftHandModel, ctx.leftHand);
                break;
        }
    }

    private void SetupBinding(InputBindingData bindingData)
    {
        var action = bindingData.actionRef.action;
        if (action == null)
        {
            Debug.LogError($"Input action is null for binding {bindingData.bindingMode}");
            return;
        }

        action.Enable();

        var state = new BindingState
        {
            action = action,
            bindingData = bindingData,
            chargeUI = bindingData.hand == Hand.Left ? ctx.leftChargeUI : ctx.rightChargeUI
        };

        SetupBindingMode(state);
        _bindingStates.Add(state);
    }

    private void SetupBindingMode(BindingState state)
    {
        var action = state.action;
        var bindingData = state.bindingData;

        switch (bindingData.bindingMode)
        {
            case BindingMode.Press:
                SetupPressBinding(state);
                break;
            case BindingMode.Charge:
                SetupChargeBinding(state);
                break;
            case BindingMode.Continuous:
                SetupContinuousBinding(state);
                break;
            case BindingMode.Release:
                SetupReleaseBinding(state);
                break;
        }
    }

    private void SetupPressBinding(BindingState state)
    {
        state.onPerform = ctx =>
        {
            if (state.CanTrigger)
            {
                state.MarkTriggered();
                ExecutePhase(state.bindingData, TriggerPhase.OnPerform);
            }
        };
        
        state.action.performed += state.onPerform;
    }

    private void SetupChargeBinding(BindingState state)
    {
        state.onStart = ctx =>
        {
            state.inputStartTime = Time.time;
            state.isHolding = true;
            
            if (state.chargeUI != null)
            {
                state.chargeUI.Reset();
                state.chargeUI.Show();
            }
            
            state.chargeCoroutine = StartCoroutine(ChargeCoroutine(state));
        };

        state.onCancel = ctx =>
        {
            state.isHolding = false;
            
            if (state.chargeCoroutine != null)
            {
                StopCoroutine(state.chargeCoroutine);
                state.chargeCoroutine = null;
            }
            
            if (state.chargeUI != null)
                state.chargeUI.Hide();

            float holdDuration = Time.time - state.inputStartTime;
            if (holdDuration >= state.bindingData.holdTime && state.CanTrigger)
            {
                state.MarkTriggered();
                ExecutePhase(state.bindingData, TriggerPhase.OnPerform);
            }
        };

        state.action.started += state.onStart;
        state.action.canceled += state.onCancel;
    }

    private void SetupContinuousBinding(BindingState state)
    {
        state.onStart = ctx =>
        {
            if (!state.CanTrigger) return;
            
            state.MarkTriggered();
            state.isHolding = true;
            state.inputStartTime = Time.time;
            
            ExecutePhase(state.bindingData, TriggerPhase.OnStart);
            
            // Start tick actions
            var tickActions = state.bindingData.bindings
                .Where(b => b.triggerPhase == TriggerPhase.OnTick)
                .ToList();
                
            foreach (var tickAction in tickActions)
            {
                var coroutine = StartCoroutine(TickCoroutine(state, tickAction));
                state.tickCoroutines.Add(coroutine);
            }
        };

        state.onCancel = ctx =>
        {
            state.isHolding = false;
            
            // Stop all tick coroutines
            foreach (var coroutine in state.tickCoroutines)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
            }
            state.tickCoroutines.Clear();
            
            ExecutePhase(state.bindingData, TriggerPhase.OnCancel);
        };

        state.action.started += state.onStart;
        state.action.canceled += state.onCancel;
    }

    private void SetupReleaseBinding(BindingState state)
    {
        state.onStart = ctx =>
        {
            state.isHolding = true;
            state.hasTriggeredRelease = false;
        };

        state.onCancel = ctx =>
        {
            state.isHolding = false;
            
            if (!state.hasTriggeredRelease && state.CanTrigger)
            {
                state.hasTriggeredRelease = true;
                state.MarkTriggered();
                ExecutePhase(state.bindingData, TriggerPhase.OnPerform);
            }
        };

        state.action.started += state.onStart;
        state.action.canceled += state.onCancel;
    }

    private IEnumerator ChargeCoroutine(BindingState state)
    {
        float elapsed = 0f;
        float holdTime = state.bindingData.holdTime;
        
        while (state.isHolding && elapsed < holdTime)
        {
            elapsed = Time.time - state.inputStartTime;
            
            if (state.chargeUI != null)
            {
                float percent = Mathf.Clamp01(elapsed / holdTime);
                state.chargeUI.SetPercent(percent);
            }
            
            yield return null;
        }
        
        if (state.chargeUI != null && state.isHolding)
            state.chargeUI.SetPercent(1f);
            
        state.chargeCoroutine = null;
    }

    private IEnumerator TickCoroutine(BindingState state, ActionBindingData actionBinding)
    {
        if (actionBinding.tickRate <= 0f) yield break;
        
        // Wait initial tick delay
        yield return new WaitForSeconds(actionBinding.tickRate);
        
        while (state.isHolding)
        {
            if (state.CanTrigger)
            {
                state.MarkTriggered();
                actionBinding.action.Execute(ctx, state.bindingData, actionBinding);
            }
            
            yield return new WaitForSeconds(actionBinding.tickRate);
        }
    }

    private void ExecutePhase(InputBindingData bindingData, TriggerPhase phase)
    {
        var actionsToExecute = bindingData.bindings
            .Where(b => b.triggerPhase == phase)
            .ToList();
            
        foreach (var actionBinding in actionsToExecute)
        {
            try
            {
                actionBinding.action.Execute(ctx, bindingData, actionBinding);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error executing weapon action: {e.Message}\n{e.StackTrace}");
            }
        }
    }

    private void CleanupBinding(BindingState state)
    {
        if (state.action == null) return;

        // Unsubscribe from events
        if (state.onStart != null) 
            state.action.started -= state.onStart;
        if (state.onPerform != null) 
            state.action.performed -= state.onPerform;
        if (state.onCancel != null) 
            state.action.canceled -= state.onCancel;

        // Stop coroutines
        if (state.chargeCoroutine != null)
        {
            StopCoroutine(state.chargeCoroutine);
            state.chargeCoroutine = null;
        }

        foreach (var coroutine in state.tickCoroutines)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        state.tickCoroutines.Clear();

        // Hide UI
        if (state.chargeUI != null)
            state.chargeUI.Hide();

        // Disable action
        state.action.Disable();
    }

    private void OnDestroy()
    {
        // Clean up all bindings
        foreach (var state in _bindingStates)
        {
            CleanupBinding(state);
        }
        _bindingStates.Clear();

        // Clean up models
        foreach (var model in _models)
        {
            if (model != null)
                Destroy(model);
        }
        _models.Clear();
    }

    #region Debug Methods
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogBindingState(BindingState state, string message)
    {
        Debug.Log($"[{state.bindingData.bindingMode}] {message} - " +
                 $"CanTrigger: {state.CanTrigger}, " +
                 $"LastTrigger: {state.lastTriggerTime:F2}, " +
                 $"Cooldown: {state.bindingData.cooldown:F2}");
    }
    #endregion
}