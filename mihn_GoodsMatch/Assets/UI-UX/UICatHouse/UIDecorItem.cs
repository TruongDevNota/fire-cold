using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDecorItem : MonoBehaviour
{
    [SerializeField] Image _iconItem;
    [SerializeField] Text _txtCoinPrice;

    [SerializeField] Button _previewBtn;
    [SerializeField] Button _unlockWithCoinBtn;
    [SerializeField] Button _unlockWithAdsBtn;

    private ItemDecorData _currData = null;
    System.Action<ItemDecorData> _onButtonPreviewClicked = null;
    System.Action<ItemDecorData> _onButtonUnlockClicked = null;

    public void Fill(ItemDecorData itemData, System.Action<ItemDecorData> previewAction = null, System.Action<ItemDecorData> unlockAction = null)
    {
        _currData = itemData;

        _iconItem.sprite = _currData.thumbUnlocked;
        _txtCoinPrice.text = _currData.unlockPrice.ToString();

        _unlockWithCoinBtn.gameObject.SetActive(_currData.unlockType == UnlockType.Gold);
        _unlockWithAdsBtn.gameObject.SetActive(_currData.unlockType == UnlockType.Ads);
        _unlockWithCoinBtn.interactable = _currData.isCanUnlock;

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
            _unlockWithCoinBtn.interactable = false;
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
