using Base.Ads;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIShopItem : MonoBehaviour
{
    [SerializeField]
    Text txt_Value;
    [SerializeField]
    Text txt_Price;
    [SerializeField]
    Button btn_BuyWithCoin;
    [SerializeField]
    Button btn_BuyWithAds;
    [SerializeField]
    Text adsTimeCoolDown;
    [SerializeField]
    float coolDownInSeconds;
    private float lastTimeViewAds;

    int index;
    int value;
    public void Init(int index, int valueToBuy, float price, System.Action callback = null)
    {
        this.index = index;
        value = valueToBuy;
        txt_Value.text = valueToBuy.ToString();
        txt_Price.text = "$"+price.ToString("n2");
        btn_BuyWithCoin?.gameObject.SetActive(index > 0);
        btn_BuyWithAds?.gameObject.SetActive(index == 0);
    }

    public void CheckTimeToBuy()
    {

    }

    public void BuyWithAds()
    {
        if (lastTimeViewAds + coolDownInSeconds > Time.realtimeSinceStartup)
            return;

        AdsManager.ShowVideoReward((e,t) =>
        {
            if(e == AdEvent.ShowSuccess || DataManager.GameConfig.isAdsByPass)
            {
                lastTimeViewAds = Time.realtimeSinceStartup;

                CoinManager.Add(value, transform);
            }
            else
            {
                Debug.Log($"!!!!! video reward fail.");
                UIToast.ShowNotice("view video reward fail!");
            }
        }, "ShopBuyCoinWithAds", "coin");
    }
}
