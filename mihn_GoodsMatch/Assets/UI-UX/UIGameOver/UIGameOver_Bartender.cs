using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Base.Ads;

public class UIGameOver_Bartender : MonoBehaviour
{
    [SerializeField] DailyRewardAsset rewardsAsset;

    [Header("Base")]
    [SerializeField]
    protected UIAnimation anim = null;
    [SerializeField] protected UIAnimation animClose = null;
    [SerializeField] protected UIUnlockNewItemScreen unlockItemScreen = null;
    public UIAnimStatus Status = UIAnimStatus.IsHide;

    [Header("Result")]
    [SerializeField] protected UIAnimation animLose = null;
    [SerializeField] protected UIAnimation animWin = null;
    [SerializeField] protected GameObject resultInforPanel = null;
    [SerializeField] protected Text requestSuccessTxt = null;
    [SerializeField] protected Text requestFailTxt = null;
    [SerializeField] Text levelTxt = null;
    
    [Header("PopupWin")]
    [SerializeField] protected Button btnNormalClaim;
    [SerializeField] protected Text txtCoinEarn;
    [SerializeField] GameObject coinEarnContainer = null;
    [SerializeField] protected Button btnScaleCoinClaim = null;
    [SerializeField] protected Text txtScaleCoinEarn;
    [SerializeField] protected Text txt_unlockValue;
    [SerializeField] Text txt_nextLevel;
    [SerializeField] Button btn_GoToNextLevel;
    
    [Header("Buttons Base")]
    [SerializeField]
    protected Button backButton = null;
    [SerializeField]
    protected Button restartButton = null;
    
    [Header("Continue")]
    [SerializeField]
    protected UIAnimation animContinue = null;
    [SerializeField]
    protected RebornType rebornType = RebornType.Continue;
    [SerializeField]
    protected RebornBy rebornBy = RebornBy.Ads;
    [SerializeField]
    protected int rebornCount = 0;
    [SerializeField]
    protected int rebornCountMax = 1;
    [SerializeField]
    protected Text rebornByTitle = null;
    [SerializeField]
    protected Text rebornByDes = null;
    [SerializeField]
    protected Text rebornByInfo = null;

    [Header("CountDown")]
    private float rebornElapsedTime = 0;
    [SerializeField]
    private int rebornElapsedMaxTime = 5;
    [SerializeField]
    protected Text rebornByCountDownText = null;
    [SerializeField]
    protected Image rebornByCountDownImage = null;
    [SerializeField]
    protected Transform rebornByCountDownImageDotTransform = null;
    [SerializeField]
    protected Button rebornBySkipButton = null;
    [SerializeField]
    protected Button noThanksButton = null;

    [Header("Continue Free")]
    [SerializeField]
    protected Button rebornByFreeButton = null;

    [Header("Continue Ads")]
    [SerializeField]
    protected Button rebornByAdsButton = null;

    [Header("Continue Coin")]
    [SerializeField]
    protected Button rebornByCoinButton = null;
    [SerializeField]
    protected Text rebornByCoinDes = null;
    [SerializeField]
    protected int rebornByCoinCost = 50;
    [SerializeField]
    protected int rebornByCoinTotalCost = 0;

    [Header("Scale coin")]
    [SerializeField]  public int bonusAds = 2;
    public float timeDelayBtnBack = 3;

    protected static UIGameOver_Bartender instance;
    protected UserData userData => DataManager.UserData;
    protected GameConfig gameConfig => DataManager.GameConfig;
    public bool IsShowContinue => rebornCount <= rebornCountMax;

    protected void Awake()
    {
        if (anim == null)
            anim = GetComponent<UIAnimation>();
        instance = this;

        animLose.gameObject.SetActive(true);
        animWin.gameObject.SetActive(true);
        animContinue.gameObject.SetActive(true);
    }
    protected void Start()
    {
        backButton?.onClick.AddListener(Btn_Back_Handle);
        restartButton?.onClick.AddListener(Btn_Restart_Handle);
        rebornByFreeButton?.onClick.AddListener(Btn_RebornByFree_Handle);
        rebornByCoinButton?.onClick.AddListener(Btn_RebornByCoin_Handle);
        rebornByAdsButton?.onClick.AddListener(Btn_RebornByAds_Handle);
        rebornBySkipButton?.onClick.AddListener(Btn_SkipCountDown_Handle);
        noThanksButton?.onClick.AddListener(Btn_NoReborn_Handle);
        btnScaleCoinClaim?.onClick.AddListener(Btn_ScaleStarClick);
        btnNormalClaim?.onClick.AddListener(Btn_StarClaimClick);
        btn_GoToNextLevel?.onClick.AddListener(Btn_NextLevel_Handle);
    }
    IEnumerator DelayShowButton(System.Action callback = null)
    {
        //backButton?.gameObject.SetActive(false);
        yield return new WaitForSeconds(timeDelayBtnBack);
        //backButton?.gameObject.SetActive(true);
        callback?.Invoke();
    }
    public void Show(GameState gameState, object data)
    {
        Status = UIAnimStatus.IsAnimationShow;

        string adsID = gameState == GameState.Complete ? "Bartender_LevelComplete" : "Bartender_LevelFail";
        CheckToShowInterstitialAds(adsID, null);

        var isWin = gameState == GameState.Complete;
        var resultDatum = (GameResult)data;
        requestSuccessTxt.text = $"{resultDatum.completePoint}";
        requestFailTxt.text = $"{resultDatum.missedPoint}";
        requestFailTxt.color = isWin ? Color.white : Color.red;
        coinEarnContainer.SetActive(isWin);

#if USE_IRON || USE_MAX || USE_ADMOB
        //AdsManager.TotalTimePlay += GameStatisticsManager.TimePlayInGameEnd;
#endif

        if (gameState == GameState.GameOver)
        {
            ShowResult(false);
            //if (IsShowContinue)
            //{
            //    SoundManager.Play("8. Time Up");
            //    ShowContinue();
            //}
            //else
            //    ShowResult(false);
        }
        else if (gameState == GameState.Complete)
        {
            ShowResult(true);
            SoundManager.Play("6. Win");
        }
    }
    public void Hide(Action onHideDone = null)
    {
        if(anim.Status != UIAnimStatus.IsHide && anim.Status != UIAnimStatus.IsAnimationHide)
            StartCoroutine(YieldHide(onHideDone));
    }
    private IEnumerator YieldHide(Action onHideDone = null)
    {
        Status = UIAnimStatus.IsAnimationHide;
        animLose.Hide();
        animContinue.Hide();
        animWin.Hide();
        resultInforPanel.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.25f);
        SoundManager.Play(GameConstants.sound_doorOpenUp);
        animClose.Hide(onCompleted: () =>
        {
            anim.Hide(() =>
            {
                onHideDone?.Invoke();
                Status = UIAnimStatus.IsHide;
            });
        });
    }

    #region GameComplete
    private void OnStarScaleHandle(int scaleValue)
    {
        //bonusAds = scaleValue;
        txtCoinEarn.text = GameStatisticsManager.goldEarn.ToString();
        txtScaleCoinEarn.text = (GameStatisticsManager.goldEarn * scaleValue).ToString();
    }
    #endregion

    public virtual void ShowResult(bool isWin)
    {
        resultInforPanel.gameObject.SetActive(true);
        int lastLevel = isWin ? DataManager.UserData.bartenderLevel - 1 : DataManager.UserData.bartenderLevel;
        if (levelTxt)
            levelTxt.text = lastLevel % 2 == 0 ? $"DAY {lastLevel / 2 + 1}" : $"NIGHT {lastLevel / 2 + 1}";
        if (txtCoinEarn)
            txtCoinEarn.text = $"{GameStatisticsManager.goldEarn}";

        if (isWin)
        {
            btnScaleCoinClaim?.gameObject.SetActive(true);
            btnScaleCoinClaim.interactable = true;
            btnNormalClaim?.gameObject.SetActive(false);
            btnNormalClaim.interactable = true;
        }

        Status = UIAnimStatus.IsAnimationShow;
        if (anim.Status != UIAnimStatus.IsShow)
        {
            anim.Show(() =>
            {
                if (isWin)
                {
                    animWin.Show(null, () => {
                        OnShowResult(true);
                    });
                }
                else
                {
                    animLose.Show(null, () => OnShowResult(false));
                }
            });
        }
        else
        {
            if (isWin)
                animWin.Show(null, () => {
                    OnShowResult(true);
                });
            else
                animLose.Show(null, () => OnShowResult(false));
        }
    }
    public void OnShowResult(bool isWin)
    {
        Status = UIAnimStatus.IsShow;
        DataManager.Save();
        OnShowButton(isWin);
    }
    public void OnShowButton(bool isWin)
    {
        if (isWin)
        {
            DOVirtual.DelayedCall(timeDelayBtnBack, () => {
                btnNormalClaim?.gameObject.SetActive(true);
            }).SetId("DelayShowNormalClaimButton");
            if(txtScaleCoinEarn)
                txtScaleCoinEarn.text = $"{GameStatisticsManager.goldEarn * bonusAds}";
        }
    }

    private void Btn_StarClaimClick()
    {
        SoundManager.Play("1. Click Button");
        btnNormalClaim.interactable = false;
        btnScaleCoinClaim.interactable = false;
        CoinManager.Add(GameStatisticsManager.goldEarn, btnNormalClaim.transform);
        DOVirtual.DelayedCall(3f, () => Btn_Next_Handle());
        DataManager.Save();
    }

    private void Btn_ScaleStarClick()
    {
        SoundManager.Play("1. Click Button");
        btnScaleCoinClaim.interactable = false;
        AdsManager.ShowVideoReward((e, t) =>
        {
            if (e == AdEvent.ShowSuccess || DataManager.GameConfig.isAdsByPass)
            {
                txtCoinEarn.DOText(GameStatisticsManager.goldEarn, GameStatisticsManager.goldEarn * bonusAds, 1.5f);
                DOVirtual.DelayedCall(1.5f, () => CoinManager.Add(GameStatisticsManager.goldEarn * bonusAds, btnScaleCoinClaim.transform));
                btnNormalClaim.interactable = false;
                DOTween.Kill("DelayShowNormalClaimButton");
                DOVirtual.DelayedCall(3f, () => Btn_Next_Handle());
                DataManager.Save();
            }
            else
            {
                
            }
        }, "ClaimStarScale", "star");
    }

    public virtual void ShowContinue()
    {
        Status = UIAnimStatus.IsAnimationShow;
        rebornType = gameConfig.rebornType;

        rebornByFreeButton?.gameObject.SetActive(false);
        rebornByCoinButton?.gameObject.SetActive(false);
        rebornByAdsButton?.gameObject.SetActive(false);
        noThanksButton?.gameObject.SetActive(false);

        if (rebornByInfo)
        {
            rebornByInfo.text = $"+{Mathf.FloorToInt(DataManager.GameConfig.rebornTimeAdding)}s";
        }

        rebornByAdsButton?.gameObject.SetActive(true);
        rebornByCoinButton?.gameObject.SetActive(true);
        rebornByCoinTotalCost = (rebornCount + 1) * rebornByCoinCost;
        rebornByCoinDes.text = rebornByCoinTotalCost.ToString();
        rebornByCoinButton.interactable = CoinManager.totalCoin >= rebornByCoinTotalCost;

        anim.Show(() =>
        {
            animContinue.Show(null, () =>
            {
                DOVirtual.DelayedCall(timeDelayBtnBack, () => noThanksButton?.gameObject.SetActive(true), true);
            }, null);
        });
    }

    protected float delta = 0;
    protected string timeStr = "";
    protected string timeStrLast = "";
    protected IEnumerator DORebornCountDown()
    {
        delta = 0;
        timeStr = "";
        timeStrLast = "";
        rebornElapsedTime = 0;
        while (rebornElapsedTime < rebornElapsedMaxTime)
        {
            if (UIManager.CurPopup != null && UIManager.CurPopup.Status != UIAnimStatus.IsHide)
            {
                //Debug.LogWarning("CurPopup: Waiting for animation DONE");
            }
            else if (UIToast.toastType == ToastType.Loading && UIToast.Status != UIAnimStatus.IsHide)
            {
                //Debug.LogWarning("UIToast: Waiting for animation DONE");
            }
            else
            {
                delta = 1 - rebornElapsedTime / rebornElapsedMaxTime;
                rebornElapsedTime += Time.deltaTime;

                if (rebornByCountDownImage)
                {
                    rebornByCountDownImage.fillAmount = delta;
                }

                if (rebornByCountDownImageDotTransform)
                {
                    rebornByCountDownImageDotTransform.SetLocalRotation2D(delta * 360f);
                }

                if (rebornByCountDownText)
                {
                    int timeCount = Mathf.FloorToInt(rebornElapsedMaxTime - rebornElapsedTime);

                    if (timeCount <= 3)
                        noThanksButton?.gameObject.SetActive(true);

                    if (timeCount >= 0)
                        timeStrLast = timeCount.ToString("#0");
                    else
                        timeStrLast = "!?";

                    if (timeStr != timeStrLast)
                    {
                        timeStr = timeStrLast;
                        rebornByCountDownText.text = timeStr;
                        UIAnimation.DoScale(rebornByCountDownText.transform, 1.2f, 0.25f, 0);
                        if (rebornElapsedTime >= 0.5f && !string.IsNullOrEmpty(timeStrLast))
                        {
                            //SoundManager.Play("sfx_timer_" + (timeCount % 2));
                        }
                    }
                }
            }
            yield return null;
        }
        rebornElapsedTime = 9999;
        //NoReborn();
    }
    public void CheckToShowInterstitialAds(string itemId, Action onDone)
    {
        if (!GameUtilities.IsShowAdsInter(DataManager.UserData.bartenderLevel + 5))
        {
            onDone?.Invoke();
            return;
        }
#if USE_IRON || USE_MAX || USE_ADMOB
        AdsManager.ShowInterstitial((s, adType) =>
        {
            UIToast.Hide();
            onDone?.Invoke();
        }, name, itemId);
#else
        UIToast.Hide();
        onDone?.Invoke();
#endif
    }

    #region Button Handle
    public void Btn_Back_Handle()
    {
        //btn_GoToNextLevel.interactable = false;
        SoundManager.Play("1. Click Button");
        rebornCount = 0;
        GameStateManager.Idle(null);
    }
    public void Btn_Restart_Handle()
    {
        SoundManager.Play("1. Click Button");
        rebornCount = 0;
        
        GameStateManager.Init(null);
        Hide();
    }
    public void Btn_RebornByFree_Handle()
    {
        Reborn();
    }
    public void Btn_RebornByCoin_Handle()
    {
        if (CoinManager.totalCoin >= rebornByCoinTotalCost)
        {
            CoinManager.Add(-rebornByCoinTotalCost);
            Reborn();
        }
    }
    public void Btn_RebornByDiamond_Handle()
    {
        Debug.LogError("RebornByDiamond NOT IMPLEMENT");
    }
    public void Btn_RebornByAds_Handle()
    {
        SoundManager.Play("1. Click Button");
        AdsManager.ShowVideoReward((e, t) =>
        {
            if (e == AdEvent.ShowSuccess || DataManager.GameConfig.isAdsByPass)
            {
                Reborn();
            }
            else
            {
                //ShowResult(GameStateManager.CurrentState == GameState.Complete || DataManager.levelSelect % DataManager.GameConfig.levelsToNextChallenge == 0);
            }
        }, "ContinueWithAds", "TimePlay");
    }

    public void Btn_NextLevel_Handle()
    {
        //btn_GoToHome.interactable = false;
        GameStateManager.LoadGame(null);
    }

    public void Btn_Next_Handle()
    {
        //UILoadGame.Init(true, null);
        SoundManager.Play("1. Click Button");
        rebornCount = 0;
        SoundManager.Play(GameConstants.sound_doorCloseDown);
        animClose.Show(null, () => 
        {
            this.PostEvent((int)EventID.OnClearLastLevel);
            unlockItemScreen.Show(() =>
            {
                Hide(() => GameStateManager.Init(null));
            });
        }, null);
    }
    protected void Btn_SkipCountDown_Handle()
    {
        rebornElapsedTime += 1f;
    }
    public void Btn_NoReborn_Handle()
    {
        //StopCoroutine(DORebornCountDown());
        if (GameStateManager.CurrentState == GameState.GameOver)
        {
            animContinue.Hide(() =>
            {
                Debug.Log("animContinue: Hide - GameStateManager: " + GameStateManager.CurrentState);
                if (GameStateManager.CurrentState == GameState.GameOver)
                    ShowResult(GameStateManager.CurrentState == GameState.Complete);
            });
        }
    }
    private void Reborn()
    {
        StopAllCoroutines();
        UIToast.Hide();
        Hide(() =>
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                GameStateManager.RebornContinue(null);
            });
            rebornCount++;
        });
    }
    #endregion
}
