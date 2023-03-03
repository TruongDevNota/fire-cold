using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MyBox;
using UnityEditor;

[CreateAssetMenu(fileName = "ItemDefinitionAsset", menuName = "DataAsset/ItemDefinitionAsset")]
public class ItemDefinitionAsset : ScriptableObject
{
    [Header("Item Definitions")]
    [SerializeField] List<ItemDefinitions> definitions;
    [SerializeField] List<GameObject> itemPrefabs;

    public List<ItemDefinitions> pDefinitions { get => definitions; }
    public List<ItemDefinitions> unlockedList
    {
        get
        {
            return definitions?.Where(x => x.unlocked).ToList();
        }
    }
    public List<ItemDefinitions> itemSaveList
    {
        get => definitions.Where(x => x.unlocked)
            .Select(x => new ItemDefinitions { id = x.id, unlocked = x.unlocked }).ToList();
    }

    public ItemDefinitions GetDefinitionByType(eItemType type)
    {
        return definitions.FirstOrDefault(x => x.itemType == type);
    }

    [ButtonMethod]
    public void ResetData()
    {
        definitions.Clear();
        
        for (int i = 0; i< itemPrefabs.Count; i++)
        {
            var datum = itemPrefabs[i].GetComponent<Goods_Item>();
            var newDefinition = new ItemDefinitions()
            {
                id = itemPrefabs[i].name.ToLower(),
                unlocked = i < 30,
                itemType = datum.Type,
                itemPrefabs = itemPrefabs[i],
                matchAmount = datum.matchAmount,
                slotNeed = (int)datum.size.x,
            };
            definitions.Add(newDefinition);
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }
}

[System.Serializable]
public class ItemDefinitions
{
    public string id;
    public eItemType itemType;
    public bool unlocked;
    public GameObject itemPrefabs;
    public int matchAmount;
    public int slotNeed;
}