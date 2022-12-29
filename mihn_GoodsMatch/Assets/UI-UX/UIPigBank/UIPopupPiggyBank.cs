using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using Base.Ads;
using Spine.Unity;

public class UIPopupPiggyBank : MonoBehaviour
{
    [SerializeField]
    Text txt_BankValue;
    [SerializeField]
    Slider slider;
    [SerializeField]
    Button btn_Claim;
    [SerializeField]
    int[] stagesValues;
    [SerializeField]
    SkeletonGraphic pigSkeAnim;
    [SerializeField]
    Transform collectTf;

    int coinToWithdraw;

    private void Start()
    {
        btn_Claim?.onClick.AddListener(OnClaim);
        slider.value = 0;
        txt_BankValue.text = "0";
        btn_Claim.interactable = false;
    }

    private void OnDestroy()
    {
        btn_Claim?.onClick.RemoveListener(OnClaim);
    }

    public void Onshow()
    {
        string pigStateName = DataManager.UserData.totalBankCoin < DataManager.GameConfig.BankCoinStage.Last() ? "empty-idle" : "full-idle";
        SetPigSkin(pigStateName, false, true);
        txt_BankValue.DOText(0, DataManager.UserData.totalBankCoin, 1f);
        slider.value = 0;
        slider.DOValue(DataManager.UserData.totalBankCoin * slider.maxValue / DataManager.GameConfig.BankCoinStage.Last(), 1f);
        CheckWithdrawAmount();
    }

    private void CheckWithdrawAmount()
    {
        coinToWithdraw = DataManager.UserData.totalBankCoin >= DataManager.GameConfig.BankCoinStage.Last() ? DataManager.GameConfig.BankCoinStage.Last()
            : DataManager.UserData.totalBankCoin >= DataManager.GameConfig.BankCoinStage[0] ? DataManager.GameConfig.BankCoinStage[0] : 0;

        btn_Claim.interactable = coinToWithdraw > 0;
    }

    private void SetPigSkin(string name, bool isDelay = false, bool isloop = false)
    {
        pigSkeAnim.AnimationState.SetAnimation(0, name, isloop);
        pigSkeAnim.timeScale = isDelay ? 0 : 1;
    }

    public void OnClaim()
    {
        if (coinToWithdraw <= 0)
            return;
        AdsManager.ShowVideoReward((e, t) =>
        {
            if(e == AdEvent.ShowSuccess || DataManager.GameConfig.isAdsByPass)
            {
                if (coinToWithdraw == DataManager.GameConfig.BankCoinStage.Last())
                    SetPigSkin("full-withdraw");

                txt_BankValue.DOText(DataManager.UserData.totalBankCoin, DataManager.UserData.totalBankCoin - coinToWithdraw, 1f);
                CoinManager.Add(coinToWithdraw, collectTf);
                DataManager.UserData.totalBankCoin -= coinToWithdraw;
                slider.DOValue(DataManager.UserData.totalBankCoin * slider.maxValue / DataManager.GameConfig.BankCoinStage.Last(), 1f);

                CheckWithdrawAmount();
            }
            else
            {

            }
        }, "UIPopupPiggyBank_OnClaim");
    }
}