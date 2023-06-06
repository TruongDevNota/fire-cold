using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "HouseDataAsset", menuName = "DataAsset/HouseDataAsset")]
public class HouseDataAsset : ScriptableObject
{
    [SerializeField]
    public List<HouseFloorData> allFloorData;

    public void UnlockFloorByIndex(int index)
    {
        allFloorData.FirstOrDefault(x => x.floorIndex == index).isUnlocked = true;
        SetDirtyAsset();
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
        return allFloorData.Select(x => x.GetSaveData()).OrderBy(x => x.index).ToList();
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
            allFloorData[i].isUnlocked = i == 0;
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

public class FloorSaveData
{
    public int index;
    public bool unlocked;
    public List<int> catUnlocked = new List<int>();
    public List<int> itemDecorUnlocked = new List<int>();
}

[System.Serializable]
public class HouseFloorData
{
    public int floorIndex;
    public bool isUnlocked;
    public int unlockPrice;
    public string name;
    
    public List<ItemDecorData> allDecorationItems;
    public List<houseCatData> allCats;

    public int itemUnlockedCount => allDecorationItems.Where(x => x.isUnlocked).Count();
    public int catUnlockedCount => allCats.Where(x => x.isUnlocked).Count();

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
            catUnlocked = allCats.Where(item => item.isUnlocked).Select(x => x.index).ToList(),
            itemDecorUnlocked = allDecorationItems.Where(item => item.isUnlocked).Select(x => x.index).ToList(),
        };
    }

    public void ConvertData(FloorSaveData saveData = null)
    {
        if (saveData == null)
            return;
        
        this.isUnlocked = saveData.unlocked;
        foreach(var item in allDecorationItems)
        {
            if(saveData.itemDecorUnlocked.Contains(item.index))
                item.isUnlocked = true;
        }
        foreach (var item in allCats)
        {
            if (saveData.catUnlocked.Contains(item.index))
                item.isUnlocked = true;
        }
    }

    public void ResetData()
    {
        for(int i = 0; i < this.allCats.Count; i++)
        {
            this.allCats[i].index = i;
            this.allCats[i].isUnlocked = false;
        }
        for(int i = 0; i < allDecorationItems.Count; i++)
        {
            this.allDecorationItems[i].index = i;
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
}

[System.Serializable]
public class houseCatData : ItemDecorData
{
    public string skinName;
}

