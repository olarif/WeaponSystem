using System;
using UnityEngine;

public class StateMachine
{
    private State _currentState;
    private PlayerController _controller;
    
    public State CurrentState => _currentState;
    public event Action<State, State> OnStateChanged;
    
    public StateMachine(PlayerController controller)
    {
        _controller = controller;
    }
    
    public void ChangeState(State newState)
    {
        if (_currentState != null && !_currentState.CanTransitionTo(newState.GetType()))
        {
            Debug.LogWarning($"Cannot transition from {_currentState.GetType().Name} to {newState.GetType().Name}");
            return;
        }

        var previousState = _currentState;
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
        
        OnStateChanged?.Invoke(previousState, newState);
    }
    
    public void Update() => _currentState?.Update();
    public void FixedUpdate() => _currentState?.FixedUpdate();
    
    public bool IsInState<T>() where T : State => _currentState is T;
    public T GetState<T>() where T : State => _currentState as T;
}