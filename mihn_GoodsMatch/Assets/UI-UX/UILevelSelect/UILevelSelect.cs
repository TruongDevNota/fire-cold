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
        itemSelectPrefab.CreatePool(DataManager.GameConfig.totalLevel);
    }

    public void OnShow()
    {
        for(int i = 1; i<= DataManager.GameConfig.totalLevel; i++)
        {
            var isExist = i <= selectItems.Count;
            var item = isExist ? selectItems[i-1] : itemSelectPrefab.Spawn(contentRect);
            if(!isExist)
                selectItems.Add(item);
            item.Fill(i, OnLevelSelectHandle, isTest);
        }
        
        anim.Show(onStart: () => { scrollRect.verticalNormalizedPosition = 1 - (DataManager.UserData.level / 3) * 1f / (totalLevel/3); });
    }

    public void OnLevelSelectHandle(int level)
    {
        DataManager.levelSelect = level;
        if (DataManager.levelSelect % DataManager.GameConfig.levelsToNextChallenge == 0)
            this.PostEvent((int)EventID.OnGoToChallengeLevel);
        else
        {
            GameStateManager.LoadGame(null);
            OnHide();
        }
    }

    public void OnHide()
    {
        anim.Hide();
    }
}
