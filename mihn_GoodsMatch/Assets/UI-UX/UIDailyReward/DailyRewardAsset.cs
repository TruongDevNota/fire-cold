using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MyBox;
using UnityEditor;

[CreateAssetMenu(fileName = "DailyRewardAsset", menuName = "DataAsset/DailyRewardAsset")]
public class DailyRewardAsset : ScriptableObject
{
    [SerializeField] List<DailyRewardDatum> list;
    [SerializeField] List<DailyRewardDatum> daySevenRewards;
    [SerializeField] List<DailyRewardDatum> levelUnlockRewards;

    public int[] GetDailyRewards()
    {
        return ConvertRewardDatum(list[Random.Range(0, list.Count)]);
    }

    public int[] GetLevelUnlockRewards()
    {
        return ConvertRewardDatum(levelUnlockRewards[Random.Range(0, levelUnlockRewards.Count)]);
    }

    public int[] GetDaySevenReward()
    {
        return ConvertRewardDatum(daySevenRewards[Random.Range(0, list.Count)]);
    }

    private int[] ConvertRewardDatum(DailyRewardDatum datum)
    {
        int coinEarn = 0;
        int buffHintEarn = 0;
        int buffSwapEarn = 0;
        foreach (var reward in datum.rewards)
        {
            switch (reward.type)
            {
                case eRewardType.Gold:
                    coinEarn = reward.amount;
                    break;
                case eRewardType.BuffSwap:
                    buffSwapEarn = reward.amount;
                    break;
                case eRewardType.BuffHint:
                    buffHintEarn = reward.amount;
                    break;
            }
        }

        return new int[] { coinEarn, buffHintEarn, buffSwapEarn };
    }
}

[System.Serializable]
public class DailyRewardDatum
{
    public List<RewardDatum> rewards;
}

[System.Serializable]
public class RewardDatum
{
    public eRewardType type;
    public int amount;
}

public enum eRewardType
{
    Gold,
    BuffHint,
    BuffSwap,
    GoldSale
}