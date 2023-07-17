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

    [Header("Stars")]
    [SerializeField] Text timeLeftText;
    [SerializeField] Text startText;
    [SerializeField] RectTransform star;
    [SerializeField] RectTransform star2;
    [SerializeField] RectTransform star2_parent;
    [SerializeField] RectTransform star3;
    [SerializeField] RectTransform star3_parent;
    [SerializeField] RectTransform timeSlideRect;
    [SerializeField] Image timeUseProcessImg;

    //[SerializeField] Slider comboTimeSlider = null;
    [SerializeField] Text comboCountText = null;
    [SerializeField] float comboCollectTimne;
    private int matchCount;

    
    [SerializeField]
    GameObject coinPrefab;
    
    public Transform defaultTarget;

    [SerializeField] int startCount;
    public int StarCount
    { 
        get { return instance.startCount; }
        set
        {
            instance.startCount = value;
        }
    }
    Coroutine coolDownCoroutine;

    float timePlayed = 0;
    float timeLeft = 0;
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

        timePlayed = BoardGame.instance.pStopWatch.ElapsedMilliseconds / 1000f;
        timeLeft = Mathf.Max(BoardGame.instance.pTimeLimitInSeconds - timePlayed, 0);
        timeLeftText.text = TimeSpan.FromSeconds(timeLeft).ToString("m':'ss");
        timeUseProcessImg.fillAmount = timeLeft / BoardGame.instance.pTimeLimitInSeconds;
        float timeUsePercent = (float)timePlayed / BoardGame.instance.pTimeLimitInSeconds;

        StarCount = timeUsePercent <= DataManager.GameConfig.threeStar ? 3 : timeUsePercent <= DataManager.GameConfig.twoStar ? 2 : 1;
        star3.gameObject?.SetActive(StarCount >= 3);
        star2.gameObject?.SetActive(StarCount >= 2);
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
                break;
            case GameState.Play:
                StarCount = 3;
                break;
            case GameState.RebornContinue:
                star3.gameObject?.SetActive(true);
                star2.gameObject?.SetActive(true);
                star.gameObject?.SetActive(true);
                timePlayed = 0;
                break;
            case GameState.WaitComplete:
                //if (ComboCount > 1)
                //{
                //    currentCoolDownTime = comboTimeSlider.value;
                //    DOTween.Kill(comboTimeSlider);
                //}
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
        StarCount = 2;
        comboCountText.text = $"x{startCount}";
        matchCount = 0;

        timeLeftText.text = TimeSpan.FromSeconds(Mathf.FloorToInt(Mathf.Max(BoardGame.instance.pTimeLimitInSeconds - timePlayed, 0))).ToString("m':'ss");
        timeUseProcessImg.fillAmount = 1;
        star3_parent.anchoredPosition = new Vector2(timeSlideRect.sizeDelta.x * (1 - DataManager.GameConfig.threeStar), star3_parent.anchoredPosition.y);
        star2_parent.anchoredPosition = new Vector2(timeSlideRect.sizeDelta.x * (1 - DataManager.GameConfig.twoStar), star2_parent.anchoredPosition.y);
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
        GameStatisticsManager.starEarn *= Mathf.Max(StarCount, 1);
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
            yield return CollectCoins(1, datum.items[i].transform.position);
        };
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
    public IEnumerator CollectCoins(int numb, Vector3 fromPos, Transform toTrans = null)
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
