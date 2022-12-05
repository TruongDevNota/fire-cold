using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using MyBox;

public class MapCreater : MonoBehaviour
{
    [Header("Test Map Data")]
    [SerializeField] bool isTest = true;
    [SerializeField] TextAsset sampleMapTextAsset;

    [Header("Map Create Config")]
    [SerializeField] ItemDefinitionAsset itemDefinitionAsset;
    [SerializeField] GameObject shelfBasePrefab;
    [SerializeField] Vector2 mapUnitSize = Vector2.one;
    [SerializeField] Vector2 shelfUnitSize = new Vector2(1f, 0.5f);
    [SerializeField] float shelfDistance = 0.5f;

    [Header("Camera Config")]
    [SerializeField] Camera mainCamera;
    [SerializeField] Vector2 cameraSizeOffset;
    [SerializeField] float[] smallCameraSizeOffsets;
    [SerializeField] float[] largeCameraSizeOffsets;

    private MapDatum currentMapDatum;
    public List<Goods_Item> itemCreated = new List<Goods_Item>();

    [ButtonMethod] public void TestMapCreat()
    {
        CreateMapFromTextAsset(sampleMapTextAsset.text);
    }

    public void ChangeCameraSize(int mapCollume)
    {
        float screenRatio = Screen.height / Screen.width;

        //mainCamera.orthographicSize = mapCollume < 3 ? cameraSizeOffset.x : cameraSizeOffset.y;

        if(mapCollume < 3)
        {
            mainCamera.orthographicSize = screenRatio < 2f ? smallCameraSizeOffsets[0] : smallCameraSizeOffsets[1];
            //mainCamera.transform.position = Vector3.zero;
        }
        else
        {
            mainCamera.orthographicSize = screenRatio < 2f ? largeCameraSizeOffsets[0] : largeCameraSizeOffsets[1];
            //mainCamera.transform.position = screenRatio < 2f ? new Vector3(0, 1f, 0) : Vector3.zero;
        }

    }

    public void CreateMapFromTextAsset(string mapData)
    {
        var data = ReadMapTextData(mapData);
        CreateMap(data);
    }

    public void CreateMap(MapDatum datum)
    {
        int maxColumn = 0;
        currentMapDatum = datum;
        if (currentMapDatum == null || currentMapDatum.lines == null || currentMapDatum.lines.Count < 1)
            return;

        //var linesPositionY = new float[currentMapDatum.lines.Count];
        int midIndexY = Mathf.FloorToInt((currentMapDatum.lines.Count - 1) * 0.5f);
        for (int i = 0; i < currentMapDatum.lines.Count; i++)
        {
            var line = currentMapDatum.lines[i];
            if (line.lineSheves == null || line.lineSheves.Count == 0)
                continue;
            maxColumn = Mathf.Max(maxColumn, line.lineSheves.Count);
            var linesPosition = (midIndexY - i) * mapUnitSize.y;
            Debug.Log($"lineSheves.Count: {line.lineSheves.Count}");
            for (int i2 = 0; i2 < line.lineSheves.Count; i2++)
            {
                if (string.IsNullOrEmpty(line.lineSheves[i2]) || line.lineSheves[i2].Contains("-"))
                    continue;

                var itemTypes = ConvertToShelfDatum(line.lineSheves[i2]);
                var sizeX = itemTypes.Count * mapUnitSize.x;
                float posX = -(sizeX * line.lineSheves.Count + shelfDistance * (line.lineSheves.Count - 1)) * 0.5f + i2 * (sizeX + shelfDistance);
                var newShelf = GameObject.Instantiate(shelfBasePrefab).GetComponent<ShelfUnit>();
                newShelf.transform.localScale = Vector3.one;
                newShelf.transform.position = new Vector2(posX, linesPosition);
                newShelf.cellAmount = itemTypes.Count;
                newShelf.InitCells();
                newShelf.transform.parent = transform;
                for (int i3 = 0; i3 < itemTypes.Count; i3++)
                {
                    if (itemTypes[i3] == eItemType.None)
                        continue;
                    var definition = itemDefinitionAsset.pDefinitions.FirstOrDefault(x => x.itemType == itemTypes[i3]);
                    if(definition == null)
                    {
                        Debug.Log($"Could not found item definition of type: [{itemTypes[i3]}]");
                        return;
                    }
                    var newItem = GameObject.Instantiate(definition.itemPrefabs).GetComponent<Goods_Item>();
                    newItem.pFirstLeftCellIndex = i3;
                    newShelf.DoPutItemFromWareHouse(newItem);
                    newItem.transform.parent = transform;
                    BoardGame.instance.items.Add(newItem);
                }
            }
        }
        ChangeCameraSize(maxColumn);
        //BoardGame.instance.ItemCount = itemCreated.Count;
    }

    public MapDatum ReadMapTextData(string content)
    {
        var newMap = new MapDatum();
        newMap.lines = new List<LineDatum>();

        List<string> lines = content.Split(new char[] { '\n' }).ToList();

        foreach(var line in lines)
        {
            var lineDatum = new LineDatum();
            lineDatum.lineSheves = line.Split(new char[] { ','}).ToList();
            newMap.lines.Add(lineDatum);
        }

        return newMap;
    }

    public List<eItemType> ConvertToShelfDatum(string shelfText)
    {
        Debug.Log($"shelf text: {shelfText}");
        var datum = new List<eItemType>();
        try
        {
            var numbers = shelfText?.Split('|')?.Select(Int32.Parse)?.ToList();

            foreach(var number in numbers)
            {
                datum.Add((eItemType)Enum.ToObject(typeof(eItemType), number));
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        return datum;
    }
}
