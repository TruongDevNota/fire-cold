using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapDatum
{
    public List<LineDatum> lines;
    public MapDatum()
    {
        lines = new List<LineDatum>();
    }
}

[System.Serializable]
public class LineDatum
{
    public List<string> lineSheves;
}

[System.Serializable]
public class ShelfDatum
{
    public List<eItemType> items;
}
