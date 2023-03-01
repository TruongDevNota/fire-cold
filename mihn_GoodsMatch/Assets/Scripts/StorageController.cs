using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StorageController : MonoBehaviour
{
    [SerializeField] int fillSlotPercent;
    [SerializeField] public int maxSetOfItems;
    [SerializeField] Transform Goods_Container;

    private List<ShelfUnit> shelves = new List<ShelfUnit>();
    private int totalCellSlot;

    public List<Goods_Item> itemsInWareHouse = new List<Goods_Item>();

    private void Start()
    {
        
    }

    public void OnPrepareNewGame()
    {
        InitStorage();
    }

    private void InitStorage()
    {
        shelves.Clear();
        totalCellSlot = 0;
        foreach (var shelf in GetComponentsInChildren<ShelfUnit>())
        {
            shelf.InitCells();
            totalCellSlot += shelf.CellAmount;
            shelves.Add(shelf);
        }
        Debug.Log($"number of shelf: {shelves.Count} - Total cell slot: {totalCellSlot}");
        CreatItemsWareHouse();
        BoardGame.instance.items = Goods_Container.GetComponentsInChildren<Goods_Item>().ToList();
    }

    public void CreatItemsWareHouse()
    {
        int typeCreatedCount = 0;
        int setOfItems = 0;
        int slotUse = 0;

        itemsInWareHouse.Clear();
        //while (setOfItems < maxSetOfItems && slotUse < totalCellSlot* fillSlotPercent/100)
        //{
        //    var definition = itemDefinitionAsset.pDefinitions[typeCreatedCount % itemDefinitionAsset.pDefinitions.Count];
        //    typeCreatedCount++;
        //    setOfItems++;
        //    slotUse += definition.slotNeed * definition.matchAmount;
        //    if (slotUse > totalCellSlot * fillSlotPercent / 100)
        //        break;
        //    for (int i = 0; i < definition.matchAmount; i++)
        //    {
        //        var newItem = GameObject.Instantiate(definition.itemPrefabs, Goods_Container).GetComponent<Goods_Item>();
        //        itemsInWareHouse.Add(newItem);
        //        newItem.gameObject.SetActive(false);
        //    }
        //}
        Debug.Log($"Total item in stock created: {itemsInWareHouse.Count}");
        int putItemTurn = 1;
        while(itemsInWareHouse.Count > 0)
        {
            PutOnItemsOnStart(putItemTurn>1);
            putItemTurn++;
        }
    }

    public void PutOnItemsOnStart(bool forcePuton = false)
    {
        for(int s = 0; s< shelves.Count; s++)
        {
            var shelf = shelves[s];
            PutItemOnShelfEmpty(shelf, forcePuton);
        }
    }

    public void PutItemOnShelfEmpty(ShelfUnit shelf, bool forcePuton = false)
    {
        for (int i = 0; i < shelf.CellAmount; i++)
        {
            if (isSkipSlot() && !forcePuton)
                continue;
            if (!shelf.cells[i].isEmpty)
                continue;
            if (itemsInWareHouse.Count <= 0)
                continue;

            var item = itemsInWareHouse.FirstOrDefault(x => !shelf.CheckIfMatchPut(x.Type) && shelf.CheckItemFitOnShelf((int)x.size.x, i) >= 0);
            if (item != null)
            {
                item.pFirstLeftCellIndex = i;
                item.pCurrentShelf = shelf;
                item.canPick = true;
                item.gameObject.SetActive(true);
                shelf.DoPutItemFromWareHouse(item);
                itemsInWareHouse.Remove(item);
            }
        }
    }

    public int checkItemLeft()
    {
        return Goods_Container.childCount;
    }

    private bool isSkipSlot()
    {
        int tem = Random.Range(1, 101);
        return tem > fillSlotPercent;
    }
}
