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

    private void Start()
    {
        btn_Select?.onClick.AddListener(OnDaySelect);
    }

    public void FillLayout(System.Action<int> onSelectDay = null)
    {
        onSelect = onSelectDay;

        bool isClaimed = dayIndex <= DataManager.UserData.dailyRewardClaimCount;
        bool canClaim = dayIndex == DataManager.UserData.dailyRewardClaimCount + 1 && (DataManager.UserData.lastdayClaimed.Day == System.DateTime.Now.Day - 1 || DataManager.UserData.dailyRewardClaimCount ==0);
        img_HeaderFade.gameObject.SetActive(isClaimed);
        img_BodyFade.gameObject.SetActive(isClaimed);
        if(dayIndex != 7)
        {
            img_HeaderBg.sprite = canClaim ? spr_ActiveHeader : spr_NormalHeader;
            img_BodyBG.sprite = canClaim ? spr_ActiveBody : spr_NormalBodyBG;
        }
    }

    public void OnDaySelect()
    {
        onSelect?.Invoke(dayIndex);
    }
}
