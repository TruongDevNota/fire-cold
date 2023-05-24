using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

public class PopupSelectMap : MonoBehaviour
{
    [SerializeField] private UIAnimation anim;
    [SerializeField] private UIMapSelectButton mapSelectPrefab;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private List<UIMapSelectButton> selectItems = new List<UIMapSelectButton>();
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private PopupUnlockPack popupItemsUnlock;

    [SerializeField] private GameObject btnBack;

    [Space(10)]
    [SerializeField] SkeletonGraphic catAnim;
    [SerializeField] float scrollTime = 1f;
    [SerializeField] Vector2 catPosition;
    [SerializeField] float catMoveTime = 1.2f;
    private RectTransform catRectTf;

    private void Start()
    {
        catRectTf = catAnim.GetComponent<RectTransform>();
    }

    public void Show(bool showUnlock = false)
    {
        SoundManager.Play("1. Click Button");
        for (int i = 0; i < DataManager.MapAsset.ListMap.Count; i++)
        {
            var isExist = i < selectItems.Count;
            var item = isExist ? selectItems[i] : mapSelectPrefab.Spawn(contentRect);
            if (!isExist)
                selectItems.Add(item);

            selectItems[i].Fill(DataManager.MapAsset.ListMap[i], OnLevelSelectHandle);
        }
        anim.Show(onStart: () => {
            catRectTf.SetParent(selectItems[DataManager.mapSelect - 1].transform);
            catRectTf.anchoredPosition = catPosition;
            catAnim.AnimationState.SetAnimation(0, GameConstants.GetRandomCatIdleAnimName(), true);

            scrollRect.vertical = !showUnlock;
            btnBack.SetActive(!showUnlock);

            if (!showUnlock)
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01((DataManager.mapSelect - 1) * 1f / DataManager.MapAsset.ListMap.Count);
            else
            {
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(DataManager.mapSelect * 1f / DataManager.MapAsset.ListMap.Count);
            }
        });
    }

    public IEnumerator YieldUnlockNewMap()
    {
        DataManager.MapAsset.ListMap[DataManager.mapSelect].hightestLevelUnlocked = 1;

        var newMapPos = Mathf.Clamp01(DataManager.mapSelect * 1f / DataManager.MapAsset.ListMap.Count);
        var oldMapPos = Mathf.Clamp01((DataManager.mapSelect - 1) * 1f / DataManager.MapAsset.ListMap.Count);

        bool unlockAnimDone = false;
        yield return selectItems[DataManager.mapSelect].YileShowUnlockAnim(() => { unlockAnimDone = true; });
        while (!unlockAnimDone)
            yield return null;
        yield return new WaitForSeconds(0.5f);

        float t = 0;
        while (t < scrollTime)
        {
            scrollRect.verticalNormalizedPosition = newMapPos - (newMapPos - oldMapPos) * Mathf.Clamp01(t / scrollTime);
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(0.2f);

        t = 0;
        catRectTf.SetParent(selectItems[DataManager.mapSelect].transform);
        var catStartPos = catRectTf.anchoredPosition;
        catAnim.AnimationState.SetAnimation(0, GameConstants.GetRandomCatMoveAnimName(), true);
        while (t < catMoveTime)
        {
            var catPos = Vector2.Lerp(catStartPos, catPosition, Mathf.Clamp01(t / catMoveTime));
            t += Time.deltaTime;
            scrollRect.verticalNormalizedPosition = oldMapPos + (newMapPos - oldMapPos) * Mathf.Clamp01(t / scrollTime);
            catRectTf.anchoredPosition = catPos;
            yield return new WaitForEndOfFrame();
        }
        catAnim.AnimationState.SetAnimation(0, GameConstants.GetRandomCatIdleAnimName(), true);
        yield return new WaitForSeconds(0.2f);

        popupItemsUnlock.OnShow(DataManager.mapSelect + 1);
    }

    public void OnPopupNewItemsClosed()
    {
        OnLevelSelectHandle(DataManager.mapSelect + 1);
    }

    public void OnLevelSelectHandle(int map)
    {
        DataManager.mapSelect = map;
        DataManager.UserData.lastMapIndexSelected = map;
        GameUIManager.PopupLevelSelect.OnShow(map);
        this.PostEvent((int)EventID.ChooseMap);
        Hide();
    }
    public void Hide()
    {
        anim.Hide();
    }

    [MyBox.ButtonMethod]
    public void UnlockNextMap()
    {
        scrollRect.vertical = false;
        DataManager.MapAsset.ListMap[DataManager.mapSelect - 1].DoUnlockAllLevel();
        StartCoroutine(YieldUnlockNewMap());
    }
}
