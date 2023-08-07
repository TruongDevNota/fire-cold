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

    public int[] GetDailyRewards(int index )
    {
        return ConvertRewardDatum(list[index]);
    }

    public int[] GetLevelUnlockRewards()
    {
        return ConvertRewardDatum(levelUnlockRewards[Random.Range(0, levelUnlockRewards.Count)]);
    }

    public ItemDecorData GetDaySevenRewardDeCor()
    {
        for(int i=DataManager.HouseAsset.allFloorData.Count-1; i>=0; i--)
        {
            if (DataManager.HouseAsset.allFloorData[i].isUnlocked)
            {
                if (DataManager.HouseAsset.allFloorData[i].allCats.Count != DataManager.HouseAsset.allFloorData[i].catUnlockedCount)
                {
                    return DataManager.HouseAsset.allFloorData[i].UnlockRandomCat();
                } else
                if (DataManager.HouseAsset.allFloorData[i].allDecorationItems.Count != DataManager.HouseAsset.allFloorData[i].itemUnlockedCount)
                {
                    return DataManager.HouseAsset.allFloorData[i].UnlockRandomDecor();
                }
            }
        }
        return null;
    }
    public int[] GetDaySevenReward()
    {
        return ConvertRewardDatum(daySevenRewards[6]);
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
    GoldSale,
    Decor,
    Cat,
}