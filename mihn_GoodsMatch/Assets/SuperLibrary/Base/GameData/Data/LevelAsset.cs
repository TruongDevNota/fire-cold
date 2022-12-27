using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "LevelAsset", menuName = "DataAsset/LevelAsset")]

public class LevelAsset : BaseAsset<LevelData>
{
    [ButtonMethod]
    public void InitLevelStar()
    {
        list.Clear();
        for(int i = 0; i < 100; i++)
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
        SetDirty();
    }
}

[System.Serializable]
public class LevelData : SaveData
{
    [Header("LevelStars")]
    public int levelStars;
}
