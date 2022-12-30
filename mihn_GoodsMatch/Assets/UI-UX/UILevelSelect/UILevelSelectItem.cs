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
    public void Fill(int level, System.Action<int> OnLevelSelect, bool isTest = false)
    {
        datumLevel = level;
        bool isChallenge = level % 10 == 0 && level != 0;
        isUnlocked = isTest ? true : level <= DataManager.UserData.level + 1;
        //img_bg.sprite = isUnlocked ? spr_UnlockBG : spr_LockBG;

        img_bg.gameObject.SetActive(isUnlocked && !isChallenge);
        img_lock.gameObject.SetActive(!isUnlocked);
        img_ChallengeBGOpen.gameObject.SetActive(isUnlocked && isChallenge);

        img_footer.sprite = isUnlocked ? spr_UnlockFooter : spr_LockFooter;
        img_lock.gameObject.SetActive(!isUnlocked);
        panel_Star?.SetActive(level<=DataManager.UserData.level);
        int stars = DataManager.LevelAsset.GetLevelStar(level-1);
        oneStarObj.SetActive(stars == 1);
        twoStarObj.SetActive(stars == 2);
        threeStarObj.SetActive(stars ==3);

        txt_level.text = $"Level {level}";
        
        onLevelSelect = OnLevelSelect;
        btn_Select.onClick.RemoveAllListeners();
        btn_Select.onClick.AddListener(ButtonLevelSelect);
    }

    public void ButtonLevelSelect()
    {
        if (!isUnlocked)
            return;
        onLevelSelect?.Invoke(datumLevel);
    }
}
