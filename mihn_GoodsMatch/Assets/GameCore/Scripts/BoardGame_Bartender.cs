using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using DG.Tweening;
using Random = UnityEngine.Random;
using System.Linq;
using Base;

public class BoardGame_Bartender : MonoBehaviour
{
    [SerializeField] GameItemAsset gameItemAsset;
    
    public Vector3 touchPositionOffset;

    private LevelConfig currentLevelConfig;
    public LevelConfig CurrentLevelConfig { get { return currentLevelConfig; } }

    private Goods_Item dragingItem = null;
    private bool isDraggingItem = false;
    private int itemCount;
    public int ItemCount 
    { 
        get => itemCount; 
        set { itemCount = value; } 
    }

    public bool isPlayingGame = false;
    public bool isPausing = false;

    public static BoardGame_Bartender instance;

    private int currentLevel;
    private Coroutine prepareMapCoroutine;

    [Header("Items List")]
    public List<Goods_Item> items = new List<Goods_Item>();
    public List<ShelfUnit> shelves = new List<ShelfUnit>();
    private List<eItemType>[] typeGroups = new List<eItemType>[4];

    [Header("Request Manager")]
    [SerializeField] RequestManager requestManager;
    [SerializeField] private List<Goods_Item> requestingItems = new List<Goods_Item>();
    private List<eItemType> requestingTypes = new List<eItemType>();

    [Header("Decor")]
    [SerializeField] UnityEngine.UI.Image bgImage;
    [SerializeField] Sprite daySprite;
    [SerializeField] Sprite nightSprite;

    [SerializeField] MapCreater mapCreater;

    [Header("Test Data")]
    [SerializeField] TextAsset testMapData;

    private void Awake()
    {
        instance = this;
        for(int i = 0; i < 4; i++)
        {
            typeGroups[i] = new List<eItemType>();
        }
    }

    private void Start()
    {
        isDraggingItem = false;
    }

    private void OnEnable()
    {
        if(instance != null && instance != this)
        {
            Destroy(instance.gameObject);
        }
        instance = this;

        bgImage.sprite = DataManager.mapSelect % 2 == 0 ? daySprite : nightSprite;

        GameStateManager.OnStateChanged += OnGameStateChangeHandler;
        this.RegisterListener((int)EventID.OnBuffHint, DoBuffHint);
        this.RegisterListener((int)EventID.OnBuffSwap, DoBuffSwap);
        this.RegisterListener((int)EventID.OnNewMatchSuccess, OnNewMatchSuccess);
        this.RegisterListener((int)EventID.OnNewRequestCreated, OnNewRequestCreated);
        this.RegisterListener((int)EventID.OnClearLastLevel, ClearMap);
        this.RegisterListener((int)EventID.OnTutStepDone, OnTutorialStepDone);
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= OnGameStateChangeHandler;
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnBuffHint, DoBuffHint);
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnBuffSwap, DoBuffSwap);
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnNewRequestCreated, OnNewRequestCreated);
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnNewMatchSuccess, OnNewMatchSuccess);
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnClearLastLevel, ClearMap);
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnTutStepDone, OnTutorialStepDone);
    }

    private void OnGameStateChangeHandler(GameState current, GameState last, object data)
    {
        if(current == GameState.Restart || current == GameState.Init)
        {
            bgImage.sprite = DataManager.mapSelect % 2 == 0 ? daySprite : nightSprite;
            //StartCoroutine(YieldInit(0));
            StartCoroutine(YieldInitMapFromTextData(null));
        }
        else if (current == GameState.Pause)
            PauseGame();
        else if(current == GameState.Play)
        {
            if(last != GameState.Pause)
            {
                StartGamePlay();
                requestManager?.StartRequest();
            }
            else
                isPausing = false;
        }
        else if (current == GameState.GameOver)
        {
            //StopAllCoroutines();
        }
        else if (current == GameState.Complete)
        {
            //StopAllCoroutines();
        }
    }

    private void Update()
    {
        foreach (var s in shelves)
            s.OnMovingFixUpdate();

        if (!isPlayingGame || isPausing)
            return;
        Vector3 touchPosition;
        if (Input.GetMouseButtonDown(0))
        {
            touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (!isDraggingItem)
                TryPickItem(touchPosition);
        }
        else if (Input.GetMouseButton(0))
        {
            touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            DraggingItem(touchPosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            DropItemOff(touchPosition);
        }
    }

    private void LateUpdate()
    {
        if (!isPlayingGame)
            return;
        //if(timeLimitInSeconds - stopwatch.ElapsedMilliseconds / 1000 <= timeToAlert && !isAlert)
        //{
        //    this.PostEvent((int)EventID.OnAlertTimeout);
        //    isAlert = true;
        //}
        //if (stopwatch.ElapsedMilliseconds / 1000 > timeLimitInSeconds)
        //    GameOverHandler();
    }

    #region SpawnItems
    private IEnumerator YieldInitMapFromTextData(MapDatum datum)
    {
        var currentMapDatum = DataManager.currLevelconfigData.mapDatum;

        foreach (var shelf in shelves)
            shelf.InitCells();
        yield return new WaitForEndOfFrame();

        foreach (var item in items)
        {
            item.Recycle();
        }
        items.Clear();
        List<ItemDatum> listC = gameItemAsset.GetListUnlockedByMapIndex(DataManager.mapSelect);
        foreach (var group in typeGroups)
            group.Clear();
        requestingItems.Clear();
        requestingTypes.Clear();

        yield return new WaitForEndOfFrame();
        for (int i = 0; i < currentMapDatum.lines.Count; i++)
        {
            var line = currentMapDatum.lines[i];
            Debug.Log($"Line {i} - shelf count = {line.lineSheves.Count}");
            for (int i2 = 0; i2 < line.lineSheves.Count; i2++)
            {
                if (string.IsNullOrEmpty(line.lineSheves[i2]) || line.lineSheves[i2].Contains("-"))
                    continue;

                var itemTypes = mapCreater.ConvertToShelfDatum(line.lineSheves[i2]);

                var shelfIndex = i * 3 + i2;
                
                for (int i3 = 0; i3 < itemTypes.Count; i3++)
                {
                    if (itemTypes[i3] == 0)
                        continue;
                    ItemDatum itemDatum;
                    
                        itemDatum = DataManager.ItemsAsset.GetItemByIndex(itemTypes[i3], DataManager.mapSelect);
                    
                        //itemDatum = DataManager.ItemsAsset.GetItemByIndex(itemTypes[i3], Store.store2);

                    if (itemDatum == null)
                    {
                        Debug.Log($"Could not found item definition of type: [{itemTypes[i3]}]");
                        yield break;
                    }
                    var newItem = itemDatum.itemProp.Spawn();
                    newItem.pFirstLeftCellIndex = i3;
                    shelves[shelfIndex].DoPutItemFromWareHouse(newItem);
                    newItem.transform.parent = shelves[shelfIndex].transform;
                    items.Add(newItem);
                    yield return new WaitForEndOfFrame();
                }
                
            }
        }
        for (int i = 0; i < listC.Count; i++)
        {
            int d = 0;
            for(int j = 0; j < items.Count; j++)
            {
                if (listC[i].itemProp.Type == items[j].Type)
                {
                    d++;
                }
            }
            if (d == 3)
            {
                typeGroups[0].Add(listC[i].itemProp.Type);
                Debug.Log(listC[i].itemProp.Type + ":3");
            }
            else if(d==2)
            {
                typeGroups[1].Add(listC[i].itemProp.Type);
                Debug.Log(listC[i].itemProp.Type + ":2");
            }
            else if(d==1)
            {
                typeGroups[2].Add(listC[i].itemProp.Type);
                Debug.Log(listC[i].itemProp.Type + ":1");
            }
            else
            {
                typeGroups[3].Add(listC[i].itemProp.Type);
                Debug.Log(listC[i].itemProp.Type + ":0");
            }
        }
        GameStateManager.Ready(null);
        UIToast.Hide();
        yield return new WaitForEndOfFrame();
    }
    private void InitItems(List<ItemDatum> allItemUnlocked)
    {
        for (int i = 0; i < allItemUnlocked.Count; i++)
        {
            if (i < 5)
            {
                typeGroups[0].Add(allItemUnlocked[i].itemProp.Type);
                AddItemsToList(allItemUnlocked[i].itemProp, 3);
            }
            else if (i < 8)
            {
                typeGroups[1].Add(allItemUnlocked[i].itemProp.Type);
                AddItemsToList(allItemUnlocked[i].itemProp, 2);
            }
            else if (i < 11)
            {
                typeGroups[2].Add(allItemUnlocked[i].itemProp.Type);
                AddItemsToList(allItemUnlocked[i].itemProp, 1);
            }
            else
                typeGroups[3].Add(allItemUnlocked[i].itemProp.Type);
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
    #endregion

    #region requestHandle
    public eItemType GetItemTypeByGroup(int groupIndex)
    {
        int index = groupIndex;
        var type = typeGroups[index].FirstOrDefault(x => !requestingTypes.Contains(x));
        while (type == eItemType.None)
        {
            index = (++index) % typeGroups.Length;
            type = typeGroups[index].FirstOrDefault(x => !requestingTypes.Contains(x));
        }
        requestingTypes.Add(type);
        return type;
    }
    private void OnNewRequestCreated(object obj)
    {
        var datum = (List<Goods_Item>)obj;
        foreach(var requestItem in datum)
        {
            requestingItems.Add(requestItem);
        }
    }
    public void OnRequestTimeout(List<Goods_Item> items)
    {
        foreach(var item in items)
            requestingItems.Remove(item);
    }
    #endregion

    #region matchHandle
    private void OnNewMatchSuccess(object obj)
    {
        StartCoroutine(YieldSpawnReplace(obj));
    }
    private IEnumerator YieldSpawnReplace(object obj)
    {
        var datum = (NewMatchDatum)obj;
        typeGroups[0].Remove(datum.type);
        typeGroups[3].Add(datum.type);
        var shelf = datum.items[0].pCurrentShelf;
        var requestingItems = this.requestingItems.FindAll(x => x.Type == datum.type);
        bool isFitRequest = requestingItems != null;
        if (isFitRequest)
        {
            this.PostEvent((int)EventID.OnMatchedRightRequest, requestingItems[0]);
            requestingTypes.Remove(requestingItems[0].Type);
            this.requestingItems.Remove(requestingItems[0]);
            SoundManager.Play("3. Scoring");
        }
        else
            this.PostEvent((int)EventID.OnMatchedWrongRequest);

        var desPos = isFitRequest ? requestingItems[0].transform.position : Vector3.zero;
        for (int i = 0; i < datum.items.Count; i++)
        {
            var posIndex = datum.items[i].pFirstLeftCellIndex;
            shelf.PickItemUpHandler(datum.items[i]);
            
            items.Remove(datum.items[i]);
            if (!isFitRequest && datum.items[i] != null)
                datum.items[i].Explode();
            else if(datum.items[i] != null)
                StartCoroutine(datum.items[i].YieldMoveThenHide(desPos));

            yield return new WaitForSeconds(0.2f);
            SpawnItemInGroup(i + 1, shelf, posIndex);
            yield return new WaitForSeconds(0.1f);
        };
        
        if (isFitRequest && requestingItems[0].gameObject != null)
            requestingItems[0].Recycle();
    }
    private void SpawnItemInGroup(int grIndex, ShelfUnit shelf, int posIndex)
    {
        var newitem = gameItemAsset.GetItemByType(typeGroups[grIndex][Random.Range(0, typeGroups[grIndex].Count)]).itemProp.Spawn();
        newitem.pFirstLeftCellIndex = posIndex;
        shelf.DoPutItemFromWareHouse(newitem);
        newitem.transform.parent = shelf.transform;
        items.Add(newitem);
        typeGroups[grIndex].Remove(newitem.Type);
        typeGroups[grIndex - 1].Add(newitem.Type);
        newitem.OnInit();
    }
    #endregion

    #region GameStateHanlde
    public void StartGamePlay()
    {
        FirebaseManager.LogLevelStart(currentLevel, $"Bartender_level_{currentLevel}");
        isPlayingGame = true;
        isPausing = false;
        isDraggingItem = false;
    }
    public void GameCompleteHandler(object resultData)
    {
        FirebaseManager.LogLevelEnd(currentLevel, $"Win_level_Bartender_{currentLevel}", true);
        isPlayingGame = false;
        if (dragingItem != null)
            dragingItem.pCurrentShelf.DoPutNewItem(dragingItem);
        dragingItem = null;

        // Default star of bartender mode = 3
        if (DataManager.MapAsset.ListMap[DataManager.mapSelect - 1].levelStars.Count >= DataManager.levelSelect)
            DataManager.MapAsset.ListMap[DataManager.mapSelect - 1].levelStars[DataManager.levelSelect - 1] = 3;
        else
            DataManager.MapAsset.ListMap[DataManager.mapSelect - 1].levelStars.Add(3);

        DataManager.levelStars = 3;
        
        GameStateManager.WaitComplete(resultData);
    }
    public void GameOverHandler(object resultData)
    {
        FirebaseManager.LogLevelEnd(currentLevel, $"Lose_level_Bartender_{currentLevel}", false);
        isPlayingGame = false;
        if (dragingItem != null)
            dragingItem.pCurrentShelf.DoPutNewItem(dragingItem);
        dragingItem = null;
        DOVirtual.DelayedCall(1f, () => GameStateManager.WaitGameOver(resultData));
    }
    public void PauseGame()
    {
        isPausing = true;
    }
    #endregion

    #region ItemsHandle
    private void TryPickItem(Vector3 touchPosion)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(touchPosion, Vector2.zero);
        foreach (var hit in hits)
        {
            if (string.Compare(hit.collider.gameObject.tag, GameConstants.goodsItemTag) == 0)
            {
                var item = hit.collider.gameObject.GetComponent<Goods_Item>();
                if (!item.canPick)
                    break;
                dragingItem = item;
                if (dragingItem.pCurrentShelf != null)
                    dragingItem.pCurrentShelf.PickItemUpHandler(dragingItem);
                isDraggingItem = true;
                //dragingItem.transform.position = touchPosion + Vector3.forward * 0.5f;
                dragingItem.OnPickUp();
                touchPositionOffset = new Vector3(dragingItem.transform.position.x - touchPosion.x, dragingItem.transform.position.y - touchPosion.y, 0) + Vector3.forward;
                break;
            }
        }
    }

    private void DraggingItem(Vector3 newPosition)
    {
        if (!isDraggingItem)
            return;
        if (dragingItem == null)
            return;
        dragingItem.transform.position = newPosition + touchPositionOffset;
    }

    private void DropItemOff(Vector3 touchPosion)
    {
        if (!isDraggingItem)
            return;
        if (dragingItem == null)
            return;

        Cell targetCell = null;
        ShelfUnit targetShelf = null;
        int targetIndex = -1;
        var raystart = touchPosion + Vector3.forward;
        RaycastHit2D[] hits = Physics2D.RaycastAll(raystart, Vector2.zero);
        foreach (var hit in hits)
        {
            if (hit.collider != null)
            {
                if (string.Compare(hit.collider.gameObject.tag, GameConstants.cellTag) == 0)
                {
                    Debug.Log($"Find Cell : {hit.collider.gameObject.name}");
                    targetCell = hit.collider.gameObject.GetComponent<Cell>();
                    targetShelf = targetCell.shelfUnit;
                }
            }
        }

        if (targetShelf != null)
            targetIndex = targetShelf.CheckItemFitOnShelf((int)dragingItem.size.x, targetCell.index);
        if (targetIndex >= 0)
        {
            dragingItem.pCurrentShelf = targetShelf;
            dragingItem.pFirstLeftCellIndex = targetIndex;
        }
        isDraggingItem = false;
        if (dragingItem.pCurrentShelf != null)
            dragingItem.pCurrentShelf.DoPutNewItem(dragingItem);
        dragingItem = null;
    }

    private void DoBuffHint(object obj)
    {
        var itemDatum = gameItemAsset.GetItemByType(items[Random.Range(0, items.Count)].Type);
        var hintItems = items.FindAll(x => x.Type == itemDatum.itemProp.Type).ToList();
        for(int i = 0; i < itemDatum.itemProp.matchAmount; i++)
        {
            var item = hintItems[i];
            item.jump(i);
        }
    }

    private void DoBuffSwap(object obj)
    {
        var swapShelf1 = shelves.FirstOrDefault(x => x.CanSwap());
        if (swapShelf1 == null)
            return;
        var item2 = items.FirstOrDefault(x => x.pCurrentShelf != swapShelf1 && x.Type == swapShelf1.ItemsOnShelf[0].Type);
        switch (swapShelf1.ItemsOnShelf.Count)
        {
            case 1:
                item2.pCurrentShelf.PickItemUpHandler(item2);
                item2.pCurrentShelf = swapShelf1;
                item2.pFirstLeftCellIndex = swapShelf1.cells.FirstOrDefault(c => c.isEmpty).index;
                item2.OnPickUp();
                item2.pCurrentShelf.DoPutNewItem(item2);
                break;
            case 2:
                var item1 = swapShelf1.ItemsOnShelf[1];
                
                var shelf2 = item2.pCurrentShelf;
                int index2 = item2.pFirstLeftCellIndex;

                item2.pCurrentShelf.PickItemUpHandler(item2);
                item2.pCurrentShelf = swapShelf1;
                item2.pFirstLeftCellIndex = item1.pFirstLeftCellIndex;

                swapShelf1.PickItemUpHandler(item1);
                item1.pCurrentShelf = shelf2;
                item1.pFirstLeftCellIndex = index2;

                item2.OnPickUp();
                item1.OnPickUp();
                swapShelf1.DoPutNewItem(item2);
                shelf2.DoPutNewItem(item1);

                break;
        }

    }
    #endregion


    private void OnTutorialStepDone(object obj)
    {
        int lastStep = (int)obj;
        if (lastStep == 0)
            isPlayingGame = false;
        if(lastStep == DataManager.GameConfig.tutBartenderLastStep)
            isPlayingGame = true;
    }

    private void ClearMap(object obj)
    {
        foreach(var item in items)
            item.Recycle();

        items.Clear();
    }
}
