using System.Collections;
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
    [SerializeField]
    private UIMainButton btn_DailyReward;
    [SerializeField]
    private UIMainButton btn_RemoveAds;
    [SerializeField]
    private UIMainButton btn_PiggyBank;
    [SerializeField]
    private Button btn_Play;
    [SerializeField]
    private Text txt_LevelPlay;
    [SerializeField]
    private Button btn_PlayChallenge;
    [SerializeField]
    private Button btn_LockChallenge;
    [SerializeField]
    Text txt_LockChallenge;

    void Awake()
    {
        Instance = this;
        anim = GetComponent<UIAnimation>();
    }

    private void Start()
    {
        btn_Play?.onClick.AddListener(Ins_BtnPlayClick);
        btn_PlayChallenge?.onClick.AddListener(Ins_BtnChallengeClick);
        
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    public void FetchData()
    {
        txt_LevelPlay.text = $"LV. {Mathf.Clamp(DataManager.UserData.level + 1, 1, DataManager.GameConfig.totalLevel-1)}";

        btn_PlayChallenge?.gameObject.SetActive(DataManager.UserData.level >= DataManager.GameConfig.levelsToNextChallenge - 1);

        btn_LockChallenge?.gameObject.SetActive(DataManager.UserData.level < DataManager.GameConfig.levelsToNextChallenge - 1);
        txt_LockChallenge.text = $"UNLOCK AT LV.{DataManager.GameConfig.levelsToNextChallenge}";

        btn_DailyReward?.Fill(DataManager.UserData.dailyRewardClaimCount == 0 || DataManager.UserData.lastdayClaimed.Day == System.DateTime.Now.Day - 1, BtnDailyRewardClick);
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
        Debug.Log($"Level data load: {DataManager.UserData.level + 1}");
        DataManager.levelSelect = Mathf.Clamp(DataManager.UserData.level % DataManager.GameConfig.totalLevel + 1, 1, DataManager.GameConfig.totalLevel - 1);
        GameStateManager.LoadGame(null);
    }

    private void Ins_BtnChallengeClick()
    {
        SoundManager.Play("1. Click Button");
        int lv = (DataManager.UserData.level % DataManager.GameConfig.totalLevel + 1) - (DataManager.UserData.level % DataManager.GameConfig.totalLevel + 1) % DataManager.GameConfig.levelsToNextChallenge;
        DataManager.levelSelect = lv;
        this.PostEvent((int)EventID.OnGoToChallengeLevel);
    }
    #endregion
}
