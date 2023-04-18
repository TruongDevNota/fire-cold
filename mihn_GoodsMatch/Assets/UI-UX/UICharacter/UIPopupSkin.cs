using Base;
using Base.Ads;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupSkin : MonoBehaviour
{
    [SerializeField]
    public UIAnimation anim;
    [SerializeField] Button btnBack = null;
    [SerializeField] RectTransform stagesRect = null;
    [SerializeField] RectTransform windowsStagesRect = null;
    [SerializeField] RectTransform floorStagesRect = null;
    [SerializeField] RectTransform ceillingStagesRect = null;
    [SerializeField] RectTransform carpetStagesRect = null;
    [SerializeField] RectTransform chaiStagesRect = null;
    [SerializeField] RectTransform tableStagesRect = null;
    [SerializeField] RectTransform lampStagesRect = null;


    [SerializeField] UISkinChild uiSkinBtnPrefab = null;
    [SerializeField] UISkinChild uiWindowsBtnPrefab = null;
    [SerializeField] UISkinChild uiFloorBtnPrefab = null;
    [SerializeField] UISkinChild uiCeillingBtnPrefab = null;
    [SerializeField] UISkinChild uiCarpetBtnPrefab = null;
    [SerializeField] UISkinChild uiChairBtnPrefab = null;
    [SerializeField] UISkinChild uiTableBtnPrefab = null;
    [SerializeField] UISkinChild uiLampBtnPrefab = null;

    [Header("GetMoreCoin")]
    [SerializeField]
    protected Button coinGetWatchAVideoButton = null;
    [SerializeField]
    protected Text coinGetWatchAVideoStatus = null;
    private SkinsAsset skins => DataManager.SkinsAsset;
    private WindowsAsset windows => DataManager.WindowAsset;
    private FloorsAsset floors => DataManager.FloorAsset;
    private CeillingAsset ceillings => DataManager.CeillingAsset;
    private CarpetsAsset carpets => DataManager.CarpetsAsset;
    private ChairsAsset chairs => DataManager.ChairsAsset;
    private TablesAsset tables => DataManager.TableAssets;
    private LampsAsset lamps => DataManager.LampsAsset;

    private List<UISkinChild> allSkins = new List<UISkinChild>();
    private SkinData currentSkinData;
    private List<UISkinChild> allWindows = new List<UISkinChild>();
    private WindowsData currentWindowsData;
    private List<UISkinChild> allFloors = new List<UISkinChild>();
    private FloorData currentFloorData;
    private List<UISkinChild> allCeillings = new List<UISkinChild>();
    private CeillingData currentCeillingData;
    private List<UISkinChild> allCarpets = new List<UISkinChild>();
    private CarpetData currentCarpetData;
    private List<UISkinChild> allChairs = new List<UISkinChild>();
    private ChairData currentChairData;
    private List<UISkinChild> allTables = new List<UISkinChild>();
    private TableData currentTableData;
    private List<UISkinChild> allLamps = new List<UISkinChild>();
    private LampData currentLamp;


    public static UIPopupSkin instance = null;
    [SerializeField]
    private Button buyTutorialBtn = null;
    private int hightLightTrigger = Animator.StringToHash("Highlighted");
    private RectTransform imgCoverRectTf = null;
    private void Awake()
    {
        instance = this;
        anim = GetComponent<UIAnimation>();
        uiSkinBtnPrefab.CreatePool(20);

        btnBack.onClick.AddListener(() =>
        {
            GameStateManager.Idle(null);
            anim.Hide();
        });
    }
    private void OnEnable()
    {
        this.RegisterListener((int)EventID.OnDecorTutorialComplete, OnDecorTutorialDone);
    }

    private void OnDecorTutorialDone(object obj)
    {
        btnBack.enabled = true;
        foreach (var item in allSkins)
        {
            item.adsBtn.enabled = true;
            item.coinBtn.enabled = true;
        }

        shopScrollView.FirstOrDefault().GetComponent<ScrollRect>().enabled = true;

        imgCoverRectTf.gameObject.Recycle();

    }

    private void OnDisable()
    {
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnDecorTutorialComplete, OnDecorTutorialDone);
    }
    private void Start()
    {
        if (coinGetWatchAVideoButton && coinGetWatchAVideoStatus)
        {
            coinGetWatchAVideoButton.onClick.AddListener(() =>
            {
                if (AdsManager.IsConnected)
                {
                    CoinManager.GetByAds(coinGetWatchAVideoStatus.transform, name);
                }
                else
                {
                    Debug.Log("?? Error ??");
                    //PopupMessage.Show("Connection Error", "Failed to connect to server. Please check your internet connection and try again!", "Okie");
                }
            });
            coinGetWatchAVideoStatus.text = CoinManager.CoinByAdsFormat();
        }
        ActiveTab();
        ChangeTab(0);
    }
    private IEnumerator FillData()
    {
        if (skins)
        {
            currentSkinData = DataManager.CurrentSkin;
            if (allSkins.Count == skins.list.Count)
            {
                for (int i = 0; i < allSkins.Count; i++)
                {
                    var stageData = skins.list[i];
                    stageData.isSelected = stageData.id == currentSkinData?.id;
                    var obj = allSkins[i];
                    obj.FillData(stageData);
                    if (stageData.isSelected)
                        currentSkinData = skins.list[i];

                    yield return null;
                }
            }
            else
            {
                allSkins.Clear();
                uiSkinBtnPrefab.RecycleAll();
                for (int i = 0; i < skins.list.Count; i++)
                {
                    var stageData = skins.list[i];
                    stageData.isSelected = stageData.id == currentSkinData?.id;
                    var obj = uiSkinBtnPrefab.Spawn(stagesRect);
                    obj.FillData(stageData);
                    obj.OnSelected = (sd) =>
                    {
                        currentSkinData = sd;
                        FirebaseManager.LogEvent("skin_click", new Dictionary<string, object> { { "id", currentSkinData.id } });
                    };
                    allSkins.Add(obj);

                    if (stageData.isSelected)
                        currentSkinData = skins.list[i];

                    // For tutorials
                    if (!DataManager.UserData.isShopTutShowed)
                    {
                        var btn = obj.GetComponent<UISkinChild>();
                        if (btn != null)
                        {
                            //0 is current selected => nothing happend
                            //Creat image at element 1
                            if (i == 1)
                            {
                                CreateImageCoverForTutorial(btn);
                            }
                            //disable button in others element greater than 1
                            else
                            {
                                btn.coinBtn.enabled = false;
                                btn.adsBtn.enabled = false;
                            }
                        }

                    }
                    yield return null;
                }
            }
        }
        if (windows)
        {
            currentWindowsData = DataManager.CurrentWindow;
            uiWindowsBtnPrefab.RecycleAll();
            if (allWindows.Count == windows.list.Count)
            {
                for (int i = 0; i < allWindows.Count; i++)
                {
                    var stageData = windows.list[i];
                    stageData.isSelected = stageData.id == currentWindowsData?.id;
                    var obj = allWindows[i];
                    obj.FillData(stageData);
                    if (stageData.isSelected)
                        currentWindowsData = windows.list[i];

                    yield return null;
                }
            }
            else
            {
                allWindows.Clear();
                for (int i = 0; i < windows.list.Count; i++)
                {
                    var stageData = windows.list[i];
                    stageData.isSelected = stageData.id == currentWindowsData?.id;
                    var obj = uiWindowsBtnPrefab.Spawn(windowsStagesRect);
                    obj.FillData(stageData);
                    obj.OnWindowsSelected = (wd) =>
                    {
                        currentWindowsData = wd;
                        FirebaseManager.LogEvent("windows_click", new Dictionary<string, object> { { "id", currentWindowsData.id } });
                    };
                    allWindows.Add(obj);

                    if (stageData.isSelected)
                        currentWindowsData = windows.list[i];

                    yield return null;
                }
            }
        }
        if (floors)
        {
            currentFloorData = DataManager.CurrentFloor;
            uiFloorBtnPrefab.RecycleAll();
            if (allFloors.Count == floors.list.Count)
            {
                for (int i = 0; i < allFloors.Count; i++)
                {
                    var stageData = floors.list[i];
                    stageData.isSelected = stageData.id == currentFloorData?.id;
                    var obj = allFloors[i];
                    obj.FillData(stageData);
                    if (stageData.isSelected)
                        currentFloorData = floors.list[i];

                    yield return null;
                }
            }
            else
            {
                allFloors.Clear();
                for (int i = 0; i < floors.list.Count; i++)
                {
                    var stageData = floors.list[i];
                    stageData.isSelected = stageData.id == currentFloorData?.id;
                    var obj = uiFloorBtnPrefab.Spawn(floorStagesRect);
                    obj.FillData(stageData);
                    obj.OnFloorSelected = (fd) =>
                    {
                        currentFloorData = fd;
                        FirebaseManager.LogEvent("floor_click", new Dictionary<string, object> { { "id", currentFloorData.id } });
                    };
                    allFloors.Add(obj);

                    if (stageData.isSelected)
                        currentFloorData = floors.list[i];

                    yield return null;
                }
            }
        }
        if (ceillings)
        {
            currentCeillingData = DataManager.CurrentCeilling;
            uiCeillingBtnPrefab.RecycleAll();
            if (allCeillings.Count == ceillings.list.Count)
            {
                for (int i = 0; i < allCeillings.Count; i++)
                {
                    var stageData = ceillings.list[i];
                    stageData.isSelected = stageData.id == currentCeillingData?.id;
                    var obj = allCeillings[i];
                    obj.FillData(stageData);
                    if (stageData.isSelected)
                        currentCeillingData = ceillings.list[i];

                    yield return null;
                }
            }
            else
            {
                allCeillings.Clear();
                for (int i = 0; i < ceillings.list.Count; i++)
                {
                    var stageData = ceillings.list[i];
                    stageData.isSelected = stageData.id == currentCeillingData?.id;
                    var obj = uiCeillingBtnPrefab.Spawn(ceillingStagesRect);
                    obj.FillData(stageData);
                    obj.OnCeillingSelected = (cd) =>
                    {
                        currentCeillingData = cd;
                        FirebaseManager.LogEvent("floor_click", new Dictionary<string, object> { { "id", currentCeillingData.id } });
                    };
                    allCeillings.Add(obj);

                    if (stageData.isSelected)
                        currentCeillingData = ceillings.list[i];

                    yield return null;
                }
            }
        }
        if (carpets)
        {
            currentCarpetData = DataManager.CurrentCarpet;
            uiCarpetBtnPrefab.RecycleAll();
            if (allCarpets.Count == carpets.list.Count)
            {
                for (int i = 0; i < allCarpets.Count; i++)
                {
                    var stageData = carpets.list[i];
                    stageData.isSelected = stageData.id == currentCarpetData?.id;
                    var obj = allCarpets[i];
                    obj.FillData(stageData);
                    if (stageData.isSelected)
                        currentCarpetData = carpets.list[i];

                    yield return null;
                }
            }
            else
            {
                allCarpets.Clear();
                for (int i = 0; i < carpets.list.Count; i++)
                {
                    var stageData = carpets.list[i];
                    stageData.isSelected = stageData.id == currentCarpetData?.id;
                    var obj = uiCarpetBtnPrefab.Spawn(carpetStagesRect);
                    obj.FillData(stageData);
                    obj.OnCarpetSelected = (cd) =>
                    {
                        currentCarpetData = cd;
                        FirebaseManager.LogEvent("carpet_click", new Dictionary<string, object> { { "id", currentCarpetData.id } });
                    };
                    allCarpets.Add(obj);

                    if (stageData.isSelected)
                        currentCarpetData = carpets.list[i];

                    yield return null;
                }
            }
        }
        if (chairs)
        {
            currentChairData = DataManager.CurrentChair;
            uiChairBtnPrefab.RecycleAll();
            if (allChairs.Count == chairs.list.Count)
            {
                for (int i = 0; i < allChairs.Count; i++)
                {
                    var stageData = chairs.list[i];
                    stageData.isSelected = stageData.id == currentChairData?.id;
                    var obj = allChairs[i];
                    obj.FillData(stageData);
                    if (stageData.isSelected)
                        currentChairData = chairs.list[i];

                    yield return null;
                }
            }
            else
            {
                allChairs.Clear();
                for (int i = 0; i < chairs.list.Count; i++)
                {
                    var stageData = chairs.list[i];
                    stageData.isSelected = stageData.id == currentChairData?.id;
                    var obj = uiChairBtnPrefab.Spawn(chaiStagesRect);
                    obj.FillData(stageData);
                    obj.OnChairSelected = (cd) =>
                    {
                        currentChairData = cd;
                        FirebaseManager.LogEvent("carpet_click", new Dictionary<string, object> { { "id", currentChairData.id } });
                    };
                    allChairs.Add(obj);

                    if (stageData.isSelected)
                        currentChairData = chairs.list[i];

                    yield return null;
                }
            }
        }
        if (tables)
        {
            currentTableData = DataManager.CurrentTable;
            uiTableBtnPrefab.RecycleAll();
            if (allTables.Count == tables.list.Count)
            {
                for (int i = 0; i < allTables.Count; i++)
                {
                    var stageData = tables.list[i];
                    stageData.isSelected = stageData.id == currentTableData?.id;
                    var obj = allTables[i];
                    obj.FillData(stageData);
                    if (stageData.isSelected)
                        currentTableData = tables.list[i];

                    yield return null;
                }
            }
            else
            {
                allTables.Clear();
                for (int i = 0; i < tables.list.Count; i++)
                {
                    var stageData = tables.list[i];
                    stageData.isSelected = stageData.id == currentTableData?.id;
                    var obj = uiTableBtnPrefab.Spawn(tableStagesRect);
                    obj.FillData(stageData);
                    obj.OnTableSelected = (td) =>
                    {
                        currentTableData = td;
                        FirebaseManager.LogEvent("table_click", new Dictionary<string, object> { { "id", currentTableData.id } });
                    };
                    allTables.Add(obj);

                    if (stageData.isSelected)
                        currentTableData = tables.list[i];

                    yield return null;
                }
            }
        }
        if (lamps)
        {
            currentLamp = DataManager.CurrentLamp;
            uiLampBtnPrefab.RecycleAll();
            if (allLamps.Count == lamps.list.Count)
            {
                for (int i = 0; i < allLamps.Count; i++)
                {
                    var stageData = lamps.list[i];
                    stageData.isSelected = stageData.id == currentLamp?.id;
                    var obj = allLamps[i];
                    obj.FillData(stageData);
                    if (stageData.isSelected)
                        currentLamp = lamps.list[i];

                    yield return null;
                }
            }
            else
            {
                allLamps.Clear();
                for (int i = 0; i < lamps.list.Count; i++)
                {
                    var stageData = lamps.list[i];
                    stageData.isSelected = stageData.id == currentLamp?.id;
                    var obj = uiLampBtnPrefab.Spawn(lampStagesRect);
                    obj.FillData(stageData);
                    obj.OnLampSelected = (ld) =>
                    {
                        currentLamp = ld;
                        FirebaseManager.LogEvent("carpet_click", new Dictionary<string, object> { { "id", currentLamp.id } });
                    };
                    allLamps.Add(obj);

                    if (stageData.isSelected)
                        currentLamp = lamps.list[i];

                    yield return null;
                }
            }
        }
    }
    private void CreateImageCoverForTutorial(UISkinChild btn)
    {
        buyTutorialBtn = btn.coinBtn;
        GameObject imgCover = new GameObject();
        imgCover.AddComponent<RectTransform>();
        imgCover.AddComponent<Image>();
        var img = imgCover.Spawn(btn.transform);
        img.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
        imgCoverRectTf = img.GetComponent<RectTransform>();
        imgCoverRectTf.anchoredPosition3D = new Vector3(0, 0, -10);
        imgCoverRectTf.sizeDelta = Vector3.one * 5000;
        imgCoverRectTf.transform.SetAsFirstSibling();

        shopScrollView.FirstOrDefault().GetComponent<ScrollRect>().enabled = false;
        btnBack.enabled = false;

        btn.handTut?.SetActive(true);

        StartCoroutine(YeildHightLight());
        CoinManager.Add(2000);
    }

    public void Show()
    {
        //CoinManager.Add(10000);
        anim.Show(() =>
        {
        });
    }
    private IEnumerator YeildHightLight()
    {
        while (true)
        {
            var anim = buyTutorialBtn.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger(hightLightTrigger);
            }
            yield return new WaitForSeconds(1);
        }
    }
    public void Hide()
    {
        if (anim.Status == UIAnimStatus.IsShow)
        {
            StopCoroutine( YeildHightLight());

            if (DataManager.SkinsAsset.Current != null)
            {
                if (currentSkinData != null)
                    currentSkinData.isSelected = false;
                DataManager.SkinsAsset.Current.isSelected = true;
                if (DataManager.SkinsAsset.Current.id != currentSkinData.id)
                    DataManager.SkinsAsset.SetChanged(DataManager.SkinsAsset.Current);
            }
            currentSkinData = null;
            anim.Hide();
        }
    }
    public void Ins_FillData()
    {
        StartCoroutine(FillData());
    }

    [SerializeField]
    private GameObject[] shopScrollView = null;
    [SerializeField]
    private Image[] buttonTabImgsNew = null;
    [SerializeField]
    private Sprite[] buttonTabSelectedSprites = null;
    [SerializeField]
    private Sprite[] buttonTabNonSelectedSprites = null;
    [SerializeField]
    private Button closeBtn = null;
    private void ActiveTab()
    {
        for (int i = 0; i < shopScrollView.Length; i++)
        {
            shopScrollView[i].SetActive(i == currentTab);
        }
    }
    private int currentTab = 0;
    public void ChangeTab(int tab)
    {
        currentTab = tab;
        ActiveTab();
        for (int i = 0; i < buttonTabImgsNew.Length; i++)
        {
            buttonTabImgsNew[i].sprite = i == tab ? buttonTabSelectedSprites[i] : buttonTabNonSelectedSprites[i];
        }
    }
}
