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
    [SerializeField] SpriteRenderer _wallSR;
    [SerializeField] SpriteRenderer _lockCoverSR;
    [SerializeField] SkeletonAnimation _lockAnim;
    [SerializeField] ButtonUnlockFloor _btnUnlock;

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

    public void Fill(HouseFloorData datum)
    {
        Index = datum.floorIndex;
        int orderAdding = Index * _orderDelta;

        _wallSR.sortingOrder = orderAdding;
        _lockCoverSR.gameObject.SetActive(!datum.isUnlocked);
        _lockAnim.gameObject.SetActive(!datum.isUnlocked);
        _btnUnlock.Fill(datum.unlockPrice);

        for (int i = 0; i < _decorObjs.Count; i++)
        {
            if(i < datum.allDecorationItems.Count)
            {
                _decorObjs[i].gameObject.SetActive(true);
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
            _lockCoverSR.DOFade(0F, 0.5f).OnComplete(() =>
            {
                ShowPopupNewDiscover();
            });
        };
    }

    private void ShowPopupNewDiscover()
    {
        UIPopupListPreview.ShowList(_dataAsset.GetFloorDataByIndex(_index).GetAllSprite());
    }

    public void Unlock()
    {
        StartCoroutine(YieldUnlock());
    }

    public void UnlockItem(string id, eHouseDecorType type)
    {
        _dataAsset.UnLockItem(Index, id, type);
    }
}

