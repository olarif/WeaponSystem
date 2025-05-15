using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Data & References")]
    [Tooltip("WeaponDataSO with three lists: inputComponents, executeComponents, onHitComponents")]
    public WeaponDataSO weaponData;
    [Tooltip("Transform for spawning rays/projectiles")]
    public Transform firePoint;
    [Tooltip("Seconds between successive fires")]
    public float cooldownTime = 0.1f;

    public WeaponState CurrentState { get; private set; } = WeaponState.Idle;
    public event Action<WeaponState, WeaponState> OnStateChanged;

    private WeaponContext _ctx;
    private List<ILifeCycleComponent> _lifecycleComps;
    private Dictionary<ILifeCycleComponent, bool> _activeMap;
    private List<IInputComponent>   _inputComps;
    private List<IExecuteComponent> _execComps;
    private float _nextFireTime;

    private void Awake()
    {
        // 1) Build the shared context
        _ctx = new WeaponContext {
            FirePoint       = firePoint,
            LineRenderer    = GetComponent<LineRenderer>(),
            PlayerCamera    = Camera.main,
            OnHitComponents = new List<IOnHitComponent>()
        };

        // 2) Initialize lists
        _lifecycleComps = new List<ILifeCycleComponent>();
        _activeMap      = new Dictionary<ILifeCycleComponent,bool>();

        // 3) Register EVERYTHING from your three lists
        void Register(ScriptableObject so)
        {
            if (so is ILifeCycleComponent lc)
            {
                lc.Initialize(_ctx);
                _lifecycleComps.Add(lc);
                _activeMap[lc] = false;

                if (so is IOnHitComponent hitComp)
                    _ctx.OnHitComponents.Add(hitComp);
            }
        }

        foreach (var so in weaponData.inputComponents    ?? Enumerable.Empty<ScriptableObject>()) Register(so);
        foreach (var so in weaponData.executeComponents  ?? Enumerable.Empty<ScriptableObject>()) Register(so);
        foreach (var so in weaponData.onHitComponents    ?? Enumerable.Empty<ScriptableObject>()) Register(so);

        // 4) Cache the IInput and IExecute lists
        _inputComps = _lifecycleComps.OfType<IInputComponent>().ToList();
        _execComps  = _lifecycleComps.OfType<IExecuteComponent>().ToList();
    }

    private void Update()
    {
        // A) Did the player press/hold? If so, we’ll transition from Idle → Firing
        bool anyInput = _inputComps.Any(i => i.CanExecute());
        switch (CurrentState)
        {
            case WeaponState.Idle:
                if (anyInput) ChangeState(WeaponState.Firing);
                break;

            case WeaponState.Firing:
                // fire once, then go to cooldown
                _nextFireTime = Time.time + cooldownTime;
                ChangeState(WeaponState.CoolingDown);
                break;

            case WeaponState.CoolingDown:
                if (Time.time >= _nextFireTime)
                    ChangeState(anyInput ? WeaponState.Firing : WeaponState.Idle);
                break;
        }

        // B) Now drive OnStart/OnUpdate/OnStop for each component
        foreach (var comp in _lifecycleComps)
        {
            bool want = (comp is IInputComponent ic && ic.CanExecute());
            bool was  = _activeMap[comp];

            if (want && !was) comp.OnStart();
            if (want)         comp.OnUpdate();
            if (!want && was) comp.OnStop();

            _activeMap[comp] = want;
        }

        // C) Finally, if we’re in Firing state, actually Execute()
        if (CurrentState == WeaponState.Firing)
            foreach (var exec in _execComps)
                exec.Execute();
    }

    private void ChangeState(WeaponState next)
    {
        var prev = CurrentState;
        CurrentState = next;
        OnStateChanged?.Invoke(prev, next);
    }

    private void OnDestroy()
    {
        // Cleanup subscriptions, etc.
        foreach (var comp in _lifecycleComps)
            comp.Cleanup();
    }
}