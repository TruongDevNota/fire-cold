using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanel_Playing : MonoBehaviour
{
    [SerializeField] Text timeLeftText;
    [SerializeField] Text levelText;

    float timePlayed = 0;

    private void LateUpdate()
    {
        if (!BoardGame.instance.isPlayingGame)
            return;
        timeLeftText.text = TimeSpan.FromSeconds(Mathf.FloorToInt(Mathf.Max(BoardGame.instance.pTimeLimitInSeconds - BoardGame.instance.pStopWatch.ElapsedMilliseconds / 1000, 0))).ToString("m':'ss");
    }
    public void OnGamePrepareHandler(int level)
    {
        timeLeftText.text = TimeSpan.FromSeconds(Mathf.FloorToInt(BoardGame.instance.pTimeLimitInSeconds)).ToString("m':'ss");
        this.timePlayed = 0;
        levelText.text = $"Lv.{level}";
    }
    public void OnGamePauseClicked()
    {
        if (!BoardGame.instance.isPlayingGame || BoardGame.instance.isPausing)
            return;
        BoardGame.instance.PauseGame();
    }
}
