using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Base.Ads;
using Spine.Unity;
using Animation = Spine.Animation;

public class UIPopupReward : MonoBehaviour
{
    [SerializeField]
    UIAnimation anim;
    [SerializeField]
    Button btn_Claim;
    [SerializeField]
    Button btn_x2Claim;
    //[SerializeField]
    //GameObject starChestObj;
    //[SerializeField]
    //GameObject levelChestObj;
    [SerializeField]
    UIChestItem coinReward;
    [SerializeField]
    UIChestItem buffIdeReward;

    [SerializeField]
    private UIPopupPigProcess popup_PigProcess;
    [SerializeField]
    public SkeletonAnimation skeletonAnimation;
    [SerializeField]
    Transform coinStarTf;

    private bool isStarChes;
    private int coinEarn;
    private int buffEarn;
    private string starChestSkin = "chest2";
    string levelChestSkin = "box";

    public void ShowStarChestReward(int coinNumber)
    {
        coinEarn = coinNumber;
        isStarChes = true;

        SetChestSkin(starChestSkin, true);
        buffIdeReward.gameObject.SetActive(false);
        coinReward.gameObject.SetActive(true);
        coinReward.Fill(coinNumber);
        anim.Show();
        SwitchActiveAllButton(true);
        btn_Claim.onClick.AddListener(BtnClaimSelect);
        btn_x2Claim.onClick.AddListener(BtnX2ClaimSelect);
    }

    public void ShowLevelChestReward(int coinNumber, int buffNum)
    {
        DataManager.UserData.LevelChesPercent = 0;

        coinEarn = coinNumber;
        isStarChes = false;
        buffEarn = buffNum;
        SetChestSkin(levelChestSkin);
        DOVirtual.DelayedCall(0.1f, () =>
        {
            SoundManager.Play("11._Tieng_mo_hop_o");
        });
        buffIdeReward.gameObject.SetActive(true);
        buffIdeReward.Fill(buffNum);
        coinReward.gameObject.SetActive(true);
        coinReward.Fill(coinNumber);
        anim.Show();
        SwitchActiveAllButton(true);
        btn_Claim.onClick.AddListener(BtnClaimSelect);
        btn_x2Claim.onClick.AddListener(BtnX2ClaimSelect);
    }

    private void SetChestSkin(string name, bool isDelay = false)
    {
        skeletonAnimation.AnimationState.SetAnimation(0, name, false);
        //skeletonAnimation.AnimationName = name;
        skeletonAnimation.timeScale = isDelay ? 0 : 1;
    }

    public void OnHide()
    {
        anim.Hide();
    }

    private void OnClaimReward()
    {
        if (isStarChes)
        {
            skeletonAnimation.timeScale = 1;
            SoundManager.Play("10._Tieng_mo_hop_bac");
            DOVirtual.DelayedCall(0.7f, () =>
            {
                CoinManager.Add(coinEarn, coinStarTf);
            });
            UIMainScreen.Instance.FetchData();
            DOVirtual.DelayedCall(1.5f, () =>
            {
                OnHide();
            });
        }
        else
        {
            //Show Pig Process
            popup_PigProcess.OnShow(coinEarn);
            DataManager.UserData.totalHintBuff++;
            OnHide();
        }
    }

    private void BtnClaimSelect()
    {
        SwitchActiveAllButton(false);
        OnClaimReward();
    }

    private void BtnX2ClaimSelect()
    {
        string placeAds = isStarChes ? "OpenIdleStarChestReward" : "UnlockLevelChestReward";
        SwitchActiveAllButton(false);
        AdsManager.ShowVideoReward((e, t) =>
        {
            var lastValue = coinEarn;
            if (e == AdEvent.ShowSuccess || DataManager.GameConfig.isAdsByPass)
            {
                coinEarn *= 2;
                buffEarn *= 2;
                coinReward.DoTextAnim(lastValue, coinEarn);
                OnClaimReward();
            }
        }, placeAds, "coin");
    }

    private void SwitchActiveAllButton(bool active)
    {
        btn_Claim.interactable = active;
        btn_x2Claim.interactable = active;
        btn_Claim.onClick.RemoveAllListeners();
        btn_x2Claim.onClick.RemoveAllListeners();
    }
}
