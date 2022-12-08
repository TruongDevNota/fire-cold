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

    [SerializeField] Vector3 touchPositionOffset;

    private StorageController storageController;
    
    private Goods_Item dragingItem = null;
    private bool isDraggingItem = false;
    private int itemCount;
    public int ItemCount 
    { 
        get => itemCount; 
        set { itemCount = value; } 
    }
    public List<Goods_Item> items = new List<Goods_Item>();

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
        PrepareSceneLevel(DataManager.selectedLevel);
    }

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += OnGameStateChangeHandler;
        this.RegisterListener((int)EventID.OnBuffHint, DoBuffHint);
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= OnGameStateChangeHandler;
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnBuffHint, DoBuffHint);
    }

    private void OnGameStateChangeHandler(GameState current, GameState last, object data)
    {
        if(current == GameState.Restart)
            PrepareSceneLevel(DataManager.selectedLevel);
        if (current == GameState.Pause)
            PauseGame();
        if (current == GameState.Play)
            StartGamePlay();
        if(current == GameState.RebornContinue)
        {
            timeLimitInSeconds += DataManager.GameConfig.rebornTimeAdding;
            GameStateManager.Ready(null);
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

    public void PrepareSceneLevel(int level)
    {
        currentLevel = level;
        if(prepareMapCoroutine != null)
            StopCoroutine(prepareMapCoroutine);
        prepareMapCoroutine = StartCoroutine(PrepareNewGame(level));
    }

    private IEnumerator PrepareNewGame(int level)
    {
        Debug.Log($"Level Select: {level}");
        isPlayingGame = false;
        timeLimitInSeconds = timeLimitDefault;
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
        DOVirtual.DelayedCall(1f, () => { GameStateManager.WaitComplete(null); });
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
        dragingItem.transform.position = newPosition + Vector3.forward * 0.5f + touchPositionOffset;
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
        
        for(int i = 0; i < definition.matchAmount; i++)
        {
            var item = items.FirstOrDefault(x => x.Type == definition.itemType);
            items.Remove(item);
            item.pCurrentShelf.PickItemUpHandler(item);
            item.Explode();
        }
    }
    #endregion

    private void ClearMap()
    {
        for (int num = transform.childCount - 1; num >= 0; num--)
        {
            Destroy(transform.GetChild(num).gameObject);
        }
        items.Clear();
    }
}
