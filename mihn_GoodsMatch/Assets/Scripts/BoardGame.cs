using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using DG.Tweening;

public class BoardGame : MonoBehaviour
{
    [Header("Sample Game")]
    [SerializeField] List<GameObject> sampleStoragePrefabs;
    [SerializeField] bool isSampleGame = false;
    [SerializeField] float timeLimitDefault;
    [SerializeField] MapCreater mapCreater;

    [SerializeField] Vector3 touchPositionOffset;

    private StorageController storageController;
    
    private Goods_Item dragingItem = null;
    private bool isDraggingItem = false;
    private int itemCreated;
    public int ItemCreated 
    { 
        get => itemCreated; 
        set { itemCreated = value; } 
    }
    private int itemEarned;
    public int ItemEarned
    {
        get => itemEarned;
        set { 
            itemEarned = value;
            if (itemEarned == itemCreated)
                GameCompleteHandler();
        }
    }

    private Stopwatch stopwatch;
    public Stopwatch pStopWatch { get { return stopwatch; } }
    private float timeLimitInSeconds;
    public float pTimeLimitInSeconds { get { return timeLimitInSeconds; } }

    private bool gameSetupDone = false;
    public bool isPlayingGame = false;
    public bool isPausing = false;

    public static BoardGame instance;

    private int currentLevel;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        isDraggingItem = false;
        PrepareSceneLevel(DataManager.demoLevel);
        
    }

    private void OnDestroy()
    {
        
    }

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += OnGameStateChangeHandler;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= OnGameStateChangeHandler;
    }

    private void OnGameStateChangeHandler(GameState current, GameState last, object data)
    {
        if(current == GameState.Restart)
            PrepareSceneLevel(DataManager.demoLevel);
        if (current == GameState.Pause)
            PauseGame();
        if (current == GameState.Play)
            StartGamePlay();
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
        StartCoroutine(PrepareNewGame(level));
    }

    private IEnumerator PrepareNewGame(int level)
    {
        Debug.Log($"Level Select: {level}");
        isPlayingGame = false;
        isPausing = false;
        timeLimitInSeconds = timeLimitDefault;
        stopwatch = new Stopwatch();
        if (isSampleGame)
        {
            if (storageController != null)
                Destroy(storageController.gameObject);
            var storage = GameObject.Instantiate(sampleStoragePrefabs[level - 1], this.transform);
            storageController = storage.GetComponent<StorageController>();
            yield return new WaitForSeconds(0.25f);
            storageController.OnPrepareNewGame();
        }
        else
        {
            string path = $"Maps/Map_Level_{level}";
            var file = Resources.Load<TextAsset>(path);
            if (file != null)
            {
                string mapData = file.text;
                mapCreater.CreateMap(mapData);
                yield return new WaitForSeconds(0.25f);
            }
            else
            {
                Debug.Log($"Load map data fail - [{path}]");
                GameStateManager.Idle(null);
                yield break;
            }
        }
        itemEarned = 0;
        GameStateManager.Ready(null);
        UIToast.Hide();
    }

    public void StartGamePlay()
    {
        isPlayingGame = true;
        isPausing = false;
        //stopwatch.Restart();
        stopwatch.Start();
        //UI_Ingame_Manager.instance.OnGameStarted();
    }

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
        foreach(var hit in hits)
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
        if(targetIndex >= 0)
        {
            dragingItem.pCurrentShelf = targetShelf;
            dragingItem.pFirstLeftCellIndex = targetIndex;
        }
        isDraggingItem = false;
        if (dragingItem.pCurrentShelf != null)
            dragingItem.pCurrentShelf.DoPutNewItem(dragingItem);
        dragingItem = null;
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

    public void RestartGame()
    {
        StartCoroutine(PrepareNewGame(currentLevel));
    }

    public void PauseGame()
    {
        stopwatch.Stop();
        isPausing = true;
    }
}
