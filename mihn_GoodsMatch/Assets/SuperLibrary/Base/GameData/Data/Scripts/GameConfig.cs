using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GameConfig : GameConfigBase
{
    [Header("ADs")]
    public bool adUseBackup = false;
    public bool adUseOpenBackup = false;
    public bool adUseNative = false;
    public bool adUseInPlay = false;
    public bool forceInterToReward = false;
    public bool forceRewardToInter = false;
    public bool forceInterEverywhere = false;
    public float timeToWaitOpenInter = 4.5f;
    public int isNeedInternet = 9999999;

    public float timePlayToShowAds = 30;
    public float timePlayReduceToShowAds = 15;
    public RebornType rebornType = RebornType.Continue;
    public RebornBy rebornBy = RebornBy.Ads;
    public int suggestUpdateVersion = 0;
    public int adRewardNotToInter = 1;
    public int adInterNotToReward = 1;
    public int adInterViewToReward = 5;
    public int coinRewardByRemoveAds = 800;
    public float removeAdsCost = 5.99f;
    public bool isAdsByPass = false;

    [Header("Rate")]
    public int promtRateAtWin = 5;

    [Header("Rewards")]
    public int starCollectStage = 100;
    public int coinRewardByStarChest = 50;
    public int unlockChestEachLevel = 25;
    public int coinRewardByLevel = 10;
    public int buffHintReward = 1;
    public bool isTestRewarPopup = false;

    [Header("Challenge")]
    public int levelsToNextChallenge = 1;

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
    [SerializeField]
    List<int> bankCoinStage = new List<int>() { 2000, 5000 };
    public List<int> BankCoinStage
    {
        get { return bankCoinStage; }
    }
    #endregion

    #region LEVEL DESIGN
    public float rebornTimeAdding;
    public int buffPrice = 500;
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

[Serializable]
public enum eMapMovingType
{
    None,
    Left,
    Right,
    Up,
    Down
}