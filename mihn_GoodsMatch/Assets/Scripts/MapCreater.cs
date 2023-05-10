using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using MyBox;
using UnityEngine.UI;

public class MapCreater : MonoBehaviour
{
    [Header("Test Map Data")]
    [SerializeField] bool isMove = true;
    [SerializeField] TextAsset sampleMapTextAsset;

    [Header("Map Create Config")]
    [SerializeField] GameObject shelfBasePrefab;
    [SerializeField] Vector2 mapUnitSize = Vector2.one;
    [SerializeField] Vector2 shelfUnitSize = new Vector2(1f, 0.5f);
    [SerializeField] float shelfDistance = 0.5f;
    [SerializeField] int defautShelfSize = 3;

    [Header("Challenge")]
    [SerializeField] eMapMovingType testMoveType;
    [SerializeField] float startSpawX;
    [SerializeField] float moveLeftSpeed;
    [SerializeField] float moveRightSpeed;

    [Header("Camera Config")]
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera inGameUICam;
    [SerializeField] Vector2 cameraSizeOffset;
    [SerializeField] float[] smallCameraSizeOffsets;
    [SerializeField] float[] largeCameraSizeOffsets;

    private MapDatum currentMapDatum;
    public List<Goods_Item> itemCreated = new List<Goods_Item>();

    public void ChangeCameraSize(int mapCollume, int mapRow)
    {
        float screenRatio = Screen.height / Screen.width;

        //mainCamera.orthographicSize = mapCollume < 3 ? cameraSizeOffset.x : cameraSizeOffset.y;
        var camSize = smallCameraSizeOffsets[0];
        if (mapCollume < 3 && mapRow < 10)
        {
            camSize = screenRatio < 2f ? smallCameraSizeOffsets[0] : smallCameraSizeOffsets[1];
            //mainCamera.transform.position = Vector3.zero;
        }
        else
        {
            camSize = screenRatio < 2f ? largeCameraSizeOffsets[0] : largeCameraSizeOffsets[1];
            //mainCamera.transform.position = screenRatio < 2f ? new Vector3(0, 1f, 0) : Vector3.zero;
        }
        mainCamera.orthographicSize = camSize;
        inGameUICam.orthographicSize = camSize;
    }

    public void CreateMap()
    {
        int maxColumn = 0;
        currentMapDatum = DataManager.currLevelconfigData.mapDatum;
        if (currentMapDatum == null || currentMapDatum.lines == null || currentMapDatum.lines.Count < 1)
            return;

        //var linesPositionY = new float[currentMapDatum.lines.Count];
        Debug.Log($"READ MAP DATA - TOTAL LINES: {currentMapDatum.lines.Count}");
        int midIndexY = Mathf.FloorToInt((currentMapDatum.lines.Count - 1) * 0.5f);
        for (int i = 0; i < currentMapDatum.lines.Count; i++)
        {
            var line = currentMapDatum.lines[i];
            if (line.lineSheves == null || line.lineSheves.Count == 0)
                continue;
            maxColumn = Mathf.Max(maxColumn, line.lineSheves.Count);
            var linesPosition = (midIndexY - i) * mapUnitSize.y;
            Debug.Log($"lineSheves.Count: {line.lineSheves.Count}");

            var leftMargin = -(defautShelfSize * line.lineSheves.Count + shelfDistance * (line.lineSheves.Count - 1)) * 0.5f;
            var rightMargin = leftMargin + (line.lineSheves.Count - 1) * (defautShelfSize + shelfDistance);
            var lineSpeed = DataManager.currLevelconfigData.config.rowsSpeed == null ? 0 : i < DataManager.currLevelconfigData.config.rowsSpeed.Count ? DataManager.currLevelconfigData.config.rowsSpeed[i] : 0;
            for (int i2 = 0; i2 < line.lineSheves.Count; i2++)
            {
                if (string.IsNullOrEmpty(line.lineSheves[i2]) || line.lineSheves[i2].Contains("-"))
                    continue;

                var itemTypes = ConvertToShelfDatum(line.lineSheves[i2]);
                float posX = leftMargin + i2 * (defautShelfSize + shelfDistance);
                
                var newShelf = shelfBasePrefab.Spawn().GetComponent<ShelfUnit>();
                newShelf.transform.localScale = Vector3.one;
                newShelf.transform.position = new Vector2(posX, linesPosition);
                newShelf.MoveDir = lineSpeed == 0 ? eMapMovingType.None : lineSpeed < 0 ? eMapMovingType.Left : eMapMovingType.Right;
                newShelf.movingSpeed = lineSpeed;

                newShelf.teleportPosX = lineSpeed < 0 ? leftMargin - (defautShelfSize + shelfDistance) : rightMargin + (defautShelfSize + shelfDistance);
                newShelf.reStarPosX = lineSpeed < 0 ? rightMargin : leftMargin;
                
                newShelf.cellAmount = itemTypes.Count;
                newShelf.InitCells();
                newShelf.transform.parent = transform;
                for (int i3 = 0; i3 < itemTypes.Count; i3++)
                {
                    if (itemTypes[i3] == 0)
                        continue;
                    
                    var itemDatum = DataManager.ItemsAsset.GetItemByIndex(itemTypes[i3]);
                    
                    if (itemDatum == null)
                    {
                        Debug.Log($"Could not found item definition of type: [{itemTypes[i3]}]");
                        return;
                    }
                    var newItem = itemDatum.itemProp.Spawn();
                    newItem.pFirstLeftCellIndex = i3;
                    newShelf.DoPutItemFromWareHouse(newItem);
                    newItem.transform.parent = newShelf.transform;
                    BoardGame.instance.items.Add(newItem);
                }
                BoardGame.instance.shelves.Add(newShelf);
            }
        }
        ChangeCameraSize(maxColumn, currentMapDatum.lines.Count);
    }

    public List<int> ConvertToShelfDatum(string shelfText)
    {
        Debug.Log($"shelf text: {shelfText}");
        var datum = new List<int>();
        try
        {
            var numbers = shelfText?.Split(GameConstants.itemSplittChar)?.Select(Int32.Parse)?.ToList();

            foreach(var number in numbers)
            {
                datum.Add((number));
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        return datum;
    }

    public LevelConfig ConvertToLevelConfig(string datum)
    {
        var config = new LevelConfig();

        var listDatum = datum.Split(GameConstants.shelfSplitChars).Where(x => x.Length > 0).ToList();

        config.gameMode = (eGameMode)int.Parse(listDatum[0]);
        Debug.Log($"ConvertToLevelConfig - gamemode: {config.gameMode}");

        config.time = int.Parse(listDatum[1]);
        Debug.Log($"ConvertToLevelConfig - level eslap time: {config.time}");

        if (listDatum.Count > 2 && listDatum[2].Length>0)
        {
            var rowSpeed = listDatum[2].Split(GameConstants.itemSplittChar).Where(x => x.Length > 0).ToList();
            foreach(var s in rowSpeed)
            {
                config.rowsSpeed.Add(float.Parse(s));
                Debug.Log($"ConvertToLevelConfig - rows speed: {float.Parse(s)}");
            }
        }

        return config;
    }
}
