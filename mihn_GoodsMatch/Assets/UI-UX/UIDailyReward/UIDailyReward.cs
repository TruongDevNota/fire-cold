using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Base.Ads;
using System;
using MyBox;

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
    private ItemDecorData item;

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
        anim.Show();
    }

    public void OnHide()
    {
        anim.Hide();
    }

    public void FetchData()
    {
        if ((DataManager.UserData.dailyRewardClaimCount == 7 && DataManager.UserData.lastdayClaimed.Day <= System.DateTime.Now.Day - 1))
            DataManager.UserData.dailyRewardClaimCount = 0;
        canClaim = DataManager.UserData.dailyRewardClaimCount == 0 || DataManager.UserData.lastdayClaimed.Day == System.DateTime.Now.Day - 1;


        
        if(DataManager.UserData.dailyRewardClaimCount < 6)
        {
            var rewards = dailyRewardAsset.GetDailyRewards(DataManager.UserData.dailyRewardClaimCount);
            coinEarn = rewards[0];
            buffHintEarn = rewards[1];
            buffSwapEarn = rewards[2];
        }
        else
        {
            var rewards = dailyRewardAsset.GetDaySevenRewardDeCor();
            item = rewards;
            coinEarn = 0;
            buffHintEarn = 0;
            buffSwapEarn = 0;
            if (rewards == null)
            {
               var rewards1 = dailyRewardAsset.GetDaySevenReward();
                coinEarn = rewards1[0];
                buffHintEarn = rewards1[1];
                buffSwapEarn = rewards1[2];
            }
        }
        btn_Claim.gameObject.SetActive(false);
        //btn_ClaimX2.gameObject.SetActive(canClaim);
        //timeLeftObj.gameObject.SetActive(!canClaim);

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
        foreach (var item in dailyItems)
            item.FillLayout();
    }
    #endregion

    private void OnClaimX2()
    {
        SoundManager.Play("1. Click Button");
        AdsManager.ShowVideoReward((e, t) =>
        {
            if(e == AdEvent.ShowSuccess)
            {
                coinEarn *= 2;
                buffHintEarn *= 2;
                buffSwapEarn *= 2;

                DOClaim();
            }
        }, "ScaleDailyReward");
    }

    public void OnClaim()
    {
        SoundManager.Play("1. Click Button");
        DOClaim();
    }

    private void DOClaim()
    {
        DataManager.UserData.dailyRewardClaimCount++;
        DataManager.UserData.lastdayClaimed = System.DateTime.Now;
        if(DataManager.UserData.dailyRewardClaimCount == 7)
        {
            popupReward.ShowDailyChestReward(item,coinEarn, buffHintEarn, buffSwapEarn,7);
        }else
            popupReward.ShowDailyChestReward(null,coinEarn, buffHintEarn, buffSwapEarn);

        foreach (var item in dailyItems)
            item.FillLayout();
    }
    [ButtonMethod]
    public void test()
    {
        DataManager.UserData.lastdayClaimed= System.DateTime.Now.AddDays(-1);
        OnShow();
    }
}
