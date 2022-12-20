using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Base.Ads;

public class UIGameOver : MonoBehaviour
{
    [Header("Base")]
    [SerializeField]
    protected UIAnimation anim = null;
    public UIAnimStatus Status = UIAnimStatus.IsHide;

    [Header("Result")]
    [SerializeField]
    protected UIAnimation animLose = null;
    [SerializeField]
    protected UIAnimation animWin = null;

    [Header("PopupWin")]
    [SerializeField]
    protected UILineRoullete lineRoullete = null;
    [SerializeField]
    protected Button btnStarClaim;
    [SerializeField]
    protected Text txtStar;
    [SerializeField]
    protected Button btnScaleStarClaim = null;
    [SerializeField]
    protected Text txtScaleStar;
    [SerializeField]
    protected Image img_newItemUnlock;
    [SerializeField]
    protected GameObject levelChest;
    [SerializeField]
    protected Image img_ChestUnlocked;
    [SerializeField]
    protected Text txt_unlockValue;
    [SerializeField]
    private UIPopupReward popupReward;
    private bool isItemUnlock;

    [Header("Stage Info")]
    [SerializeField]
    protected Text txtLevel;
    
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

    [Header("Continue Diamond")]
    [SerializeField]
    protected Button rebornByDiamondButton = null;
    [SerializeField]
    protected Text rebornByDiamondDes = null;
    [SerializeField]
    protected int rebornByDiamondCost = 10;
    [SerializeField]
    protected int rebornByDiamonTotalCost = 0;

    [Header("PlacementNames")]
    [SerializeField]
    protected string placementReborn = "Reborn";

    [SerializeField]
    protected string soundScoreFxCount = "sfx_score_count";
    [SerializeField]
    protected string soundScoreFxComplete = "sfx_score_complete";
    [SerializeField]
    protected string soundScoreFxBest = "sfx_score_best";

    [SerializeField]
    protected string soundFxCount = "";
    [SerializeField]
    protected string soundFxEnd = "";

    [Header("X5 coin")]
    public int bonusAds = 1;
    public float timeDelayBtnBack = 3;

    protected static UIGameOver instance;
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
        rebornByDiamondButton?.onClick.AddListener(Btn_RebornByDiamond_Handle);
        rebornByAdsButton?.onClick.AddListener(Btn_RebornByAds_Handle);
        rebornBySkipButton?.onClick.AddListener(Btn_SkipCountDown_Handle);
        noThanksButton?.onClick.AddListener(Btn_NoReborn_Handle);
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

#if USE_IRON || USE_MAX || USE_ADMOB
        // AdsManager.TotalTimePlay += GameStatisticsManager.TimePlayInGameEnd;
#endif

        if (gameState == GameState.GameOver)
        {
            if (IsShowContinue)
            {
                SoundManager.Play("8. Time Up");
                ShowContinue();
            }
            else
                ShowResult(false);
        }
        else if (gameState == GameState.Complete)
        {
            ShowResult(true);
            SoundManager.Play("6. Win");
        }
    }
    public void Hide(Action onHideDone = null)
    {
        Status = UIAnimStatus.IsAnimationHide;
        animLose.Hide();
        animContinue.Hide();
        animWin.Hide();
        anim.Hide(() =>
        {
            onHideDone?.Invoke();
            Status = UIAnimStatus.IsHide;
        });
    }

    #region GameComplete
    private void OnStarScaleHandle(int scaleValue)
    {
        bonusAds = scaleValue;
        txtStar.text = GameStatisticsManager.starEarn.ToString();
        txtScaleStar.text = (GameStatisticsManager.starEarn * scaleValue).ToString();
    }
    #endregion

    public virtual void ShowResult(bool isWin)
    {
        //UIToast.ShowLoading("", 1);
        if (txtLevel)
            //txtLevel.text = $"LEVEL {DataManager.UserData.level + 1}";
            txtLevel.text = $"LEVEL {DataManager.UserData.level + 1}";
        if (txtStar)
            txtStar.text = $"+{GameStatisticsManager.starEarn}";

        if (isWin)
        {
            //isItemUnlock = DataManager.UserData.level % 5 == 1;
            isItemUnlock = false;
            img_newItemUnlock.gameObject.SetActive(isItemUnlock);
            levelChest.gameObject.SetActive(!isItemUnlock);
            img_ChestUnlocked.fillAmount = DataManager.UserData.LevelChesPercent * 1f / 100;
            txt_unlockValue.text = isItemUnlock ? $"New Item" : $"{DataManager.UserData.LevelChesPercent}%";
        }

        btnScaleStarClaim?.gameObject.SetActive(false);
        
        Status = UIAnimStatus.IsAnimationShow;
        if (anim.Status != UIAnimStatus.IsShow)
        {
            anim.Show(() =>
            {
                if (isWin)
                {
                    animWin.Show(null, () => {
                        OnShowResult(true);
                        lineRoullete.StartRoullete(OnStarScaleHandle);
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
                    lineRoullete.StartRoullete();
                });
            else
                animLose.Show(null, () => OnShowResult(false));
        }
    }
    public void OnShowResult(bool isWin)
    {
        Status = UIAnimStatus.IsShow;
        OnShowButton(isWin);
    }
    public void OnShowButton(bool isWin)
    {
        if (isWin)
        {
            btnStarClaim.interactable = true;
            btnScaleStarClaim.interactable = true;
            btnScaleStarClaim?.onClick.RemoveAllListeners();

            DataManager.Save();
            btnStarClaim?.gameObject.SetActive(false);
            DOVirtual.DelayedCall(timeDelayBtnBack, () => {
                btnStarClaim?.gameObject.SetActive(true);
                btnStarClaim?.onClick.RemoveAllListeners();
                btnStarClaim?.onClick.AddListener(() =>
                {
                    btnStarClaim.interactable = false;
                    btnScaleStarClaim.interactable = false;
                    lineRoullete.StopRoulelete();
                    StarManager.Add(GameStatisticsManager.starEarn, btnStarClaim.transform);
                    DOVirtual.DelayedCall(3f, () => Btn_Next_Handle());
                });
            });
            txtScaleStar.text = $"{GameStatisticsManager.goldEarn * bonusAds}";
            btnScaleStarClaim?.gameObject.SetActive(true);
            btnScaleStarClaim?.onClick.AddListener(() =>
            {
                btnStarClaim.interactable = false;
                btnScaleStarClaim.interactable = false;
                lineRoullete.StopRoulelete();
                AdsManager.ShowVideoReward((e, t) =>
                {
                    if (e == AdEvent.ShowSuccess)
                    {
                        StarManager.Add(GameStatisticsManager.starEarn * bonusAds, btnScaleStarClaim.transform);
                    }
                    DOVirtual.DelayedCall(3f, () => Btn_Next_Handle());
                }, "ClaimStarScale", "star");
            });
        }
    }
    public virtual void ShowContinue()
    {
        Status = UIAnimStatus.IsAnimationShow;
        rebornType = gameConfig.rebornType;

        rebornByFreeButton?.gameObject.SetActive(false);
        rebornByCoinButton?.gameObject.SetActive(false);
        rebornByDiamondButton?.gameObject.SetActive(false);
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
        //CoinManager.Add(GameStatisticsManager.goldEarn);
        rebornCount = 0;
        GameStateManager.Idle(null);
        Hide(() =>
        {
            CheckToShowInterstitialAds("Back", null);
        });
    }
    public void Btn_Restart_Handle()
    {
        rebornCount = 0;
        
        GameStateManager.Restart(null);
        Hide(() =>
        {
            CheckToShowInterstitialAds("Restart", null);
        });
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
        AdsManager.ShowVideoReward((e, t) =>
        {
            if (e == AdEvent.ShowSuccess)
            {
                Reborn();
            }
        }, "ContinueWithAds", "TimePlay");
    }
    public void Btn_Next_Handle()
    {
        Hide(() =>
        {
            rebornCount = 0;
            CheckToShowInterstitialAds("Next", null);

            //Check to show reward popup
            if(DataManager.UserData.LevelChesPercent >= 100)
            {
                popupReward.ShowLevelChestReward(DataManager.levelSelect * DataManager.GameConfig.coinRewardByLevel, DataManager.GameConfig.buffHintReward);
            }
            else
            {
                DataManager.levelSelect++;
                GameStateManager.Idle(null);
            }
        });
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
