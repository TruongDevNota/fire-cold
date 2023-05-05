using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMapSelectButton : MonoBehaviour
{
    [SerializeField] Button btn_SelectMap;
    [SerializeField] Text txt_map; 
    bool isUnlocked = false;
    private System.Action<int> onMapSelect;
    private int datumMap;
    public void Fill(int mapIndex, System.Action<int> OnMapSelect,bool isTest)
    {
        datumMap = mapIndex;
        isUnlocked = isTest ? true : mapIndex <= DataManager.UserData.mapIndex + 1;
        txt_map.text = $"Map {mapIndex}";
        onMapSelect = OnMapSelect;
        btn_SelectMap.onClick.RemoveAllListeners();
        btn_SelectMap.onClick.AddListener(ButtonMapSelect);
    }

    private void ButtonMapSelect()
    {
        if (!isUnlocked)
            return;
        SoundManager.Play("1. Click Button");
        onMapSelect?.Invoke(datumMap);
    }
}
