using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseFloor : MonoBehaviour
{
    [SerializeField] List<DecorItem> decorObjs = new List<DecorItem>();
    [SerializeField] int _orderDelta = 100;

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
}
