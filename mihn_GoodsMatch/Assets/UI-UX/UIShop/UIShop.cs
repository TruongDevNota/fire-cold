using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIShop : MonoBehaviour
{
    [SerializeField]
    UIShopItem uiItemPrefab;
    [SerializeField]
    ScrollRect scrollRect;
    [SerializeField]
    RectTransform contentParent;
    [SerializeField]
    int itemNum;
    [SerializeField]
    List<int> values;
    [SerializeField]
    List<float> price;

    List<UIShopItem> shopItems;
    private void Start()
    {
        uiItemPrefab.CreatePool(itemNum);
    }

    private void Init()
    {
        for(int i = 0; i < itemNum; i++)
        {
            var item = uiItemPrefab.Spawn(contentParent);
            item.Init(i, values[i], price[i]);
        }
    }

    public void OnShow()
    {
    }
}
