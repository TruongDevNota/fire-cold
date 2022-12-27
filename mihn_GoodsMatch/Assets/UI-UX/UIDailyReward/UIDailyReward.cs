using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Base.Ads;

public class UIDailyReward : MonoBehaviour
{
    [SerializeField] DailyRewardAsset dailyRewardAsset;

    [SerializeField] UIAnimation anim;
    [SerializeField] UIPopupReward popupReward;
    [SerializeField] Button btn_Claim;
    [SerializeField] Button btn_ClaimX2;
    [SerializeField] Button btn_Close;
    [SerializeField] List<UIDailyRewardItem> dailyItems;

    bool canClaim = false;

    private int coinEarn;
    private int buffHintEarn;
    private int buffSwapEarn;

    private void Start()
    {
        btn_Claim?.onClick.AddListener(OnClaim);
        btn_ClaimX2.onClick.AddListener(OnClaimX2);
        btn_Close?.onClick.AddListener(OnHide);
    }

    public void OnShow()
    {
        FetchData();
        anim.Show(onStart: () => {
            btn_Claim.interactable = canClaim;
            btn_ClaimX2.interactable = canClaim;
        });
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
        ConvertRewardDatum(dailyRewardAsset.GetDailyRewardsByDayIndex(DataManager.UserData.dailyRewardClaimCount));
        foreach (var item in dailyItems)
            item.FillLayout();
    }

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
        btn_Claim.interactable = false;
        btn_ClaimX2.interactable = false;
        FetchData();
    }

    private void ConvertRewardDatum(DailyRewardDatum datum)
    {
        coinEarn = 0;
        buffHintEarn = 0;
        buffSwapEarn = 0;
        foreach(var reward in datum.rewards)
        {
            switch (reward.type)
            {
                case eRewardType.Gold:
                    coinEarn = reward.amount;
                    break;
                case eRewardType.BuffSwap:
                    buffSwapEarn = reward.amount;
                    break;
                case eRewardType.BuffHint:
                    buffHintEarn = reward.amount;
                    break;
            }
        }
    }
}
