using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(UIAnimation))]
public class UIDecor : MonoBehaviour
{
    [SerializeField]
    private UIAnimation anim;
    [SerializeField]
    private Image wallImg = null;
    [SerializeField]
    private Image windowsImg = null;
    [SerializeField]
    private Image floorImg = null;
    [SerializeField]
    private Image ceillingImg = null;
    [SerializeField]
    private Image carpetImg = null;
    [SerializeField]
    private Image chairsImg = null;
    [SerializeField]
    private Image tableImg = null;
    [SerializeField]
    private Image lampImg = null;

    private void OnEnable()
    {
        DataManager.OnLoaded += DataManager_OnLoaded;

        SkinsAsset.OnChanged += SkinsAsset_OnChanged;
        WindowsAsset.OnChanged += WindowsAsset_OnChanged;
        FloorsAsset.OnChanged += FloorsAsset_OnChanged;
        CeillingAsset.OnChanged += CeillingAsset_OnChanged;
        CarpetsAsset.OnChanged += CarpetsAsset_OnChanged;
        ChairsAsset.OnChanged += ChairsAsset_OnChanged;
        TablesAsset.OnChanged += TablesAsset_OnChanged;
        LampsAsset.OnChanged += LampsAsset_OnChanged;
    }

    private void LampsAsset_OnChanged(LampData current, List<LampData> list)
    {
        SetLampDecorSprite(current);
    }

    private void TablesAsset_OnChanged(TableData current, List<TableData> list)
    {
        SetTableDecorSprite(current);
    }

    private void ChairsAsset_OnChanged(ChairData current, List<ChairData> list)
    {
        SetChairDecorSprite(current);
    }

    private void CarpetsAsset_OnChanged(CarpetData current, List<CarpetData> list)
    {
        SetCarpetDocorSprite(current);
    }

    private void CeillingAsset_OnChanged(CeillingData current, List<CeillingData> list)
    {
        SetCeillingDecorSprite(current);
    }

    private void FloorsAsset_OnChanged(FloorData current, List<FloorData> list)
    {
        floorImg.sprite = current.main;
    }

    private void WindowsAsset_OnChanged(WindowsData current, List<WindowsData> list)
    {
        SetWindowsDecorSprite(current);
    }

    private void DataManager_OnLoaded(GameData gameData)
    {
        wallImg.sprite = DataManager.SkinsAsset.Current.main;

        SetWindowsDecorSprite(DataManager.WindowAsset.Current);

        floorImg.sprite = DataManager.FloorAsset.Current.main;

        SetCeillingDecorSprite(DataManager.CeillingAsset.Current);

        SetCarpetDocorSprite(DataManager.CarpetsAsset.Current);

        SetChairDecorSprite(DataManager.ChairsAsset.Current);

        SetTableDecorSprite(DataManager.TableAssets.Current);

        SetLampDecorSprite(DataManager.LampsAsset.Current);
    }
    private void SetChairDecorSprite(ChairData current)
    {
        chairsImg.sprite = current.main;
        chairsImg.SetNativeSize();
        var rect = chairsImg.GetComponent<RectTransform>();
        var rootSize = rect.sizeDelta;
        rect.sizeDelta = rootSize * 0.5f;
    }
    private void SetTableDecorSprite(TableData current)
    {
        tableImg.sprite = current.main;
        tableImg.SetNativeSize();
        var rect = tableImg.GetComponent<RectTransform>();
        var rootSize = rect.sizeDelta;
        rect.sizeDelta = rootSize * 0.5f;
    }
    private void SetLampDecorSprite(LampData current)
    {
        lampImg.sprite = current.main;
        lampImg.SetNativeSize();
        var rect = lampImg.GetComponent<RectTransform>();
        var rootSize = rect.sizeDelta;
        rect.sizeDelta = rootSize * 0.5f;
    }
    private void SetCarpetDocorSprite(CarpetData current)
    {
        carpetImg.sprite = current.main;
        carpetImg.SetNativeSize();
        var rect = carpetImg.GetComponent<RectTransform>();
        var rootSize = rect.sizeDelta;
        rect.sizeDelta = rootSize * 0.5f;
    }
    private void SetWindowsDecorSprite(WindowsData current)
    {
        windowsImg.sprite = current.main;
        windowsImg.SetNativeSize();
        var rect = windowsImg.GetComponent<RectTransform>();
        var rootSize = rect.sizeDelta;
        rect.sizeDelta = rootSize * 0.5f;
    }
    private void SetCeillingDecorSprite(CeillingData current)
    {
        ceillingImg.sprite = current.main;
        ceillingImg.SetNativeSize();
        var ceillingrect = ceillingImg.GetComponent<RectTransform>();
        var ceillingrootSize = ceillingrect.sizeDelta;
        ceillingrect.sizeDelta = ceillingrootSize * 0.5f;
    }

    private void SkinsAsset_OnChanged(SkinData current, List<SkinData> list)
    {
        wallImg.sprite = current.main;
    }
    private void OnDisable()
    {
        SkinsAsset.OnChanged -= SkinsAsset_OnChanged;
        WindowsAsset.OnChanged -= WindowsAsset_OnChanged;
        FloorsAsset.OnChanged -= FloorsAsset_OnChanged;
        CeillingAsset.OnChanged -= CeillingAsset_OnChanged;
        CarpetsAsset.OnChanged -= CarpetsAsset_OnChanged;
    }
    public void Show()
    {
        anim.Show();
    }
    public void Hide()
    {
        anim.Hide();
    }
}
