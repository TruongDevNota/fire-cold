using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MyBox;
using UnityEditor;

[CreateAssetMenu(fileName = "GameItemsAsset", menuName = "DataAsset/GameItemAsset")]
public class GameItemAsset : BaseAsset<ItemDatum>
{
    [SerializeField] List<Goods_Item> itemModels;

    public List<ItemDatum> lockedList
    {
        get
        {
            return list?.Where(x => !x.isUnlocked).ToList();
        }
    }
    public ItemDatum GetItemDatumById(string id)
    {
        return list?.FirstOrDefault(x => string.Compare(x.id, id) == 0);
    }
    public ItemDatum GetItemByType(eItemType type)
    {
        return list?.FirstOrDefault(x => x.itemProp.Type == type);
    }
    public ItemDatum GetItemByIndex(int index,Store store)
    {
        List<ItemDatum> listC=new List<ItemDatum>();
        
        for(int i = 0; i < unlockedList.Count; i++)
        {
            if (unlockedList[i].Store == store)
            {
                listC.Add(unlockedList[i]);
            }
        }
        return listC[index - 1];
    }
    public void UnlockNewItemById(string id)
    {
        var item = GetItemDatumById(id);
        if (item == null || item.isUnlocked)
            return;
        item.isUnlocked = true;
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    public void ConvertItemData(List<SaveData> saveData)
    {
        foreach (var i in saveData)
        {
            var temp = list.FirstOrDefault(x => x.id == i.id);
            if (temp != null)
            {
                if (i.isUnlocked)
                    temp.isUnlocked = i.isUnlocked;
            }
        }
    }


    [ButtonMethod]
    public override void ResetData()
    {
        list.Clear();

        for (int i = 0; i < itemModels.Count; i++)
        {
            var datum = new ItemDatum()
            {
                Store = i % 2 == 0 ? Store.store1 : Store.store2,
                id = itemModels[i].name.ToLower(),
                isUnlocked = true,
                itemProp = itemModels[i],
                unlockValue = 100,
            };
            list.Add(datum);
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }
}

[System.Serializable]
public class ItemDatum : SaveData
{
    public Goods_Item itemProp;
    public int unlockValue;
    
    public Store Store;
}
public enum Store
{
    store1,
    store2
}
