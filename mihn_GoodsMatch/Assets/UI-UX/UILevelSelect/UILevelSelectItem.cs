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
    Sprite spr_UnlockIcon;
    [SerializeField]
    Sprite spr_LockIcon;
    [SerializeField]
    Sprite spr_UnlockFooter;
    [SerializeField]
    Sprite spr_LockFooter;

    [SerializeField]
    private Image img_bg;
    [SerializeField]
    private Image img_icon;
    [SerializeField]
    private Image img_footer;
    [SerializeField]
    private Text txt_level;
    [SerializeField]
    private Button btn_Select;

    private int datumLevel;
    private System.Action<int> onLevelSelect;
    public void Fill(int level, System.Action<int> OnLevelSelect, bool isTest = false)
    {
        bool isUnlocked = isTest ? true : level <= DataManager.UserData.level + 1;
        img_bg.sprite = isUnlocked ? spr_UnlockBG : spr_LockBG;
        img_footer.sprite = isUnlocked ? spr_UnlockFooter : spr_LockFooter;
        img_icon.sprite = isUnlocked ? spr_UnlockIcon : spr_LockIcon;
        txt_level.text = $"Level {level}";
        datumLevel = level;
        onLevelSelect = OnLevelSelect;
        btn_Select.onClick.RemoveAllListeners();
        btn_Select.onClick.AddListener(ButtonLevelSelect);
    }

    public void ButtonLevelSelect()
    {
        onLevelSelect?.Invoke(datumLevel);
    }
}
