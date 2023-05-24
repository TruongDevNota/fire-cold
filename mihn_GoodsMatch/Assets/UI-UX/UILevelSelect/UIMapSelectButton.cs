using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

public class UIMapSelectButton : MonoBehaviour
{
    [SerializeField] Button btn_SelectMap;
    [SerializeField] Image img_Map;
    [SerializeField] Text txt_MapTitle;
    [SerializeField] GameObject starOb;
    [SerializeField] Text txt_Star;
    [SerializeField] SkeletonGraphic anim_Lock;
    [SerializeField] GameObject leftPathOb;
    [SerializeField] GameObject rightPathOb;

    [SerializeField] Color lockMapColor;
    [SerializeField] Color unlockMapColor;
    
    private System.Action<int> onMapSelect;
    private MapData mapData;
    public void Fill(MapData datum, System.Action<int> OnMapSelect)
    {
        mapData = datum;
        img_Map.sprite = DataManager.MapAsset.mapIcons[mapData.mapIndex - 1];
        img_Map.SetColor(mapData.isUnlocked ? unlockMapColor : lockMapColor);
        txt_MapTitle.text = mapData.mapName;
        starOb?.SetActive(mapData.isUnlocked);
        txt_Star.text = string.Format("{0}/{1}", mapData.GetAllStarClaimed(), mapData.totalLevel*3);
        
        leftPathOb.SetActive(mapData.mapIndex % 2 == 1 && mapData.mapIndex < DataManager.MapAsset.totalMap);
        rightPathOb.SetActive(mapData.mapIndex % 2 == 0 && mapData.mapIndex < DataManager.MapAsset.totalMap);

        anim_Lock.gameObject.SetActive(!mapData.isUnlocked);
        if (!mapData.isUnlocked)
            anim_Lock.AnimationState.SetAnimation(0, "idle", true);

        onMapSelect = OnMapSelect;
        btn_SelectMap.onClick.RemoveAllListeners();
        btn_SelectMap.onClick.AddListener(ButtonMapSelect);
    }



    public IEnumerator YileShowUnlockAnim(System.Action onAnimcomplete = null)
    {
        Debug.Log($"Start show unlock anim on map: {mapData.mapName}");

        anim_Lock.AnimationState.SetAnimation(0, "unlocked", false);
        anim_Lock.Update(0);
        yield return new WaitForEndOfFrame();
        anim_Lock.AnimationState.Complete += delegate
        {
            anim_Lock.gameObject.SetActive(false);
            img_Map.SetColor(unlockMapColor);
            onAnimcomplete?.Invoke();
        };
    }

    private void ButtonMapSelect()
    {
        if (!mapData.isUnlocked)
            return;
        SoundManager.Play("1. Click Button");
        onMapSelect?.Invoke(mapData.mapIndex);
    }

    [MyBox.ButtonMethod]
    public void TestUnlockAnim()
    {
        StartCoroutine(YileShowUnlockAnim());
    }
}
