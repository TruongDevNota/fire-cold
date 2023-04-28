using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SpinContent : MonoBehaviour
{
    [SerializeField] private Image image = null;
    [SerializeField] private Text rewardTxt = null;
    [SerializeField] private Image rewardIconImg = null;
    private void OnEnable()
    {
        rewardTxt.text = "";
    }
    public void Init(LuckySpinReward reward)
    {
        image.color = reward.color;
        rewardTxt.text = /*reward.rewardsTypes == LuckyRewardsTypes.Coins ? $"{reward.rewardAmount}" : ""*/"";

        // if (reward.rewardsTypes == LuckyRewardsTypes.Skin)
        // {
        //     var randomIndex = Random.Range(0, DataManager.SkinsAsset.list.Count);
        //     var randomSkin = DataManager.SkinsAsset.list[randomIndex];
        //     rewardIconImg.sprite = randomSkin.main;
        //     DataManager.UserData.CurrentTempSkinIndex = randomIndex;
        // }
        // else
        {
            rewardIconImg.sprite = reward.rewardSpriteIcon;
        }
    }
    public void Init(LuckySpinReward reward, SkinData wall)
    {
        image.color = reward.color;
        reward.rewardSpriteIcon = wall.main;
        rewardIconImg.sprite = reward.rewardSpriteIcon;
        reward.tmpRewardSkin = wall;
    }
    public void Init(LuckySpinReward reward, FloorData wall)
    {
        image.color = reward.color;
        reward.rewardSpriteIcon = wall.main;
        rewardIconImg.sprite = wall.main;
        reward.tmpRewardFloor = wall;
    }
    public void Init(LuckySpinReward reward, WindowsData wall)
    {
        image.color = reward.color;
        reward.rewardSpriteIcon = wall.main;
        rewardIconImg.sprite = wall.main;
        reward.tmpRewardWindows = wall;
    }
    public void Init(LuckySpinReward reward, CarpetData wall)
    {
        image.color = reward.color;
        reward.rewardSpriteIcon = wall.main;
        rewardIconImg.sprite = wall.main;
        reward.tmpRewardCarpet = wall;
    }
    public void Init(LuckySpinReward reward, CeillingData wall)
    {
        image.color = reward.color;
        reward.rewardSpriteIcon = wall.main;
        rewardIconImg.sprite = wall.main;
        reward.tmpRewardCeilling = wall;
    }
    public void Init(LuckySpinReward reward, ChairData wall)
    {
        image.color = reward.color;
        reward.rewardSpriteIcon = wall.main;
        rewardIconImg.sprite = wall.main;
        reward.tmpRewardChair = wall;
    }
    public void Init(LuckySpinReward reward, TableData wall)
    {
        image.color = reward.color;
        reward.rewardSpriteIcon = wall.main;
        rewardIconImg.sprite = wall.main;
        reward.tmpRewardTable = wall;
    }
    public void Init(LuckySpinReward reward, LampData wall)
    {
        image.color = reward.color;
        reward.rewardSpriteIcon = wall.main;
        rewardIconImg.sprite = wall.main;
        reward.tmpRewardLamp = wall;
    }
}
