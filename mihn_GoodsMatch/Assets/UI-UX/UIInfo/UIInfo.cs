using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIInfo : MonoBehaviour
{
    [SerializeField] Text levelTxt = null;
    [SerializeField] Text timeLeftText;
    [SerializeField] Slider comboTimeSlider = null;
    [SerializeField] Text comboCountText = null;

    float timePlayed = 0;

    private void Awake()
    {
        GameStateManager.OnStateChanged += GameStateManager_OnStateChanged;
    }

    private void LateUpdate()
    {
        if (GameStateManager.CurrentState != GameState.Play)
            return;
        
        if ((float)BoardGame.instance.pStopWatch.ElapsedMilliseconds / 1000 >= timePlayed + 1)
        {
            timePlayed = BoardGame.instance.pStopWatch.ElapsedMilliseconds / 1000;
        }
        timeLeftText.text = TimeSpan.FromSeconds(Mathf.FloorToInt(Mathf.Max(BoardGame.instance.pTimeLimitInSeconds - timePlayed, 0))).ToString("m':'ss");
    }

    private void GameStateManager_OnStateChanged(GameState current, GameState last, object data)
    {
        if (current == GameState.Init || current == GameState.Restart)
        {
            this.timePlayed = 0;
        }
        else if (current == GameState.Ready)
        {
            levelTxt.text = $"LEVEL {DataManager.UserData.level + 1}";
            timeLeftText.text = TimeSpan.FromSeconds(Mathf.FloorToInt(BoardGame.instance.pTimeLimitInSeconds)).ToString("m':'ss");
        }
    }
}
