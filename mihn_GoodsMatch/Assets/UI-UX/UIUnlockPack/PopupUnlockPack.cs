using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
using DG.Tweening;

public class PopupUnlockPack : MonoBehaviour
{
    [SerializeField] UIAnimation anim;
    [SerializeField] RectTransform container;
    [SerializeField] Image itemPrefab;
    private List<Image> items = new List<Image>();
    
    public void OnShow(int mapIndex)
    {
        var itemData = DataManager.ItemsAsset.GetlistNewUnlock(mapIndex);
        for (int i = 0; i < itemData.Count; i++)
        {
            bool exist = i < items.Count;
            var current = exist ? items[i] : itemPrefab.Spawn(container);
            current.sprite = itemData[i].itemProp.itemIcon;
            if(!exist)
                items.Add(current);
        }
        for (int j = items.Count - 1; j >= itemData.Count; j--)
        {
            items[j].Recycle();
            items.RemoveAt(j);
        }

        anim.Show();
    }
}
