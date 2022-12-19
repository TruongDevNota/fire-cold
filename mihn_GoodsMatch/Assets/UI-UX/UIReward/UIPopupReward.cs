using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Base.Ads;

public class UIPopupReward : MonoBehaviour
{
    [SerializeField]
    UIAnimation anim;
    [SerializeField]
    Button btn_Claim;
    [SerializeField]
    Button btn_x2Claim;
    [SerializeField]
    GameObject starChestObj;
    [SerializeField]
    GameObject levelChestObj;
    [SerializeField]
    UIChestItem coinReward;
    [SerializeField]
    UIChestItem buffIdeReward;

    [SerializeField]
    private UIPopupPigProcess popup_PigProcess;

    private bool isStarChes;
    private int coinEarn;
    private int buffEarn;

    public void ShowStarChestReward(int coinNumber)
    {
        coinEarn = coinNumber;
        isStarChes = true;

        starChestObj.gameObject.SetActive(true);
        levelChestObj.gameObject.SetActive(false);
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

        starChestObj.gameObject.SetActive(false);
        levelChestObj.gameObject.SetActive(true);
        buffIdeReward.gameObject.SetActive(true);
        buffIdeReward.Fill(buffNum);
        coinReward.gameObject.SetActive(true);
        coinReward.Fill(coinNumber);

        anim.Show();
        SwitchActiveAllButton(true);
        btn_Claim.onClick.AddListener(BtnClaimSelect);
        btn_x2Claim.onClick.AddListener(BtnX2ClaimSelect);
    }

    public void OnHide()
    {
        anim.Hide();
    }

    private void OnClaimReward()
    {
        if (isStarChes)
        {
            CoinManager.Add(coinEarn, btn_Claim.transform);
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
            if (e == AdEvent.Success || DataManager.GameConfig.isAdsByPass)
            {
                coinEarn *= 2;
                buffEarn *= 2;
                coinReward.DoTextAnim(lastValue, coinEarn);
            }
            else
            {
                Debug.Log($"!!!!! video reward fail.");
                UIToast.ShowNotice("view video reward fail!");
            }
            OnClaimReward();
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
