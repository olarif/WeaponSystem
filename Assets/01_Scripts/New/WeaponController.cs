using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    WeaponDataSO        _data;
    WeaponContext       _ctx;
    readonly List<Handler> _handlers = new();
    
    public GameObject _model;
    
    public void Initialize(WeaponDataSO data, WeaponContext ctx)
    {
        _data = data;
        _ctx  = ctx;
        
        _ctx.WeaponController = this;

        AttachModel();
        BindInputs();
    }

    void AttachModel()
    {
        if (_data == null) return;
        // clear any previous
        _ctx.FirePoints.Clear();

        // helper to spawn one side
        void SpawnSide(GameObject prefab, Transform parent)
        {
            if (prefab == null || parent == null) return;
            _model = Instantiate(prefab, parent);
            _model.transform.localPosition = _data.modelPositionOffset;
            _model.transform.localRotation = Quaternion.Euler(_data.modelRotationOffset);

            // register its FirePoint
            var fp = _model.transform.Find("FirePoint");
            if (fp != null)
                _ctx.FirePoints.Add(fp);
            else
                Debug.LogWarning($"[{name}] no ‘FirePoint’ child on {_model.name}", this);
        }

        // decide which to spawn
        switch (_data.defaultHand)
        {
            case WeaponDataSO.Hand.Right:
                SpawnSide(_data.rightHandModel, _ctx.rightHand);
                break;
            case WeaponDataSO.Hand.Left:
                SpawnSide(_data.leftHandModel, _ctx.leftHand);
                break;
            case WeaponDataSO.Hand.Both:
                SpawnSide(_data.rightHandModel, _ctx.rightHand);
                SpawnSide(_data.leftHandModel,  _ctx.leftHand);
                break;
        }
    }

    void BindInputs()
    {
        foreach (var b in _data.bindings)
        {
            if (b.inputAction == null || b.inputAction.action == null)
            {
                Debug.LogError($"Binding for {b.eventType} has no InputActionReference assigned.");
                continue;
            }

            var act = b.inputAction.action;
            act.Enable();

            var handler = new Handler(b, _ctx, act);
            handler.Enable();
            _handlers.Add(handler);
        }
    }

    void OnDestroy()
    {
        // Clean up
        foreach (var h in _handlers)
            h.Disable();
        _handlers.Clear();
    }

    void Update()
    {
        foreach (var h in _handlers)
            h.Update();
    }

    // -------------------- Inner Handler --------------------
    class Handler
    {
        readonly WeaponDataSO.InputBinding _b;
        readonly WeaponContext             _ctx;
        readonly InputAction               _action;
    
        bool isContinuous;
        bool isHolding;
        float holdStart;
        float lastFireTime = -Mathf.Infinity;

        private readonly List<ChargeUI> _chargeUIs = new List<ChargeUI>();

        public Handler(WeaponDataSO.InputBinding b, WeaponContext ctx, InputAction action)
        {
            _b      = b;
            _ctx    = ctx;
            _action = action;
            
            switch (b.fireHand)
            {
                case WeaponDataSO.Hand.Left:
                    if (ctx.leftChargeUI != null)  _chargeUIs.Add(ctx.leftChargeUI);
                    break;
                case WeaponDataSO.Hand.Right:
                    if (ctx.rightChargeUI != null) _chargeUIs.Add(ctx.rightChargeUI);
                    break;
                case WeaponDataSO.Hand.Both:
                    if (ctx.leftChargeUI != null)  _chargeUIs.Add(ctx.leftChargeUI);
                    if (ctx.rightChargeUI != null) _chargeUIs.Add(ctx.rightChargeUI);
                    break;
            }
        }
        
        private void OnFire(InputAction.CallbackContext _) 
        {
            TryFire();
        }

        private void OnHoldStarted(InputAction.CallbackContext _) 
        {
            isHolding = true;
            holdStart = Time.time;
            
            foreach (var ui in _chargeUIs)
            {
                ui.Reset();
                ui.Show();
            }
        }

        private void OnHoldRelease(InputAction.CallbackContext _) 
        {
            isHolding = false;
            
            foreach (var ui in _chargeUIs)
                ui.Hide();
            
            if (Time.time - holdStart >= _b.holdTime)
                TryFire();
        }

        private void OnStartContinuous(InputAction.CallbackContext _) 
        {
            isContinuous = true;
        }

        private void OnCancelContinuous(InputAction.CallbackContext _) 
        {
            isContinuous = false;
        }

        public void Enable()
        {
            switch (_b.eventType)
            {
                case WeaponInputEvent.Press:
                    _action.performed += OnFire;
                    break;
                case WeaponInputEvent.Release:
                    _action.canceled  += OnFire;
                    break;
                case WeaponInputEvent.Charge:
                    _action.started   += OnHoldStarted;
                    _action.canceled  += OnHoldRelease;
                    break;
                case WeaponInputEvent.Continuous:
                    _action.started   += OnStartContinuous;
                    _action.canceled  += OnCancelContinuous;
                    break;
            }
            _action.Enable();
        }

        public void Disable()
        {
            switch (_b.eventType)
            {
                case WeaponInputEvent.Press:
                    _action.performed -= OnFire;
                    break;
                case WeaponInputEvent.Release:
                    _action.canceled  -= OnFire;
                    break;
                case WeaponInputEvent.Charge:
                    _action.started   -= OnHoldStarted;
                    _action.canceled  -= OnHoldRelease;
                    break;
                case WeaponInputEvent.Continuous:
                    _action.started   -= OnStartContinuous;
                    _action.canceled  -= OnCancelContinuous;
                    break;
            }
            _action.Disable();
        }

        public void Update()
        {
            if (_b.eventType == WeaponInputEvent.Charge && isHolding)
            {
                // update charge percent
                float held = Time.time - holdStart;
                float pct  = held / _b.holdTime;
                foreach (var ui in _chargeUIs)
                    ui.SetPercent(pct);
            }

            if (_b.eventType == WeaponInputEvent.Continuous && isContinuous)
            {
                foreach (var ui in _chargeUIs)
                    ui.Hide();
                
                TryFire();
            }
               
        }

        void TryFire()
        {
            if (Time.time < lastFireTime + _b.fireRate)
                return;
            foreach (var action in _b.actions)
                action?.Execute(_ctx, _b);
            lastFireTime = Time.time;
        }
    }
}
