using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DecorItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private SpriteRenderer _unlockedSR = null;
    [SerializeField] private SpriteRenderer _lockSR = null;
    [SerializeField] private GameObject _unlockWithAdsObj;
    [SerializeField] private GameObject _unlockWithCoinObj;
    [SerializeField] private TextMeshPro _tmpPrice = null;

    [SerializeField] BoxCollider2D _boxCollider = null;
    [SerializeField]

    private ItemDecorData _currDatum = null;
    private int _sortingOrderOffset;

    private void Awake()
    {
        if (_unlockedSR != null)
            _sortingOrderOffset = _unlockedSR.sortingOrder;
    }

    public void SetSortingOrder(int addAmount)
    {
        if (_unlockedSR != null)
            _unlockedSR.sortingOrder = _sortingOrderOffset + addAmount;
        if (_lockSR != null)
            _lockSR.sortingOrder = _sortingOrderOffset + addAmount;
    }

    public void Fill(ItemDecorData datum)
    {
        _currDatum = datum;
        _tmpPrice.text = _currDatum.unlockPrice.ToString();
        _unlockedSR.gameObject.SetActive(_currDatum.isUnlocked);
        _lockSR.gameObject?.SetActive(!_currDatum.isUnlocked);
        _unlockWithAdsObj?.SetActive(_currDatum.unlockType == UnlockType.Ads);
        _unlockWithCoinObj.SetActive(_currDatum.unlockType == UnlockType.Gold);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_currDatum == null || _currDatum.isUnlocked)
            return;

        Debug.Log($"Try to unlock house decor: floorIndex={_currDatum.floorIndex}, itemIndex={_currDatum.index}");
        //PostEvent Unlock Item

        if(_currDatum.unlockType == UnlockType.Gold)
        {
            if (CoinManager.totalCoin < _currDatum.unlockPrice)
            {
                UIToast.ShowError("Not enought gold");
                return;
            }

            CoinManager.Add(-_currDatum.unlockPrice);
            _currDatum.isUnlocked = true;
            Fill(_currDatum);
        }
        else if(_currDatum.unlockType == UnlockType.Ads)
        {
            Base.Ads.AdsManager.ShowVideoReward((e, t) =>
            {
                if (e == AdEvent.ShowSuccess)
                {
                    _currDatum.isUnlocked = true;
                    Fill(_currDatum);
                }
                else
                {
                    Base.Ads.AdsManager.ShowNotice(e);
                }
            }, $"RV_Unlock_HouseItem");
        }
    }
}
