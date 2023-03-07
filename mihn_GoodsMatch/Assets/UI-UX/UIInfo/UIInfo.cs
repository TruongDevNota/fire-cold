using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIInfo : MonoBehaviour
{
    private static UIInfo instance;

    [Header("Base")]
    [SerializeField] UIAnimation anim;
    [SerializeField] Button settingButton;
    
    [SerializeField] Text timeLeftText;
    [SerializeField] Text startText;

    [SerializeField] Slider comboTimeSlider = null;
    [SerializeField] Text comboCountText = null;
    [SerializeField] float comboCollectTimne;
    float comboTimeCooldown;
    private int matchCount;

    [Header("Star effect")]
    [SerializeField]
    GameObject coinPrefab;
    public Transform defaultTarget;

    [SerializeField] float currentCoolDownTime;
    [SerializeField] int comboCount;
    public int ComboCount
    { 
        get { return instance.comboCount; }
        set
        {
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
        instance = this;
        coinPrefab.CreatePool(10);
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
        this.RegisterListener((int)EventID.OnNewMatchSuccess, OnNewMatchSuccess);
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= GameStateManager_OnStateChanged;
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
                break;
            case GameState.Ready:
                
                break;
            case GameState.Pause:
                currentCoolDownTime = comboTimeSlider.value;
                DOTween.Kill(comboTimeSlider);
                break;
            case GameState.Play:
                ComboCount = 3;
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
                }
                StartCoroutine(YieldShowBonus());
                break;
        }
    }
    public void Show()
    {
        this.timePlayed = 0;
        timeLeftText.text = "-:--";
        GameStatisticsManager.starEarn = 0;
        startText.text = "0";
        ComboCount = 3;
        comboCountText.text = $"x{comboCount}";
        matchCount = 0;

        timeLeftText.text = TimeSpan.FromSeconds(Mathf.FloorToInt(Mathf.Max(BoardGame.instance.pTimeLimitInSeconds - timePlayed, 0))).ToString("m':'ss");
        comboTimeCooldown = BoardGame.instance.pTimeLimitInSeconds / 3;
        comboTimeSlider.minValue = 0;
        comboTimeSlider.maxValue = comboTimeCooldown;
        comboTimeSlider.value = comboTimeCooldown;
        currentCoolDownTime = comboTimeCooldown;
        anim.Show();
    }
    public void Hide()
    {
        anim.Hide();
    }
    private IEnumerator YieldShowBonus()
    {
        yield return new WaitForSeconds(0.15f);
        var lastStar = GameStatisticsManager.starEarn;
        GameStatisticsManager.starEarn *= Mathf.Max(ComboCount, 1);
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
    }

    private void OnNewMatchSuccess(object obj)
    {
        matchCount++;
        var lastStar = GameStatisticsManager.starEarn;
        GameStatisticsManager.starEarn += matchCount % 9 + 1;
        startText.DOText(lastStar, GameStatisticsManager.starEarn, 0.5f, 0.5f);
        StartCoroutine(YieldCollectCoin(obj));
    }

    private IEnumerator YieldCollectCoin(object obj)
    {
        var datum = (NewMatchDatum)obj;
        var shelf = datum.items[0].pCurrentShelf;
        for (int i = 0; i < datum.items.Count; i++)
        {
            var posIndex = datum.items[i].pFirstLeftCellIndex;
            shelf.PickItemUpHandler(datum.items[i]);
            datum.items[i].Explode();
            yield return CollectStars(1, datum.items[i].transform.position);
        };
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
            var item = instance.coinPrefab.Spawn();
            item.transform.position = fromTrans.position;
            item.transform.DOScale(1f, 0).SetUpdate(true);
            item.transform.DOScale(0.8f, 0.8f).SetUpdate(true);
            item.transform.DOMove(instance.defaultTarget.position, 1f).OnComplete(() =>
            {
                item.Recycle();
            }).SetUpdate(true);
        }
        DOVirtual.DelayedCall(1f,() => SoundManager.Play("5. Star to target")).SetUpdate(true);
    }
    public IEnumerator CollectStars(int numb, Vector3 fromPos, Transform toTrans = null)
    {
        yield return new WaitForSeconds(0.1f);
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
        }
    }
}
