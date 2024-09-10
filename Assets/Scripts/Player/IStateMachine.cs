using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;
public class IStateMachine<TStateType, TStateMachine> : NetworkBehaviour where TStateMachine: NetworkBehaviour
                                                                        where TStateType:struct, Enum
{
    Dictionary<TStateType, State> states = new Dictionary<TStateType, State>();
    protected TStateType currentStateType;
    protected State CurrentState
    {
        get
        {
            states.TryGetValue(currentStateType, out State state);
            return state;
        }
    }
    public void RegisterState(State state)
    {
        states[state.type] = state;
    }
    public bool ChangeState(TStateType newStateType, params object[] args)
    {
        if (currentStateType.CompareTo(newStateType) == 0) return false;
        if(!states.TryGetValue(newStateType, out State newState))
        {
            Debug.Log("State not registered");
            return false;
        }
        State prevState = CurrentState;
        newState.OnStateEnter(currentStateType, args);
        prevState.OnStateExit(newStateType, args);
        currentStateType = newStateType;
        return true;
    }
    public abstract class State
    {
        public abstract TStateType type { get; }
        TStateMachine player;
        public virtual void OnStateEnter(TStateType prevStateType, object[] args) { }
        public virtual void OnStateExit(TStateType nextStateType, object[] args) { }
        public virtual void OnStateUpdate() { }
        public virtual void OnStateFixedUpdate() { }
        public State(TStateMachine Player)
        {
            player = Player;
        }
    }
    
}

