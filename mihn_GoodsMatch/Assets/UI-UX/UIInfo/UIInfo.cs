using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIInfo : MonoBehaviour
{
    private static UIInfo instance;

    [SerializeField] Text levelTxt = null;
    [SerializeField] Text timeLeftText;
    [SerializeField] Text startText;

    [SerializeField] Slider comboTimeSlider = null;
    [SerializeField] Text comboCountText = null;
    [SerializeField] float comboTimeCooldown = 3f;

    float currentCoolDownTime;
    int comboCount = 0;
    public int ComboCount
    { 
        get { return instance.comboCount; }
        set
        {
            if(value != instance.comboCount)
                instance.comboCount = value;
            if (instance.comboCount != 0)
                instance.DoComboCountDown();
            instance.comboCountText.text = $"COMBO X{instance.comboCount}";
        }
    }
    Coroutine coolDownCoroutine;

    float timePlayed = 0;
    int currentStar;

    private void Awake()
    {
        GameStateManager.OnStateChanged += GameStateManager_OnStateChanged;
        instance = this;
    }

    private void OnEnable()
    {
        this.RegisterListener((int)EventID.OnNewMatchSuccess, OnNewMatchSuccess);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnNewMatchSuccess, OnNewMatchSuccess);
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
        switch (current)
        {
            case GameState.Init:
            case GameState.Restart:
                this.timePlayed = 0;
                timeLeftText.text = TimeSpan.FromSeconds(Mathf.FloorToInt(Mathf.Max(BoardGame.instance.pTimeLimitInSeconds - timePlayed, 0))).ToString("m':'ss");
                comboTimeSlider.minValue = 0;
                comboTimeSlider.maxValue = comboTimeCooldown;
                comboTimeSlider.value = 0;
                currentCoolDownTime = comboTimeCooldown;
                ComboCount = 0;
                GameStatisticsManager.starEarn = 0;
                startText.text = "0";
                break;
            case GameState.Ready:
                levelTxt.text = $"LEVEL {DataManager.UserData.level}";
                timeLeftText.text = TimeSpan.FromSeconds(Mathf.FloorToInt(Mathf.Max(BoardGame.instance.pTimeLimitInSeconds - timePlayed, 0))).ToString("m':'ss");
                break;
            case GameState.Pause:
                currentCoolDownTime = comboTimeSlider.value;
                DOTween.Kill(comboTimeSlider);
                break;
            case GameState.Play:
                if (last == GameState.Pause)
                {
                    DoComboCountDown();
                }
                break;
        }
    }

    private void OnNewMatchSuccess(object obj)
    {
        ComboCount++;
        var lastStar = GameStatisticsManager.starEarn;
        GameStatisticsManager.starEarn += DataManager.GameConfig.startPerMatch;
        startText.DOText(lastStar, GameStatisticsManager.starEarn, 1f);
    }

    private void DoComboCountDown()
    {
        DOTween.Kill(comboTimeSlider);
        comboTimeSlider.value = currentCoolDownTime;
        comboTimeSlider.DOValue(0, currentCoolDownTime).OnComplete(() =>
        {
            ComboCount = 0;
            currentCoolDownTime = comboTimeCooldown;
        });
    }
}
