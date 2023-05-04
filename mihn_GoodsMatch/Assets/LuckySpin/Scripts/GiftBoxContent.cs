using UnityEngine;
using UnityEngine.UI;
public class GiftBoxContent : MonoBehaviour
{
    [SerializeField]
    private Image iconImg = null;
    [SerializeField]
    private Text totalSpinRequiredTxt = null;
    [SerializeField]
    private Sprite lockIconSpr = null;
    [SerializeField]
    private Sprite tickIconSpr = null;
    [HideInInspector]
    public GiftBoxReward gift;
    public void Init(GiftBoxReward reward)
    {
        iconImg.sprite = DataManager.UserData.TotalSpinTime < reward.spinTimeRequiredToGetThisGift ? lockIconSpr : DataManager.UserData.GetLuckyWheelOpenedGiftState(reward.index) ? tickIconSpr : reward.rewardSpriteIcon;
        totalSpinRequiredTxt.text = $"{reward.spinTimeRequiredToGetThisGift}";
        gift = reward;
    }
    public void Init(GiftBoxReward reward, WindowsData data)
    {
        iconImg.sprite = DataManager.UserData.TotalSpinTime < reward.spinTimeRequiredToGetThisGift ? lockIconSpr : DataManager.UserData.GetLuckyWheelOpenedGiftState(reward.index) ? tickIconSpr : reward.rewardSpriteIcon;
        totalSpinRequiredTxt.text = $"{reward.spinTimeRequiredToGetThisGift}";
        reward.tmpRewardWindows = data;
        gift = reward;
    }
    public void Init(GiftBoxReward reward, SkinData data)
    {
        iconImg.sprite = DataManager.UserData.TotalSpinTime < reward.spinTimeRequiredToGetThisGift ? lockIconSpr : DataManager.UserData.GetLuckyWheelOpenedGiftState(reward.index) ? tickIconSpr : reward.rewardSpriteIcon;
        totalSpinRequiredTxt.text = $"{reward.spinTimeRequiredToGetThisGift}";
        reward.tmpRewardSkin = data;
        gift = reward;
    }
    public void Init(GiftBoxReward reward, FloorData data)
    {
        iconImg.sprite = DataManager.UserData.TotalSpinTime < reward.spinTimeRequiredToGetThisGift ? lockIconSpr : DataManager.UserData.GetLuckyWheelOpenedGiftState(reward.index) ? tickIconSpr : reward.rewardSpriteIcon;
        totalSpinRequiredTxt.text = $"{reward.spinTimeRequiredToGetThisGift}";
        reward.tmpRewardFloor = data;
        gift = reward;
    }
    public void Init(GiftBoxReward reward, CarpetData data)
    {
        iconImg.sprite = DataManager.UserData.TotalSpinTime < reward.spinTimeRequiredToGetThisGift ? lockIconSpr : DataManager.UserData.GetLuckyWheelOpenedGiftState(reward.index) ? tickIconSpr : reward.rewardSpriteIcon;
        totalSpinRequiredTxt.text = $"{reward.spinTimeRequiredToGetThisGift}";
        reward.tmpRewardCarpet = data;
        gift = reward;
    }
    public void Init(GiftBoxReward reward, CeillingData data)
    {
        iconImg.sprite = DataManager.UserData.TotalSpinTime < reward.spinTimeRequiredToGetThisGift ? lockIconSpr : DataManager.UserData.GetLuckyWheelOpenedGiftState(reward.index) ? tickIconSpr : reward.rewardSpriteIcon;
        totalSpinRequiredTxt.text = $"{reward.spinTimeRequiredToGetThisGift}";
        reward.tmpRewardCeilling = data;
        gift = reward;
    }
    public void Init(GiftBoxReward reward, ChairData data)
    {
        iconImg.sprite = DataManager.UserData.TotalSpinTime < reward.spinTimeRequiredToGetThisGift ? lockIconSpr : DataManager.UserData.GetLuckyWheelOpenedGiftState(reward.index) ? tickIconSpr : reward.rewardSpriteIcon;
        totalSpinRequiredTxt.text = $"{reward.spinTimeRequiredToGetThisGift}";
        reward.tmpRewardChair = data;
        gift = reward;
    }
    public void Init(GiftBoxReward reward, TableData data)
    {
        iconImg.sprite = DataManager.UserData.TotalSpinTime < reward.spinTimeRequiredToGetThisGift ? lockIconSpr : DataManager.UserData.GetLuckyWheelOpenedGiftState(reward.index) ? tickIconSpr : reward.rewardSpriteIcon;
        totalSpinRequiredTxt.text = $"{reward.spinTimeRequiredToGetThisGift}";
        reward.tmpRewardTable = data;
        gift = reward;
    }
    public void Init(GiftBoxReward reward, LampData data)
    {
        iconImg.sprite = DataManager.UserData.TotalSpinTime < reward.spinTimeRequiredToGetThisGift ? lockIconSpr : DataManager.UserData.GetLuckyWheelOpenedGiftState(reward.index) ? tickIconSpr : reward.rewardSpriteIcon;
        totalSpinRequiredTxt.text = $"{reward.spinTimeRequiredToGetThisGift}";
        reward.tmpRewardLamp = data;
        gift = reward;
    }

    public void Unlock()
    {
        iconImg.sprite = gift.rewardSpriteIcon;
    }
    public void Ins_Onclick()
    {
        if (DataManager.UserData.TotalSpinTime >= gift.spinTimeRequiredToGetThisGift)
        {
            if (!DataManager.UserData.GetLuckyWheelOpenedGiftState(gift.index))
            {
                DataManager.UserData.SetLuckyWheelOpenedGiftState(gift.index, true);
                this.PostEvent((int)EventID.OnReceiverGift, gift);
                iconImg.sprite = tickIconSpr;
            }
        }
    }
}
