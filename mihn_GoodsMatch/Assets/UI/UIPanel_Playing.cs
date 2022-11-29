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
        if ((float)BoardGame.instance.pStopWatch.ElapsedMilliseconds / 1000 >= timePlayed + 1)
        {
            timePlayed = BoardGame.instance.pStopWatch.ElapsedMilliseconds / 1000;
        }
        timeLeftText.text = TimeSpan.FromSeconds(Mathf.FloorToInt(Mathf.Max(BoardGame.instance.pTimeLimitInSeconds - timePlayed, 0))).ToString("mm':'ss");
    }
    public void OnGamePrepareHandler(int level)
    {
        timeLeftText.text = TimeSpan.FromSeconds(Mathf.FloorToInt(BoardGame.instance.pTimeLimitInSeconds)).ToString("mm':'ss");
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
