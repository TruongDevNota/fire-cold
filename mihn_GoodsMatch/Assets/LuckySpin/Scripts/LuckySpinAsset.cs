using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
[CreateAssetMenu(fileName = "LuckySpinAsset", menuName = "DataAsset/LuckySpinDataAsset")]
public class LuckySpinAsset : ScriptableObject
{
    public int timeBetweenTwoFreeSpinInSeconds;
    public LuckySpinReward[] luckySpinRewards;

    [Header("Gift Box")]
    public GiftBoxReward[] giftBoxRewards;
}
