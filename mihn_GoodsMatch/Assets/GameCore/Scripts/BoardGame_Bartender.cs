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
    [SerializeField] ItemDefinitionAsset itemDefinitionAsset;
    
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

    private int matchCount = 0;

    private bool gameSetupDone = false;
    public bool isPlayingGame = false;
    public bool isPausing = false;

    private bool isChallengeGame = false;

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

        GameStateManager.OnStateChanged += OnGameStateChangeHandler;
        this.RegisterListener((int)EventID.OnBuffHint, DoBuffHint);
        this.RegisterListener((int)EventID.OnBuffSwap, DoBuffSwap);
        this.RegisterListener((int)EventID.OnNewMatchSuccess, OnNewMatchSuccess);
        this.RegisterListener((int)EventID.OnNewRequestCreated, OnNewRequestCreated);
    }

    private void OnDisable()
    {
        
        GameStateManager.OnStateChanged -= OnGameStateChangeHandler;
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnBuffHint, DoBuffHint);
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnBuffSwap, DoBuffSwap);
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnNewRequestCreated, OnNewRequestCreated);
    }

    private void OnGameStateChangeHandler(GameState current, GameState last, object data)
    {
        if(current == GameState.Restart || current == GameState.Init)
        {
            StartCoroutine(YieldInit(0));
        }
        else if (current == GameState.Pause)
            PauseGame();
        else if(current == GameState.Play)
        {
            StartGamePlay();
            requestManager?.StartRequest();
        }
        else if(current == GameState.RebornContinue)
        {
            GameStateManager.Play(null);
        }
        else if (current == GameState.GameOver)
        {
            StopAllCoroutines();
        }
        else if (current == GameState.Complete)
        {
            StopAllCoroutines();
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
    private IEnumerator YieldInit(int level)
    {
        int emptyCount = 0;
        int startShelfIndex = UnityEngine.Random.Range(0, shelves.Count);

        foreach (var shelf in shelves)
            shelf.InitCells();
        yield return new WaitForEndOfFrame();
        foreach (var group in typeGroups)
            group.Clear();
        requestingItems.Clear();
        requestingTypes.Clear();

        foreach (var item in items)
        {
            item.Recycle();
        }
        items.Clear();
        yield return new WaitForEndOfFrame();
        var allItemUnlocked = NextList(gameItemAsset.unlockedList);
        InitItems((List<ItemDatum>)allItemUnlocked);
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < items.Count; i++)
        {
            if (emptyCount < 3)
            {
                int emptyChange = Random.Range(0, 100);
                if (emptyChange < 10)
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

            items[i] = items[i].Spawn();
            items[i].pFirstLeftCellIndex = index;
            shelves[startShelfIndex % shelves.Count].DoPutItemFromWareHouse(items[i]);
            items[i].transform.parent = shelves[startShelfIndex % shelves.Count].transform;
            startShelfIndex += 2;
            yield return new WaitForEndOfFrame();
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
        var requestingItem = requestingItems.FirstOrDefault(x => x.Type == datum.type);
        bool isFitRequest = requestingItem != null;
        if (isFitRequest)
        {
            this.PostEvent((int)EventID.OnMatchedRightRequest, requestingItem);
            requestingTypes.Remove(requestingItem.Type);
            requestingItems.Remove(requestingItem);
            SoundManager.Play("3. Scoring");
        }
        else
            this.PostEvent((int)EventID.OnMatchedWrongRequest);
        for (int i = 0; i < datum.items.Count; i++)
        {
            var posIndex = datum.items[i].pFirstLeftCellIndex;
            shelf.PickItemUpHandler(datum.items[i]);
            
            items.Remove(datum.items[i]);
            if (!isFitRequest && datum.items[i] != null)
                datum.items[i].Explode();
            else
                StartCoroutine(datum.items[i].YieldMoveThenHide(requestingItem.transform.position));

            yield return new WaitForSeconds(0.2f);
            SpawnItemInGroup(i + 1, shelf, posIndex);
            yield return new WaitForSeconds(0.1f);
        };
        if (isFitRequest && requestingItem.gameObject != null)
            requestingItem.Recycle();
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
        FirebaseManager.LogLevelStart(currentLevel, $"level_{currentLevel}");
        isPlayingGame = true;
        isPausing = false;
        isDraggingItem = false;
    }
    public void GameCompleteHandler(object resultData)
    {
        FirebaseManager.LogLevelEnd(currentLevel, $"Win_level_{currentLevel}");
        isPlayingGame = false;
        if (dragingItem != null)
            dragingItem.pCurrentShelf.DoPutNewItem(dragingItem);
        dragingItem = null;
        //DataManager.UserData.LevelChesPercent += DataManager.GameConfig.unlockChestEachLevel;
        GameStateManager.WaitComplete(resultData);
    }
    public void GameOverHandler(object resultData)
    {
        FirebaseManager.LogLevelEnd(currentLevel, $"Lose_level_{currentLevel}");
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
        var definition = itemDefinitionAsset.GetDefinitionByType(items[Random.Range(0, items.Count)].Type);
        var hintItems = items.FindAll(x => x.Type == definition.itemType).ToList();
        for(int i = 0; i < definition.matchAmount; i++)
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

    private void ClearMap()
    {
        foreach(var item in items)
            item.Recycle();
        foreach(var shelf in shelves)
            shelf.Recycle();

        //for (int num = transform.childCount - 1; num >= 0; num--)
        //{
        //    Destroy(transform.GetChild(num).gameObject);
        //}
        items.Clear();
        shelves.Clear();
    }
}
