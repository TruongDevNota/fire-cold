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

    public DailyRewardDatum GetDailyRewardsByDayIndex(int index)
    {
        return index < list.Count ? list[index] : null;
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