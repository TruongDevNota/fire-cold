using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelConfig
{
    public eGameMode gameMode;
    public int time;
    public List<float> rowsSpeed;

    public LevelConfig()
    {
        gameMode = eGameMode.Normal;
        time = 0;
        rowsSpeed = new List<float>();
    }
}
