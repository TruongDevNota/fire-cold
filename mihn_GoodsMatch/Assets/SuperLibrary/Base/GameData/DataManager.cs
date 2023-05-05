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

    public static SkinData CurrentSkin
    {
        get => SkinsAsset?.Current;
        set => SkinsAsset.Current = value;
    }
    [SerializeField]
    protected SkinsAsset skinsAsset = null;
    public static SkinsAsset SkinsAsset { get; private set; }


    public static WindowsData CurrentWindow
    {
        get => WindowAsset?.Current;
        set => WindowAsset.Current = value;
    }
    [SerializeField]
    protected WindowsAsset windowAsset = null;
    public static WindowsAsset WindowAsset { get; private set; }


    public static FloorData CurrentFloor
    {
        get => FloorAsset?.Current;
        set => FloorAsset.Current = value;
    }
    [SerializeField]
    protected FloorsAsset floorAsset = null;
    public static FloorsAsset FloorAsset { get; private set; }

    public static CeillingData CurrentCeilling
    {
        get => CeillingAsset?.Current;
        set => CeillingAsset.Current = value;
    }
    [SerializeField]
    protected CeillingAsset ceillingAsset = null;
    public static CeillingAsset CeillingAsset { get; private set; }

    public static CarpetData CurrentCarpet
    {
        get => CarpetsAsset?.Current;
        set => CarpetsAsset.Current = value;
    }
    [SerializeField]
    protected CarpetsAsset carpetsAsset = null;
    public static CarpetsAsset CarpetsAsset { get; private set; }
    public static ChairData CurrentChair
    {
        get => ChairsAsset?.Current;
        set => ChairsAsset.Current = value;
    }
    [SerializeField]
    protected ChairsAsset chairsAsset = null;
    public static ChairsAsset ChairsAsset { get; private set; }

    public static TableData CurrentTable
    {
        get => TableAssets?.Current;
        set => TableAssets.Current = value;
    }
    [SerializeField]
    protected TablesAsset tableAssets = null;
    public static TablesAsset TableAssets { get; private set; }

    public static LampData CurrentLamp
    {
        get => LampsAsset?.Current;
        set => LampsAsset.Current = value;
    }
    [SerializeField]
    protected LampsAsset lampsAsset = null;
    public static LampsAsset LampsAsset { get; private set; }

    public static LevelAsset LevelAsset { get; private set; }
    public static GameItemAsset ItemsAsset { get; private set; }
    public static GameData gameData { get; private set; }
    private static DataManager instance { get; set; }
    #endregion

    public static int levelSelect = 0;
    public static int mapSelect = 0;
    public static int levelStars = 1;
    public static eGameMode currGameMode = eGameMode.Normal;
    //public static nomalMode currnomalMode = nomalMode.Store1;

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

            gameData.walls = SkinsAsset.itemSaveList;
            gameData.windows = WindowAsset.itemSaveList;
            gameData.floors = FloorAsset.itemSaveList;
            gameData.ceillings = CeillingAsset.itemSaveList;
            gameData.carpets = CarpetsAsset.itemSaveList;
            gameData.chairs = ChairsAsset.itemSaveList;
            gameData.tables = TableAssets.itemSaveList;
            gameData.lamps = LampsAsset.itemSaveList;
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

            if (SkinsAsset == null)
            {
                SkinsAsset = ScriptableObject.CreateInstance("SkinsAsset") as SkinsAsset;
                foreach (var i in instance.skinsAsset.list)
                    SkinsAsset.list.Add(i);
            }
            else
                Debug.Log("SkinsAsset is not NULL");

            if (WindowAsset == null)
            {
                WindowAsset = ScriptableObject.CreateInstance("WindowsAsset") as WindowsAsset;
                foreach (var i in instance.windowAsset.list)
                    WindowAsset.list.Add(i);
            }
            else
                Debug.Log("WindowAsset is not NULL");


            if (FloorAsset == null)
            {
                FloorAsset = ScriptableObject.CreateInstance("FloorsAsset") as FloorsAsset;
                foreach (var i in instance.floorAsset.list)
                    FloorAsset.list.Add(i);
            }
            else
                Debug.Log("FloorAsset is not NULL");

            if (CeillingAsset == null)
            {
                CeillingAsset = ScriptableObject.CreateInstance("CeillingAsset") as CeillingAsset;
                foreach (var i in instance.ceillingAsset.list)
                    CeillingAsset.list.Add(i);
            }
            else
                Debug.Log("CeillingAsset is not NULL");

            if (CarpetsAsset == null)
            {
                CarpetsAsset = ScriptableObject.CreateInstance("CarpetsAsset") as CarpetsAsset;
                foreach (var i in instance.carpetsAsset.list)
                    CarpetsAsset.list.Add(i);
            }
            else
                Debug.Log("CarpetsAsset is not NULL");

            if (ChairsAsset == null)
            {
                ChairsAsset = ScriptableObject.CreateInstance("ChairsAsset") as ChairsAsset;
                foreach (var i in instance.chairsAsset.list)
                    ChairsAsset.list.Add(i);
            }
            else
                Debug.Log("ChairsAsset is not NULL");

            if (TableAssets == null)
            {
                TableAssets = ScriptableObject.CreateInstance("TablesAsset") as TablesAsset;
                foreach (var i in instance.tableAssets.list)
                    TableAssets.list.Add(i);
            }
            else
                Debug.Log("TableAssets is not NULL");

            if (LampsAsset == null)
            {
                LampsAsset = ScriptableObject.CreateInstance("LampsAsset") as LampsAsset;
                foreach (var i in instance.lampsAsset.list)
                    LampsAsset.list.Add(i);
            }
            else
                Debug.Log("LampsAsset is not NULL");

            //Load gamedata
            GameData loadData = FileExtend.LoadData<GameData>("GameData") as GameData;
            if (loadData != null)
            {
                if (loadData.levelStars != null && loadData.levelStars.Any())
                    LevelAsset.ConvertLevelStars(loadData.levelStars);

                if (loadData.itemData != null && loadData.itemData.Any())
                    ItemsAsset.ConvertItemData(loadData.itemData);

                if (loadData.walls != null && loadData.walls.Any())
                    SkinsAsset.ConvertToData(loadData.walls);
                if (loadData.windows != null && loadData.windows.Any())
                    WindowAsset.ConvertToData(loadData.windows);
                if (loadData.floors != null && loadData.floors.Any())
                    FloorAsset.ConvertToData(loadData.floors);
                if (loadData.ceillings != null && loadData.ceillings.Any())
                    CeillingAsset.ConvertToData(loadData.ceillings);
                if (loadData.carpets != null && loadData.carpets.Any())
                    CarpetsAsset.ConvertToData(loadData.carpets);
                if (loadData.chairs != null && loadData.chairs.Any())
                    ChairsAsset.ConvertToData(loadData.chairs);
                if (loadData.tables != null && loadData.tables.Any())
                    TableAssets.ConvertToData(loadData.tables);
                if (loadData.lamps != null && loadData.lamps.Any())
                    LampsAsset.ConvertToData(loadData.lamps);

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