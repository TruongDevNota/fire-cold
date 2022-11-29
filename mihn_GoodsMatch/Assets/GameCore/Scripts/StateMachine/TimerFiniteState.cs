using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerFiniteState : CharacterFinitState
{
    public float totalTime { get; protected set; }
    protected float process => Mathf.Clamp01(elapsedTime / (totalTime <= 0 ? 1 : totalTime));

    protected float elapsedTime;
    protected bool isTimeOut;

    public TimerFiniteState(Character character, FiniteStateMachine stateMachine, float time) : base(character, stateMachine)
    {
        totalTime = time;
        elapsedTime = 0;
    }

    public override void Enter(object data)
    {
        base.Enter(data);
        isTimeOut = false;
    }

    public override void DoUpdate()
    {
        base.DoUpdate();
        if (!isTimeOut)
        {
            if(elapsedTime >= totalTime)
            {
                isTimeOut = true;
                OnDone?.Invoke();
            }
            else
            {
                character.PostEvent((int)EventID.OnFSMStateUpdate, 
                    new MessageFSMUpdate(this, process, elapsedTime, totalTime));
                elapsedTime += Time.deltaTime;
            }
        }
    }
}
