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

    [Header("Home Buttons")]
    [SerializeField]
    private UIMainButton btn_StarChest;
    [SerializeField]
    private Text txt_StarCollected;
    [SerializeField]
    private Image img_starFill;
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
        txt_StarCollected.text = $"{DataManager.UserData.totalStar}/{DataManager.GameConfig.starCollectStage}";
        img_starFill.fillAmount = DataManager.UserData.totalStar * 1f / DataManager.GameConfig.starCollectStage;
        btn_StarChest.Fill(DataManager.UserData.totalStar >= DataManager.GameConfig.starCollectStage, BtnStarChestClick);
        txt_LevelPlay.text = $"LV. {DataManager.UserData.level + 1}";

        btn_PlayChallenge?.gameObject.SetActive(DataManager.UserData.level >= DataManager.GameConfig.levelsToNextChallenge - 1);

        btn_LockChallenge?.gameObject.SetActive(DataManager.UserData.level < DataManager.GameConfig.levelsToNextChallenge - 1);
        txt_LockChallenge.text = $"UNLOCK AT LV.{DataManager.GameConfig.levelsToNextChallenge}";
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
    private void BtnStarChestClick()
    {
        if (DataManager.GameConfig.isTestRewarPopup)
        {
            popupReward.ShowStarChestReward(DataManager.GameConfig.coinRewardByStarChest);
            return;
        }
        
        if (DataManager.UserData.totalStar < DataManager.GameConfig.starCollectStage)
            return;
        StarManager.Add(-DataManager.GameConfig.starCollectStage);
        popupReward.ShowStarChestReward(DataManager.GameConfig.coinRewardByStarChest);
    }

    public void Ins_BtnPlayClick()
    {
        Debug.Log($"Level data load: {DataManager.UserData.level + 1}");
        DataManager.levelSelect = DataManager.UserData.level + 1;
        GameStateManager.LoadGame(null);
    }

    private void Ins_BtnChallengeClick()
    {
        int lv = (DataManager.UserData.level + 1) - (DataManager.UserData.level + 1) % DataManager.GameConfig.levelsToNextChallenge;
        DataManager.levelSelect = lv;
        this.PostEvent((int)EventID.OnGoToChallengeLevel);
    }
    #endregion
}
