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
    public int adRewardNotToInter = 1;
    public int adInterNotToReward = 1;
    public int adInterViewToReward = 5;
    public int coinRewardByRemoveAds = 800;
    public float removeAdsCost = 5.99f;
    public bool isAdsByPass = false;

    [Header("Level Star Rating")]
    public float threeStar = 0.75f;
    public float twoStar = 0.9f;
    public int totalLevel = 100;

    [Header("Rate")]
    public int promtRateAtWin = 5;

    [Header("Rewards")]
    public int unlockChestEachLevel = 25;
    public int buffHintReward = 1;
    public int buyCoinWithAdsCoolDownInSeconds = 300;
    public bool isTestRewarPopup = false;

    [Header("Challenge")]
    public int starsToUnlockChallenge = 5;
    public int playChallengeCoinUse = 100;

    [Header("Bartender")]
    public int starsToUnlockBartender = 6;
    public int coinEarnInDay = 3;
    public int coinEarnInNight = 5;
    public int requestMissLimit = 3;
    public int levelTimeBase_bartender = 60;
    public int levelTimeIncrease_bartender = 10;
    public int maxLevelTime_bartender = 300;
    public float timeWaitOfSingleRequest = 15f;
    public float timeWaitOfDoubleRequest = 25f;
    public float timeComboEslap = 10f;
    public float timeToCheckRequest = 5f;
    public int minEasyRequest = 20;
    public int levelToRequestx2 = 10;
    public int maxCombo_bartender = 5;
    public int doubleRequestRepeat = 20;

    [Header("Tutorial")]
    public int tutBartenderLastStep = 7;
    [Header("UnlockMap")]
    public int[] starsToUnlockMap = { 10, 10, 10, 10, 10 };
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
    public int maxLevel = 100;
    public int levelOpenShopDecor=5;
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