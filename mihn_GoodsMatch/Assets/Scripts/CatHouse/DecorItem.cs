using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DecorItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private SpriteRenderer _unlockedSR = null;
    [SerializeField] private SpriteRenderer _lockSR = null;
    [SerializeField] private GameObject _lockGroupObj = null;
    [SerializeField] private GameObject _unlockWithAdsObj;
    [SerializeField] private GameObject _unlockWithCoinObj;
    [SerializeField] private TextMeshPro _tmpPrice = null;

    [SerializeField] BoxCollider2D _boxCollider = null;

    private HouseFloor _floor;
    private ItemDecorData _currDatum;
    private int _sortingOrderOffset;

    private void Awake()
    {
        if (_unlockedSR != null)
            _sortingOrderOffset = _unlockedSR.sortingOrder;
    }

    private void SetSortingOrder(int addAmount)
    {
        if (_unlockedSR != null)
            _unlockedSR.sortingOrder = _sortingOrderOffset + addAmount;
        if (_lockSR != null)
            _lockSR.sortingOrder = _sortingOrderOffset + addAmount;
    }

    public void Fill(HouseFloor floor, ItemDecorData datum, int addSortingOrder)
    {
        _floor = floor;
        _currDatum = datum;
        SetSortingOrder(addSortingOrder);

        Fetch(_currDatum);
    }

    private void Fetch(ItemDecorData datum)
    {
        _tmpPrice.text = _currDatum.unlockPrice.ToString();
        _unlockedSR.gameObject.SetActive(_currDatum.isUnlocked);
        _lockGroupObj?.SetActive(!_currDatum.isUnlocked);
        _unlockWithAdsObj?.SetActive(_currDatum.unlockType == UnlockType.Ads);
        _unlockWithCoinObj.SetActive(_currDatum.unlockType == UnlockType.Gold);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_currDatum == null || _currDatum.isUnlocked)
            return;

        Debug.Log($"Try to unlock house decor: floorIndex={_currDatum.floorIndex}, itemIndex={_currDatum.index}");
        
        if(_currDatum.unlockType == UnlockType.Gold)
        {
            if (CoinManager.totalCoin < _currDatum.unlockPrice)
            {
                UIToast.ShowError("Not enought gold");
                return;
            }

            CoinManager.Add(-_currDatum.unlockPrice);
            _currDatum.isUnlocked = true;
            _floor.UnlockItem(_currDatum.id, _currDatum.type);
            Fetch(_currDatum);
        }
        else if(_currDatum.unlockType == UnlockType.Ads)
        {
            Base.Ads.AdsManager.ShowVideoReward((e, t) =>
            {
                if (e == AdEvent.ShowSuccess)
                {
                    _floor.UnlockItem(_currDatum.id, _currDatum.type);
                    _currDatum.isUnlocked = true;
                    Fetch(_currDatum);
                }
                else
                {
                    Base.Ads.AdsManager.ShowNotice(e);
                }
            }, $"RV_Unlock_HouseItem");
        }
    }
}
