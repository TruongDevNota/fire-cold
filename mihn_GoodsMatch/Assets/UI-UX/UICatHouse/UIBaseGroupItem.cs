using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBaseGroupItem : MonoBehaviour
{
    [SerializeField] Text _txtGroupName;
    [SerializeField] Text _txtUnlockAmount;

    [SerializeField] UIDecorItem _itemPrefab;
    [SerializeField] RectTransform _itemContainerRect;

    private HouseFloorData _floorData;
    [SerializeField, MyBox.ReadOnly] private List<UIDecorItem> _listItem = new List<UIDecorItem>();

    private string unlockTextFormat = "{0}/{1}";
    private void Awake()
    {
        _itemPrefab.CreatePool(30);
    }

    private void Start()
    {
        
    }

    public virtual void Fill(HouseFloorData data, eHouseDecorType type)
    {
        _floorData = data;

        _txtGroupName.text = data.name;

        if (type == eHouseDecorType.Cat)
        {
            for (int i = 0; i < data.allCats.Count; i++)
            {
                var exist = i < _listItem.Count;
                var newItem = exist ? _listItem[i] : _itemPrefab.Spawn(_itemContainerRect);
                newItem.Fill(data,data.allCats[i],null, OnBtnBuyClicked);
                if(!exist)
                    _listItem.Add(newItem);
            }
            for (int j = _listItem.Count -1; j >= data.allCats.Count; j--)
            {
                _listItem[j].Recycle();
                _listItem.RemoveAt(j);
            }
            _txtUnlockAmount.text = string.Format(unlockTextFormat, data.catUnlockedCount, data.allCats.Count);
        }
        else if(type == eHouseDecorType.Item)
        {
            for (int i = 0; i < data.allDecorationItems.Count; i++)
            {
                var exist = i < _listItem.Count;
                var newItem = exist ? _listItem[i] : _itemPrefab.Spawn(_itemContainerRect);
                newItem.Fill(data,data.allDecorationItems[i],null, OnBtnBuyClicked);
                if (!exist)
                    _listItem.Add(newItem);
            }
            for (int j = _listItem.Count - 1; j >= data.allDecorationItems.Count; j--)
            {
                _listItem[j].Recycle();
                _listItem.RemoveAt(j);
            }
            _txtUnlockAmount.text = string.Format(unlockTextFormat, data.itemUnlockedCount, data.allDecorationItems.Count);
        }
    }

    protected virtual void OnBtnBuyClicked(ItemDecorData data)
    {
        if(data.type == eHouseDecorType.Cat)
        {
            this.PostEvent((int)EventID.OnUnlockCatSuccess, data);
            Debug.Log("mua thanh cong");
        }
        if (data.type == eHouseDecorType.Item)
        {
            this.PostEvent((int)EventID.OnUnlockItemSuccess, data);
            Debug.Log("mua thanh cong");
        }
        this.PostEvent((int)EventID.OnFloorUnlocked);
    }
}
