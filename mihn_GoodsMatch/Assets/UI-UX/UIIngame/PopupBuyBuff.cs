using Base.Ads;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupBuyBuff : MonoBehaviour
{
    [SerializeField]
    UIAnimation anim;
    [SerializeField]
    Text txt_Title;
    [SerializeField]
    Image img_icon;
    [SerializeField]
    Text txt_buyPrice;
    [SerializeField]
    Button btn_BuyWithCoin;
    [SerializeField]
    Button btn_BuyWithAds;
    [SerializeField]
    Sprite hintSprite;
    [SerializeField]
    Sprite swapSprite;

    private BuffType eBuffType;

    private void Start()
    {
        btn_BuyWithCoin?.onClick.AddListener(() =>
        {
            if(DataManager.UserData.totalCoin >= DataManager.GameConfig.buffPrice)
            {
                OnBuySuccess();
            }
            else
            {
                UIToast.ShowNotice("Not enought gold!");
            }
            btn_BuyWithCoin.interactable = DataManager.UserData.totalCoin >= DataManager.GameConfig.buffPrice;
        });

        btn_BuyWithAds?.onClick.AddListener(() =>
        {
            AdsManager.ShowVideoReward((e, t) =>
            {
                if (e == AdEvent.Success || DataManager.GameConfig.isAdsByPass)
                {
                    OnBuySuccess();
                }
                else
                {
                    Debug.Log($"!!!!! video reward fail.");
                    UIToast.ShowNotice("view video reward fail!");
                }
            }, "BuyBuffInGame");
        });
    }

    public void Onshow(BuffType type)
    {
        if (GameStateManager.CurrentState != GameState.Play)
            return;
        GameStateManager.Pause(null);
        eBuffType = type;
        btn_BuyWithCoin.interactable = DataManager.UserData.totalCoin >= DataManager.GameConfig.buffPrice;
        btn_BuyWithAds.interactable = true;
        txt_buyPrice.text = DataManager.GameConfig.buffPrice.ToString();
        txt_Title.text = type == BuffType.Hint ? "HINT" : "SWAP";
        img_icon.sprite = type == BuffType.Hint ? hintSprite : swapSprite;
        anim.Show();
    }

    public void OnHide()
    {
        anim.Hide(onCompleted:() => {
            if (GameStateManager.CurrentState == GameState.Pause)
                GameStateManager.Play(null);
        });
    }

    private void OnBuySuccess()
    {
        CoinManager.Add(-DataManager.GameConfig.buffPrice);
        switch (eBuffType)
        {
            case BuffType.Hint:
                DataManager.UserData.totalHintBuff++;
                break;
            case BuffType.Swap:
                DataManager.UserData.totalSwapBuff++;
                break;
        }
        OnHide();
    }
}

public enum BuffType
{
    Hint = 1,
    Swap = 2,
}
