using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using DG.Tweening;
using Random = UnityEngine.Random;
using System.Linq;

public class BoardGame : MonoBehaviour
{
    [Header("Sample Game")]
    [SerializeField] List<GameObject> sampleStoragePrefabs;
    [SerializeField] float timeLimitDefault;
    [SerializeField] MapCreater mapCreater;

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
    public List<Goods_Item> items = new List<Goods_Item>();
    public List<ShelfUnit> shelves = new List<ShelfUnit>();

    public Stopwatch stopwatch;
    public Stopwatch pStopWatch { get { return stopwatch; } }
    public float timeLimitInSeconds;
    public float pTimeLimitInSeconds { get { return timeLimitInSeconds; } }

    private bool gameSetupDone = false;
    public bool isPlayingGame = false;
    public bool isPausing = false;

    public static BoardGame instance;

    private int currentLevel;
    private Coroutine prepareMapCoroutine;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        isDraggingItem = false;
        PrepareSceneLevel();
    }

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += OnGameStateChangeHandler;
        this.RegisterListener((int)EventID.OnBuffHint, DoBuffHint);
        this.RegisterListener((int)EventID.OnBuffSwap, DoBuffSwap);
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= OnGameStateChangeHandler;
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnBuffHint, DoBuffHint);
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnBuffSwap, DoBuffSwap);
    }

    private void OnGameStateChangeHandler(GameState current, GameState last, object data)
    {
        if(current == GameState.Restart || current == GameState.Init)
            PrepareSceneLevel();
        if (current == GameState.Pause)
            PauseGame();
        if (current == GameState.Play)
            StartGamePlay();
        if(current == GameState.RebornContinue)
        {
            timeLimitInSeconds += DataManager.GameConfig.rebornTimeAdding;
            GameStateManager.Play(null);
        }
    }

    private void Update()
    {
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
        if (stopwatch.ElapsedMilliseconds / 1000 > timeLimitInSeconds)
            GameOverHandler();
    }

    public void PrepareSceneLevel()
    {
        currentLevel = DataManager.levelSelect <= 30 ? DataManager.levelSelect : Random.Range(20, 30);
        if(prepareMapCoroutine != null)
            StopCoroutine(prepareMapCoroutine);
        prepareMapCoroutine = StartCoroutine(PrepareNewGame(currentLevel));
    }

    private IEnumerator PrepareNewGame(int level)
    {
        Debug.Log($"Level Select: {level}");
        isPlayingGame = false;
        
        stopwatch = new Stopwatch();

        ClearMap();

        string path = $"Maps/Map_Level_{level}";
        var file = Resources.Load<TextAsset>(path);
        if (file != null)
        {
            string mapData = file.text;
            mapCreater.CreateMapFromTextAsset(mapData);
        }
        else
        {
            Debug.Log($"Load map data fail - [{path}]");
            GameStateManager.Idle(null);
            yield break;
        }

        string configPath = $"Configs/Config_Level_{level}";
        var config = Resources.Load<TextAsset>(configPath);
        if (config != null)
        {
            currentLevelConfig = JsonUtility.FromJson<LevelConfig>(config.text);
            timeLimitInSeconds = currentLevelConfig.time;
        }
        else
        {
            Debug.LogError($"Load level config fail - [{configPath}]");
            timeLimitInSeconds = timeLimitDefault;
            //GameStateManager.Idle(null);
            //yield break;
        }

        GameStateManager.Ready(null);
        UIToast.Hide();
    }

    public void StartGamePlay()
    {
        isPlayingGame = true;
        isPausing = false;
        //stopwatch.Restart();
        stopwatch.Start();
        isDraggingItem = false;
        //UI_Ingame_Manager.instance.OnGameStarted();
    }

    public void CheckGameComplete()
    {
        if (items.Count <= 0)
            GameCompleteHandler();
    }

    private void GameCompleteHandler()
    {
        Debug.Log("****  GameComplete  ****");
        stopwatch.Stop();
        isPlayingGame = false;
        DataManager.UserData.LevelChesPercent += DataManager.GameConfig.unlockChestEachLevel;
        GameStateManager.WaitComplete(null);
        //DOVirtual.DelayedCall(1f, () => 
        //{ 
        //    GameStateManager.WaitComplete(null); 
        //});
        //UI_Ingame_Manager.instance.OnGameOverHandle(true);
    }

    private void GameOverHandler()
    {
        Debug.Log("###### GameOver #####");
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
        Debug.Log($"On stop touch at position: {touchPosion}");
        Debug.Log($"Ray start at {raystart}");
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
        Debug.Log(targetIndex);
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
