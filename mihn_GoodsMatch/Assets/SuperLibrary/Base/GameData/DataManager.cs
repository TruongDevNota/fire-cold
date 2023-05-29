using System;
using System.Linq;
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class DataManager : MonoBehaviour
{
    #region STATIC
    public static GameConfig GameConfig => instance.configAsset.gameConfig;
    public static UserData UserData
    {
        get { return gameData?.user; }
    }
    public static LevelData CurrentLevelData
    {
        get => LevelAsset?.Current;
        set => LevelAsset.Current = value;
    }
    
    public static LevelAsset LevelAsset { get; private set; }
    public static GameItemAsset ItemsAsset { get; private set; }
    public static GameData gameData { get; private set; }
    private static DataManager instance { get; set; }
    #endregion

    public static int levelSelect = 0;
    public static int levelStars = 1;
    public static eGameMode currGameMode = eGameMode.Normal;

    [Space(10)]
    [Header("Default Data")]
    [SerializeField]
    protected ConfigAsset configAsset = null;
    [SerializeField]
    protected LevelAsset levelAsset = null;
    [SerializeField]
    protected GameItemAsset gameItemAsset = null;

    public static bool IsFirstTime = false;

    [Header("GameData auto SAVE LOAD")]
    [SerializeField]
    protected bool saveOnPause = true;
    [SerializeField]
    protected bool saveOnQuit = true;

    public delegate void LoadedDelegate(GameData gameData);
    public static event LoadedDelegate OnLoaded;

    public static bool adInterOrRewardClicked = false;

    #region BASE
    private void Awake()
    {
        instance = this;
    }

    public static void Save(bool saveCloud = true)
    {
        if (instance && gameData != null && gameData.user != null)
        {
            var time = DateTime.Now;
            gameData.user.LastTimeUpdate = DateTime.Now;
            gameData.levelStars = LevelAsset.saveList;
            gameData.itemData = ItemsAsset.itemSaveList;

            Debug.Log("ConvertData in " + (DateTime.Now - time).TotalMilliseconds + "ms");
            FileExtend.SaveData<GameData>("GameData", gameData);
            Debug.Log("SaveData in " + (DateTime.Now - time).TotalMilliseconds + "ms");

            if (saveCloud)
            {
                //Save cloud in here;
                Debug.Log("Save cloud is not implement!");
            }
        }
    }

    public static IEnumerator DoLoad()
    {
        if (instance)
        {
            var elapsedTime = 0f;
            if (gameData == null)
                Load();
            else
                Debug.LogWarning("GameData not NULL");

            while (gameData == null || LevelAsset == null)
            {
                if (elapsedTime < 5)
                {
                    Debug.LogWarning("GameData load " + elapsedTime.ToString("0.0"));
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
        }
    }

    public static void Load()
    {
        var time = DateTime.Now;
        if (instance)
        {
            //Create default
            var tempData = new GameData();

            if (LevelAsset == null)
            {
                LevelAsset = ScriptableObject.CreateInstance("LevelAsset") as LevelAsset;
                foreach (var i in instance.levelAsset.list)
                    LevelAsset.list.Add(i);
            }
            else
                Debug.Log("LevelAsset is not NULL");

            if(ItemsAsset == null)
            {
                ItemsAsset = ScriptableObject.CreateInstance("GameItemAsset") as GameItemAsset;
                foreach (var i in instance.gameItemAsset.list)
                    ItemsAsset.list.Add(i);
            }
            else
                Debug.Log("GameItemAsset is not NULL");

            //Load gamedata
            GameData loadData = FileExtend.LoadData<GameData>("GameData") as GameData;
            if (loadData != null)
            {
                if (loadData.levelStars != null && loadData.levelStars.Any())
                    LevelAsset.ConvertLevelStars(loadData.levelStars);

                if (loadData.itemData != null && loadData.itemData.Any())
                    ItemsAsset.ConvertItemData(loadData.itemData);

                if (loadData.user != null)
                {
                    tempData.user = loadData.user;
                    if ((DateTime.Now - tempData.user.LastTimeUpdate).TotalSeconds >= 15 * 60)
                        tempData.user.Session++;

                    if (tempData.user.VersionInstall == 0)
                        tempData.user.VersionInstall = UIManager.BundleVersion;
                    tempData.user.VersionCurrent = UIManager.BundleVersion;
                }

                Debug.Log("LoadData in " + (DateTime.Now - time).TotalMilliseconds + "ms");
            }
            else
            {
                tempData.user.Session++;
                Debug.Log("CreateData in " + (DateTime.Now - time).TotalMilliseconds + "ms");
            }
            gameData = tempData;

            if (gameData.user.TotalTimePlay == 0)
            {
                IsFirstTime = true;
                gameData.user.TotalTimePlay++;
            }
        }
        else
        {
            throw new Exception("Data Manager instance is NULL. Maybe it hasn't been created.");
        }
        OnLoaded?.Invoke(gameData);
    }

    public static void Reset()
    {
        var path = FileExtend.FileNameToPath("GameData.gd");
        FileExtend.DeleteFile(path);
        PlayerPrefs.DeleteAll();
        Debug.Log("Reset game data");
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause && !GameStateManager.isBusy && saveOnPause)
            Save(false);
    }

    private void OnApplicationQuit()
    {
        if (saveOnQuit)
            Save(true);
    }

    public void ResetAndUpdateData()
    {
        try
        {
            levelAsset.ResetData();
            gameItemAsset.ResetData();
            Reset();
            Debug.Log("Reset and Update data to BUILD!!!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Please update and save DATA before build!!!");
            Debug.LogException(ex);
        }
    }
    #endregion

}