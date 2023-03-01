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

    private eItemType currType;
    private int price = 100;

    public void Init(ItemDatum datum)
    {
        this.currType = datum.itemProp.Type;
        price = datum.unlockValue;
        itemIcon.sprite = datum.itemProp.itemIcon;

        unlockByCoinBtn?.gameObject.SetActive(true);
        if(unlockByCoinBtn)
            unlockByCoinBtn.interactable = CoinManager.totalCoin >= datum.unlockValue;
        unlockByAdsBtn?.gameObject.SetActive(true);
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
        //show ads
    }

    private void OnItemUnlocked()
    {
        CoinManager.Add(-price);
        DataManager.GameItemData.UnlockNewItem(currType);
        DataManager.Save();
        claimedButton.SetActive(true);
        unlockByCoinBtn?.gameObject.SetActive(false);
        unlockByAdsBtn?.gameObject.SetActive(false);
    }
}
