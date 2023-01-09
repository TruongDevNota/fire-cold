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
    Image img_adsIcon;
    [SerializeField]
    Image img_notify;
    private float lastTimeViewAds;

    int index;
    int value;

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void Init(int index, int valueToBuy, float price, System.Action callback = null)
    {
        this.index = index;
        value = valueToBuy;
        txt_Value.text = valueToBuy.ToString();
        txt_Price.text = "$"+price.ToString("n2");
        btn_BuyWithCoin?.gameObject.SetActive(index > 0);
        img_adsIcon.gameObject.SetActive(true);
        btn_BuyWithAds?.gameObject.SetActive(index == 0);
        if (index == 0)
            CheckTimeToBuy();
    }

    public void CheckTimeToBuy()
    {
        bool isCanBuy = IsCanBuyCoinWithAds();
        btn_BuyWithAds.interactable = isCanBuy;
        img_adsIcon.gameObject.SetActive(isCanBuy);
        adsTimeCoolDown.gameObject.SetActive(!isCanBuy);
        img_notify?.gameObject.SetActive(isCanBuy);
        if (!isCanBuy)
            StartCoroutine(YieldCountDownBuyWithAds());
    }

    private bool IsCanBuyCoinWithAds()
    {
        return System.DateTime.Now.Subtract(System.TimeSpan.FromSeconds(DataManager.GameConfig.buyCoinWithAdsCoolDownInSeconds)) > DataManager.UserData.lastTimeBuyCoinWithAds;
    }

    private IEnumerator YieldCountDownBuyWithAds()
    {
        var wait1 = new WaitForSeconds(1);
        
        while (!IsCanBuyCoinWithAds())
        {
            //int elapTime = (System.DateTime.Now - DataManager.UserData.lastTimeBuyCoinWithAds).Seconds;
            adsTimeCoolDown.text = (DataManager.UserData.lastTimeBuyCoinWithAds - System.DateTime.Now).Add(System.TimeSpan.FromSeconds(DataManager.GameConfig.buyCoinWithAdsCoolDownInSeconds)).ToString("m':'ss");
            yield return wait1;
        }

        CheckTimeToBuy();
    }

    public void BuyWithAds()
    {
        SoundManager.Play("1. Click Button");
        if (!IsCanBuyCoinWithAds())
            return;

        AdsManager.ShowVideoReward((e,t) =>
        {
            if(e == AdEvent.ShowSuccess || DataManager.GameConfig.isAdsByPass)
            {
                lastTimeViewAds = Time.realtimeSinceStartup;
                CoinManager.Add(value, transform);
                btn_BuyWithAds.interactable = false;
                DataManager.UserData.lastTimeBuyCoinWithAds = System.DateTime.Now;
                CheckTimeToBuy();
                DataManager.Save();
            }
            else
            {

            }
        }, "ShopBuyCoinWithAds", "coin");
    }
}
