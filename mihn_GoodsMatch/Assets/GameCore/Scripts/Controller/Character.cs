using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Character : Unit
{
    public FiniteState CurrentState => fsm.CurrentState;

    [SerializeField] protected Animator anim = null;

    protected override void Awake()
    {
        base.Awake();

        if (anim == null)
            anim = GetComponentInChildren<Animator>();

        if (anim == null)
            throw new Exception($"{gameObject.name} => Animator is not exist!!!");
    }
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
            delayCoroutine = null;
        }
    }

    public virtual void Init()
    {
        this.PostEvent((int)EventID.OnInit, this);
    }
    public virtual void ChangeExercises(string idEx)
    {
    }

    public void SetAnimationFloat(int param, float value, bool force = false)
    {
        if(force)
            anim.SetFloat(param, value);
        else
            anim.SetFloat(param, value, 0.1f, Time.deltaTime);
    }

    public void SetAnimationFloat(int param, int value)
    {
        anim.SetFloat(param, value);
    }

    public void SetAnimationBool(int param, bool value)
    {
        anim.SetBool(param, value);
    }

    public void TriggerAnimation(int param)
    {
        anim.SetTrigger(param);
    }
    public void ResetTriggerAnimation(params int[] param)
    {
        for (int i = 0; i < param.Length; i++)
            anim.ResetTrigger(param[i]);
    }


    private Coroutine delayCoroutine;
    IEnumerator DoDelayCall(float time, System.Action callback)
    {
        if (time <= 0)
            yield return null;

        var wait = new WaitForSeconds(time);
        yield return wait;

        callback?.Invoke();
    }
    protected void DelayCall(float time, System.Action callback)
    {
        if(delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
            delayCoroutine = null;
        }
        delayCoroutine = StartCoroutine(DoDelayCall(time, callback));
    }
}
