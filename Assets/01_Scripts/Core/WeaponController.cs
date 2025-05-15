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
    public LineRenderer lineRenderer;
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
            LineRenderer    = lineRenderer,
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
        // 1) Check if any input is currently active
        bool anyInput = _inputComps.Any(i => i.CanExecute());

        // 2) Drive lifecycle on every ILifecycleComponent
        foreach (var comp in _lifecycleComps)
        {
            bool want = false;

            // Inputs want() whenever their button/action is down
            if (comp is IInputComponent inp)
            {
                want = inp.CanExecute();
            }
            // Execute modules want() *exactly* when input is down as well
            else if (comp is IExecuteComponent)
            {
                want = anyInput;
            }
            // OnHitComponents are passive (they just sit in context), 
            // so they never get their own start/stop here.

            bool was = _activeMap[comp];

            if (want && !was)
            {
                Debug.Log($"OnStart: {comp.GetType().Name}");
                comp.OnStart();
            }

            if (want)
            {
                comp.OnUpdate();
            }

            if (!want && was)
            {
                Debug.Log($"OnStop: {comp.GetType().Name}");
                comp.OnStop();
            }

            _activeMap[comp] = want;
        }

        // 3) We still call Execute() ourselves, if you’re using ExecuteComponent.Execute()
        if (anyInput)
        {
            foreach (var exec in _execComps)
                exec.Execute();
        }
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