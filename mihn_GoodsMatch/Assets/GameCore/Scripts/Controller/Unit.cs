using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    public Transform tf { get; private set; }

    [SerializeField] protected FiniteStateMachine fsm;

    protected virtual void Awake()
    {
        tf = transform;
        fsm = new FiniteStateMachine();
    }
    protected virtual void OnEnable()
    {
    }

    protected virtual void OnDisable()
    {
    }
    protected virtual void Update()
    {
        fsm?.CurrentState?.HandleInput();
        fsm?.CurrentState?.DoUpdate();
    }
    protected void FixedUpdate()
    {
        fsm?.CurrentState?.DoFixedUpdate();
    }
}
