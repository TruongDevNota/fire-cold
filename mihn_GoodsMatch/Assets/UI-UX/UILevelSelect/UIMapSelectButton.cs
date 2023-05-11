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
        img_Map.sprite = DataManager.MapAsset.mapIcon[mapData.mapIndex - 1];
        img_Map.color = mapData.isUnlocked ? unlockMapColor : lockMapColor;
        txt_MapTitle.text = mapData.mapName;
        starOb?.SetActive(mapData.isUnlocked);
        txt_Star.text = string.Format("{0}/{1}", mapData.GetAllStarClaimed(), mapData.totalLevel*3);
        anim_Lock.gameObject.SetActive(!mapData.isUnlocked);

        leftPathOb.SetActive(mapData.mapIndex % 2 == 1 && mapData.mapIndex < DataManager.MapAsset.totalMap);
        rightPathOb.SetActive(mapData.mapIndex % 2 == 0 && mapData.mapIndex < DataManager.MapAsset.totalMap);

        onMapSelect = OnMapSelect;
        btn_SelectMap.onClick.RemoveAllListeners();
        btn_SelectMap.onClick.AddListener(ButtonMapSelect);
    }

    private void ButtonMapSelect()
    {
        if (!mapData.isUnlocked)
            return;
        SoundManager.Play("1. Click Button");
        onMapSelect?.Invoke(mapData.mapIndex);
    }
}
