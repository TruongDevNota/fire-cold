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

    }
}

[Serializable]
public class Map
{
    public int mapindex;
    public int totalLevel;
    public LevelAsset levelAsset;
}