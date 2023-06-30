using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "HouseDataAsset", menuName = "DataAsset/HouseDataAsset")]
public class HouseDataAsset : ScriptableObject
{
    [SerializeField]
    public List<HouseFloorData> allFloorData = new List<HouseFloorData>();

    //[SerializeField]
    //public List<FloorSaveData> floorSaveData = new List<FloorSaveData>();

    public bool CheckFoorUnlockable(int index)
    {
        if(index < 1 || index > allFloorData.Count)
            return false;
        return index == 1 || allFloorData[index-1].isUnlocked || allFloorData[index - 2].CanUnlockNextFoor();
    }

    public void UnlockFloorByIndex(int index)
    {
        allFloorData.FirstOrDefault(x => x.floorIndex == index).isUnlocked = true;
        SetDirtyAsset();
        DataManager.Save();
    }

    public void UnLockItem(int index, string id, eHouseDecorType type)
    {
        GetItemData(index, id, type).isUnlocked = false;
        SetDirtyAsset();
        DataManager.Save();
    }

    public HouseFloorData GetFloorDataByIndex(int index)
    {
        return allFloorData.FirstOrDefault(x => x.floorIndex == index);
    }

    public ItemDecorData GetItemData(int floorIndex, string id, eHouseDecorType type)
    {
        return allFloorData.FirstOrDefault(x => x.floorIndex == floorIndex).GetItemData(id, type);
    }

    public List<FloorSaveData> GetSaveData()
    {
        var floorSaveData = new List<FloorSaveData>();
        floorSaveData = allFloorData.Select(x => x.GetSaveData()).OrderBy(x => x.index).ToList();
        return floorSaveData;
    }

    public void ConvertData(List<FloorSaveData> data)
    {
        foreach(var floor in allFloorData)
        {
            floor.ConvertData(data.FirstOrDefault(x => x.index == floor.floorIndex));
        }
    }

    [MyBox.ButtonMethod]
    public void ResetData()
    {
        for(int i = 0; i < allFloorData.Count; i++)
        {
            allFloorData[i].floorIndex = i + 1;
            allFloorData[i].isUnlocked = false;
            allFloorData[i].unlockCountRequire = 5;
            allFloorData[i].ResetData();
        }

        SetDirtyAsset();
    }

    private void SetDirtyAsset()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

[System.Serializable]
public class FloorSaveData
{
    public int index;
    public bool unlocked;
    public List<string> catUnlocked;
    public List<string> itemDecorUnlocked;
}

[System.Serializable]
public class HouseFloorData
{
    public int floorIndex;
    public bool isUnlocked;
    public int unlockPrice;
    public string name;
    public int unlockCountRequire;

    public List<ItemDecorData> allDecorationItems;
    public List<HouseCatData> allCats;

    public int itemUnlockedCount => allDecorationItems.Where(x => x.isUnlocked).Count();
    public int catUnlockedCount => allCats.Where(x => x.isUnlocked).Count();
    public bool CanUnlockNextFoor()
    {
        return itemUnlockedCount >= unlockCountRequire;
    }
    public List<Sprite> GetAllSprite()
    {
        var allSprites = allDecorationItems.Select(x => x.thumb).ToList();
        foreach (var item in allCats)
            allSprites.Add(item.thumb);
        return allSprites;
    }
    public ItemDecorData GetItemData(string id, eHouseDecorType type)
    {
        if (type == eHouseDecorType.Item)
            return allDecorationItems.FirstOrDefault(x => string.Compare(x.id, id) == 0);
        else
            return allCats.FirstOrDefault(x => string.Compare(x.id, id) == 0);
    }
    public FloorSaveData GetSaveData()
    {
        return new FloorSaveData()
        {
            index = this.floorIndex,
            unlocked = this.isUnlocked,
            catUnlocked = allCats.Where(item => item.isUnlocked).Select(x => x.id).ToList(),
            itemDecorUnlocked = allDecorationItems.Where(item => item.isUnlocked).Select(x => x.id).ToList(),
        };
    }
    public void ConvertData(FloorSaveData saveData = null)
    {
        if (saveData == null)
            return;
        
        this.isUnlocked = saveData.unlocked;
        foreach(var item in allDecorationItems)
        {
            if(saveData.itemDecorUnlocked.Contains(item.id))
                item.isUnlocked = true;
        }
        foreach (var item in allCats)
        {
            if (saveData.catUnlocked.Contains(item.id))
                item.isUnlocked = true;
        }
    }
    public void ResetData()
    {
        for(int i = 0; i < this.allCats.Count; i++)
        {
            allCats[i].type = eHouseDecorType.Cat;
            this.allCats[i].isUnlocked = false;
            allCats[i].SetIndexAndId(this.floorIndex, i + 1, eHouseDecorType.Cat);
        }
        for(int i = 0; i < allDecorationItems.Count; i++)
        {
            allDecorationItems[i].SetIndexAndId(this.floorIndex, i+1, eHouseDecorType.Item);
            allDecorationItems[i].type = eHouseDecorType.Item;
            this.allDecorationItems[i].isUnlocked = false;
        }
    }
}

[System.Serializable] 
public class ItemDecorData : SaveData
{
    public Sprite thumb;
    public int floorIndex;
    public eHouseDecorType type;

    public void SetIndexAndId(int floorIndex, int itemIndex, eHouseDecorType type)
    {
        this.type = type;
        this.floorIndex = floorIndex;
        this.index = itemIndex;
        this.id = type == eHouseDecorType.Item ? $"floor_{floorIndex}-item_{itemIndex}" : $"floor_{floorIndex}-cat_{itemIndex}";
        if (thumb == null)
            Debug.LogError($"Decor Asset item type = {this.type}, id = {id} miss sprite");
        this.name = type == eHouseDecorType.Item ? thumb.name.ToLower() : $"cat_{thumb.name.ToLower()}";
        //Demo data
        this.unlockType = itemIndex % 2 == 0 ? UnlockType.Ads : UnlockType.Gold;
        this.unlockPrice = unlockType == UnlockType.Ads ? 1 : itemIndex * 100;
    }
}

[System.Serializable]
public class HouseCatData : ItemDecorData
{
    public string skinName;
}

