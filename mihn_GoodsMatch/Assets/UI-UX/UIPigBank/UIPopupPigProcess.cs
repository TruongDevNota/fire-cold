using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

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
    
    public void OnShow(int coinNumber)
    {
        txt_coinSave.text = coinNumber > 0 ? "0" : "MAX";
        txt_MinStage.text = DataManager.GameConfig.BankCoinStage[0].ToString();
        txt_MaxStage.text = DataManager.GameConfig.BankCoinStage.Last().ToString();
        float lastvalue = DataManager.UserData.totalBankCoin * 1f / DataManager.GameConfig.BankCoinStage.Last();
        DataManager.UserData.totalBankCoin += coinNumber;
        slider.value = lastvalue;
        btn_Continue.gameObject.SetActive(false);
        btn_Continue.onClick.RemoveAllListeners();

        anim.Show(null, () =>
        {
            if(coinNumber > 0)
            {
                txt_coinSave.DOText(0, coinNumber, 1f);
                slider.DOValue(DataManager.UserData.totalBankCoin * 1f / DataManager.GameConfig.BankCoinStage.Last(), 1f);
                DOVirtual.DelayedCall(1f, () =>
                {
                    btn_Continue.gameObject.SetActive(true);
                    btn_Continue.interactable = true;
                    btn_Continue.onClick.AddListener(BtnContinueClick);
                });
            }
        });
    }

    private void BtnContinueClick()
    {
        btn_Continue.interactable = false;
        anim.Hide();
        GameStateManager.LoadGame(null);
    }
}
