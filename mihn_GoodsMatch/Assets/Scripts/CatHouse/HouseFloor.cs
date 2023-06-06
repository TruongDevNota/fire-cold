using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using DG.Tweening;
using System.Linq;

public class HouseFloor : MonoBehaviour
{
    private HouseDataAsset _dataAsset => DataManager.HouseAsset;

    [SerializeField] List<DecorItem> _decorObjs = new List<DecorItem>();
    [SerializeField] int _orderDelta = 100;
    [SerializeField] List<CatControl> _cats = new List<CatControl>();
    [SerializeField] SpriteRenderer _lockCoverSR;
    [SerializeField] SkeletonAnimation _lockAnim;

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

    public void SetItemsOrderSorting()
    {
        int orderAdding = _index * _orderDelta;
        foreach (var item in _decorObjs)
            item.SetSortingOrder(orderAdding);
    }

    public void Fill(HouseFloorData datum)
    {
        Index = datum.floorIndex;
        SetItemsOrderSorting();

        _lockCoverSR.gameObject.SetActive(datum.isUnlocked);

        for (int i = 0; i < _decorObjs.Count; i++)
        {
            _decorObjs[i].gameObject.SetActive(i < datum.allDecorationItems.Count && datum.allDecorationItems[i].isUnlocked);
        }
        
        for(int i = 0; i < _cats.Count; i++)
        {
            _cats[i].gameObject.SetActive(i < datum.allCats.Count && datum.allCats[i].isUnlocked);
        }
    }

    public IEnumerator YieldUnlock()
    {
        _dataAsset.UnlockFloorByIndex(Index);
        DataManager.Save();
        yield return new WaitForSeconds(0.5f);

        _lockAnim.AnimationState.SetAnimation(0, "unlocked", false);
        _lockAnim.Update(0);
        yield return new WaitForEndOfFrame();
        _lockAnim.AnimationState.Complete += delegate
        {
            _lockAnim.gameObject.SetActive(false);
            _lockCoverSR.DOFade(0F, 0.5f);
            ShowPopupNewDiscover();
        };
    }

    private void ShowPopupNewDiscover()
    {
        UIPopupListPreview.ShowList(_dataAsset.GetFloorDataByIndex(_index).GetAllSprite());
    }

    [MyBox.ButtonMethod]
    public void ShowUnlockAnim()
    {
        StartCoroutine(YieldUnlock());
    }
}

