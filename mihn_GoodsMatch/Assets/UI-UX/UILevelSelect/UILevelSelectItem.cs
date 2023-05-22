using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILevelSelectItem : MonoBehaviour
{
    [SerializeField] GameObject bglock;
    [SerializeField] GameObject bgNormalUnlocked;
    [SerializeField] GameObject bgSpecial;
    [SerializeField] GameObject lockItem;
    [SerializeField] GameObject specialItem;
    [SerializeField] GameObject curent;
    
    [SerializeField] Text txt_level;
    [SerializeField] Button btn_Select;

    [SerializeField] GameObject panel_Star;
    [SerializeField] GameObject oneStarObj;
    [SerializeField] GameObject twoStarObj;
    [SerializeField] GameObject threeStarObj;

    bool isUnlocked = false;
    private int level;
    private System.Action<int> onLevelSelect;
    public void Fill(int level, System.Action<int> OnLevelSelect,int mapIndex)
    {
        this.level = level;
        bool isSpecialLevel = level % 4 == 0 && level != 0;
        isUnlocked = level <= DataManager.MapAsset.ListMap[mapIndex-1].hightestLevelUnlocked;

        bglock.SetActive(!isUnlocked && !isSpecialLevel);
        bgNormalUnlocked.SetActive(isUnlocked && !isSpecialLevel);
        bgSpecial.SetActive(isSpecialLevel);
        lockItem.SetActive(!isUnlocked);
        specialItem.SetActive(isSpecialLevel && isUnlocked);

        panel_Star?.SetActive(level<= DataManager.MapAsset.ListMap[mapIndex - 1].hightestLevelUnlocked);
        int stars = level <= DataManager.MapAsset.ListMap[DataManager.mapSelect - 1].levelStars.Count ? DataManager.MapAsset.ListMap[DataManager.mapSelect-1].levelStars[level-1] : 0;
        oneStarObj.SetActive(stars == 1);
        twoStarObj.SetActive(stars == 2);
        threeStarObj.SetActive(stars ==3);
        txt_level.gameObject.SetActive(isUnlocked);
        txt_level.text = $"{level}";
        
        onLevelSelect = OnLevelSelect;
        btn_Select.onClick.RemoveAllListeners();
        btn_Select.onClick.AddListener(ButtonLevelSelect);
    }

    public void ButtonLevelSelect()
    {
        if (!isUnlocked)
            return;
        SoundManager.Play("1. Click Button");
        onLevelSelect?.Invoke(level);
    }
}
