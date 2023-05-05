using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapAsset", menuName = "DataAsset/MapAsset")]

public class MapData : ScriptableObject
{
    public int totalMap;
    public List<Map> listMaps;
    [ButtonMethod]
    public void ConfigMap()
    {
        for (int i = 1; i <= totalMap; i++)
        {
            var map = new Map();
            map.mapindex = i;
            map.totalLevel = 30;
            listMaps.Add(map);
        }

        for (int i = 0; i < listMaps.Count; i++)
        {
            listMaps[i].Config();
        }

    }
    [ButtonMethod]
    public void Reset()
    {
        for (int i = 0; i < listMaps.Count; i++)
        {
            listMaps[i].Lock();
        }
    }
}

[Serializable]
public class Map
{
    public int mapindex;
    public int totalLevel;
    public List<Level> listLevels = new List<Level>();
    public void Config()
    {
        for (int i = 1; i <= totalLevel; i++)
        {
            var lv = new Level();
            lv.levelindex = i;
            lv.isUnlocked = i == 1;
            lv.isSelecting = i == 1;
            lv.levelStars = 0;
            listLevels.Add(lv);
        }
    }
    public void Lock()
    {
        for (int i = 0; i < listLevels.Count; i++)
        {
            listLevels[i].isUnlocked = i == 0;
            listLevels[i].isSelecting = i == 0;
            listLevels[i].levelStars = 0;
        }
    }
}
[Serializable]
public class Level
{
    public int levelindex;
    public bool isUnlocked;
    public bool isSelecting;
    public int levelStars;
}
