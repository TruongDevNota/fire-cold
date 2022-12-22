using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIInfo : MonoBehaviour
{
    private static UIInfo instance;

    [SerializeField] Text timeLeftText;
    [SerializeField] Text startText;

    [SerializeField] Slider comboTimeSlider = null;
    [SerializeField] Text comboCountText = null;
    [SerializeField] float comboCollectTimne;
    float comboTimeCooldown;
    private int matchCount;

    [Header("Star effect")]
    [SerializeField]
    GameObject starPrefab;
    public Transform defaultTarget;

    float currentCoolDownTime;
    int comboCount;
    public int ComboCount
    { 
        get { return instance.comboCount; }
        set
        {
            if (value != instance.comboCount)
                instance.comboCount = value;
            if (value < 1)
                instance.comboCountText.text = $"X1";
            if (instance.comboCount > 0)
            {
                currentCoolDownTime = comboTimeCooldown;
                instance.DoComboCountDown();
            }
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
                timeLeftText.text = "-:--";
                GameStatisticsManager.starEarn = 0;

                comboCount = 3;
                startText.text = "x3";
                comboTimeCooldown = BoardGame.instance.pTimeLimitInSeconds / 3;
                comboTimeSlider.minValue = 0;
                comboTimeSlider.maxValue = comboTimeCooldown;
                comboTimeSlider.value = comboTimeCooldown;
                currentCoolDownTime = comboTimeCooldown;
                matchCount = 0;
                break;
            case GameState.Ready:
                timeLeftText.text = TimeSpan.FromSeconds(Mathf.FloorToInt(Mathf.Max(BoardGame.instance.pTimeLimitInSeconds - timePlayed, 0))).ToString("m':'ss");
                break;
            case GameState.Pause:
                currentCoolDownTime = comboTimeSlider.value;
                DOTween.Kill(comboTimeSlider);
                break;
            case GameState.Play:
                if(ComboCount > 0)
                    DoComboCountDown();
                break;
            case GameState.RebornContinue:
                ComboCount = 0;
                break;
            case GameState.WaitComplete:
                if (ComboCount > 1)
                {
                    currentCoolDownTime = comboTimeSlider.value;
                    DOTween.Kill(comboTimeSlider);
                    StartCoroutine(YieldShowBonus());
                }
                else
                    GameStateManager.Complete(null);
                break;
        }
    }

    private IEnumerator YieldShowBonus()
    {
        var lastStar = GameStatisticsManager.starEarn;
        GameStatisticsManager.starEarn *= ComboCount;
        startText.DOText(lastStar, GameStatisticsManager.starEarn, comboCollectTimne);
        float t = 0;
        int remainTime = Mathf.FloorToInt(Mathf.Max(BoardGame.instance.pTimeLimitInSeconds - timePlayed, 0));
        while (t< comboCollectTimne)
        {
            var v = remainTime * (1 - t / comboCollectTimne);
            timeLeftText.text = TimeSpan.FromSeconds(v).ToString("m':'ss");
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        timeLeftText.text = "0:00";

        yield return new WaitForSeconds(0.2f);
        GameStateManager.Complete(null);
    }

    private void OnNewMatchSuccess(object obj)
    {
        matchCount++;
        var lastStar = GameStatisticsManager.starEarn;
        GameStatisticsManager.starEarn += matchCount % 9 + 1;
        startText.DOText(lastStar, GameStatisticsManager.starEarn, 1f);
    }

    private void DoComboCountDown()
    {
        DOTween.Kill(comboTimeSlider);
        comboTimeSlider.value = currentCoolDownTime;
        instance.comboCountText.text = $"X{instance.comboCount}";
        instance.comboCountText.transform.DOScale(2f, 0f);
        instance.comboCountText.transform.DOScale(1f, 0.5f);
        comboTimeSlider.DOValue(0, currentCoolDownTime).OnComplete(() =>
        {
            ComboCount--;
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
