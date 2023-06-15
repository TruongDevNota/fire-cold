using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Base.Ads;
using Spine.Unity;
using Animation = Spine.Animation;
using System.Collections;

public class UIPopupReward : MonoBehaviour
{
    [SerializeField]
    UIAnimation anim;
    [SerializeField]
    Button btn_Claim;
    [SerializeField]
    Button btn_x2Claim;
    [SerializeField]
    Button No_Thanks;
    //[SerializeField]
    //GameObject starChestObj;
    //[SerializeField]
    //GameObject levelChestObj;
    [SerializeField]
    UIChestItem coinReward;
    [SerializeField]
    UIChestItem buffHintReward;
    [SerializeField]
    UIChestItem buffSwapReward;

    //[SerializeField]
    //private UIPopupPigProcess popup_PigProcess;
    [SerializeField]
    public SkeletonAnimation skeletonAnimation;
    [SerializeField]
    Transform coinStarTf;

    private bool isDailyRewardChest;
    private int coinEarn;
    private int buffHintEarn;
    private int buffSwapEarn;
    private string starChestSkin = "chest2";
    string levelChestSkin = "box";
    private bool isShowNoThanks;

    private void Start()
    {
        btn_Claim.onClick.AddListener(BtnClaimSelect);
        btn_x2Claim.onClick.AddListener(BtnX2ClaimSelect);
        No_Thanks.onClick.AddListener(OnHide);
    }

    public void ShowDailyChestReward(int coinNumber = 0, int buffHint = 0, int buffSwap = 0, int index = 0)
    {
        isDailyRewardChest = true;
        isShowNoThanks = true;
        coinEarn = coinNumber;
        buffHintEarn = buffHint;
        buffSwapEarn = buffSwap;
        SetChestSkin(starChestSkin, true);
        SetActiveRewards();
        coinReward.gameObject.SetActive(true);
        coinReward.Fill(coinNumber);
        btn_Claim.gameObject.SetActive(false);
        btn_x2Claim.gameObject.SetActive(false);
        No_Thanks.gameObject.SetActive(false);
        anim.Show(null, onCompleted: () =>
        {
            OnClaimReward();
            if(index==7)
                btn_x2Claim.gameObject.SetActive(false);
            else
                btn_x2Claim.gameObject.SetActive(true);
            StartCoroutine(ShowNoThanks());
        });
    }
    public IEnumerator ShowNoThanks()
    {
        yield return new WaitForSeconds(5);
        if (isShowNoThanks)
            No_Thanks.gameObject.SetActive(true);
    }
    public void ShowLevelChestReward(int coinNumber = 0, int buffHint = 0, int buffSwap = 0)
    {
        DataManager.UserData.LevelChesPercent = 0;
        isDailyRewardChest = false;
        coinEarn = coinNumber;
        buffHintEarn = buffHint;
        buffSwapEarn = buffSwap;
        SetChestSkin(levelChestSkin);
        DOVirtual.DelayedCall(0.1f, () =>
        {
            SoundManager.Play("11._Tieng_mo_hop_o");
        });

        SetActiveRewards();
        SwitchActiveAllButton(true);
        anim.Show();
    }

    private void SetActiveRewards()
    {
        coinReward.gameObject.SetActive(coinEarn > 0);
        coinReward.Fill(coinEarn);
        buffHintReward.gameObject.SetActive(buffHintEarn > 0);
        buffHintReward.Fill(buffHintEarn);
        buffSwapReward.gameObject.SetActive(buffSwapEarn > 0);
        buffSwapReward.Fill(buffSwapEarn);
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
        if (isDailyRewardChest)
        {
            skeletonAnimation.timeScale = 1;
            SoundManager.Play("10._Tieng_mo_hop_bac");
            DOVirtual.DelayedCall(0.7f, () =>
            {
                CoinManager.Add(coinEarn, coinStarTf);
                DataManager.UserData.totalHintBuff += buffHintEarn;
                DataManager.UserData.totalSwapBuff += buffSwapEarn;
            });
            UIMainScreen.Instance.FetchData();
            DOVirtual.DelayedCall(2.5f, () =>
            {
                //OnHide();
            });
        }
        else
        {
            CoinManager.Add(coinEarn, coinStarTf);
            //Show Pig Process
            DataManager.UserData.totalHintBuff += buffHintEarn;
            DataManager.UserData.totalSwapBuff += buffSwapEarn;
            DOVirtual.DelayedCall(2f, () =>
            {
                OnHide();
                if (DataManager.levelSelect > DataManager.GameConfig.totalLevel)
                {
                    DataManager.levelSelect = DataManager.GameConfig.totalLevel;
                    anim.Hide();
                    GameStateManager.Idle(null);
                    return;
                }
                DataManager.currLevelconfigData.config.gameMode = eGameMode.Normal;
                GameStateManager.LoadGame(null);
            });
        }
        DataManager.Save();
    }

    private void BtnClaimSelect()
    {
        SoundManager.Play("1. Click Button");
        SwitchActiveAllButton(false);
        OnClaimReward();
    }

    private void BtnX2ClaimSelect()
    {
        SoundManager.Play("1. Click Button");
        string placeAds = isDailyRewardChest ? "OpenIdleStarChestReward" : "UnlockLevelChestReward";
        AdsManager.ShowVideoReward((e, t) =>
        {
            var lastValue = coinEarn;
            if (e == AdEvent.ShowSuccess || DataManager.GameConfig.isAdsByPass)
            {
                //SwitchActiveAllButton(false);
                isShowNoThanks = false;
                coinReward.DoTextAnim(lastValue, coinEarn);
                CoinManager.Add(coinEarn, coinStarTf);
                DataManager.UserData.totalHintBuff += buffHintEarn;
                DataManager.UserData.totalSwapBuff += buffSwapEarn;
                UIMainScreen.Instance.FetchData();
                btn_Claim.gameObject.SetActive(false);
                btn_x2Claim.gameObject.SetActive(false);
                //OnClaimReward();
                DOVirtual.DelayedCall(2f, () =>
                {
                    OnHide();
                });
            }
            else
            {

            }
        }, placeAds, "coin");
    }

    private void SwitchActiveAllButton(bool active)
    {
        btn_Claim.gameObject.SetActive(true);
        btn_x2Claim.gameObject.SetActive(true);
        btn_Claim.interactable = active;
        btn_x2Claim.interactable = active;
    }
}
