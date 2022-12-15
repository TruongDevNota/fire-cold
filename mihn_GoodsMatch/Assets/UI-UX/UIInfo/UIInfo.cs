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

    [Header("Star effect")]
    [SerializeField]
    GameObject starPrefab;
    public Transform defaultTarget;

    float currentCoolDownTime;
    int comboCount = 0;
    public int ComboCount
    { 
        get { return instance.comboCount; }
        set
        {
            if(value != instance.comboCount)
                instance.comboCount = value;
            comboTimeSlider.gameObject.SetActive(instance.comboCount != 0);
            if (instance.comboCount != 0)
                instance.DoComboCountDown();
        }
    }
    Coroutine coolDownCoroutine;

    float timePlayed = 0;
    int currentStar;

    private void Awake()
    {
        GameStateManager.OnStateChanged += GameStateManager_OnStateChanged;
        instance = this;
        starPrefab.CreatePool(10);
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
                timeLeftText.text = ("-:--");
                comboTimeSlider.minValue = 0;
                comboTimeSlider.maxValue = comboTimeCooldown;
                comboTimeSlider.value = 0;
                currentCoolDownTime = comboTimeCooldown;
                ComboCount = 0;
                GameStatisticsManager.starEarn = 0;
                startText.text = "0";
                break;
            case GameState.Ready:
                levelTxt.text = $"LEVEL {DataManager.levelSelect}";
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
        instance.comboCountText.text = $"X{instance.comboCount}";
        instance.comboCountText.transform.DOScale(2f, 0f);
        instance.comboCountText.transform.DOScale(1f, currentCoolDownTime/2);
        comboTimeSlider.DOValue(0, currentCoolDownTime).OnComplete(() =>
        {
            ComboCount = 0;
            currentCoolDownTime = comboTimeCooldown;
        });
    }

    public static void CollectStars(int numb, Transform fromTrans, Transform toTrans = null)
    {
        
        for(int i = 0; i < numb; i++)
        {
            var item = instance.starPrefab.Spawn();
            item.transform.position = fromTrans.position;
            item.transform.DOScale(1f, 0);
            item.transform.DOScale(0.8f, 0.8f);
            item.transform.DOMove(instance.defaultTarget.position, 1f).OnComplete(() =>
            {
                item.Recycle();
            });
        }
        DOVirtual.DelayedCall(1f,() => SoundManager.Play("5. Star to target"));
    }

}
