using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using DG.Tweening;
using System.Linq;

public class HouseFloor : MonoBehaviour
{
    private HouseDataAsset _dataAsset => DataManager.HouseAsset;
    private HouseFloorData _currData => _dataAsset.GetFloorDataByIndex(_index);

    [SerializeField] List<DecorItem> _decorObjs = new List<DecorItem>();
    [SerializeField] int _orderDelta = 100;
    [SerializeField] List<CatControl> _cats = new List<CatControl>();
    [SerializeField] SpriteRenderer _wallSR;
    [SerializeField] SkeletonAnimation _lockAnim;
    [SerializeField] ButtonUnlockFloor _btnUnlock;
    [SerializeField] Color _lockColor;
    [SerializeField] Color _unlockedColor;

    [SerializeField, MyBox.ReadOnly]
    private int _index;
    public int Index 
    { 
        get { return _index; } 
        private set { _index = value; } 
    }

    public void SetIndex(int value)
    {
        Index = value;
    }

    private void OnEnable()
    {
        this.RegisterListener((int)EventID.OnUnlockCatSuccess, UnlockCat);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnUnlockCatSuccess, UnlockCat);
    }

    public void Fill(HouseFloorData datum)
    {
        Index = datum.floorIndex;
        int orderAdding = Index * _orderDelta;
        
        _wallSR.sortingOrder = orderAdding;
        _lockAnim.gameObject.SetActive(!datum.isUnlocked);
        _btnUnlock.Fill(datum.unlockPrice);

        _wallSR.color = datum.isUnlocked ? _unlockedColor : _lockColor;

        for (int i = 0; i < _decorObjs.Count; i++)
        {
            if(i < datum.allDecorationItems.Count)
            {
                _decorObjs[i].gameObject.SetActive(datum.isUnlocked);
                _decorObjs[i].Fill(this, datum.allDecorationItems[i], orderAdding);
            }
            else
                _decorObjs[i].gameObject.SetActive(false);
        }
        
        for(int i = 0; i < _cats.Count; i++)
        {
            _cats[i].gameObject.SetActive(i < datum.allCats.Count && datum.allCats[i].isUnlocked);
        }
    }
    public IEnumerator YieldUnlock()
    {
        CoinManager.Add(-_currData.unlockPrice);
        _dataAsset.UnlockFloorByIndex(Index);
        DataManager.Save();
        yield return new WaitForSeconds(0.5f);

        _lockAnim.AnimationState.SetAnimation(0, "unlocked", false);
        _lockAnim.Update(0);
        _btnUnlock.gameObject.SetActive(false);
        yield return new WaitForEndOfFrame();
        _lockAnim.AnimationState.Complete += delegate
        {
            _lockAnim.gameObject.SetActive(false);
            _wallSR.DOColor(_unlockedColor, 0.5f).OnComplete(() =>
            {
                ShowPopupNewDiscover();
            });
        };
    }
    private void ShowPopupNewDiscover()
    {
        UIPopupListPreview.ShowList(_dataAsset.GetFloorDataByIndex(_index).GetAllSprite());
        this.PostEvent((int)EventID.OnFloorUnlocked);
    }
    public void Unlock()
    {
        if(CoinManager.totalCoin < _currData.unlockPrice)
        {
            Debug.Log("Not enought money to unlock floor");
            UIToast.ShowNotice("Not enought money to unlock floor");
            return;
        }

        StartCoroutine(YieldUnlock());
    }
    public void UnlockItem(string id, eHouseDecorType type)
    {
        _dataAsset.UnLockItem(Index, id, type);
    }
    public void UnlockCat(object obj)
    {
        var datum = (HouseCatData)obj;
        if (datum.floorIndex != Index)
            return;
        if (datum.index < 0 || datum.index >= _cats.Count)
            return;

        _dataAsset.UnLockItem(Index, datum.id, eHouseDecorType.Cat);
        _cats[datum.index].ActiveOnfloor(datum);
    }
}

