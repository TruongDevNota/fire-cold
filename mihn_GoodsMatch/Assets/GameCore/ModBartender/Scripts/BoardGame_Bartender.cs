using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public class BoardGame_Bartender : MonoBehaviour
{
    [SerializeField] GameItemAsset gameItemAsset;

    private bool isAlert = false;

    [Header("Draging")]
    [SerializeField] public Vector3 touchPositionOffset;
    private Goods_Item dragingItem = null;
    private bool isDraggingItem = false;

    [Header("Items List")]
    public List<Goods_Item> items = new List<Goods_Item>();
    public List<ShelfUnit> shelves = new List<ShelfUnit>();
    private List<eItemType> typeGroup1 = new List<eItemType>();
    private List<eItemType> typeGroup2 = new List<eItemType>();
    private List<eItemType> typpGroup3 = new List<eItemType>();
    private List<eItemType> typeGroup4 = new List<eItemType>();

    private void Awake()
    {
        GameStateManager.OnStateChanged += OnGameStateChangeHandler;
    }
    private void OnDestroy()
    {
        GameStateManager.OnStateChanged -= OnGameStateChangeHandler;
    }

    private void Start()
    {
        
    }

    private void OnGameStateChangeHandler(GameState current, GameState last, object data)
    {
        if (current == GameState.Restart || current == GameState.Init)
        {
            isAlert = false;
            StartCoroutine(YieldInit(0));
        }
        if (current == GameState.Pause)
        {

        }
        if (current == GameState.Play)
        {

        }
        if (current == GameState.RebornContinue)
        {
            isAlert = false;
            
            GameStateManager.Play(null);
        }
    }

    private IEnumerator YieldInit(int level)
    {
        int emptyCount = 0;
        int startShelfIndex = UnityEngine.Random.Range(0, shelves.Count);

        foreach(var shelf in shelves)
            shelf.InitCells();
        yield return new WaitForEndOfFrame();
        typeGroup1.Clear();
        typeGroup2.Clear();
        typpGroup3.Clear();
        typeGroup4.Clear();
        foreach (var item in items)
        {
            if(item != null)
                item.Recycle();
        }
        items.Clear();
        yield return new WaitForEndOfFrame();
        var allItemUnlocked = NextList(gameItemAsset.unlockedList);
        InitItems((List<ItemDatum>)allItemUnlocked);
        yield return new WaitForEndOfFrame();

        for(int i = 0; i < items.Count; i++)
        {
            if(emptyCount < 3)
            {
                int emptyChange = Random.Range(0, 100);
                if(emptyChange < 10)
                {
                    emptyCount++;
                    startShelfIndex += 2;
                }
            }

            var index = shelves[startShelfIndex % shelves.Count].CheckItemFitOnShelf(1, 0);
            while (index < 0)
            {
                startShelfIndex += 2;
                index = shelves[startShelfIndex % shelves.Count].CheckItemFitOnShelf(1, 0);
            }

            var newItem = items[i].Spawn();
            newItem.pFirstLeftCellIndex = index;
            shelves[startShelfIndex % shelves.Count].DoPutNewItem(newItem);
            newItem.transform.parent = shelves[startShelfIndex % shelves.Count].transform;
            startShelfIndex += 2;
            yield return new WaitForEndOfFrame();
        }

        GameStateManager.Ready(null);
        UIToast.Hide();
    }

    private void InitItems(List<ItemDatum> allItemUnlocked)
    {
        for (int i = 0; i < allItemUnlocked.Count; i++)
        {
            if (i < 4)
            {
                typeGroup1.Add(allItemUnlocked[i].itemProp.Type);
                AddItemsToList(allItemUnlocked[i].itemProp, 3);
            }
            else if (i < 8)
            {
                typeGroup2.Add(allItemUnlocked[i].itemProp.Type);
                AddItemsToList(allItemUnlocked[i].itemProp, 2);
            }
            else if (i < 12)
            {
                typpGroup3.Add(allItemUnlocked[i].itemProp.Type);
                AddItemsToList(allItemUnlocked[i].itemProp, 1);
            }
            else
                typeGroup4.Add(allItemUnlocked[i].itemProp.Type);
        }
    }
    private void AddItemsToList(Goods_Item item, int amount = 1)
    {
        for (int i = 0; i < amount; i++)
            items.Add(item);
    }

    public static IList<T> NextList<T>(IEnumerable<T> source)
    {
        System.Random r = new System.Random();
        var list = new List<T>();
        foreach (var item in source)
        {
            var i = r.Next(list.Count + 1);
            if (i == list.Count)
            {
                list.Add(item);
            }
            else
            {
                var temp = list[i];
                list[i] = item;
                list.Add(temp);
            }
        }
        return list;
    }
}
