using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIShop : MonoBehaviour
{
    [SerializeField]
    UIAnimation anim;
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

    List<UIShopItem> shopItems = new List<UIShopItem>();
    private void Start()
    {
        uiItemPrefab.CreatePool(itemNum);
    }

    public void Init()
    {
        for(int i = 0; i < itemNum; i++)
        {
            var exist = i < shopItems.Count;
            var item = exist? shopItems[i] : uiItemPrefab.Spawn(contentParent);
            item.Init(i, values[i], price[i]);
            if (!exist)
                shopItems.Add(item);
        }
    }

    public void OnShow()
    {
        anim.Show();
        Init();
    }
    public void OnHide()
    {
        anim.Hide();
    }
}
