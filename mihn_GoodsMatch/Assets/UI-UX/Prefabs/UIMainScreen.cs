using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using UnityEngine.UI;
using System;

public class UIMainScreen : MonoBehaviour
{
    public static UIMainScreen Instance;
    public UIAnimStatus Status => anim.Status;

    private UIAnimation anim;
    void Awake()
    {
        Instance = this;
        anim = GetComponent<UIAnimation>();
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    
    public void Show(TweenCallback onStart = null, TweenCallback onCompleted = null)
    {
        //start
        anim.Show(onStart, () =>
        {
            onCompleted?.Invoke();
        });
        
    }

    public void Hide()
    {
        anim.Hide();
    }

    public void Ins_BtnPlayClick()
    {
        if (GameStateManager.CurrentState == GameState.Idle)
            GameStateManager.LoadGame(null);
    }
}
