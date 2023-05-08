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

public class BoardGame : MonoBehaviour
{
    [SerializeField] MapCreater mapCreater;
    [SerializeField] float timeToAlert = 5f;
    private bool isAlert = false;

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

    private int matchCount = 0;

    private bool gameSetupDone = false;
    public bool isPlayingGame = false;
    public bool isPausing = false;

    private bool isChallengeGame = false;

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
    }

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += OnGameStateChangeHandler;
        this.RegisterListener((int)EventID.OnBuffHint, DoBuffHint);
        this.RegisterListener((int)EventID.OnBuffSwap, DoBuffSwap);
        this.RegisterListener((int)EventID.OnNewMatchSuccess, OnNewMatchSuccess);
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= OnGameStateChangeHandler;
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnBuffHint, DoBuffHint);
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnBuffSwap, DoBuffSwap);
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnNewMatchSuccess, OnNewMatchSuccess);
    }

    private void OnGameStateChangeHandler(GameState current, GameState last, object data)
    {
        if (current == GameState.Restart || current == GameState.Init)
        {
            isAlert = false;
            isChallengeGame = data != null && (bool)data;
            PrepareSceneLevel(isChallengeGame);
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

    public void PrepareSceneLevel(bool isChallenge = false)
    {
        if (DataManager.currGameMode == eGameMode.Normal)
            currentLevel = DataManager.levelSelect;
        else if (DataManager.currGameMode == eGameMode.Challenge)
            currentLevel = DataManager.UserData.challengeLevel + 1;
        if (prepareMapCoroutine != null)
            StopCoroutine(prepareMapCoroutine);
        prepareMapCoroutine = StartCoroutine(PrepareNewGame(currentLevel));
    }

    private IEnumerator PrepareNewGame(int level)
    {
        Debug.Log($"Level Select: {level}");
        isPlayingGame = false;

        stopwatch = new Stopwatch();

        ClearMap();
        if (DataManager.currGameMode == eGameMode.Normal)
        {
            int levelIndex = level;
            string path = $"Maps/Map_Level_{levelIndex}";
            var file = Resources.Load<TextAsset>(path);
            string configPath = $"Configs/Config_Level_{levelIndex}";
            var config = Resources.Load<TextAsset>(configPath);

            if (file == null || config == null)
            {
                Debug.Log($"Load text data fail - MapPath =[{path}] /n configPath = [{configPath}]");
                GameStateManager.Idle(null);
                yield break;
            }
            else
            {
                currentLevelConfig = JsonUtility.FromJson<LevelConfig>(config.text);
                timeLimitInSeconds = currentLevelConfig.time;
                mapCreater.CreateMapFromTextAsset(file.text, currentLevelConfig.rowsSpeed);
            }
        }
        else
        {
            int levelIndex = level;
            string path = $"ChallengeMaps/Map_Challenge_{levelIndex}";
            var file = Resources.Load<TextAsset>(path);
            string configPath = $"ChallengeMaps/Config_Challenge_{levelIndex}";
            var config = Resources.Load<TextAsset>(configPath);
            if (file == null)
            {
                DataManager.UserData.isMaxLevelChallenge = true;
                levelIndex = level-1;
                path = $"ChallengeMaps/Map_Challenge_{levelIndex}";
                file = Resources.Load<TextAsset>(path);
                configPath = $"ChallengeMaps/Config_Challenge_{levelIndex}";
                config = Resources.Load<TextAsset>(configPath);
            }
            if (file == null || config == null)
            {
                Debug.Log($"Load text data fail - MapPath =[{path}] /n configPath = [{configPath}]");
                GameStateManager.Idle(null);
                yield break;
            }
            else
            {
                currentLevelConfig = JsonUtility.FromJson<LevelConfig>(config.text);
                timeLimitInSeconds = currentLevelConfig.time;
                mapCreater.CreateMapFromTextAsset(file.text, currentLevelConfig.rowsSpeed);
            }
        }

        GameStateManager.Ready(null);
        UIToast.Hide();
    }

    public void StartGamePlay()
    {
        FirebaseManager.LogLevelStart(currentLevel, $"level_{currentLevel}");
        isPlayingGame = true;
        isPausing = false;
        //stopwatch.Restart();
        stopwatch.Start();
        isDraggingItem = false;
        //UI_Ingame_Manager.instance.OnGameStarted();
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
        FirebaseManager.LogLevelEnd(currentLevel, $"Win_level_{currentLevel}", true);
        stopwatch.Stop();
        isPlayingGame = false;
        DataManager.UserData.LevelChesPercent += DataManager.GameConfig.unlockChestEachLevel;

        float timeUsePercent = (float)stopwatch.Elapsed.TotalSeconds / timeLimitInSeconds;
        int starNum = timeUsePercent <= DataManager.GameConfig.threeStar ? 3 : timeUsePercent <= DataManager.GameConfig.twoStar ? 2 : 1;
        Debug.Log($"Time used: [{stopwatch.Elapsed.TotalSeconds}] - Equal [{timeUsePercent:P1}] Percent - Got [{starNum}] stars");
        StarManager.Add(starNum);
        DataManager.MapAsset.listMaps[DataManager.mapSelect-1].levelAsset.UpdateLevelStar(currentLevel - 1, starNum);
        DataManager.levelStars = starNum;
        GameStateManager.WaitComplete(null);
    }

    private void GameOverHandler()
    {
        FirebaseManager.LogLevelEnd(currentLevel, $"Lose_level_{currentLevel}", false);
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
}
