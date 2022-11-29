using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class AnimationStateMachine : StateMachineBehaviour
{
    [SerializeField] AnimationID animID;
    [PositiveValueOnly] [SerializeField] float triggerAt = 0;

    private bool isTrigger = true;
    private GameObject source = null;
    private MessageAnimator enterMsg = null, exitMsg = null, triggerMsg = null;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (source == null)
            source = animator.gameObject;

        if (enterMsg == null)
        {
            enterMsg = new MessageAnimator
            {
                animId = animID,
                state = AnimationState.Enter,
                source = source
            };
        }
        if (triggerMsg == null)
        {
            triggerMsg = new MessageAnimator
            {
                animId = animID,
                state = AnimationState.Trigger,
                source = source
            };
        }
        if (exitMsg == null)
        {
            exitMsg = new MessageAnimator
            {
                animId = animID,
                state = AnimationState.Exit,
                source = source
            };
        }


        EventDispatcher.Instance?.PostEvent((int)EventID.OnAnimatorTrigger, enterMsg);

        if(triggerAt <= 0)
        {
            isTrigger = true;
            EventDispatcher.Instance?.PostEvent((int)EventID.OnAnimatorTrigger, triggerMsg);
        }
        else
        {
            isTrigger = false;
        }
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        if(triggerAt > 0 && !isTrigger && stateInfo.normalizedTime >= triggerAt)
        {
            isTrigger = true;
            EventDispatcher.Instance?.PostEvent((int)EventID.OnAnimatorTrigger, triggerMsg);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        EventDispatcher.Instance?.PostEvent((int)EventID.OnAnimatorTrigger, exitMsg);
    }
}

[System.Serializable]
public enum AnimationState
{
    Enter = 0,
    Exit = 1,
    Trigger = 2,
}

[System.Serializable]
public enum AnimationID
{
    None = 0,
    Attack = 1,
    GetSlap = 2,
    Sad = 3,
}
