using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using DG.Tweening;
using Random = UnityEngine.Random;
using System.Linq;
using Base;
using MyBox;

public class BoardGame : MonoBehaviour
{
    [SerializeField] Image bgImg;
    [SerializeField] Sprite[] bgSprites;

    [SerializeField] MapCreater mapCreater;
    [SerializeField] float timeToAlert = 5f;
    private bool isAlert = false;

    [Header("Tutorial")]
    [SerializeField] GameObject tutHand;
    [SerializeField] GameObject tutText;

    public Vector3 touchPositionOffset;

    private LevelConfig currentLevelConfig;
    public LevelConfig CurrentLevelConfig => CurrentLevelConfig;

    private Goods_Item dragingItem = null;
    private bool isDraggingItem = false;
    private int itemCount;
    public int ItemCount
    {
        get => itemCount;
        set { itemCount = value; }
    }
    public List<Goods_Item> items = new List<Goods_Item>();
    public List<ShelfUnit> shelves = new List<ShelfUnit>();
    public List<Goods_Item> itemSwap = new List<Goods_Item>();
    public Stopwatch stopwatch;
    public Stopwatch pStopWatch { get { return stopwatch; } }
    public float timeLimitInSeconds;
    public float pTimeLimitInSeconds { get { return timeLimitInSeconds; } }

    private int matchCount = 0;

    private bool gameSetupDone = false;
    public bool isPlayingGame = false;
    public bool isPausing = false;
    public bool isShowTut = false;

    private bool isChallengeGame = false;

    public static BoardGame instance;

    private Coroutine prepareMapCoroutine;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        isDraggingItem = false;
    }

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += OnGameStateChangeHandler;
        this.RegisterListener((int)EventID.OnBuffHint, DoBuffHint);
        this.RegisterListener((int)EventID.OnBuffSwap, DoBuffSwap);
        this.RegisterListener((int)EventID.BuffHintStartTime, BuffHintStartTime);
        this.RegisterListener((int)EventID.BuffHintStopTime, BuffHintStopTime);
        this.RegisterListener((int)EventID.OnNewMatchSuccess, OnNewMatchSuccess);
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= OnGameStateChangeHandler;
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnBuffHint, DoBuffHint);
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnBuffSwap, DoBuffSwap);
        EventDispatcher.Instance?.RemoveListener((int)EventID.BuffHintStartTime, BuffHintStartTime);
        EventDispatcher.Instance?.RemoveListener((int)EventID.BuffHintStopTime, BuffHintStopTime);
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnNewMatchSuccess, OnNewMatchSuccess);
    }

    private void BuffHintStopTime(object obj)
    {
        stopwatch.Stop();
    }

    private void BuffHintStartTime(object obj)
    {
        stopwatch.Start();
    }

    private void OnGameStateChangeHandler(GameState current, GameState last, object data)
    {
        if (current == GameState.Restart || current == GameState.Init)
        {
            isAlert = false;
            isChallengeGame = data != null && (bool)data;
            PrepareSceneLevel();
        }
        if (current == GameState.Pause)
            PauseGame();
        if (current == GameState.Play)
            StartGamePlay();
        if (current == GameState.RebornContinue)
        {
            isAlert = false;
            timeLimitInSeconds += DataManager.GameConfig.rebornTimeAdding;
            GameStateManager.Play(null);
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
        if (timeLimitInSeconds - stopwatch.ElapsedMilliseconds / 1000 <= timeToAlert && !isAlert)
        {
            this.PostEvent((int)EventID.OnAlertTimeout);
            isAlert = true;
        }
        if (stopwatch.ElapsedMilliseconds / 1000 > timeLimitInSeconds)
            GameOverHandler();
    }

    public void PrepareSceneLevel()
    {
        if (prepareMapCoroutine != null)
            StopCoroutine(prepareMapCoroutine);
        prepareMapCoroutine = StartCoroutine(PrepareNewGame());
    }

    private IEnumerator PrepareNewGame()
    {
        Debug.Log($"Level Select: {DataManager.mapSelect}-{DataManager.levelSelect}");
        isPlayingGame = false;
        stopwatch = new Stopwatch();

        //bgImg.sprite = bgSprites[(DataManager.mapSelect - 1) % bgSprites.Length];

        ClearMap();
        currentLevelConfig = DataManager.currLevelconfigData.config;
        timeLimitInSeconds = currentLevelConfig.time;
        mapCreater.CreateMap();


        isShowTut = DataManager.mapSelect == 1 && DataManager.levelSelect == 1 && !DataManager.UserData.tutNormalDone;
        tutHand?.SetActive(isShowTut);
        tutText?.SetActive(isShowTut);
        
        yield return new WaitForEndOfFrame();
        GameStateManager.Ready(null);
        UIToast.Hide();
    }

    public void StartGamePlay()
    {
        FirebaseManager.LogLevelStart(DataManager.levelSelect, $"level_{DataManager.levelSelect}");
        isPlayingGame = true;
        isPausing = false;
        stopwatch.Start();
        isDraggingItem = false;
    }

    public void CheckGameComplete()
    {
        if (!isPlayingGame)
            return;
        if (items.Count <= 0)
            GameCompleteHandler();
    }

    private void GameCompleteHandler()
    {
        Debug.Log("****  GameComplete  ****");
        FirebaseManager.LogLevelEnd(DataManager.levelSelect, $"Win_level_{DataManager.levelSelect}", true);
        stopwatch.Stop();
        isPlayingGame = false;
        DataManager.UserData.LevelChesPercent += DataManager.GameConfig.unlockChestEachLevel;

        float timeUsePercent = (float)stopwatch.Elapsed.TotalSeconds / timeLimitInSeconds;
        int starNum = timeUsePercent <= DataManager.GameConfig.threeStar ? 3 : timeUsePercent <= DataManager.GameConfig.twoStar ? 2 : 1;
        Debug.Log($"Time used: [{stopwatch.Elapsed.TotalSeconds}] - Equal [{timeUsePercent:P1}] Percent - Got [{starNum}] stars");

        if (DataManager.MapAsset.ListMap[DataManager.mapSelect - 1].levelStars.Count >= DataManager.levelSelect)
        {
            if(DataManager.MapAsset.ListMap[DataManager.mapSelect - 1].levelStars[DataManager.levelSelect - 1] < starNum)
            {
                DataManager.MapAsset.ListMap[DataManager.mapSelect - 1].levelStars[DataManager.levelSelect - 1] = starNum;
            }
        }
        else
            DataManager.MapAsset.ListMap[DataManager.mapSelect - 1].levelStars.Add(starNum);

        DataManager.levelStars = starNum;

        GameStateManager.WaitComplete(null);
    }

    private void GameOverHandler()
    {
        FirebaseManager.LogLevelEnd(DataManager.levelSelect, $"Lose_level_{DataManager.levelSelect}", false);
        stopwatch.Stop();
        isPlayingGame = false;
        if (dragingItem != null)
            dragingItem.pCurrentShelf.DoPutNewItem(dragingItem);
        dragingItem = null;
        GameStateManager.WaitGameOver(null);
        //UI_Ingame_Manager.instance.OnGameOverHandle(false);
    }

    public void PauseGame()
    {
        stopwatch.Stop();
        isPausing = true;
    }

    #region ItemsHandle
    private void TryPickItem(Vector3 touchPosion)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(touchPosion, Vector2.zero);
        foreach (var hit in hits)
        {
            if (string.Compare(hit.collider.gameObject.tag, GameConstants.goodsItemTag) == 0)
            {
                if (isShowTut)
                {
                    tutHand?.SetActive(false);
                    tutText?.SetActive(false);
                    isShowTut = false;
                    DataManager.UserData.tutNormalDone = true;
                }
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
        Goods_Item endPosItem = null;
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
        {
            //targetIndex = targetShelf.CheckItemFitOnShelf((int)dragingItem.size.x, targetCell.index);
            endPosItem = targetShelf.ItemsOnShelf.FirstOrDefault(x => x.pFirstLeftCellIndex == targetCell.index);

            if (endPosItem != null)
            {
                DoSwapItems(dragingItem, endPosItem);
            }
            else
            {
                dragingItem.pCurrentShelf = targetShelf;
                dragingItem.pFirstLeftCellIndex = targetCell.index;

                dragingItem.pCurrentShelf.DoPutNewItem(dragingItem);
            }
        }
        else
        {
            dragingItem.pCurrentShelf.DoPutNewItem(dragingItem);
        }
        
        //Debug.Log(targetIndex);
        //if (targetIndex >= 0)
        //{
        //    dragingItem.pCurrentShelf = targetShelf;
        //    dragingItem.pFirstLeftCellIndex = targetIndex;

        //    if (dragingItem.pCurrentShelf != null)
        //        dragingItem.pCurrentShelf.DoPutNewItem(dragingItem);
        //}
        //else
        //{
        //    DoSwapItems(dragingItem, targetShelf.ItemsOnShelf.FirstOrDefault(x => x.pFirstLeftCellIndex == targetCell.index));
        //}

        isDraggingItem = false;
        dragingItem = null;
    }

    private void DoBuffHint(object obj)
    {
        var prop = DataManager.ItemsAsset.GetItemByType(items[Random.Range(0, items.Count)].Type).itemProp;
        var hintItems = items.FindAll(x => x.Type == prop.Type).ToList();
        for (int i = 0; i < prop.matchAmount; i++)
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

                DoSwapItems(item1, item2);
                break;
        }
    }

    private void DoSwapItems(Goods_Item item1, Goods_Item item2)
    {
        var shelf1 = item1.pCurrentShelf;
        var shelf2 = item2.pCurrentShelf;
        int index2 = item2.pFirstLeftCellIndex;

        item2.pCurrentShelf.PickItemUpHandler(item2);
        item2.pCurrentShelf = shelf1;
        item2.pFirstLeftCellIndex = item1.pFirstLeftCellIndex;

        shelf1.PickItemUpHandler(item1);
        item1.pCurrentShelf = shelf2;
        item1.pFirstLeftCellIndex = index2;

        item2.OnPickUp();
        item1.OnPickUp();
        shelf1.DoPutNewItem(item2);
        shelf2.DoPutNewItem(item1);
    }

    #endregion

    #region matchedHandle
    private void OnNewMatchSuccess(object obj)
    {
        SoundManager.Play("3. Scoring");
        var datum = (NewMatchDatum)obj;
        foreach (var item in datum.items)
            items.Remove(item);
        CheckGameComplete();
    }
    #endregion

    private void ClearMap()
    {
        foreach (var item in items)
            item.Recycle();
        foreach (var shelf in shelves)
            shelf.Recycle();

        //for (int num = transform.childCount - 1; num >= 0; num--)
        //{
        //    Destroy(transform.GetChild(num).gameObject);
        //}
        items.Clear();
        shelves.Clear();
    }
    [ButtonMethod]
    public void TestSwap()
    {
        itemSwap.Clear();
        foreach(var item in items)
        {
            itemSwap.Add(item);
        }
        for (int i = itemSwap.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, itemSwap.Count);
            Goods_Item temp = itemSwap[i];
            itemSwap[i] = itemSwap[j];
            itemSwap[j] = temp;
        }
        for(int i = 0; i < itemSwap.Count;)
        {
            if(i+1< itemSwap.Count)
                DoSwapItems(itemSwap[i], itemSwap[i+1]);
            i += 2;
        }
        
    }
}
