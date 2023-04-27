using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventDataModel
{
    
}

public enum eGameMode
{
    Normal = 0,
    Bartender = 1,
    Challenge=2,
}
//public enum nomalMode
//{
//    Store1=0,
//    Store2=1,
//}
[Serializable]
public class NewMatchDatum
{
    public eItemType type;
    public List<Goods_Item> items;

    public NewMatchDatum(eItemType _type, List<Goods_Item> _items)
    {
        this.type = _type;
        this.items = _items;
    }
}
