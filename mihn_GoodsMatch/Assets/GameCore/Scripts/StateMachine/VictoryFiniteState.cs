using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryFiniteState : CharacterFinitState
{
    private int victoryParam = Animator.StringToHash("Victory");
    private int victoryTypeParam = Animator.StringToHash("VictoryType");
    public VictoryFiniteState(Character character, FiniteStateMachine stateMachine) : base(character, stateMachine)
    {
    }

    public override void Enter(object data)
    {
        base.Enter(data);
        character.SetAnimationFloat(victoryTypeParam, Random.Range(0, 12));
        character.ResetTriggerAnimation(victoryParam);
        character.TriggerAnimation(victoryParam);
    }

    public override void Exit()
    {
        base.Exit();
        character.ResetTriggerAnimation(victoryParam);
    }
}
