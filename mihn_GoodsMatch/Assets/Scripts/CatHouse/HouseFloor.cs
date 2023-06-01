using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseFloor : MonoBehaviour
{
    [SerializeField] List<DecorItem> decorObjs = new List<DecorItem>();
    [SerializeField] int _orderDelta = 100;
    [SerializeField] List<CatControl> cats = new List<CatControl>();
    [SerializeField] SpriteRenderer lockCoverSR;

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
        foreach (var item in decorObjs)
            item.SetSortingOrder(orderAdding);
    }

    public void Fill(HouseFloorData datum)
    {
        lockCoverSR.gameObject.SetActive(datum.isUnlocked);

        for (int i = 0; i < decorObjs.Count; i++)
        {
            decorObjs[i].gameObject.SetActive(i < datum.allDecorationItems.Count && datum.allDecorationItems[i].isUnlocked);
        }
        
        for(int i = 0; i < cats.Count; i++)
        {
            cats[i].gameObject.SetActive(i < datum.allCats.Count && datum.allCats[i].isUnlocked);
        }
    }
}

