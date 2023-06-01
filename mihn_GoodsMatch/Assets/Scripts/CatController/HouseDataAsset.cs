using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "HouseDataAsset", menuName = "DataAsset/HouseDataAsset")]
public class HouseDataAsset : ScriptableObject
{
    [SerializeField]
    public List<HouseFloorData> allFloorData;

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
    
    public List<ItemDecorData> allDecorationItems;
    public List<houseCatData> allCats;

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
public class ItemDecorData
{
    public int index;
    public Sprite icon;
    public bool isUnlocked;
    public UnlockType unlockType;
    public int price;
}

[System.Serializable]
public class houseCatData : ItemDecorData
{
    public string skinName;
}

