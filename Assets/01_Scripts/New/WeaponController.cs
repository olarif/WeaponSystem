using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    [Tooltip("Assign your WeaponSO asset here")]
    public WeaponDataSO weapon;
    static WeaponContext ctx;
    
    void Awake() => ctx = GetComponent<WeaponContext>();

    private class Handler
    {
        public WeaponDataSO.InputBinding b;
        public GameObject owner;

        // For Hold
        float holdStart;

        // For Continuous
        bool isContinuous;
        float lastFireTime;

        public Handler(WeaponDataSO.InputBinding binding, GameObject o)
        {
            b     = binding;
            owner = o;
        }

        public void Enable()
        {
            var act = b.inputAction?.action;
            if (act == null) return;

            switch (b.eventType)
            {
                case WeaponInputEvent.Press:
                    act.performed += OnFire; 
                    break;
                case WeaponInputEvent.Release:
                    act.canceled  += OnFire; 
                    break;
                case WeaponInputEvent.Hold:
                    act.started   += ctx => holdStart = Time.time;
                    act.canceled  += OnHoldRelease;
                    break;
                case WeaponInputEvent.Continuous:
                    act.started   += OnStartContinuous;
                    act.canceled  += OnCancelContinuous;
                    break;
            }
        }

        public void Disable()
        {
            var act = b.inputAction?.action;
            if (act == null) return;

            switch (b.eventType)
            {
                case WeaponInputEvent.Press:
                    act.performed -= OnFire; 
                    break;
                case WeaponInputEvent.Release:
                    act.canceled  -= OnFire; 
                    break;
                case WeaponInputEvent.Hold:
                    act.started   -= ctx => holdStart = Time.time;
                    act.canceled  -= OnHoldRelease;
                    break;
                case WeaponInputEvent.Continuous:
                    act.started   -= OnStartContinuous;
                    act.canceled  -= OnCancelContinuous;
                    break;
            }
        }

        public void Update()
        {
            if (b.eventType == WeaponInputEvent.Continuous && isContinuous)
            {
                if (Time.time >= lastFireTime + b.fireRate)
                {
                    Fire();
                    lastFireTime = Time.time;
                }
            }
        }

        private void OnFire(InputAction.CallbackContext _) => Fire();

        private void OnHoldRelease(InputAction.CallbackContext _)
        {
            if (Time.time - holdStart >= b.holdTime)
                Fire();
        }

        private void OnStartContinuous(InputAction.CallbackContext _)
        {
            isContinuous   = true;
            lastFireTime   = Time.time - b.fireRate; // so it fires immediately
        }

        private void OnCancelContinuous(InputAction.CallbackContext _)
        {
            isContinuous = false;
        }

        private void Fire()
        {
            foreach (var action in b.actions)
                action.Execute(ctx, b);
            
        }
    }

    private readonly List<Handler> handlers = new List<Handler>();

    void OnEnable()
    {
        handlers.Clear();
        if (weapon == null) return;
        foreach (var b in weapon.bindings)
        {
            var h = new Handler(b, gameObject);
            h.Enable();
            handlers.Add(h);
        }
    }

    void OnDisable()
    {
        foreach (var h in handlers) h.Disable();
        handlers.Clear();
    }

    void Update()
    {
        foreach (var h in handlers)
            h.Update();
    }
}
