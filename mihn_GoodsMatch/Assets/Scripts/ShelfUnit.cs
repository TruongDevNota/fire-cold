using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class ShelfUnit : MonoBehaviour
{
    public int cellAmount;
    [SerializeField] GameObject cellPrefab;
    [SerializeField] float cellWidth = 1.1f;

    public List<Cell> cells;
    public Vector2 init_position;

    public int CellAmount { get => cellAmount; }
    private List<Goods_Item> itemsOnShelf = new List<Goods_Item>();
    public List<Goods_Item> ItemsOnShelf 
    {
        get { return itemsOnShelf; }
    }

    public void InitCells()
    {
        cells.Clear();
        for (int i = 0; i < cellAmount; i++)
        {
            var go = GameObject.Instantiate(cellPrefab);
            go.transform.position = new Vector3(transform.position.x + i*cellWidth, transform.position.y, transform.position.z);
            go.transform.parent = transform;
            var newCell = go.GetComponent<Cell>();
            newCell.isEmpty = true;
            newCell.shelfUnit = this;
            newCell.index = i;
            cells.Add(newCell);
        }
    }

    #region PutItemOn

    public void DoPutItemFromWareHouse(Goods_Item item)
    {
        itemsOnShelf.Add(item);
        for (int i = 0; i < (int)item.size.x; i++)
        {
            cells[item.pFirstLeftCellIndex + i].isEmpty = false;
        }
        item.transform.position = GetItemPositionOnShelf(item.size, item.pFirstLeftCellIndex);
        item.pCurrentShelf = this;
    }

    public void DoPutNewItem(Goods_Item item)
    {
        itemsOnShelf.Add(item);
        StartCoroutine(YieldPutItemOn(item));
    }

    private IEnumerator YieldPutItemOn(Goods_Item item)
    {
        var itemPos = GetItemPositionOnShelf(item.size, item.pFirstLeftCellIndex);
        for (int i = 0; i < (int)item.size.x; i++)
        {
            cells[item.pFirstLeftCellIndex + i].isEmpty = false;
        }
        item.OnPutUpShelf();
        yield return item.tfMoving.YiledMovingInSameTime(itemPos);
        CheckMatch(item.Type);
    }

    public Vector3 GetItemPositionOnShelf(Vector3 itemSize, int itemFirstLeftIndex)
    {
        return new Vector3(transform.position.x + itemFirstLeftIndex + itemSize.x * 0.5f, transform.position.y, transform.position.z);
    }

    public int CheckItemFitOnShelf(int itemsizeX, int firstcheckIndex)
    {
        for (int i = 1; i <= itemsizeX; i++)
        {
            if (CheckItemFitCell(itemsizeX, firstcheckIndex))
                return firstcheckIndex;
        }
        for (int i = 0; i < cells.Count; i++)
        {
            if (i == firstcheckIndex)
                continue;
            if (CheckItemFitCell(itemsizeX, i))
                return i;
        }
        return -1;
    }

    private bool CheckItemFitCell(int itemsizeX, int cellIndex)
    {
        for (int i = 0; i < itemsizeX; i++)
        {
            if (cells.Count < cellIndex + i)
                return false;
            if (!cells[cellIndex + i].isEmpty)
                return false;
        }
        return true;
    }
    #endregion

    #region PickItemUp
    public void PickItemUpHandler(Goods_Item item)
    {
        for (int i = 0; i < (int)item.size.x; i++)
        {
            cells[item.pFirstLeftCellIndex + i].isEmpty = true;
        }
        itemsOnShelf.Remove(item);
    }
    #endregion

    #region Scoring
    private void CheckMatch(eItemType type)
    {
        List<Goods_Item> matchItems = new List<Goods_Item>();
        foreach(var item in itemsOnShelf)
        {
            if(item.Type == type)
                matchItems.Add(item);
        }
        if(matchItems.Count <= 0)
            return;
        if(matchItems.Count == matchItems[0].matchAmount)
        {
            this.PostEvent((int)EventID.OnNewMatchSuccess);
            SoundManager.Play("3. Scoring");
            int index = -1;
            foreach (var item in matchItems)
            {
                index++;
                PickItemUpHandler(item);
                itemsOnShelf.Remove(item);
                BoardGame.instance?.items.Remove(item);
                item.Explode(index);
            }
            Debug.Log($"New match item type: {type}");
        }
    }
    public bool CheckIfMatchPut(eItemType type)
    {
        List<Goods_Item> matchItems = new List<Goods_Item>();
        foreach(var item in itemsOnShelf)
        {
            if(item.Type == type)
                matchItems.Add(item);
        }
        if(matchItems.Count <= 0)
            return false;
        return matchItems.Count == matchItems[0].matchAmount -1;
    }

    public bool CanSwap()
    {
        switch (itemsOnShelf.Count)
        {
            case 1:
                return true;
            case 2:
                return itemsOnShelf[0].Type != itemsOnShelf[1].Type;
            default:
                return false;
        }
    }
    #endregion
}
