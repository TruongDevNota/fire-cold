using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventDataModel
{
    
}

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
