using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelConfig
{
    public eGameMode gameMode;
    public int time;
    public List<float> rowsSpeed;
    public bool hasOrderMode;

    #region Bartender
    public int bar_MaxMissing;
    #endregion

    public LevelConfig()
    {
        gameMode = eGameMode.Normal;
        time = 0;
        rowsSpeed = new List<float>();
        bar_MaxMissing = 0;
    }
}

[System.Serializable]
public class LevelConfigData
{
    public LevelConfig config;
    public MapDatum mapDatum;

    public LevelConfigData()
    {
        config = new LevelConfig();
        mapDatum = new MapDatum();
    }
}