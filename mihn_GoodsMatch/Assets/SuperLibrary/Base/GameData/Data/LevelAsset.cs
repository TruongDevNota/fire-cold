using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "LevelAsset", menuName = "DataAsset/LevelAsset")]

public class LevelAsset : BaseAsset<LevelData>
{
    public List<LevelData> saveList
    {
        get => list.Where(x => x.isUnlocked || x.levelStars > 0)
            .Select(x => new LevelData { id = x.id, isUnlocked = x.isUnlocked, isSelected = x.isSelected, count = x.count, unlockPay = x.unlockPay, levelStars = x.levelStars }).ToList();
    }

    [ButtonMethod]
    public void InitLevelStar()
    {
        list.Clear();
        for(int i = 0; i <= 100; i++)
        {
            list.Add(new LevelData() { levelStars = 0, id = (i+1).ToString(), index = i + 1 });
        }
        SetDirty();
    }

    public override void ResetData()
    {
        base.ResetData();
        for (int i = 0; i < list.Count; i++)
        {
            list[i].levelStars = 0;
        }
    }

    public int GetLevelStar(int level)
    {
        return level < list.Count ? list[level].levelStars : 0;
    }

    public void UpdateLevelStar(int levelIndex, int stars)
    {
        list[levelIndex].levelStars = Mathf.Max(stars, list[levelIndex].levelStars);
        list[levelIndex].isUnlocked = true;
        SetDirty();
    }

    public void ConvertLevelStars(List<LevelData> saveData)
    {
        foreach (var i in saveData)
        {
            var temp = list.FirstOrDefault(x => x.id == i.id);
            if (temp != null)
            {
                temp.count = i.count;
                temp.unlockPay = i.unlockPay;
                temp.levelStars = i.levelStars;
                if (i.isUnlocked)
                    temp.isUnlocked = i.isUnlocked;
                if (i.isSelected)
                    temp.isSelected = i.isSelected;
            }
        }
    }
}

[System.Serializable]
public class LevelData : SaveData
{
    [Header("LevelStars")]
    public int levelStars;
}
