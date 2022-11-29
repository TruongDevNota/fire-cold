using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageFSMUpdate
{
    public FiniteState state;
    public float process;
    public float elapsed;
    public float total;

    public MessageFSMUpdate(FiniteState s, float p, float e, float t)
    {
        state = s;
        process = p;
        elapsed = e;
        total = t;
    }
}
