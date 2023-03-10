using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using MyBox;

public class UIInfo_Bartender : MonoBehaviour
{
    private static UIInfo_Bartender instance;
    private GameConfig config => DataManager.GameConfig;

    [Header("Base")]
    [SerializeField] UIAnimation anim;
    [SerializeField] Button settingButton;
    [SerializeField] Text coinEarnText;

    [Header("Level data")]
    [SerializeField] Slider timeLeftSlider = null;
    [ReadOnly, SerializeField] int maxRequestMissed = 3;
    [ReadOnly, SerializeField] float totalTime = 180f;
    [SerializeField] Text requestMissText;
    [SerializeField] Text timeLeftTxt = null;
    float timePlayed = 0;
    int currRequestMissed = 0;

    [Header("Combo")]
    [SerializeField] Slider comboTimeSlider = null;
    [SerializeField] Text comboCountText = null;
    [ReadOnly, SerializeField] float comboTimeCooldown = 8f;
    float currentComboTimeLeft;
    int comboCount;
    public int ComboCount
    {
        get { return instance.comboCount; }
        set
        {
            instance.comboCount = Mathf.Min(value, DataManager.GameConfig.maxCombo_bartender);
            if (instance.comboCount > 0)
            {
                currentComboTimeLeft = config.timeComboEslap;
                if(instance.comboCount > 1)
                {
                    this.PostEvent((int)EventID.OnNewCombo, ComboCount-1);
                    Debug.Log($"MATCH COMBO - COUNT = {ComboCount - 1}");
                }
            }
        }
    }
    Coroutine comboCooldownCoroutine;

    [Header("COIN")]
    [SerializeField] GameObject coinPrefab;
    [SerializeField] float animCollectDelay = 1f;
    public Transform defaultTarget;

    [Header("TUTORIAL")]
    [SerializeField] UITutorial uiTutPanel;
    private bool showTut = false;

    [Header("DEBUG ")]
    [ReadOnly, SerializeField] int currLevel;
    [ReadOnly, SerializeField] float comboTime;

    int currRequestComplete;
    private Coroutine collectCoinCoroutine = null;
    
    private void Awake()
    {
        instance = this;
        coinPrefab.CreatePool(10);
    }
    private void Start()
    {
        settingButton?.onClick.AddListener(() =>
        {
            if (GameStateManager.CurrentState == GameState.Play)
            {
                GameStateManager.Pause(null);
            }
        });
    }
    private void OnEnable()
    {
        GameStateManager.OnStateChanged += GameStateManager_OnStateChanged;
        this.RegisterListener((int)EventID.OnMatchedRightRequest, OnNewRequestSuccess);
        this.RegisterListener((int)EventID.OnRequestTimeout, OnRequestMissed);
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= GameStateManager_OnStateChanged;
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnMatchedRightRequest, OnNewRequestSuccess);
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnRequestTimeout, OnRequestMissed);
    }

    private void LateUpdate()
    {
        if (GameStateManager.CurrentState != GameState.Play)
            return;
        if(timePlayed >= totalTime)
        {
            BoardGame_Bartender.instance?.GameCompleteHandler(new GameResult() { completePoint = currRequestComplete, missedPoint = currRequestMissed });
            return;
        }
        timePlayed += Time.deltaTime;
        timeLeftTxt.text = TimeSpan.FromSeconds(Mathf.CeilToInt(Mathf.Max(totalTime - timePlayed, 0))).ToString("m':'ss");
        //timeLeftSlider.value = totalTime - timePlayed;
        if (currentComboTimeLeft < 0)
            ComboCount = 0;
        if (ComboCount > 0)
            currentComboTimeLeft -= Time.deltaTime;
        comboTime = currentComboTimeLeft;
        
    }

    private void GameStateManager_OnStateChanged(GameState current, GameState last, object data)
    {
        switch (current)
        {
            case GameState.Init:
            case GameState.Restart:
                break;
            case GameState.Ready:
                break;
            case GameState.Pause:
                //currentComboTimeLeft = comboTimeSlider.value;
                //DOTween.Kill(comboTimeSlider);
                break;
            case GameState.Play:
                break;
            case GameState.RebornContinue:
                ComboCount = 0;
                break;
            case GameState.WaitComplete:
                //DOTween.Kill(comboTimeSlider);
                DOTween.Kill(this);
                StopAllCoroutines();
                break;
            case GameState.WaitGameOver:
                //DOTween.Kill(comboTimeSlider);
                DOTween.Kill(this);
                StopAllCoroutines();
                break;
        }
    }
    private void OnNewGameStart()
    {
        showTut = DataManager.UserData.bartenderLevel == 0 && !DataManager.UserData.tutBartenderDone;
        uiTutPanel.gameObject.SetActive(showTut);
        
        currLevel = DataManager.UserData.bartenderLevel;
        GameStatisticsManager.goldEarn = 0;
        coinEarnText.text = "0";

        totalTime = Mathf.Min(config.maxLevelTime_bartender, config.levelTimeBase_bartender + DataManager.UserData.bartenderLevel * config.levelTimeIncrease_bartender);
        timePlayed = 0;
        timeLeftTxt.text = TimeSpan.FromSeconds(Mathf.CeilToInt(totalTime)).ToString("m':'ss");

        maxRequestMissed = config.requestMissLimit;
        comboTimeCooldown = config.timeComboEslap;

        if (timeLeftSlider != null)
        {
            timeLeftSlider.minValue = 0;
            timeLeftSlider.maxValue = totalTime;
            timeLeftSlider.value = totalTime;
        }

        ComboCount = 0;
        currRequestComplete = 0;
        currRequestMissed = 0;
        requestMissText.text = $"{maxRequestMissed}/{maxRequestMissed}";
    }
    public void Show()
    {
        OnNewGameStart();
        anim.Show(null, () =>
        {
            if (showTut)
                this.PostEvent((int)EventID.OnTutStepDone, 0);
        });
    }
    public void Hide()
    {
        anim.Hide();
    }

    private void OnNewRequestSuccess(object obj)
    {
        var item = (Goods_Item)obj;
        ComboCount++;
        int coinEarn = DataManager.UserData.bartenderLevel % 2 == 0 ? DataManager.GameConfig.coinEarnInDay : DataManager.GameConfig.coinEarnInNight;
        coinEarn = Mathf.Max(coinEarn, coinEarn * ComboCount);
        var lastcoin = GameStatisticsManager.goldEarn;
        GameStatisticsManager.goldEarn += coinEarn;
        coinEarnText.DOText(lastcoin, GameStatisticsManager.goldEarn, 1f, 1f);
        currRequestComplete++;
        StartCoroutine(CollectStars(coinEarn, item.transform.position));
    }
    private void OnRequestMissed(object obj)
    {
        if (timePlayed >= totalTime)
            return;

        currRequestMissed++;
        requestMissText.text = $"{Mathf.Max(0,  maxRequestMissed - currRequestMissed)}/{maxRequestMissed}";

        if (currRequestMissed >= maxRequestMissed)
        {
            BoardGame_Bartender.instance?.GameOverHandler(new GameResult() { completePoint = currRequestComplete, missedPoint = currRequestMissed });
        }
    }
    private void DoComboCountDown()
    {
        comboTimeSlider.gameObject.SetActive(true);
        DOTween.Kill(comboTimeSlider);
        comboTimeSlider.value = currentComboTimeLeft;
        instance.comboCountText.text = $"X{instance.comboCount}";
        instance.comboCountText.transform.DOScale(3f, 0f);
        instance.comboCountText.transform.DOScale(1f, 0.5f);
        comboTimeSlider.DOValue(0, currentComboTimeLeft).OnComplete(() =>
        {
            ComboCount = 0;
            currentComboTimeLeft = config.timeComboEslap; ;
            comboTimeSlider.gameObject.SetActive(false);
        });
    }

    public IEnumerator CollectStars(int numb, Vector3 fromPos, Transform toTrans = null)
    {
        numb = Mathf.Min(5, numb);
        yield return new WaitForSeconds(animCollectDelay);
        var endPos = toTrans != null ? toTrans.position : instance.defaultTarget.position;
        for (int i = 0; i < numb; i++)
        {
            var item = instance.coinPrefab.Spawn();
            item.transform.position = fromPos;
            item.transform.DOScale(0.8f, 0).SetId($"Collect-coin-{item.name}");
            item.transform.DOMove(endPos, 1f).OnComplete(() => { 
                item.Recycle();
                SoundManager.Play("5. Star to target");
            }).SetId("CollectCoinIngame").SetUpdate(true);
            yield return new WaitForSeconds(0.2f);
        }
    }
}

[Serializable]
public class GameResult
{
    public int completePoint;
    public int missedPoint;
}