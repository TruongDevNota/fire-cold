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

    void Awake()
    {
        Instance = this;
        anim = GetComponent<UIAnimation>();
    }

    private void Start()
    {
        
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

    public void Ins_BtnPlayClick()
    {
        if (GameStateManager.CurrentState == GameState.Idle)
            GameStateManager.LoadGame(null);
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
#endregion
}
