﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using UnityEngine.UI;
using System;

public class UIMainScreen : MonoBehaviour
{
    public static UIMainScreen Instance;
    public UIAnimStatus Status => anim.Status;
    private UIAnimation anim;

    [SerializeField]
    private UIPopupReward popupReward;
    [SerializeField]
    private UIDailyReward popupDailyReward;

    [Header("Home Buttons")]
    [SerializeField] private UIMainButton btn_DailyReward;
    [SerializeField] private UIMainButton btn_RemoveAds;
    //[SerializeField] private UIMainButton btn_PiggyBank;
    [SerializeField] private Button btn_Play;
    [SerializeField] private Text txt_LevelPlay;
    [SerializeField] private Button btn_PlayChallenge;
    [SerializeField] private GameObject lockChallengeBtn;
    [SerializeField] Text txt_LockChallenge;
    [SerializeField] Text txt_LockShopDecor;
    [SerializeField] Button PlayBartenderBtn;
    [SerializeField] Text txt_LockBartender;

    [Header("Suggestion")]
    [SerializeField] GameObject suggestFadeImg = null;
    [SerializeField] GameObject suggestBartenderOb = null;

    [SerializeField] Text infoLevelPlaying;

    void Awake()
    {
        Instance = this;
        anim = GetComponent<UIAnimation>();
    }

    private void Start()
    {
        btn_Play?.onClick.AddListener(Ins_BtnPlayClick);
        btn_PlayChallenge?.onClick.AddListener(Ins_BtnChallengeClick);
        PlayBartenderBtn?.onClick.AddListener(BtnPlayBartenderClicked);
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    public void FetchData()
    {
        //txt_LevelPlay.text = $"LV. {Mathf.Clamp(DataManager.UserData.level + 1, 1, DataManager.GameConfig.totalLevel-1)}";

        //btn_PlayChallenge?.gameObject.SetActive(DataManager.UserData.level >= DataManager.GameConfig.starsToNextChallenge - 1);
        //lockChallengeBtn?.SetActive(DataManager.UserData.level < DataManager.GameConfig.levelsToNextChallenge - 1);
        //txt_LockChallenge.text = $"UNLOCK AT LV.{DataManager.GameConfig.levelsToNextChallenge}";
        txt_LockShopDecor.text = $"LV.{DataManager.GameConfig.levelOpenShopDecor}";

        //PlayBartenderBtn?.gameObject.SetActive(true);
        //lockBartenderBtn?.SetActive(DataManager.UserData.level < DataManager.GameConfig.levelsToUnlockBartender - 1);
       // txt_LockBartender.text = $"Unlock at lv.{DataManager.GameConfig.levelsToUnlockBartender}";
        //txt_LockBartender.gameObject.SetActive(DataManager.UserData.level < DataManager.GameConfig.levelsToUnlockBartender - 1 && !DataManager.UserData.isModeBartenderSuguested);

        btn_DailyReward?.Fill(DataManager.UserData.dailyRewardClaimCount == 0 || DataManager.UserData.lastdayClaimed.Day == System.DateTime.Now.Day - 1, BtnDailyRewardClick);

        //if(DataManager.UserData.level >= 5 && !DataManager.UserData.isModeBartenderSuguested)
        //{
        //    suggestFadeImg?.SetActive(true);
        //    suggestBartenderOb.SetActive(true);
        //    DataManager.UserData.isModeBartenderSuguested = true;
        //    DataManager.Save();
        //}
        //else
        //{
        //    suggestFadeImg?.SetActive(false);
        //    suggestBartenderOb.SetActive(false);
        //}
        if (DataManager.MapAsset.ListMap[DataManager.UserData.lastMapIndexSelected - 1].hightestLevelUnlocked >= 2)
        {
            infoLevelPlaying.gameObject.SetActive(true);
            infoLevelPlaying.text = $"{DataManager.MapAsset.ListMap[DataManager.UserData.lastMapIndexSelected - 1].mapName} - Level {DataManager.MapAsset.ListMap[DataManager.UserData.lastMapIndexSelected - 1].hightestLevelUnlocked}";
        }
            
        else
            infoLevelPlaying.gameObject.SetActive(false);
    }

    public void Show(TweenCallback onStart = null, TweenCallback onCompleted = null)
    {
        //start
        FetchData();
        anim.Show(onStart, () =>
        {
            onCompleted?.Invoke();
        });
    }

    public void Hide()
    {
        anim.Hide();
    }

    #region ButtonHandle
    private void BtnDailyRewardClick()
    {
        SoundManager.Play("1. Click Button");
        popupDailyReward.OnShow();
    }
    public void Ins_BtnPlayClick()
    {
        SoundManager.Play("1. Click Button");
        DataManager.mapSelect = DataManager.UserData.lastMapIndexSelected;
        if (!DataManager.UserData.isMapSelectShowed&&
            DataManager.MapAsset.ListMap[DataManager.mapSelect-1].hightestLevelUnlocked== DataManager.MapAsset.ListMap[DataManager.mapSelect - 1].totalLevel)
        {
            DataManager.UserData.isMapSelectShowed = true;
            GameUIManager.PopupMapSelect.Show();
        }
        else
        {
            DataManager.levelSelect = DataManager.MapAsset.ListMap[DataManager.UserData.lastMapIndexSelected - 1].hightestLevelUnlocked;
            GameStateManager.LoadGame(null);
        }
            

    }
    public void Ins_BtnChallengeClick()
    {
        //SoundManager.Play("1. Click Button");
        //int lv = (DataManager.UserData.level % DataManager.GameConfig.totalLevel + 1) - (DataManager.UserData.level % DataManager.GameConfig.totalLevel + 1) % DataManager.GameConfig.starsToNextChallenge;
        //DataManager.levelSelect = lv;
        //DataManager.currGameMode = eGameMode.Normal;
        //this.PostEvent((int)EventID.OnGoToChallengeLevel);
    }
    public void BtnPlayBartenderClicked()
    {
        //SoundManager.Play("1. Click Button");
        //if (!DataManager.UserData.isModeBartenderSuguested)
        //    this.PostEvent((int)EventID.OnModeBartenderUnlocked);
        //else
        //{
        //    DataManager.currGameMode = eGameMode.Bartender;
        //    GameStateManager.LoadGame(null);
        //}
    }
    public void Ins_BtnLuckySpinClick()
    {
        SoundManager.Play("1. Click Button");
        if (GameStateManager.CurrentState == GameState.Idle)
        {
            GameStateManager.LuckyWheel(null);
        }
    }
    public void BtnShopDecorClicked()
    {
        SoundManager.Play("1. Click Button");
        if (GameStateManager.CurrentState == GameState.Idle)
        {
            GameStateManager.InShop(null);
        }
    }
    public void OnButtonHouseClicked()
    {
        SoundManager.Play("1. Click Button");
        GameStateManager.HouseDecoration(null);
    }
    public void BtnSelectLevel()
    {
        GameUIManager.PopupLevelSelect.OnShow(DataManager.UserData.lastMapIndexSelected);
        SoundManager.Play("1. Click Button");
    }
    #endregion
}
