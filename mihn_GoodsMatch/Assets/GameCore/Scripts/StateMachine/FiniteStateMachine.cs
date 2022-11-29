using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiniteStateMachine
{
    public FiniteState CurrentState { get; private set; }

    public void Clear()
    {
        CurrentState = null;
    }

    public void Initialize(FiniteState startingState)
    {
        if (startingState == null)
            return;
        CurrentState = startingState;
        startingState.Enter(null);
    }

    public void ChangeState(FiniteState newState, object data = null)
    {
        if(CurrentState != null)
            CurrentState.Exit();

        CurrentState = newState;
        newState.Enter(data);
    }
}
