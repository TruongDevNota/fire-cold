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

    private void Awake()
    {
        itemSelectPrefab.CreatePool(totalLevel);
    }

    public void OnShow()
    {
        for(int i = 1; i<= totalLevel; i++)
        {
            var isExist = i < selectItems.Count;
            var item = isExist ? selectItems[i-1] : itemSelectPrefab.Spawn(contentRect);
            if(!isExist)
                selectItems.Add(item);
            item.Fill(i, OnLevelSelectHandle, isTest);
        }
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(DataManager.UserData.level * 1f / totalLevel);
        anim.Show();
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
}
