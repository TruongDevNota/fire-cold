using Base.Ads;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkinChild : MonoBehaviour
{
    [Header("Thumb")]
    [SerializeField] Image imgMain = null;
    [SerializeField] Image backgrounMain = null;
    [SerializeField] Sprite firstSprite = null;
    [SerializeField] Sprite selectedSprite = null;
    [SerializeField] Sprite nonSelectSprite = null;
    [SerializeField] GameObject selectedObj = null;

    [Header("Gold")]
    [SerializeField] Button btnGold = null;
    [SerializeField] Text txtGold = null;

    [Header("Ads")]
    [SerializeField] Button btnAds = null;
    [SerializeField] Text txtAds = null;

    [Header("Star")]
    [SerializeField] Button btnStar = null;
    [SerializeField] Text txtStar = null;

    [Header("Equipe")]
    [SerializeField] Button btnEquipe = null;

    [Header("Tut")]
    [SerializeField] GameObject _handTut = null;

    public Action<SkinData> OnSelected = delegate { };
    public Action<WindowsData> OnWindowsSelected = delegate { };
    public Action<FloorData> OnFloorSelected = delegate { };
    public Action<CeillingData> OnCeillingSelected = delegate { };
    public Action<CarpetData> OnCarpetSelected = delegate { };
    public Action<ChairData> OnChairSelected = delegate { };
    public Action<TableData> OnTableSelected = delegate { };
    public Action<LampData> OnLampSelected = delegate { };

    [HideInInspector]
    public SkinData data;
    [HideInInspector]
    public WindowsData windowsData;
    [HideInInspector]
    public FloorData floorData;
    [HideInInspector]
    public CeillingData ceillingData;
    [HideInInspector]
    public CarpetData carpetData;
    [HideInInspector]
    public ChairData chairData;
    [HideInInspector]
    public TableData tableData;
    [HideInInspector]
    public LampData lampData;
    public Button coinBtn => btnGold;
    public Button adsBtn => btnAds;
    public GameObject handTut => _handTut;

    private Vector2 noDecorIconSize = new Vector2(60, 90);
    private void OnEnable()
    {
        SkinsAsset.OnChanged += SkinsAsset_OnChanged;
        WindowsAsset.OnChanged += WindowsAsset_OnChanged;
        FloorsAsset.OnChanged += FloorsAsset_OnChanged;
        CeillingAsset.OnChanged += CeillingAsset_OnChanged;
        CarpetsAsset.OnChanged += CarpetsAsset_OnChanged;
        ChairsAsset.OnChanged += ChairsAsset_OnChanged;
        TablesAsset.OnChanged += TablesAsset_OnChanged;
        LampsAsset.OnChanged += LampsAsset_OnChanged;

        if (handTut != null)
            handTut.SetActive(false);
    }

    private void LampsAsset_OnChanged(LampData current, List<LampData> list)
    {
        FillData(lampData);
    }

    private void TablesAsset_OnChanged(TableData current, List<TableData> list)
    {
        FillData(tableData);
    }

    private void ChairsAsset_OnChanged(ChairData current, List<ChairData> list)
    {
        FillData(chairData);
    }

    private void CarpetsAsset_OnChanged(CarpetData current, List<CarpetData> list)
    {
        FillData(carpetData);
    }

    private void CeillingAsset_OnChanged(CeillingData current, List<CeillingData> list)
    {
        FillData(ceillingData);
    }

    private void FloorsAsset_OnChanged(FloorData current, List<FloorData> list)
    {
        FillData(floorData);
    }

    private void WindowsAsset_OnChanged(WindowsData current, List<WindowsData> list)
    {
        FillData(windowsData);
    }

    private void OnDisable()
    {
        SkinsAsset.OnChanged -= SkinsAsset_OnChanged;
        WindowsAsset.OnChanged -= WindowsAsset_OnChanged;
        FloorsAsset.OnChanged -= FloorsAsset_OnChanged;
        CeillingAsset.OnChanged -= CeillingAsset_OnChanged;
        CarpetsAsset.OnChanged -= CarpetsAsset_OnChanged;
        ChairsAsset.OnChanged -= ChairsAsset_OnChanged;
        TablesAsset.OnChanged -= TablesAsset_OnChanged;
        LampsAsset.OnChanged -= LampsAsset_OnChanged;
    }

    private void SkinsAsset_OnChanged(SkinData current, List<SkinData> list)
    {
        FillData(data);
    }
    public void FillData(SkinData skinData)
    {
        imgMain.sprite = skinData.index == 0 ? firstSprite : skinData.main;
        var rect = imgMain.GetComponent<RectTransform>();
        if (skinData.index != 0)
        {
            //imgMain.SetNativeSize();
        }
        rect.anchoredPosition = new Vector2(0, 8);
       // rect.sizeDelta = skinData.index == 0 ? noDecorIconSize : rect.sizeDelta * 0.04f;

        selectedObj.SetActive(skinData.isSelected && skinData.isUnlocked);
        backgrounMain.sprite = (skinData.isSelected && skinData.isUnlocked) ? selectedSprite : nonSelectSprite;

        btnGold.gameObject.SetActive(!skinData.isUnlocked && skinData._unlockType == UnlockType.Gold);
        txtGold.text = skinData._unlockPrice.ToString();
        btnAds.gameObject.SetActive(!skinData.isUnlocked && skinData._unlockType == UnlockType.Ads);
        txtAds.text = /*skinData._unlockPrice.ToString()*/"FREE";

        if (skinData._unlockType == UnlockType.Gold && DataManager.UserData.TotalWin >= skinData._unlockPrice)
        {
            if (!skinData.isUnlocked)
            {
                skinData.isUnlocked = true;
                skinData.isSelected = false;
            }
            DataManager.Save();
        }
        btnStar.gameObject.SetActive(/*!windowsData.isUnlocked && windowsData._unlockType == UnlockType.Gold*/false);
        txtStar.text = $"{DataManager.UserData.TotalWin}/{skinData._unlockPrice}";
        btnEquipe.gameObject.SetActive(!skinData.isSelected && skinData.isUnlocked);
        //btnEquiped.gameObject.SetActive(windowsData.isSelected && windowsData.isUnlocked);

        data = skinData;
    }
    public void FillData(WindowsData windowsData)
    {
        imgMain.sprite = windowsData.index == 0 ? firstSprite : windowsData.main;

        var rect = imgMain.GetComponent<RectTransform>();
        if (windowsData.index != 0)
        {
            imgMain.SetNativeSize();
        }
        rect.anchoredPosition = new Vector2(0, 8);
        rect.sizeDelta = windowsData.index == 0 ? noDecorIconSize : rect.sizeDelta * 0.09f;

        selectedObj.SetActive(windowsData.isSelected && windowsData.isUnlocked);
        backgrounMain.sprite = (windowsData.isSelected && windowsData.isUnlocked) ? selectedSprite : nonSelectSprite;


        btnGold.gameObject.SetActive(!windowsData.isUnlocked && windowsData._unlockType == UnlockType.Gold);
        txtGold.text = windowsData._unlockPrice.ToString();
        btnAds.gameObject.SetActive(!windowsData.isUnlocked && windowsData._unlockType == UnlockType.Ads);
        txtAds.text = /*windowsData._unlockPrice.ToString()*/"FREE";

        if (windowsData._unlockType == UnlockType.Gold && DataManager.UserData.TotalWin >= windowsData._unlockPrice)
        {
            if (!windowsData.isUnlocked)
            {
                windowsData.isUnlocked = true;
                windowsData.isSelected = false;
            }
            DataManager.Save();
        }
        btnStar.gameObject.SetActive(/*!windowsData.isUnlocked && windowsData._unlockType == UnlockType.Gold*/false);
        txtStar.text = $"{DataManager.UserData.TotalWin}/{windowsData._unlockPrice}";

        btnEquipe.gameObject.SetActive(!windowsData.isSelected && windowsData.isUnlocked);
        //btnEquiped.gameObject.SetActive(windowsData.isSelected && windowsData.isUnlocked);

        this.windowsData = windowsData;
    }
    public void FillData(FloorData floorData)
    {
        imgMain.sprite = floorData.index == 0 ? firstSprite : floorData.main;

        var rect = imgMain.GetComponent<RectTransform>();
        if (floorData.index != 0)
        {
            imgMain.SetNativeSize();
        }
        rect.anchoredPosition = new Vector2(0, 8);
        rect.sizeDelta = floorData.index == 0 ? noDecorIconSize : rect.sizeDelta * 0.04f;

        selectedObj.SetActive(floorData.isSelected && floorData.isUnlocked);
        backgrounMain.sprite = (floorData.isSelected && floorData.isUnlocked) ? selectedSprite : nonSelectSprite;


        btnGold.gameObject.SetActive(!floorData.isUnlocked && floorData._unlockType == UnlockType.Gold);
        txtGold.text = floorData._unlockPrice.ToString();
        btnAds.gameObject.SetActive(!floorData.isUnlocked && floorData._unlockType == UnlockType.Ads);
        txtAds.text =/* floorData._unlockPrice.ToString()*/"FREE";

        if (floorData._unlockType == UnlockType.Gold && DataManager.UserData.TotalWin >= floorData._unlockPrice)
        {
            if (!floorData.isUnlocked)
            {
                floorData.isUnlocked = true;
                floorData.isSelected = false;
            }
            DataManager.Save();
        }
        btnStar.gameObject.SetActive(/*!windowsData.isUnlocked && windowsData._unlockType == UnlockType.Gold*/false);
        txtStar.text = $"{DataManager.UserData.TotalWin}/{floorData._unlockPrice}";
        btnEquipe.gameObject.SetActive(!floorData.isSelected && floorData.isUnlocked);
        //btnEquiped.gameObject.SetActive(windowsData.isSelected && windowsData.isUnlocked);

        this.floorData = floorData;
    }
    public void FillData(CeillingData ceillingData)
    {
        imgMain.sprite = ceillingData.index == 0 ? firstSprite : ceillingData.main;

        var rect = imgMain.GetComponent<RectTransform>();
        if (ceillingData.index != 0)
        {
            imgMain.SetNativeSize();
        }
        rect.anchoredPosition = new Vector2(0, 8);
        rect.sizeDelta = ceillingData.index == 0 ? noDecorIconSize : rect.sizeDelta * 0.09f;

        selectedObj.SetActive(ceillingData.isSelected && ceillingData.isUnlocked);
        backgrounMain.sprite = (ceillingData.isSelected && ceillingData.isUnlocked) ? selectedSprite : nonSelectSprite;


        btnGold.gameObject.SetActive(!ceillingData.isUnlocked && ceillingData._unlockType == UnlockType.Gold);
        txtGold.text = ceillingData._unlockPrice.ToString();
        btnAds.gameObject.SetActive(!ceillingData.isUnlocked && ceillingData._unlockType == UnlockType.Ads);
        txtAds.text = /*ceillingData._unlockPrice.ToString()*/"FREE";

        if (ceillingData._unlockType == UnlockType.Gold && DataManager.UserData.TotalWin >= ceillingData._unlockPrice)
        {
            if (!ceillingData.isUnlocked)
            {
                ceillingData.isUnlocked = true;
                ceillingData.isSelected = false;
            }
            DataManager.Save();
        }
        btnStar.gameObject.SetActive(/*!windowsData.isUnlocked && windowsData._unlockType == UnlockType.Gold*/false);
        txtStar.text = $"{DataManager.UserData.TotalWin}/{ceillingData._unlockPrice}";
        btnEquipe.gameObject.SetActive(!ceillingData.isSelected && ceillingData.isUnlocked);
        //btnEquiped.gameObject.SetActive(windowsData.isSelected && windowsData.isUnlocked);

        this.ceillingData = ceillingData;
    }
    public void FillData(CarpetData carpetData)
    {
        imgMain.sprite = carpetData.index == 0 ? firstSprite : carpetData.main;

        var rect = imgMain.GetComponent<RectTransform>();
        if (carpetData.index != 0)
        {
            imgMain.SetNativeSize();
        }
        rect.anchoredPosition = new Vector2(0, 8);
        rect.sizeDelta = carpetData.index == 0 ? noDecorIconSize : rect.sizeDelta * 0.075f;

        selectedObj.SetActive(carpetData.isSelected && carpetData.isUnlocked);
        backgrounMain.sprite = (carpetData.isSelected && carpetData.isUnlocked) ? selectedSprite : nonSelectSprite;


        btnGold.gameObject.SetActive(!carpetData.isUnlocked && carpetData._unlockType == UnlockType.Gold);
        txtGold.text = carpetData._unlockPrice.ToString();
        btnAds.gameObject.SetActive(!carpetData.isUnlocked && carpetData._unlockType == UnlockType.Ads);
        txtAds.text = /*carpetData._unlockPrice.ToString()*/"FREE";

        if (carpetData._unlockType == UnlockType.Gold && DataManager.UserData.TotalWin >= carpetData._unlockPrice)
        {
            if (!carpetData.isUnlocked)
            {
                carpetData.isUnlocked = true;
                carpetData.isSelected = false;
            }
            DataManager.Save();
        }
        btnStar.gameObject.SetActive(/*!windowsData.isUnlocked && windowsData._unlockType == UnlockType.Gold*/false);
        txtStar.text = $"{DataManager.UserData.TotalWin}/{carpetData._unlockPrice}";
        btnEquipe.gameObject.SetActive(!carpetData.isSelected && carpetData.isUnlocked);
        //btnEquiped.gameObject.SetActive(windowsData.isSelected && windowsData.isUnlocked);

        this.carpetData = carpetData;
    }
    public void FillData(ChairData chairData)
    {
        imgMain.sprite = chairData.index == 0 ? firstSprite : chairData.main;

        var rect = imgMain.GetComponent<RectTransform>();
        if (chairData.index != 0)
        {
            imgMain.SetNativeSize();
        }
        rect.anchoredPosition = new Vector2(0, 8);
        rect.sizeDelta = chairData.index == 0 ? noDecorIconSize : rect.sizeDelta * 0.09f;

        selectedObj.SetActive(chairData.isSelected && chairData.isUnlocked);
        backgrounMain.sprite = (chairData.isSelected && chairData.isUnlocked) ? selectedSprite : nonSelectSprite;


        btnGold.gameObject.SetActive(!chairData.isUnlocked && chairData._unlockType == UnlockType.Gold);
        txtGold.text = chairData._unlockPrice.ToString();
        btnAds.gameObject.SetActive(!chairData.isUnlocked && chairData._unlockType == UnlockType.Ads);
        txtAds.text = /*chairData._unlockPrice.ToString()*/"FREE";

        if (chairData._unlockType == UnlockType.Gold && DataManager.UserData.TotalWin >= chairData._unlockPrice)
        {
            if (!chairData.isUnlocked)
            {
                chairData.isUnlocked = true;
                chairData.isSelected = false;
            }
            DataManager.Save();
        }
        btnStar.gameObject.SetActive(/*!windowsData.isUnlocked && windowsData._unlockType == UnlockType.Gold*/false);
        txtStar.text = $"{DataManager.UserData.TotalWin}/{chairData._unlockPrice}";
        btnEquipe.gameObject.SetActive(!chairData.isSelected && chairData.isUnlocked);
        //btnEquiped.gameObject.SetActive(windowsData.isSelected && windowsData.isUnlocked);

        this.chairData = chairData;
    }
    public void FillData(TableData table)
    {
        imgMain.sprite = table.index == 0 ? firstSprite : table.main;

        var rect = imgMain.GetComponent<RectTransform>();
        if (table.index != 0)
        {
            imgMain.SetNativeSize();
        }
        rect.anchoredPosition = new Vector2(0, 8);
        rect.sizeDelta = table.index == 0 ? noDecorIconSize : rect.sizeDelta * 0.14f;

        selectedObj.SetActive(table.isSelected && table.isUnlocked);
        backgrounMain.sprite = (table.isSelected && table.isUnlocked) ? selectedSprite : nonSelectSprite;


        btnGold.gameObject.SetActive(!table.isUnlocked && table._unlockType == UnlockType.Gold);
        txtGold.text = table._unlockPrice.ToString();
        btnAds.gameObject.SetActive(!table.isUnlocked && table._unlockType == UnlockType.Ads);
        txtAds.text = /*table._unlockPrice.ToString()*/"FREE";

        if (table._unlockType == UnlockType.Gold && DataManager.UserData.TotalWin >= table._unlockPrice)
        {
            if (!table.isUnlocked)
            {
                table.isUnlocked = true;
                table.isSelected = false;
            }
            DataManager.Save();
        }
        btnStar.gameObject.SetActive(/*!windowsData.isUnlocked && windowsData._unlockType == UnlockType.Gold*/false);
        txtStar.text = $"{DataManager.UserData.TotalWin}/{table._unlockPrice}";
        btnEquipe.gameObject.SetActive(!table.isSelected && table.isUnlocked);
        //btnEquiped.gameObject.SetActive(windowsData.isSelected && windowsData.isUnlocked);

        tableData = table;
    }
    public void FillData(LampData carpetData)
    {
        imgMain.sprite = carpetData.index == 0 ? firstSprite : carpetData.main;

        var rect = imgMain.GetComponent<RectTransform>();
        if (carpetData.index != 0)
        {
            //imgMain.SetNativeSize();
        }
        rect.anchoredPosition = new Vector2(0, 8);
        //rect.sizeDelta = carpetData.index == 0 ? noDecorIconSize : rect.sizeDelta * 0.14f;

        selectedObj.SetActive(carpetData.isSelected && carpetData.isUnlocked);
        backgrounMain.sprite = (carpetData.isSelected && carpetData.isUnlocked) ? selectedSprite : nonSelectSprite;


        btnGold.gameObject.SetActive(!carpetData.isUnlocked && carpetData._unlockType == UnlockType.Gold);
        txtGold.text = carpetData._unlockPrice.ToString();
        btnAds.gameObject.SetActive(!carpetData.isUnlocked && carpetData._unlockType == UnlockType.Ads);
        txtAds.text = /*carpetData._unlockPrice.ToString()*/"FREE";

        if (carpetData._unlockType == UnlockType.Gold && DataManager.UserData.TotalWin >= carpetData._unlockPrice)
        {
            if (!carpetData.isUnlocked)
            {
                carpetData.isUnlocked = true;
                carpetData.isSelected = false;
            }
            DataManager.Save();
        }
        btnStar.gameObject.SetActive(/*!windowsData.isUnlocked && windowsData._unlockType == UnlockType.Gold*/false);
        txtStar.text = $"{DataManager.UserData.TotalWin}/{carpetData._unlockPrice}";
        btnEquipe.gameObject.SetActive(!carpetData.isSelected && carpetData.isUnlocked);
        //btnEquiped.gameObject.SetActive(windowsData.isSelected && windowsData.isUnlocked);

        lampData = carpetData;
    }
    public void Ins_OnSelected()
    {
        DataManager.SkinsAsset.SetChanged(data);
        OnSelected?.Invoke(data);
    }
    public void Ins_OnEquipe()
    {
        DataManager.SkinsAsset.Current = data;
        DataManager.Save();
    }
    public void Ins_OnEquipeWindow()
    {
        DataManager.WindowAsset.Current = windowsData;
        DataManager.Save();
    }
    public void Ins_OnEquipeFloor()
    {
        DataManager.FloorAsset.Current = floorData;
        DataManager.Save();
    }
    public void Ins_OnEquipeCeilling()
    {
        DataManager.CeillingAsset.Current = ceillingData;
        DataManager.Save();
    }
    public void Ins_OnEquipeCarpet()
    {
        DataManager.CarpetsAsset.Current = carpetData;
        DataManager.Save();
    }
    public void Ins_OnEquipeChair()
    {
        DataManager.ChairsAsset.Current = chairData;
        DataManager.Save();
    }
    public void Ins_OnEquipeTable()
    {
        DataManager.TableAssets.Current = tableData;
        DataManager.Save();
    }
    public void Ins_OnEquipelamp()
    {
        DataManager.LampsAsset.Current = lampData;
        DataManager.Save();
    }
    public void Ins_OnBuy()
    {
        if (!data.isUnlocked)
        {
            if (data._unlockType == UnlockType.Gold)
            {
                if (DataManager.UserData.totalCoin >= data._unlockPrice)
                {
                    CoinManager.Add(-data._unlockPrice);
                    data.isUnlocked = true;
                    DataManager.SkinsAsset.Current = data;
                    if (!DataManager.UserData.isShopTutShowed)
                    {
                        DataManager.UserData.isShopTutShowed = true;
                        handTut.SetActive(false);
                        this.PostEvent((int)EventID.OnDecorTutorialComplete);
                    }
                    DataManager.Save();

                }
                else
                {
                    UIToast.ShowNotice("Not enough money!");
                }
            }
            if (data._unlockType == UnlockType.Ads)
            {
                AdsManager.ShowVideoReward((e, t) =>
                {
                    if (e == AdEvent.ShowSuccess)
                    {
                        data.isUnlocked = true;
                        DataManager.SkinsAsset.Current = data;
                        DataManager.Save();
                    }
                    else
                    {
                        AdsManager.ShowNotice(e);
                    }
                }, $"unlock_wall_{data.index}");
            }
        }
    }
    public void Ins_OnBuyWindows()
    {
        if (!windowsData.isUnlocked)
        {
            if (windowsData._unlockType == UnlockType.Gold)
            {
                if (DataManager.UserData.totalCoin >= windowsData._unlockPrice)
                {
                    CoinManager.Add(-windowsData._unlockPrice);
                    windowsData.isUnlocked = true;
                    DataManager.WindowAsset.Current = windowsData;
                    DataManager.Save();

                }
                else
                {
                    UIToast.ShowNotice("Not enough money!");
                }
            }
            if (windowsData._unlockType == UnlockType.Ads)
            {
                AdsManager.ShowVideoReward((e, t) =>
                {
                    if (e == AdEvent.ShowSuccess)
                    {
                        windowsData.isUnlocked = true;
                        DataManager.WindowAsset.Current = windowsData;
                        DataManager.Save();
                    }
                    else
                    {
                        AdsManager.ShowNotice(e);
                    }
                }, $"unlock_windows_{data.index}");
            }
        }
    }
    public void Ins_OnBuyFloor()
    {
        if (!floorData.isUnlocked)
        {
            if (floorData._unlockType == UnlockType.Gold)
            {
                if (DataManager.UserData.totalCoin >= floorData._unlockPrice)
                {
                    CoinManager.Add(-floorData._unlockPrice);
                    floorData.isUnlocked = true;
                    DataManager.FloorAsset.Current = floorData;
                    DataManager.Save();

                }
                else
                {
                    UIToast.ShowNotice("Not enough money!");
                }
            }
            if (floorData._unlockType == UnlockType.Ads)
            {
                AdsManager.ShowVideoReward((e, t) =>
                {
                    if (e == AdEvent.ShowSuccess)
                    {
                        floorData.isUnlocked = true;
                        DataManager.FloorAsset.Current = floorData;
                        DataManager.Save();
                    }
                    else
                    {
                        AdsManager.ShowNotice(e);
                    }
                }, $"unlock_floor_{floorData.index}");
            }
        }
    }
    public void Ins_OnBuyCeilling()
    {
        if (!ceillingData.isUnlocked)
        {
            if (ceillingData._unlockType == UnlockType.Gold)
            {
                if (DataManager.UserData.totalCoin >= ceillingData._unlockPrice)
                {
                    CoinManager.Add(-ceillingData._unlockPrice);
                    ceillingData.isUnlocked = true;
                    DataManager.CeillingAsset.Current = ceillingData;
                    DataManager.Save();

                }
                else
                {
                    UIToast.ShowNotice("Not enough money!");
                }
            }
            if (ceillingData._unlockType == UnlockType.Ads)
            {
                AdsManager.ShowVideoReward((e, t) =>
                {
                    if (e == AdEvent.ShowSuccess)
                    {
                        ceillingData.isUnlocked = true;
                        DataManager.CeillingAsset.Current = ceillingData;
                        DataManager.Save();
                    }
                    else
                    {
                        AdsManager.ShowNotice(e);
                    }
                }, $"unlock_ceilling_{ceillingData.index}");
            }
        }
    }
    public void Ins_OnBuyCarpet()
    {
        if (!carpetData.isUnlocked)
        {
            if (carpetData._unlockType == UnlockType.Gold)
            {
                if (DataManager.UserData.totalCoin >= carpetData._unlockPrice)
                {
                    CoinManager.Add(-carpetData._unlockPrice);
                    carpetData.isUnlocked = true;
                    DataManager.CarpetsAsset.Current = carpetData;
                    DataManager.Save();

                }
                else
                {
                    UIToast.ShowNotice("Not enough money!");
                }
            }
            if (carpetData._unlockType == UnlockType.Ads)
            {
                AdsManager.ShowVideoReward((e, t) =>
                {
                    if (e == AdEvent.ShowSuccess)
                    {
                        carpetData.isUnlocked = true;
                        DataManager.CarpetsAsset.Current = carpetData;
                        DataManager.Save();
                    }
                    else
                    {
                        AdsManager.ShowNotice(e);
                    }
                }, $"unlock_carpet_{carpetData.index}");
            }
        }
    }
    public void Ins_OnBuyChair()
    {
        if (!chairData.isUnlocked)
        {
            if (chairData._unlockType == UnlockType.Gold)
            {
                if (DataManager.UserData.totalCoin >= chairData._unlockPrice)
                {
                    CoinManager.Add(-chairData._unlockPrice);
                    chairData.isUnlocked = true;
                    DataManager.ChairsAsset.Current = chairData;
                    DataManager.Save();

                }
                else
                {
                    UIToast.ShowNotice("Not enough money!");
                }
            }
            if (chairData._unlockType == UnlockType.Ads)
            {
                AdsManager.ShowVideoReward((e, t) =>
                {
                    if (e == AdEvent.ShowSuccess)
                    {
                        chairData.isUnlocked = true;
                        DataManager.ChairsAsset.Current = chairData;
                        DataManager.Save();
                    }
                    else
                    {
                        AdsManager.ShowNotice(e);
                    }
                }, $"unlock_chair_{chairData.index}");
            }
        }
    }
    public void Ins_OnBuyTable()
    {
        if (!tableData.isUnlocked)
        {
            if (tableData._unlockType == UnlockType.Gold)
            {
                if (DataManager.UserData.totalCoin >= tableData._unlockPrice)
                {
                    CoinManager.Add(-tableData._unlockPrice);
                    tableData.isUnlocked = true;
                    DataManager.TableAssets.Current = tableData;
                    DataManager.Save();

                }
                else
                {
                    UIToast.ShowNotice("Not enough money!");
                }
            }
            if (tableData._unlockType == UnlockType.Ads)
            {
                AdsManager.ShowVideoReward((e, t) =>
                {
                    if (e == AdEvent.ShowSuccess)
                    {
                        tableData.isUnlocked = true;
                        DataManager.TableAssets.Current = tableData;
                        DataManager.Save();
                    }
                    else
                    {
                        AdsManager.ShowNotice(e);
                    }
                }, $"unlock_table_{tableData.index}");
            }
        }
    }
    public void Ins_OnBuyLamp()
    {
        if (!lampData.isUnlocked)
        {
            if (lampData._unlockType == UnlockType.Gold)
            {
                if (DataManager.UserData.totalCoin >= lampData._unlockPrice)
                {
                    CoinManager.Add(-lampData._unlockPrice);
                    lampData.isUnlocked = true;
                    DataManager.LampsAsset.Current = lampData;
                    DataManager.Save();

                }
                else
                {
                    UIToast.ShowNotice("Not enough money!");
                }
            }
            if (lampData._unlockType == UnlockType.Ads)
            {
                AdsManager.ShowVideoReward((e, t) =>
                {
                    if (e == AdEvent.ShowSuccess)
                    {
                        lampData.isUnlocked = true;
                        DataManager.LampsAsset.Current = lampData;
                        DataManager.Save();
                    }
                    else
                    {
                        AdsManager.ShowNotice(e);
                    }
                }, $"unlock_lamp_{lampData.index}");
            }
        }
    }
}
