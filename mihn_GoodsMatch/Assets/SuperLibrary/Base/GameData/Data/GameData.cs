﻿using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public UserData user = new UserData();
    public List<MapData> listMaps = new List<MapData>();
    public List<FloorSaveData> floorsData = new List<FloorSaveData>();
    //public List<SaveData> walls = new List<SaveData>();
    //public List<SaveData> windows = new List<SaveData>();
    //public List<SaveData> floors = new List<SaveData>();
    //public List<SaveData> ceillings = new List<SaveData>();
    //public List<SaveData> carpets = new List<SaveData>();
    //public List<SaveData> chairs = new List<SaveData>();
    //public List<SaveData> tables = new List<SaveData>();
    //public List<SaveData> lamps = new List<SaveData>();
}
