using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Hand { Right, Left, Both}
public enum TriggerPhase { OnStart, OnPerform, OnCancel, OnTick }
public enum BindingMode { Press, Charge, Continuous, Release }

[Serializable]
public class ActionBindingData
{
    public TriggerPhase triggerPhase;
    public float tickRate;
    [SerializeReference] public IWeaponAction action;
}

[Serializable]
public class InputBindingData
{
    public BindingMode bindingMode = BindingMode.Press;
    public InputActionReference actionRef;
    public Hand hand;
    public float holdTime;
    public float cooldown;
    public List<ActionBindingData> bindings = new();
    
    [NonSerialized] public float lastPerformTime = -Mathf.Infinity;
    [NonSerialized] public BeamController activeBeam;
}