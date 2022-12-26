using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDailyRewardItem : MonoBehaviour
{
    [SerializeField] DailyRewardAsset dailyRewardAsset;

    [SerializeField] int dayIndex;
    [SerializeField] Button btn_Select;
    [SerializeField] Image img_HeaderBg;
    [SerializeField] Image img_BodyBG;
    [SerializeField] Image img_HeaderFade;
    [SerializeField] Image img_BodyFade;

    [SerializeField] Sprite spr_ActiveHeader;
    [SerializeField] Sprite spr_NormalHeader;
    [SerializeField] Sprite spr_ActiveBody;
    [SerializeField] Sprite spr_NormalBodyBG;
    
    System.Action<int> onSelect;

    public void FillLayout(System.Action<int> onSelectDay = null)
    {
        onSelect = onSelectDay;
    }

    public void OnDaySelect()
    {
        onSelect?.Invoke(dayIndex);
    }
}
