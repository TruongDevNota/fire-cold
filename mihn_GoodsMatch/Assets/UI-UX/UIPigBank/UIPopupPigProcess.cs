using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using Base.Ads;
using System;

public class UIPopupPigProcess : MonoBehaviour
{
    [SerializeField]
    private UIAnimation anim;
    [SerializeField]
    private Text txt_coinSave;
    [SerializeField]
    private Slider slider;
    [SerializeField]
    private Text txt_MinStage;
    [SerializeField]
    private Text txt_MaxStage;
    [SerializeField]
    private Button btn_Continue;
    [SerializeField]
    SkeletonGraphic pigSkeAnim;

    public void OnShow(int coinNumber)
    {
        txt_coinSave.text = coinNumber > 0 ? "0" : "MAX";
        txt_MinStage.text = DataManager.GameConfig.BankCoinStage[0].ToString();
        txt_MaxStage.text = DataManager.GameConfig.BankCoinStage.Last().ToString();
        float lastvalue = DataManager.UserData.totalBankCoin * slider.maxValue / DataManager.GameConfig.BankCoinStage.Last();

        slider.value = lastvalue;
        btn_Continue.gameObject.SetActive(false);
        btn_Continue.onClick.RemoveAllListeners();

        string idleName = DataManager.UserData.totalBankCoin >= DataManager.GameConfig.BankCoinStage.Last() ? "full-idle" : "empty-idle";
        SetPigSkin(idleName);

        anim.Show(null, () =>
        {
            if (coinNumber > 0)
            {
                DataManager.UserData.totalBankCoin += coinNumber;
                txt_coinSave.DOText(0, coinNumber, 1f);
                slider.DOValue(DataManager.UserData.totalBankCoin * slider.maxValue / DataManager.GameConfig.BankCoinStage.Last(), 1f);
                SetPigSkin("empty-deposit");
                DOVirtual.DelayedCall(3f, () =>
                {
                    btn_Continue.gameObject.SetActive(true);
                    btn_Continue.onClick.AddListener(BtnContinueClick);
                    if (DataManager.UserData.totalBankCoin >= DataManager.GameConfig.BankCoinStage.Last())
                        SetPigSkin("full-idle", false, true);
                    else
                        SetPigSkin("empty-idle", false, true);
                });
            }
        });
    }

    private void SetPigSkin(string name, bool isDelay = false, bool isloop = false)
    {
        pigSkeAnim.AnimationState.SetAnimation(0, name, isloop);
        pigSkeAnim.timeScale = isDelay ? 0 : 1;
    }

    private void BtnContinueClick()
    {
        if (GameStateManager.CurrentState == GameState.Idle)
            return;
        SoundManager.Play("1. Click Button");

        if (DataManager.levelSelect > DataManager.GameConfig.totalLevel)
        {
            DataManager.levelSelect = DataManager.GameConfig.totalLevel;
            anim.Hide();
            GameStateManager.Idle(null);
            return;
        }


        DataManager.currLevelconfigData.config.gameMode = eGameMode.Normal;
        GameStateManager.LoadGame(null);

        anim.Hide();
    }
}
