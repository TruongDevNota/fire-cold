using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDecorItem : MonoBehaviour
{
    [SerializeField] Image _iconItem;
    [SerializeField] Text _txtCoinPrice = null;
    [SerializeField] Image iconCatFootPrints;
    [SerializeField] Image img_Grey;

    [SerializeField] Button _previewBtn;
    [SerializeField] Button _unlockWithCoinBtn = null;
    [SerializeField] Button _unlockWithAdsBtn = null;

    private ItemDecorData _currData = null;
    private HouseFloorData floorData = null;
    System.Action<ItemDecorData> _onButtonPreviewClicked = null;
    System.Action<ItemDecorData> _onButtonUnlockClicked = null;
    private void OnEnable()
    {
        this.RegisterListener((int)EventID.OnUnlockCatSuccess, FillAgain);
        this.RegisterListener((int)EventID.OnUnlockItemSuccess, FillAgain);
    }

    private void FillAgain(object obj)
    {
        Fill(floorData,_currData,_onButtonPreviewClicked,_onButtonUnlockClicked);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnUnlockCatSuccess, FillAgain);
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnUnlockItemSuccess, FillAgain);
    }
    public void Fill(HouseFloorData m_floorData,ItemDecorData itemData, System.Action<ItemDecorData> previewAction = null, System.Action<ItemDecorData> unlockAction = null)
    {
        floorData = m_floorData;
        _currData = itemData;
        _iconItem.sprite = _currData.thumb;
        if(_txtCoinPrice)
            _txtCoinPrice.text = _currData.unlockPrice.ToString();

        _unlockWithCoinBtn?.gameObject.SetActive(_currData.unlockType == UnlockType.Gold&&!_currData.isUnlocked);
        _unlockWithAdsBtn?.gameObject.SetActive(_currData.unlockType == UnlockType.Ads && !_currData.isUnlocked);
        iconCatFootPrints?.gameObject.SetActive(_currData.isUnlocked);
        img_Grey?.gameObject.SetActive(!floorData.isUnlocked || (floorData.isUnlocked  && !_currData.isUnlocked));
        if (_unlockWithCoinBtn)
            _unlockWithCoinBtn.interactable = floorData.isUnlocked;
        if (_unlockWithAdsBtn)
            _unlockWithAdsBtn.interactable = floorData.isUnlocked;

        _onButtonPreviewClicked = previewAction;
        _onButtonUnlockClicked = unlockAction;
    }

    public void OnButtonPreviewClicked()
    {
        this.PostEvent((int)EventID.ShowItemPreview, new ItemPreivewDatum() { floorIndex = _currData.floorIndex, itemID = _currData.id, type = _currData.type }); 
        if(_onButtonPreviewClicked != null)
            _onButtonPreviewClicked.Invoke(_currData);
    }

    public void OnButtonUnlockWithCoinClicked()
    {
        if(CoinManager.totalCoin < _currData.unlockPrice)
        {
            UIToast.ShowError("Not enought gold");
            return;
        }

        if(_onButtonUnlockClicked != null)
        {
            CoinManager.Add(-_currData.unlockPrice);
            _onButtonUnlockClicked.Invoke(_currData);
        }
    }

    public void OnButtonUnlockWithAdsClicked()
    {
        if (_onButtonUnlockClicked != null)
        {
            _unlockWithAdsBtn.interactable = false;
            Base.Ads.AdsManager.ShowVideoReward((e, t) =>
            {
                if (e == AdEvent.ShowSuccess)
                {
                    _onButtonUnlockClicked.Invoke(_currData);
                }
                else
                {
                    _unlockWithAdsBtn.interactable = true;
                    Base.Ads.AdsManager.ShowNotice(e);
                }
            }, $"unlock_HouseItem_Cat");
        }
    }
}

public class ItemPreivewDatum
{
    public int floorIndex;
    public string itemID;
    public eHouseDecorType type;
}
