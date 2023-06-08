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
    private ScrollRect scrollRect;
    [SerializeField]
    private RectTransform contentRect;

    private List<UILevelSelectItem> selectItems = new List<UILevelSelectItem>();
    [SerializeField] private Image BG=null;
    [SerializeField] private Sprite[] bgSprites;
    [Space(10)]
    [SerializeField] private Image Title = null;
    [SerializeField] private GameObject[] mapTitleObs;

    private void Awake()
    {
        itemSelectPrefab.CreatePool(DataManager.GameConfig.totalLevel);
    }

    public void OnShow(int mapIndex)
    {
        SoundManager.Play("1. Click Button");
        for (int i = 1; i <= DataManager.MapAsset.ListMap[mapIndex - 1].totalLevel; i++)
        {
            var isExist = i <= selectItems.Count;
            var item = isExist ? selectItems[i - 1] : itemSelectPrefab.Spawn(contentRect);
            if (!isExist)
                selectItems.Add(item);
            item.Fill(i, OnLevelSelectHandle, mapIndex);
        }
        int lastLevel = DataManager.levelSelect == 0 ? DataManager.UserData.level[mapIndex-1] : DataManager.levelSelect - 1;
        BG.sprite = bgSprites[DataManager.mapSelect - 1];
        for(int i = 1 ; i <= mapTitleObs.Length; i++)
        {
            mapTitleObs[i - 1].SetActive(i == DataManager.mapSelect);
        }
        anim.Show(onStart: () => { scrollRect.verticalNormalizedPosition = 1 - (lastLevel / 3) * 1f / (DataManager.GameConfig.totalLevel / 3); });
    }

    public void OnLevelSelectHandle(int level)
    {
        DataManager.levelSelect = level;
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
    public void HomeBtn()
    {
        SoundManager.Play("1. Click Button");
        OnHide();
    }
    [MyBox.ButtonMethod]
    public void UnlockAllMapLevel()
    {
        DataManager.MapAsset.ListMap[DataManager.mapSelect - 1].DoUnlockAllLevel();
        OnShow(DataManager.mapSelect);
    }
}