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
                index = i,
                itemType = datum.Type,
                itemPrefabs = itemPrefabs[i],
                matchAmount = datum.matchAmount,
                slotNeed = (int)datum.size.x,
            };
            definitions.Add(newDefinition);
        }
        EditorUtility.SetDirty(this);
    }
}

[System.Serializable]
public class ItemDefinitions
{
    public string id;
    public int index;
    public eItemType itemType;
    public GameObject itemPrefabs;
    public int matchAmount;
    public int slotNeed;
}