using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG;
using DG.Tweening;

public class UIDailyRewardItem : MonoBehaviour
{
    [SerializeField] DailyRewardAsset dailyRewardAsset;

    [SerializeField] int dayIndex;
    [SerializeField] Button btn_Select;
    [SerializeField] Image img_HeaderBg;
    [SerializeField] Image img_BodyBG;
    [SerializeField] Image img_HeaderFade;
    [SerializeField] Image img_BodyFade;
    [SerializeField] Image img_Today;
    [SerializeField] Image img_Fill;
    [SerializeField] Image img_Icon;

    [SerializeField] Sprite spr_ActiveHeader;
    [SerializeField] Sprite spr_NormalHeader;
    [SerializeField] Sprite spr_ActiveBody;
    [SerializeField] Sprite spr_NormalBodyBG;
    [SerializeField] Text dayIndextxt;
    [SerializeField] Text rewardTxt;
    [SerializeField] Color canClaimcolor;
    [SerializeField] Color cantClaimcolor;


    System.Action<int> onSelect;

    private void Start()
    {
        btn_Select?.onClick.AddListener(OnDaySelect);
    }

    public void FillLayout(System.Action<int> onSelectDay = null)
    {
        onSelect = onSelectDay;
        bool isToday = DataManager.UserData.dailyRewardClaimCount == dayIndex - 1 && DataManager.UserData.lastdayClaimed.Day <= System.DateTime.Now.Day - 1
            || DataManager.UserData.dailyRewardClaimCount == dayIndex && DataManager.UserData.lastdayClaimed.Day == System.DateTime.Now.Day;
        bool isClaimed = dayIndex <= DataManager.UserData.dailyRewardClaimCount;
        bool canClaim = dayIndex == DataManager.UserData.dailyRewardClaimCount + 1 && (DataManager.UserData.lastdayClaimed.Day == System.DateTime.Now.Day - 1 || DataManager.UserData.dailyRewardClaimCount == 0);
        img_HeaderFade.gameObject.SetActive(isClaimed);
        img_BodyFade.gameObject.SetActive(isClaimed);
        if (isToday || isClaimed)
            img_Fill?.gameObject.SetActive(false);
        else
            img_Fill?.gameObject.SetActive(true);

        img_Today?.gameObject.SetActive(isToday);
        img_HeaderBg.sprite = canClaim ? spr_ActiveHeader : spr_NormalHeader;
        img_BodyBG.sprite = canClaim ? spr_ActiveBody : spr_NormalBodyBG;
        if (!canClaim)
            img_Icon?.GetComponent<DOTweenAnimation>().DOPause();
        else
            img_Icon?.GetComponent<DOTweenAnimation>().DOPlay();
        dayIndextxt.text = dayIndex.ToString();
        if (canClaim || isClaimed)
            rewardTxt.GetComponent<Outline>().effectColor = canClaimcolor;
        else
            rewardTxt.GetComponent<Outline>().effectColor = cantClaimcolor;
        if (canClaim)
            btn_Select.GetComponent<Button>().enabled = true;
        else
            btn_Select.GetComponent<Button>().enabled = false;
    }

    public void OnDaySelect()
    {
        onSelect?.Invoke(dayIndex);
    }
}
