using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class BoardGame : MonoBehaviour
{
    [Header("Sample Game")]
    [SerializeField] List<GameObject> sampleStoragePrefabs;
    [SerializeField] bool isSampleGame = false;
    [SerializeField] float timeLimitDefault;

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
        UI_Ingame_Manager.instance.OnGameInitHandle();
    }

    private void Update()
    {
        Vector3 touchPosition;
        if (Input.GetMouseButtonDown(0))
        {
            touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TryPickItem(touchPosition);
        }
        else if (Input.GetMouseButton(0))
        {
            if (!isDraggingItem)
                return;
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
        if (isSampleGame)
        {
            currentLevel = level;
            StartCoroutine(PrepareNewGame(level));
        }
        else
        {
            Debug.LogError($"[Prepare Scene] :Not hander for normal game yet!");
        }
    }

    private IEnumerator PrepareNewGame(int level)
    {
        Debug.Log($"Level Select: {level}");
        isPlayingGame = false;
        isPausing = false;
        timeLimitInSeconds = timeLimitDefault;
        stopwatch = new Stopwatch();

        if(storageController != null)
            Destroy(storageController.gameObject);

        var storage = GameObject.Instantiate(sampleStoragePrefabs[level - 1], this.transform);
        storageController = storage.GetComponent<StorageController>();

        UI_Ingame_Manager.instance.OnGamePrepareHandle(currentLevel);
        storageController.OnPrepareNewGame();
        yield return new WaitForSeconds(0.25f);
        UI_Ingame_Manager.instance.OnGamePrepareDone();
    }

    public void StartGamePlay()
    {
        isPlayingGame = true;
        isPausing = false;
        stopwatch.Restart();
        UI_Ingame_Manager.instance.OnGameStarted();
    }

    private void TryPickItem(Vector2 touchPosion)
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

        dragingItem.transform.position = newPosition + Vector3.forward * 0.5f;
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
        UI_Ingame_Manager.instance.OnGameOverHandle(true);
    }

    private void GameOverHandler()
    {
        Debug.Log("###### GameOver #####");
        stopwatch.Stop();
        isPlayingGame = false;
        UI_Ingame_Manager.instance.OnGameOverHandle(false);
    }

    public void RestartGame()
    {
        StartCoroutine(PrepareNewGame(currentLevel));
    }

    public void PauseGame()
    {
        stopwatch.Stop();
        isPausing = true;
        UI_Ingame_Manager.instance.OnGamePauseHandle();
    }
}
