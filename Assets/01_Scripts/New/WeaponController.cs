using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    WeaponDataSO        _data;
    WeaponContext       _ctx;
    readonly List<Handler> _handlers = new();

    /// <summary>
    /// Call this immediately after Instantiate on equip.
    /// </summary>
    public void Initialize(WeaponDataSO data, WeaponContext ctx)
    {
        _data = data;
        _ctx  = ctx;

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
            var model = Instantiate(prefab, parent);
            model.transform.localPosition = _data.modelPositionOffset;
            model.transform.localRotation = Quaternion.Euler(_data.modelRotationOffset);

            // register its FirePoint
            var fp = model.transform.Find("FirePoint");
            if (fp != null)
                _ctx.FirePoints.Add(fp);
            else
                Debug.LogWarning($"[{name}] no ‘FirePoint’ child on {model.name}", this);
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

        float holdStart;
        bool  isContinuous;
        float lastFire;
        float cooldown;

        public Handler(WeaponDataSO.InputBinding b, WeaponContext ctx, InputAction action)
        {
            _b      = b;
            _ctx    = ctx;
            _action = action;
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
                case WeaponInputEvent.Hold:
                    _action.started   += ctx => holdStart = Time.time;
                    _action.canceled  += OnHoldRelease;
                    break;
                case WeaponInputEvent.Continuous:
                    _action.started   += OnStartContinuous;
                    _action.canceled  += OnCancelContinuous;
                    break;
            }
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
                case WeaponInputEvent.Hold:
                    _action.started   -= ctx => holdStart = Time.time;
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
            if (_b.eventType == WeaponInputEvent.Continuous && isContinuous)
            {
                if (Time.time >= lastFire + _b.fireRate)
                {
                    DoFire();
                    lastFire = Time.time;
                }
            }
        }

        void OnFire(InputAction.CallbackContext _)
        {
            if (Time.time < cooldown) return;
            DoFire();
            cooldown = Time.time + _b.fireRate;
        }

        void OnHoldRelease(InputAction.CallbackContext _)
        {
            if (Time.time - holdStart >= _b.holdTime)
                DoFire();
        }

        void OnStartContinuous(InputAction.CallbackContext _)
        {
            isContinuous = true;
            lastFire     = Time.time - _b.fireRate; // so first Fire happens immediately
        }

        void OnCancelContinuous(InputAction.CallbackContext _)
        {
            isContinuous = false;
        }

        void DoFire()
        {
            foreach (var action in _b.actions)
                action.Execute(_ctx, _b);
        }
    }
}
