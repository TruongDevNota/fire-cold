using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using Base.Ads;

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
        btn_Claim.interactable = DataManager.UserData.totalBankCoin >= DataManager.GameConfig.BankCoinStage[0];
        txt_BankValue.DOText(0, DataManager.UserData.totalBankCoin, 1f);
        slider.value = 0;
        slider.DOValue(DataManager.UserData.totalBankCoin * slider.maxValue / DataManager.GameConfig.BankCoinStage.Last(), 1f);
    }

    public void OnClaim()
    {
        AdsManager.ShowVideoReward((e, t) =>
        {
            if(e == AdEvent.ShowSuccess || DataManager.GameConfig.isAdsByPass)
            {
                CoinManager.Add(DataManager.UserData.totalBankCoin);
                DataManager.UserData.totalBankCoin = 0;
            }
        }, "UIPopupPiggyBank_OnClaim");
    }
}
