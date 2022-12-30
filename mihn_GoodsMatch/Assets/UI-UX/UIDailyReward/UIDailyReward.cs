using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Base.Ads;
using System;

public class UIDailyReward : MonoBehaviour
{
    [SerializeField] DailyRewardAsset dailyRewardAsset;

    [SerializeField] UIAnimation anim;
    [SerializeField] UIPopupReward popupReward;
    [SerializeField] Button btn_Claim;
    [SerializeField] Button btn_ClaimX2;
    [SerializeField] Button btn_Close;
    [SerializeField] List<UIDailyRewardItem> dailyItems;

    [SerializeField] GameObject timeLeftObj;
    [SerializeField] Text txt_timeLeft;
    DateTime endTime;
    TimeSpan remainTime;
    Coroutine countDownCoroutine;
    private bool isCountDown = false;


    bool canClaim = false;

    private int coinEarn;
    private int buffHintEarn;
    private int buffSwapEarn;

    private void OnDisable()
    {
        if (countDownCoroutine != null)
            StopCoroutine(countDownCoroutine);
    }

    private void Start()
    {
        btn_Claim?.onClick.AddListener(OnClaim);
        btn_ClaimX2.onClick.AddListener(OnClaimX2);
        btn_Close?.onClick.AddListener(OnHide);
    }

    public void OnShow()
    {
        FetchData();
        if(anim.Status == UIAnimStatus.IsShow)
            ShowCountDown();
        else
        {
            anim.Show(onStart: () => {
                btn_Claim.interactable = canClaim;
                btn_ClaimX2.interactable = canClaim;
                if (!canClaim)
                    ShowCountDown();
            });
        }
    }

    public void OnHide()
    {
        anim.Hide();
    }

    public void FetchData()
    {
        if ((DataManager.UserData.dailyRewardClaimCount > 0 && DataManager.UserData.lastdayClaimed.Day < System.DateTime.Now.Day - 1) ||
            (DataManager.UserData.dailyRewardClaimCount == 7 && DataManager.UserData.lastdayClaimed.Day < System.DateTime.Now.Day - 1))
            DataManager.UserData.dailyRewardClaimCount = 0;
        canClaim = DataManager.UserData.dailyRewardClaimCount == 0 || DataManager.UserData.lastdayClaimed.Day == System.DateTime.Now.Day - 1;

        var rewards = DataManager.UserData.dailyRewardClaimCount < 6 ? dailyRewardAsset.GetDailyRewards() : dailyRewardAsset.GetDaySevenReward();

        coinEarn = rewards[0];
        buffHintEarn = rewards[1];
        buffSwapEarn = rewards[2];

        btn_Claim.gameObject.SetActive(canClaim);
        btn_ClaimX2.gameObject.SetActive(canClaim);
        timeLeftObj.gameObject.SetActive(!canClaim);

        foreach (var item in dailyItems)
            item.FillLayout();
    }

    #region CountDown

    public void ShowCountDown()
    {
        DateTime nextDay = DateTime.Now.AddDays(1);
        endTime = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, 0, 0, 0);
        remainTime = endTime.Subtract(DateTime.Now);
        isCountDown = true;
        if (countDownCoroutine != null)
            StopCoroutine(countDownCoroutine);
        countDownCoroutine = StartCoroutine(YieldCountDown());
    }

    private IEnumerator YieldCountDown()
    {
        while (isCountDown)
        {
            txt_timeLeft.text = remainTime.ToString("hh':'mm':'ss");
            yield return new WaitForSeconds(1);
            remainTime = remainTime.Subtract(TimeSpan.FromSeconds(1d));
            if (remainTime <= TimeSpan.Zero)
            {
                OnStopCountDown();
            }
        }
    }

    private void OnStopCountDown()
    {
        isCountDown = false;
        txt_timeLeft.text = "--:--:--";
        OnShow();
    }
    #endregion


    private void OnClaimX2()
    {
        AdsManager.ShowVideoReward((e, t) =>
        {
            if(e == AdEvent.ShowSuccess)
            {
                coinEarn *= 2;
                buffHintEarn *= 2;
                buffSwapEarn *= 2;

                OnClaim();
            }
        }, "ScaleDailyReward");
    }

    private void OnClaim()
    {
        DataManager.UserData.dailyRewardClaimCount++;
        DataManager.UserData.lastdayClaimed = System.DateTime.Now;
        popupReward.ShowDailyChestReward(coinEarn, buffHintEarn, buffSwapEarn);
        //btn_Claim.interactable = false;
        //btn_ClaimX2.interactable = false;
        //btn_Claim.gameObject.SetActive(false);
        //btn_ClaimX2.gameObject.SetActive(false);
        //FetchData();
        OnShow();
    }
}
