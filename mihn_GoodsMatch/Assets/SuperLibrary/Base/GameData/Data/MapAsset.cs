using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "MapAsset", menuName = "DataAsset/MapAsset")]

public class MapAsset : ScriptableObject
{
    public int totalMap;
    [SerializeField]
    public List<MapData> ListMap = new List<MapData>();
    [SerializeField]
    public List<Sprite> mapIcons = new List<Sprite>();
    
    public void ConverToData(List<MapData> saveData)
    {
        foreach (var i in saveData)
        {
            var temp = ListMap.FirstOrDefault(x => x.mapIndex == i.mapIndex);
            if (temp != null)
            {
                temp.mapIndex = i.mapIndex;
                temp.totalLevel = i.totalLevel;
                temp.hightestLevelUnlocked = i.hightestLevelUnlocked;
                temp.levelStars.Clear();
                temp.levelStars = i.levelStars;
                temp.mapName = i.mapName;
            }
        }
    }

    public void UnlockAllLevel()
    {
        foreach (var map in ListMap)
        {
            map.hightestLevelUnlocked = map.totalLevel;
            map.levelStars = new List<int>();
            for(int i = 0; i < map.totalLevel; i++)
            {
                map.levelStars.Add(3);
            }
        }
        SaveChangeOnEditor();
    }

    [ButtonMethod]
    public void ResetAndUpdateToBuild()
    {
        foreach(var map in ListMap)
        {
            map.hightestLevelUnlocked = map.mapIndex == 1 ? 1 : 0;
            map.levelStars = new List<int>();
            if(map.mapIndex == 1)
                map.levelStars.Add(0);
        }
        SaveChangeOnEditor();
    }

    [ButtonMethod]
    public void CreatMaps()
    {
        ListMap.Clear();
        for (int i = 1; i <= totalMap; i++)
        {
            var map = new MapData();
            map.mapIndex = i;
            map.totalLevel = 20;
            map.hightestLevelUnlocked = map.mapIndex == 1 ? 1 : 0;
            ListMap.Add(map);
        }
        SaveChangeOnEditor();
    }

    public void SaveChangeOnEditor()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

[Serializable]
public class MapData
{
    public int mapIndex;
    public string mapName;
    public int totalLevel;
    public bool isUnlocked => hightestLevelUnlocked > 0;
    public int hightestLevelUnlocked;
    public List<int> levelStars;

    public int GetAllStarClaimed()
    {
        var count = 0;
        foreach (var star in levelStars)
            count += star;
        return count;
    }

    public void DoUnlockAllLevel()
    {
        hightestLevelUnlocked = totalLevel;
        levelStars = new List<int>();
        for (int i = 0; i < totalLevel; i++)
        {
            levelStars.Add(3);
        }
    }
}