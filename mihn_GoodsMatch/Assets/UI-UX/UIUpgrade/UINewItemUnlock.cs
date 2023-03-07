using Base.Ads;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyBox;

public class UINewItemUnlock : MonoBehaviour
{
    [SerializeField] Button unlockByCoinBtn;
    [SerializeField] Button unlockByAdsBtn;
    [SerializeField] GameObject claimedButton;
    [SerializeField] Image itemIcon;
    [SerializeField] Text priceText = null;

    [SerializeField] ParticleSystem unlockFx = null;

    private string itemId;
    private int price = 100;

    private void Awake()
    {
        unlockByCoinBtn?.onClick.AddListener(OnClaimButtonClick);
        unlockByAdsBtn?.onClick.AddListener(OnClaimByAdsClick);
    }

    private void OnEnable()
    {
        UserData.OnCoinChanged += OnTotalCoinChanged;
    }
    private void OnDisable()
    {
        UserData.OnCoinChanged -= OnTotalCoinChanged;
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
        CoinManager.Add(-price);
        OnItemUnlocked();
    }

    private void OnClaimByAdsClick()
    {
        SoundManager.Play(GameConstants.sound_Button_Clicked);
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
        SoundManager.Play(GameConstants.sound_Item_unlocked);
        DataManager.ItemsAsset.UnlockNewItemById(itemId);
        DataManager.Save();
        claimedButton.SetActive(true);
        unlockByCoinBtn?.gameObject.SetActive(false);
        unlockByAdsBtn?.gameObject.SetActive(false);
        ShowItemUnlockFX();
    }

    private void OnTotalCoinChanged(int change, int current)
    {
        if (unlockByCoinBtn)
            unlockByCoinBtn.interactable = current >= price;
    }

    [ButtonMethod]
    private void ShowItemUnlockFX()
    {
        if(unlockFx)
            unlockFx.Play();
    }
}
