using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupSelectMap : MonoBehaviour
{
    [SerializeField] private UIAnimation anim;
    [SerializeField] private Button btn_PlayChallenge;
    [SerializeField] private GameObject lockChallengeBtn;
    [SerializeField] private Button PlayBartenderBtn;
    [SerializeField] private GameObject lockBartenderBtn;

    [Header("Suggestion")]
    [SerializeField] GameObject suggestFadeImg = null;
    [SerializeField] GameObject suggestBartenderOb = null;
    // Start is called before the first frame update
    void Start()
    {
        btn_PlayChallenge?.onClick.AddListener(Ins_BtnChallengeClick);
        PlayBartenderBtn?.onClick.AddListener(BtnPlayBartenderClicked);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Show()
    {
        anim.Show();
        FetchData();
    }

    public void Hide()
    {
        anim.Hide();
    }
    public void FetchData()
    {
       // txt_LevelPlay.text = $"LV. {Mathf.Clamp(DataManager.UserData.level + 1, 1, DataManager.GameConfig.totalLevel - 1)}";

        btn_PlayChallenge?.gameObject.SetActive(DataManager.UserData.totalStar >= DataManager.GameConfig.starsToUnlockChallenge);
        lockChallengeBtn?.SetActive(DataManager.UserData.totalStar < DataManager.GameConfig.starsToUnlockChallenge);
        // txt_LockChallenge.text = $"UNLOCK AT LV.{DataManager.GameConfig.levelsToNextChallenge}";
        //btn_shopDecor?.gameObject.SetActive(DataManager.UserData.level >= DataManager.GameConfig.levelOpenShopDecor - 1);
        // lockShopDecorBtn?.SetActive(DataManager.UserData.level < DataManager.GameConfig.levelOpenShopDecor - 1);
        //txt_LockShopDecor.text = $"UNLOCK AT LV.{DataManager.GameConfig.levelOpenShopDecor}";
        lockBartenderBtn?.gameObject.SetActive(DataManager.UserData.totalStar < DataManager.GameConfig.starsToUnlockBartender);
        PlayBartenderBtn?.gameObject.SetActive(DataManager.UserData.totalStar >= DataManager.GameConfig.starsToUnlockBartender);
        //lockBartenderBtn?.SetActive(DataManager.UserData.level < DataManager.GameConfig.levelsToUnlockBartender - 1);
        //txt_LockBartender.text = $"Unlock at lv.{DataManager.GameConfig.levelsToUnlockBartender}";
        //txt_LockBartender.gameObject.SetActive(DataManager.UserData.level < DataManager.GameConfig.levelsToUnlockBartender - 1 && !DataManager.UserData.isModeBartenderSuguested);

        //btn_DailyReward?.Fill(DataManager.UserData.dailyRewardClaimCount == 0 || DataManager.UserData.lastdayClaimed.Day == System.DateTime.Now.Day - 1, BtnDailyRewardClick);

        //if (DataManager.UserData.level >= 5 && !DataManager.UserData.isModeBartenderSuguested)
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
    }
    public void Ins_BtnChallengeClick()
    {
        SoundManager.Play("1. Click Button");
        int lv = DataManager.UserData.challengeLevel;
        DataManager.currGameMode = eGameMode.Challenge;
        this.PostEvent((int)EventID.OnGoToChallengeLevel);
        Hide();
    }
    public void BtnPlayBartenderClicked()
    {
        SoundManager.Play("1. Click Button");
        if (!DataManager.UserData.isModeBartenderSuguested)
            this.PostEvent((int)EventID.OnModeBartenderUnlocked);
        else
        {
            DataManager.currGameMode = eGameMode.Bartender;
            GameStateManager.LoadGame(null);
        }
        Hide();
    }   
}
