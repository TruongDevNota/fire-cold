using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(UIAnimation))]
public class RewardPopUp : MonoBehaviour
{
    [Serializable]
    public class WinRewardDatum
    {
        public int amount;
        public SkinData rewardSkinData;
    }

    [SerializeField]
    private UIAnimation anim = null;
    [SerializeField]
    private Image rewardImg = null;
    [SerializeField]
    private Text rewardTxtValue = null;

    [Header("Buttons")]
    [SerializeField] GameObject freeClaimGroup;
    [SerializeField] GameObject watchVideoToClaimGroup;

    [SerializeField] Button claimByWatchingVideoButton;
    [SerializeField] Button noThanksButton;

    [SerializeField]
    private Button x2RewardBtn = null;
    [SerializeField]
    private Button claimRewardBtn = null;

    private LuckySpinReward luckyReward = null;
    private GiftBoxReward giftBox = null;
    private WinRewardDatum winRewardDatum = null;
    //private RandomGift randomGift = null;
    private void Awake()
    {
        this.RegisterListener((int)EventID.OnShowRewardPopup, OnShow);
    }

    private void OnDestroy()
    {
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnShowRewardPopup, OnShow);
    }

    private void OnShow(object obj)
    {
        luckyReward = null;
        giftBox = null;
        winRewardDatum = null;

        anim.Show(() =>
        {
            Configure(obj);
        });
    }
    private int luckyType = 0;
    private void Configure(object obj)
    {
        freeClaimGroup.SetActive(!(obj is WinRewardDatum));
        watchVideoToClaimGroup.SetActive(obj is WinRewardDatum);

        if (obj is WinRewardDatum)
        {
            luckyType = 0;
            winRewardDatum = (WinRewardDatum)obj;
            x2RewardBtn.gameObject.SetActive(false);
            rewardImg.sprite = winRewardDatum.rewardSkinData.main;
            rewardImg.SetNativeSize();
            rewardTxtValue.gameObject.SetActive(false);
        }
        else if (obj is LuckySpinReward)
        {
            var reward = (LuckySpinReward)obj;
            if (reward != null)
            {
                luckyType = 1;
                luckyReward = reward;
                x2RewardBtn.gameObject.SetActive(reward.rewardsTypes == LuckyRewardsTypes.Coins);
                rewardImg.sprite = /*reward.tmpRewardSkin != null ? reward.tmpRewardSkin.main :*/ reward.rewardSpriteIcon;
                rewardTxtValue.text = $"{reward.rewardAmount}";
                rewardTxtValue.gameObject.SetActive(reward.rewardsTypes == LuckyRewardsTypes.Coins);
                claimRewardBtn.onClick.RemoveAllListeners();
                claimRewardBtn.onClick.AddListener(() =>
                {
                    Base.Ads.AdsManager.ShowInterstitial((a,e)=> { },"claim_lucky_spin_reward");
                    HandleClaimSpinReward();
                });
            }
        }
        else if (obj is GiftBoxReward)
        {
            var reward = (GiftBoxReward)obj;
            if (reward != null)
            {
                luckyType = 2;
                giftBox = reward;
                x2RewardBtn.gameObject.SetActive(reward.rewardsTypes == LuckyRewardsTypes.Coins);
                rewardImg.sprite =/* reward.tmpRewardSkin != null ? reward.tmpRewardSkin.main :*/ reward.rewardSpriteIcon;
                rewardImg.SetNativeSize();
                rewardTxtValue.text = $"{reward.rewardAmount}";
                rewardTxtValue.gameObject.SetActive(reward.rewardsTypes == LuckyRewardsTypes.Coins);
                claimRewardBtn.onClick.RemoveAllListeners();
                claimRewardBtn.onClick.AddListener(() =>
                {
                    //Base.Ads.AdsManager.ShowInterstitial((a,e)=> { },"claim_gift_box_reward");
                    HandleClaimGiftReward();
                });
            }
        }
        //else if (obj is RandomGift)
        //{
        //    var reward = (RandomGift)obj;
        //    if (reward != null)
        //    {
        //        luckyType = 3;
        //        randomGift = reward;
        //        x2RewardBtn.gameObject.SetActive(reward.rewardsTypes == LuckyRewardsTypes.Coins);
        //        rewardImg.sprite =/* reward.tmpRewardSkin != null ? reward.tmpRewardSkin.main :*/ reward.rewardSpriteIcon;

        //        rewardTxtValue.text = $"{reward.rewardAmount}";
        //        rewardTxtValue.gameObject.SetActive(reward.rewardsTypes == LuckyRewardsTypes.Coins);

        //        claimRewardBtn.onClick.RemoveAllListeners();
        //        claimRewardBtn.onClick.AddListener(() =>
        //        {
        //            //Base.Ads.AdsManager.ShowInterstitial((a,e)=> { },"claim_random_box_reward");
        //            HandleClaimRandomGift();
        //        });
        //    }
        //}
    }

    public void Ins_BtnCloseClick()
    {
        anim.Hide();
    }

    private void HandleClaimSkinReward()
    {
        DataManager.CurrentSkin = winRewardDatum.rewardSkinData;

        anim.Hide();
    }

    public void HandleClaimSpinReward()
    {
        //UIToast.ShowNotice($"Claim: {luckyReward.rewardsTypes} {luckyReward.rewardSpriteIcon}");

        this.PostEvent((int)EventID.OnClaimReward, luckyReward);

        switch (luckyReward.rewardsTypes)
        {
            case LuckyRewardsTypes.Coins:
                CoinManager.Add(luckyReward.rewardAmount);
                if (luckyReward.rewardAmount > 200)
                {
                    Base.Ads.AdsManager.ShowInterstitial((a, t) => { }, $"Claim_Coin_{luckyReward.rewardAmount}");
                }
                break;
            case LuckyRewardsTypes.Wall:
                luckyReward.tmpRewardSkin.isUnlocked = true;
                DataManager.SkinsAsset.Current = luckyReward.tmpRewardSkin;
                DataManager.Save();
                break;
            case LuckyRewardsTypes.Floor:
                luckyReward.tmpRewardFloor.isUnlocked = true;
                DataManager.FloorAsset.Current = luckyReward.tmpRewardFloor;
                DataManager.Save();
                break;
            case LuckyRewardsTypes.Windows:
                luckyReward.tmpRewardWindows.isUnlocked = true;
                DataManager.WindowAsset.Current = luckyReward.tmpRewardWindows;
                DataManager.Save();
                break;
            case LuckyRewardsTypes.Carpet:
                luckyReward.tmpRewardCarpet.isUnlocked = true;
                DataManager.CarpetsAsset.Current = luckyReward.tmpRewardCarpet;
                DataManager.Save();
                break;
            case LuckyRewardsTypes.Ceilling:
                luckyReward.tmpRewardCeilling.isUnlocked = true;
                DataManager.CeillingAsset.Current = luckyReward.tmpRewardCeilling;
                DataManager.Save();
                break;
            case LuckyRewardsTypes.Chair:
                luckyReward.tmpRewardChair.isUnlocked = true;
                DataManager.ChairsAsset.Current = luckyReward.tmpRewardChair;
                DataManager.Save();
                break;
            case LuckyRewardsTypes.Table:
                luckyReward.tmpRewardTable.isUnlocked = true;
                DataManager.TableAssets.Current = luckyReward.tmpRewardTable;
                DataManager.Save();
                break;
            case LuckyRewardsTypes.Lamp:
                luckyReward.tmpRewardLamp.isUnlocked = true;
                DataManager.LampsAsset.Current = luckyReward.tmpRewardLamp;
                DataManager.Save();
                break;
        }

        anim.Hide();
    }
    public void HandleClaimGiftReward()
    {
        //UIToast.ShowNotice("Claim Gift Reward");
        this.PostEvent((int)EventID.OnClaimGiftBox, giftBox);

        switch (giftBox.rewardsTypes)
        {
            case LuckyRewardsTypes.Coins:
                CoinManager.Add(giftBox.rewardAmount);
                break;
            case LuckyRewardsTypes.Wall:
                giftBox.tmpRewardSkin.isUnlocked = true;
                DataManager.SkinsAsset.Current = giftBox.tmpRewardSkin;
                DataManager.Save();
                break;
            case LuckyRewardsTypes.Floor:
                giftBox.tmpRewardFloor.isUnlocked = true;
                DataManager.FloorAsset.Current = giftBox.tmpRewardFloor;
                DataManager.Save();
                break;
            case LuckyRewardsTypes.Windows:
                giftBox.tmpRewardWindows.isUnlocked = true;
                DataManager.WindowAsset.Current = giftBox.tmpRewardWindows;
                DataManager.Save();
                break;
            case LuckyRewardsTypes.Carpet:
                giftBox.tmpRewardCarpet.isUnlocked = true;
                DataManager.CarpetsAsset.Current = giftBox.tmpRewardCarpet;
                DataManager.Save();
                break;
            case LuckyRewardsTypes.Ceilling:
                giftBox.tmpRewardCeilling.isUnlocked = true;
                DataManager.CeillingAsset.Current = giftBox.tmpRewardCeilling;
                DataManager.Save();
                break;
            case LuckyRewardsTypes.Chair:
                giftBox.tmpRewardChair.isUnlocked = true;
                DataManager.ChairsAsset.Current = giftBox.tmpRewardChair;
                DataManager.Save();
                break;
            case LuckyRewardsTypes.Table:
                giftBox.tmpRewardTable.isUnlocked = true;
                DataManager.TableAssets.Current = giftBox.tmpRewardTable;
                DataManager.Save();
                break;
            case LuckyRewardsTypes.Lamp:
                giftBox.tmpRewardLamp.isUnlocked = true;
                DataManager.LampsAsset.Current = giftBox.tmpRewardLamp;
                DataManager.Save();
                break;
        }

        anim.Hide();
    }
    ////public void HandleClaimRandomGift()
    //{
    //    this.PostEvent((int)EventID.OnClaimGiftBox, randomGift);

    //    switch (randomGift.rewardsTypes)
    //    {
    //        case LuckyRewardsTypes.Coins:
    //            CoinManager.Add(randomGift.rewardAmount);
    //            break;
    //        case LuckyRewardsTypes.Wall:
    //            randomGift.skinData.isUnlocked = true;
    //            DataManager.SkinsAsset.Current = randomGift.skinData;
    //            DataManager.Save();
    //            break;
    //        case LuckyRewardsTypes.Floor:
    //            randomGift.floorData.isUnlocked = true;
    //            DataManager.FloorAsset.Current = randomGift.floorData;
    //            DataManager.Save();
    //            break;
    //        case LuckyRewardsTypes.Windows:
    //            randomGift.windowData.isUnlocked = true;
    //            DataManager.WindowAsset.Current = randomGift.windowData;
    //            DataManager.Save();
    //            break;
    //        case LuckyRewardsTypes.Carpet:
    //            randomGift.carpetData.isUnlocked = true;
    //            DataManager.CarpetsAsset.Current = randomGift.carpetData;
    //            DataManager.Save();
    //            break;
    //        case LuckyRewardsTypes.Ceilling:
    //            randomGift.ceillingData.isUnlocked = true;
    //            DataManager.CeillingAsset.Current = randomGift.ceillingData;
    //            DataManager.Save();
    //            break;
    //        case LuckyRewardsTypes.Chair:
    //            randomGift.chairData.isUnlocked = true;
    //            DataManager.ChairsAsset.Current = randomGift.chairData;
    //            DataManager.Save();
    //            break;
    //        case LuckyRewardsTypes.Table:
    //            randomGift.tableData.isUnlocked = true;
    //            DataManager.TableAssets.Current = randomGift.tableData;
    //            DataManager.Save();
    //            break;
    //        case LuckyRewardsTypes.Lamp:
    //            randomGift.lampData.isUnlocked = true;
    //            DataManager.LampsAsset.Current = randomGift.lampData;
    //            DataManager.Save();
    //            break;
    //    }

    //    anim.Hide();
    //}
    public void Ins_BtnX2Click()
    {
        Base.Ads.AdsManager.ShowVideoReward((e, t) =>
        {
            if (e == AdEvent.ShowSuccess)
            {
                rewardTxtValue.text = $"{luckyReward.rewardAmount * 3}";
                if (luckyType == 1)
                {
                    var doubleReward = luckyReward;
                    doubleReward.rewardAmount *= 3;
                    this.PostEvent((int)EventID.OnX2Coin, doubleReward.rewardAmount);
                    CoinManager.Add(doubleReward.rewardAmount, rewardImg.transform);
                }
                else if (luckyType == 2)
                {
                    var doubleReward = giftBox;
                    doubleReward.rewardAmount *= 3;
                    this.PostEvent((int)EventID.OnX2Coin, doubleReward.rewardAmount);
                    CoinManager.Add(doubleReward.rewardAmount, rewardImg.transform);
                }

            }
            Base.Ads.AdsManager.ShowNotice(e);
        }, "x2_lucky_coin");
        anim.Hide();
    }

    public void Ins_ClaimByWatchingVideo()
    {
        Base.Ads.AdsManager.ShowVideoReward((e, t) =>
        {
            if (e == AdEvent.ShowSuccess)
            {
                DataManager.CurrentSkin = winRewardDatum.rewardSkinData;
            }
            Base.Ads.AdsManager.ShowNotice(e);
        }, "win_claim_reward");

        anim.Hide();
    }

    public void Ins_NoThanks()
    {
        anim.Hide();
    }
}
