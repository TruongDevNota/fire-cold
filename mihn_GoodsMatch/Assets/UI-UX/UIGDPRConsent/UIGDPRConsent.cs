using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGDPRConsent : MonoBehaviour
{
    public delegate void OnChangedConsent();
    public OnChangedConsent onChangedConsent;

    [SerializeField] UIAnimation _animation;

    public void Show()
    {
        _animation.Show();
    }

    public void Ins_AllowConsent()
    {
        IronSource.Agent.setConsent(true);

        onChangedConsent?.Invoke();

        _animation.Hide();
    }

    public void Ins_WontAllowConsent()
    {
        IronSource.Agent.setConsent(false);

        onChangedConsent?.Invoke();

        _animation.Hide();
    }
}
