using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiniteState
{
    public const float STOPPING_DISTANCE = 0.5f;

    public System.Action OnDone = delegate { };
    public GameObject source { get; protected set; }

    protected FiniteStateMachine finiteStateMachine;

    protected FiniteState(GameObject go, FiniteStateMachine fsm)
    {
        source = go;
        finiteStateMachine = fsm;
    }

    public virtual void Enter(object data)
    {
    }

    public virtual void HandleInput()
    {

    }

    public virtual void DoUpdate()
    {
    }

    public virtual void DoFixedUpdate()
    {

    }

    public virtual void Exit()
    {
    }
    public virtual void Cancel()
    {
    }
}

public class CharacterFinitState : FiniteState
{
    public Character character { get; protected set; }
    public CharacterFinitState(Character character, FiniteStateMachine fsm) : base (character.gameObject, fsm)
    {
        this.character = character;
    }

    public override void Enter(object data)
    {
        base.Enter(data);
        character.PostEvent((int)EventID.OnFSMStateEnter, this);
    }
    public override void Exit()
    {
        base.Exit();
        character.PostEvent((int)EventID.OnFSMStateExit, this);
    }
}

