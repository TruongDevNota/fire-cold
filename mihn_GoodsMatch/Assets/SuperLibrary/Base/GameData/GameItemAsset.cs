using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MyBox;
using UnityEditor;

[CreateAssetMenu(fileName = "GameItemsAsset", menuName = "DataAsset/GameItemAsset")]
public class GameItemAsset : ScriptableObject
{
    [SerializeField] List<ItemDatum> list;
    [SerializeField] List<Goods_Item> itemModels;

    public List<ItemDatum> unlockedList
    {
        get
        {
            return list?.Where(x => x.unlocked).ToList();
        }
    }
    public List<ItemDatum> itemSaveList
    {
        get => list?.Where(x => x.unlocked)
            .Select(x => new ItemDatum { id = x.id, unlocked = x.unlocked }).ToList();
    }
    public ItemDatum GetItemDatumById(string id)
    {
        return list?.FirstOrDefault(x => string.Compare(x.id, id) == 0);
    }
    public ItemDatum GetItemByType(eItemType type)
    {
        return list?.FirstOrDefault(x => x.itemProp.Type == type);
    }
    public void UnlockNewItem(eItemType type)
    {
        var item = GetItemByType(type);
        if (item == null || item.unlocked)
            return;
        item.unlocked = true;
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    [ButtonMethod]
    public void ResetData()
    {
        list.Clear();

        for (int i = 0; i < itemModels.Count; i++)
        {
            var datum = new ItemDatum()
            {
                id = itemModels[i].name.ToLower(),
                unlocked = i < 18,
                itemProp = itemModels[i],
                earnValue = 5 + i/10,
                unlockValue = 100,
                cleanFee = 3 + i / 10,
            };
            list.Add(datum);
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }
}

[System.Serializable]
public class ItemDatum
{
    public string id;
    public bool unlocked;
    public Goods_Item itemProp;
    public int earnValue;
    public int unlockValue;
    public int cleanFee;
}
