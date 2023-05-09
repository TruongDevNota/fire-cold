using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupSelectMap : MonoBehaviour
{
    [SerializeField] private UIAnimation anim;
    [SerializeField] private UIMapSelectButton mapSelectPrefab;
    [SerializeField]
    private RectTransform contentRect;
    private List<UIMapSelectButton> selectItems = new List<UIMapSelectButton>();
    private bool isTest = false;
    [SerializeField]
    private ScrollRect scrollRect;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Show()
    {
        SoundManager.Play("1. Click Button");
        for (int i = 1; i <=DataManager.MapAsset.totalMap; i++)
        {
            var isExist = i <= selectItems.Count;
            var item = isExist ? selectItems[i - 1] : mapSelectPrefab.Spawn(contentRect);
            if (!isExist)
                selectItems.Add(item);
            item.Fill(i, OnLevelSelectHandle, isTest);
        }
        anim.Show();
        //int lastMap = DataManager.mapSelect == 0 ? DataManager.UserData.mapIndex : DataManager.mapSelect - 1;
        //anim.Show(onStart: () => { scrollRect.verticalNormalizedPosition = 1 - (lastMap / 3) * 1f / (DataManager.GameConfig.totalMap / 3); });
    }
    public void OnLevelSelectHandle(int map)
    {
        DataManager.mapSelect = map;
        GameUIManager.PopupLevelSelect.OnShow(map);
        this.PostEvent((int)EventID.ChooseMap);
        Hide();
    }
    public void Hide()
    {
        anim.Hide();
    }
}
