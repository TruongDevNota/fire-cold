using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILevelSelectItem : MonoBehaviour
{
    [SerializeField]
    Sprite spr_UnlockBG;
    [SerializeField]
    Sprite spr_LockBG;
    [SerializeField]
    Sprite spr_LockIcon;
    [SerializeField]
    Sprite spr_LockFooter;
    [SerializeField]
    Sprite spr_UnlockFooter;

    [SerializeField] Image img_ChallengeBGOpen;
    [SerializeField]
    private Image img_bg;
    [SerializeField]
    private Image img_lock;
    [SerializeField]
    private Image img_curent;
    [SerializeField]
    private GameObject panel_Star;
    [SerializeField]
    private Image img_footer;
    [SerializeField]
    private Text txt_level;
    [SerializeField]
    private Button btn_Select;
    [SerializeField] GameObject oneStarObj;
    [SerializeField] GameObject twoStarObj;
    [SerializeField] GameObject threeStarObj;

    bool isUnlocked = false;
    private int datumLevel;
    private System.Action<int> onLevelSelect;
    public void Fill(int level, System.Action<int> OnLevelSelect,int mapIndex, bool isTest = false)
    {
        datumLevel = level;
        bool isChallenge = level % 10 == 0 && level != 0;
        isUnlocked = isTest ? true : level <= DataManager.UserData.level[mapIndex-1] + 1;
        //img_bg.sprite = isUnlocked ? spr_UnlockBG : spr_LockBG;

        img_bg.gameObject.SetActive(isUnlocked );
        img_curent.gameObject.SetActive(isUnlocked && level == DataManager.UserData.level[mapIndex - 1] + 1);
        img_lock.gameObject.SetActive(!isUnlocked && !isChallenge);
        img_ChallengeBGOpen.gameObject.SetActive(!isUnlocked && isChallenge);

        //img_footer.sprite = isUnlocked ? spr_UnlockFooter : spr_LockFooter;
        //img_lock.gameObject.SetActive(!isUnlocked);
        panel_Star?.SetActive(level<=DataManager.UserData.level[mapIndex-1]);
        int stars = DataManager.MapAsset.listMaps[DataManager.mapSelect-1].levelAsset.GetLevelStar(level-1);
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
        onLevelSelect?.Invoke(datumLevel);
    }
}
