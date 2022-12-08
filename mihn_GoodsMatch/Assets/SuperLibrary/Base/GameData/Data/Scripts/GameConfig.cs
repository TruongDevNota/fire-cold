using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GameConfig
{
    [Header("ADs")]
    public float timePlayToShowAds = 30;
    public float timePlayReduceToShowAds = 15;
    public RebornType rebornType = RebornType.Continue;
    public RebornBy rebornBy = RebornBy.Ads;
    public int suggestUpdateVersion = 0;
    public int adRewardNotToInter = 1;
    public int adInterNotToReward = 1;
    public int adInterViewToReward = 5;

    [Header("Rate")]
    public int promtRateAtWin = 5;

    #region MONEY
    [Header("Money")]
    [SerializeField]
    private int _goldByAds = 150;
    public int goldByAds
    {
        get
        {
            if (_goldByAds <= 0)
                _goldByAds = 150;
            return _goldByAds;
        }
        set
        {
            if (value != _goldByAds)
                _goldByAds = value;
        }
    }
    public int startPerMatch = 5;
    #endregion

    #region LEVEL DESIGN
    public int rebornTimeAdding;
    #endregion
}

[Serializable]
public enum RebornType
{
    Continue,
    Checkpoint
}

[Serializable]
public enum RebornBy
{
    Free,
    Gold,
    Gem,
    Ads
}

[SerializeField]
public class BossData
{
    public int level;
    public int health;
    public int damage;
}