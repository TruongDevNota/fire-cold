using Base.Ads;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UINewItemUnlock : MonoBehaviour
{
    [SerializeField] Button unlockByCoinBtn;
    [SerializeField] Button unlockByAdsBtn;
    [SerializeField] GameObject claimedButton;
    [SerializeField] Image itemIcon;
    [SerializeField] Text priceText = null;

    private string itemId;
    private int price = 100;

    private void Awake()
    {
        unlockByCoinBtn?.onClick.AddListener(OnClaimButtonClick);
        unlockByAdsBtn?.onClick.AddListener(OnClaimByAdsClick);
    }

    public void Init(ItemDatum datum)
    {
        this.itemId = datum.id;
        price = datum.unlockValue;
        itemIcon.sprite = datum.itemProp.itemIcon;

        unlockByCoinBtn?.gameObject.SetActive(true);
        if(unlockByCoinBtn)
            unlockByCoinBtn.interactable = CoinManager.totalCoin >= datum.unlockValue;
        unlockByAdsBtn?.gameObject.SetActive(true);
        if (unlockByAdsBtn)
            unlockByAdsBtn.interactable = true;
        claimedButton.SetActive(false);

        if (priceText)
            priceText.text = price.ToString();
    }

    private void OnClaimButtonClick()
    {
        if (CoinManager.totalCoin < price)
        {
            unlockByCoinBtn.interactable = false;
            return;
        }
        OnItemUnlocked();
    }

    private void OnClaimByAdsClick()
    {
        AdsManager.ShowVideoReward((e, t) =>
        {
            if (e == AdEvent.ShowSuccess || DataManager.GameConfig.isAdsByPass)
            {
                OnItemUnlocked();
            }
            else
            {

            }
        }, "UnlockNewItem", "newItem");
    }

    private void OnItemUnlocked()
    {
        CoinManager.Add(-price);
        DataManager.ItemsAsset.UnlockNewItemById(itemId);
        DataManager.Save();
        claimedButton.SetActive(true);
        unlockByCoinBtn?.gameObject.SetActive(false);
        unlockByAdsBtn?.gameObject.SetActive(false);
    }
}
