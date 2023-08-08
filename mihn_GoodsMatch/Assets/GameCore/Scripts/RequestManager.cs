using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RequestManager : MonoBehaviour
{
    [SerializeField] GameItemAsset gameItemAsset;
    private GameConfig config => DataManager.GameConfig;
    public BoardGame_Bartender boardGame;
    public BoardGame boardGame1;

    [SerializeField] BarRequest[] bars;
    [SerializeField] float waitTime = 10f;
    [SerializeField] int[] groupPercent;
    [SerializeField] int[] itemRequestPercent;

    [Header("Request create config")]
    [SerializeField] float nextRequestTime = 10f;

    int totalPercentOfGroupType;
    int totalPercentOfItemInRequet;
    int requestCount = 0;

    private Coroutine spawnRequestCoroutine = null;

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += GameStateManager_OnStateChanged;
        this.RegisterListener((int)EventID.StartModeOrder, StartModeOrder);
        foreach (var bar in bars)
            bar.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= GameStateManager_OnStateChanged;
        EventDispatcher.Instance?.RemoveListener((int)EventID.StartModeOrder, StartModeOrder);
        StopAllCoroutines();
    }
    private void GameStateManager_OnStateChanged(GameState current, GameState last, object data)
    {
        if(current == GameState.Play)
        {
            
        }
        else if(current == GameState.Restart)
        {
            if (spawnRequestCoroutine != null)
                StopCoroutine(spawnRequestCoroutine);
            foreach (var bar in bars)
            {
                if (bar.gameObject.activeInHierarchy)
                    bar.OnLevelEnd(true, true);
            }
        }
        else if (current == GameState.WaitComplete)
        {
            if (spawnRequestCoroutine != null)
                StopCoroutine(spawnRequestCoroutine);
            foreach (var bar in bars)
            {
                if (bar.gameObject.activeInHierarchy)
                    bar.OnLevelEnd(true);
            }
        }
        else if (current == GameState.WaitGameOver)
        {
            if (spawnRequestCoroutine != null)
                StopCoroutine(spawnRequestCoroutine);
            foreach (var bar in bars)
            {
                if (bar.gameObject.activeInHierarchy)
                    bar.OnLevelEnd(false);
            }
        }
    }
    public void StartRequest()
    {
        foreach(var bar in bars)
        {
            if (bar.gameObject.activeInHierarchy)
            {
                bar.ClearAll();
                bar.gameObject.SetActive(false);
            }  
        }

        if (spawnRequestCoroutine != null)
            StopCoroutine(spawnRequestCoroutine);
        spawnRequestCoroutine = StartCoroutine(YieldSpawnRequest());
    }
    public void StartRequestOrder()
    {
        foreach (var bar in bars)
        {
            bar.ShowRequestItem();
        }

    }
    private IEnumerator YieldSpawnRequest()
    {
        requestCount = 0;
        yield return new WaitForSeconds(2f);
        while (true)
        {
            var validBar = bars.FirstOrDefault(bar => !bar.gameObject.activeInHierarchy);
            if(validBar != null)
            {
                validBar.ShowRequestItem();
            }
            yield return new WaitForSeconds(config.timeToCheckRequest);
        }
    }

    public RequestDatum CreateRequest()
    {
        requestCount++;
        var itemCount = DataManager.UserData.bartenderPlayCount > config.levelToRequestx2 && requestCount % config.doubleRequestRepeat == 0 ? 2 : 1;
        var request = new RequestDatum();
        request.id = requestCount;
        request.types = new List<eItemType>();
        request.waitTime = waitTime + (itemCount - 1) * waitTime * 0.5f;
        for (int i = 0; i < itemCount; i++)
        {
            eItemType requestType;
            if (boardGame1==null)
                requestType = boardGame.GetItemTypeByGroup(0);
            else
                requestType = boardGame1.GetItemTypeByGroup(0);
            request.types.Add(requestType);
        }
        return request;
    }

    public int GetGroupTypeIndex()
    {
        if (requestCount < config.minEasyRequest)
            return 0;
        totalPercentOfGroupType = 0;
        foreach (var percent in groupPercent)
            totalPercentOfGroupType += percent;
        return GetRandomIndexByRatio(groupPercent, totalPercentOfGroupType);
    }

    public int GetAmountItemOfRequest()
    {
        if (DataManager.UserData.bartenderPlayCount + 1 < config.levelToRequestx2)
            return 1;

        totalPercentOfItemInRequet = 0;
        foreach (var percent in itemRequestPercent)
            totalPercentOfItemInRequet += percent;
        return GetRandomIndexByRatio(itemRequestPercent, totalPercentOfItemInRequet) + 1;
    }

    private int GetRandomIndexByRatio(int[] percentValues, int totalPercent)
    {
        var value = Random.Range(0, totalPercent);
        for(int i = 0; i < percentValues.Length; i++)
        {
            if (value < percentValues[i])
                return i;
            else
                value -= percentValues[i];
        }
        return percentValues.Length - 1;
    }
    private void SetGameLayerRecursive(GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        foreach (Transform child in gameObject.transform)
        {
            SetGameLayerRecursive(child.gameObject, layer);
        }
    }
    public void StartModeOrder(object obj)
    {
        if (!BoardGame.instance.hasOrderMode)
        {
            return;
        }
        StartRequestOrder();
        foreach (var bar in bars)
        {
            if (bar.gameObject.activeInHierarchy)
            {
                SetGameLayerRecursive(bar.gameObject, 5);
            }
        }
        
        
    }
}

[System.Serializable]
public class RequestDatum
{
    public int id;
    public List<eItemType> types;
    public float waitTime;
}
