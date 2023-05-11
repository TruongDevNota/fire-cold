using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapAsset", menuName = "DataAsset/MapAsset")]

public class MapAsset : ScriptableObject
{
    public int totalMap;
    [SerializeField]
    private List<MapData> listMaps = new List<MapData>();
    [SerializeField]
    public List<Sprite> mapIcon = new List<Sprite>();

    public List<MapData> ListMap { get { return listMaps; } }


    [ButtonMethod]
    public void ResetAndUpdateToBuild()
    {
        foreach(var map in listMaps)
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
        listMaps.Clear();
        for (int i = 1; i <= totalMap; i++)
        {
            var map = new MapData();
            map.mapIndex = i;
            map.totalLevel = 20;
            map.hightestLevelUnlocked = map.mapIndex == 1 ? 1 : 0;
            listMaps.Add(map);
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
}