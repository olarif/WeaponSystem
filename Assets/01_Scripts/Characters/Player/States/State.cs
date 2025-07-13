using System;
using UnityEngine;

public abstract class State
{
    protected PlayerController Controller;
    protected PlayerStatsSO PlayerStats;
    
    public State(PlayerController controller)
    {
        Controller = controller;
        PlayerStats = controller.Stats;
    }

    public virtual void Enter() {}
    public virtual void Exit() {}
    public virtual void Update() {}
    public virtual void FixedUpdate() {}
    public virtual bool CanTransitionTo(Type stateType) => true;
}