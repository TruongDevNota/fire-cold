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
    [SerializeField] Button btn_PlayFree;

    private void Start()
    {
        btn_PlayWithCoin?.onClick.AddListener(BtnPlayWithCoinClick);
        btn_PlayWithAds?.onClick.AddListener(BtnPlayWithAdsClick);
        btn_Close?.onClick.AddListener(OnSkip);
        btn_PlayFree?.onClick.AddListener(BtnPlayFreeClick);
        this.RegisterListener((int)EventID.OnGoToChallengeLevel, OnShow);
    }

    public void OnShow(object obj)
    {
        txt_CoinPrice.text = DataManager.GameConfig.playChallengeCoinUse.ToString();
        btn_Close.gameObject.SetActive(false);
        btn_PlayWithAds?.gameObject.SetActive(DataManager.UserData.isChallengePlayed);
        btn_PlayWithCoin?.gameObject.SetActive(DataManager.UserData.isChallengePlayed);
        btn_PlayFree?.gameObject.SetActive(!DataManager.UserData.isChallengePlayed);
        anim.Show(null, onCompleted: () =>
        {
            btn_PlayWithCoin.interactable = DataManager.UserData.totalCoin >= DataManager.GameConfig.playChallengeCoinUse;
            if(DataManager.UserData.isChallengePlayed)
                DOVirtual.DelayedCall(2f, () => btn_Close.gameObject.SetActive(true));
        });
    }

    private void BtnPlayWithCoinClick()
    {
        SoundManager.Play("1. Click Button");
        CoinManager.Add(-DataManager.GameConfig.playChallengeCoinUse);
        DataManager.currGameMode = eGameMode.Normal;
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
                DataManager.currGameMode = eGameMode.Normal;
                GameStateManager.LoadGame(true);
                anim.Hide();
            }
        }, "PlayChallenge");
    }

    private void BtnPlayFreeClick()
    {
        SoundManager.Play("1. Click Button");
        DataManager.currGameMode = eGameMode.Normal;
        DataManager.UserData.isChallengePlayed = true;
        GameStateManager.LoadGame(true);
        DataManager.Save();
        anim.Hide();
    }

    private void OnSkip()
    {
        anim.Hide();
        if (GameStateManager.CurrentState == GameState.Idle)
            return;
        if (DataManager.levelSelect < DataManager.GameConfig.totalLevel && DataManager.levelSelect <= DataManager.UserData.level + 1)
        {
            DataManager.levelSelect++;
            DataManager.currGameMode = eGameMode.Normal;
            GameStateManager.LoadGame(null);
        }
        else
            GameStateManager.Idle(null);
    }
}
