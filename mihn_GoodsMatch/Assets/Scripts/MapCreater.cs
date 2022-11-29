using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCreater : MonoBehaviour
{
    [SerializeField] bool isTest = true;
    [SerializeField] SampleMapAsset sampleMapDataAsset;

    [SerializeField] ItemDefinitionAsset itemDefinitionAsset;
    [SerializeField] Vector2 mapUnitSize = Vector2.one;

    private MapDatum currentMapDatum;

    public void CreateMap(MapDatum datum = null)
    {
        currentMapDatum = isTest ? sampleMapDataAsset.sampleMap : datum;
        if (currentMapDatum == null || currentMapDatum.lines == null || currentMapDatum.lines.Count < 1)
            return;

        //var linesPositionY = new float[currentMapDatum.lines.Count];
        int midIndexY = Mathf.FloorToInt((currentMapDatum.lines.Count - 1) * 0.5f);
        for (int i = 0; i < currentMapDatum.lines.Count; i++)
        {
            var line = currentMapDatum.lines[i];
            if (line.lineSheves == null || line.lineSheves.Count == 0)
                continue;

            var linesPosition = (midIndexY - i) * mapUnitSize.y;
            
        }

    }
}
