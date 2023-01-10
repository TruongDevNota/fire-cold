using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Base.Ads;

public class UIPopupChallenge : MonoBehaviour
{
    [SerializeField] UIAnimation anim;
    [SerializeField] Button btn_PlayWithCoin;
    [SerializeField] Text txt_CoinPrice;
    [SerializeField] Button btn_PlayWithAds;
    [SerializeField] Button btn_Close;

    private void Start()
    {
        btn_PlayWithCoin?.onClick.AddListener(BtnPlayWithCoinClick);
        btn_PlayWithAds?.onClick.AddListener(BtnPlayWithAdsClick);
        btn_Close?.onClick.AddListener(OnSkip);
        this.RegisterListener((int)EventID.OnGoToChallengeLevel, OnShow);
    }

    public void OnShow(object obj)
    {
        txt_CoinPrice.text = DataManager.GameConfig.playChallengeCoinUse.ToString();
        btn_Close.gameObject.SetActive(false);
        anim.Show(null, onCompleted: () =>
        {
            btn_PlayWithCoin.interactable = DataManager.UserData.totalCoin >= DataManager.GameConfig.playChallengeCoinUse;
            DOVirtual.DelayedCall(2f, () => btn_Close.gameObject.SetActive(true));
        });
    }

    private void BtnPlayWithCoinClick()
    {
        SoundManager.Play("1. Click Button");
        CoinManager.Add(-DataManager.GameConfig.playChallengeCoinUse);
        GameStateManager.LoadGame(true);
        anim.Hide();
    }

    private void BtnPlayWithAdsClick()
    {
        SoundManager.Play("1. Click Button");
        AdsManager.ShowVideoReward((e, t) =>
        {
            if(e == AdEvent.ShowSuccess || DataManager.GameConfig.isAdsByPass)
            {
                GameStateManager.LoadGame(true);
                anim.Hide();
            }
        }, "PlayChallenge");
    }

    private void OnSkip()
    {
        anim.Hide();
        if (DataManager.levelSelect >= 21 || DataManager.levelSelect % 2 == 0)
        {
            AdsManager.ShowInterstitial((s, adType) =>
            {
                UIToast.Hide();
            }, name, "GiveUpChallenge");
        } 
        if (GameStateManager.CurrentState != GameState.Idle)
            GameStateManager.Idle(null);
    }
}
