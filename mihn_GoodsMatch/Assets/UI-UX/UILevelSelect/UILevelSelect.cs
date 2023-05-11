using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILevelSelect : MonoBehaviour
{
    [SerializeField]
    private UIAnimation anim;
    [SerializeField]
    private UILevelSelectItem itemSelectPrefab;
    [SerializeField]
    private int totalLevel = 30;
    [SerializeField]
    private ScrollRect scrollRect;
    [SerializeField]
    private RectTransform contentRect;

    [Header("Test level")]
    [SerializeField]
    private bool isTest = false;

    private List<UILevelSelectItem> selectItems = new List<UILevelSelectItem>();
    [SerializeField] private Image BG=null;
    [SerializeField] private Sprite[] imageBG;
    [SerializeField] private Image Title = null;
    [SerializeField] private Sprite[] imageTitle;

    private void Awake()
    {
        itemSelectPrefab.CreatePool(DataManager.GameConfig.totalLevel);
    }

    public void OnShow(int mapIndex)
    {
        SoundManager.Play("1. Click Button");
        for (int i = 1; i <= DataManager.MapAsset.listMaps[DataManager.mapSelect-1].totalLevel; i++)
        {
            
            var isExist = i <= selectItems.Count;
            var item = isExist ? selectItems[i - 1] : itemSelectPrefab.Spawn(contentRect);
            if (!isExist)
                selectItems.Add(item);
            item.Fill(i, OnLevelSelectHandle,mapIndex, isTest);
        }
        int lastLevel = DataManager.levelSelect == 0 ? DataManager.UserData.level[mapIndex-1] : DataManager.levelSelect - 1;
        anim.Show(onStart: () => { scrollRect.verticalNormalizedPosition = 1 - (lastLevel / 3) * 1f / (DataManager.GameConfig.totalLevel / 3); });
        BG.sprite = imageBG[DataManager.mapSelect - 1];
        Title.sprite = imageTitle[DataManager.mapSelect - 1];
    }

    public void OnLevelSelectHandle(int level)
    {
        DataManager.levelSelect = level;
        //DataManager.SetCurrLevelConfigData();
        GameStateManager.LoadGame(null);
        OnHide();
    }

    public void OnHide()
    {
        anim.Hide();
    }
    public void Close()
    {
        OnHide();
        foreach(Transform i in contentRect.transform)
        {
            Destroy(i.gameObject);
        }
        selectItems.Clear();
        GameUIManager.PopupMapSelect.Show();
    }
}