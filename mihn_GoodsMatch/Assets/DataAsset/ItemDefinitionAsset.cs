using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDefinitionAsset", menuName = "DataAsset/ItemDefinitionAsset")]
public class ItemDefinitionAsset : ScriptableObject
{
    [Header("Item Definitions")]
    [SerializeField] List<ItemDifinition> definitions;

    public List<ItemDifinition> pDefinitions { get => definitions; }
}

[System.Serializable]
public class ItemDifinition
{
    public string id;
    public eItemType itemType;
    public GameObject itemPrefabs;
    public int matchAmount;
    public int slotNeed;
    public int pointEarn;
    public int levelUnlock;
}