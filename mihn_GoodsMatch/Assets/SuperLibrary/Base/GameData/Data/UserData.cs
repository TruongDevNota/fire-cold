using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Base.Ads;
using System.Text;

[Serializable]
public class UserData : UserAnalysic
{
    public delegate void VIPChangedDelegate(bool isVip);
    public static event VIPChangedDelegate OnVIPChanged;

    public delegate void RemovedAdsChangedDelegate(bool isRemoveAds);
    public static event RemovedAdsChangedDelegate OnRemovedAdsChanged;

    [Header("Data")]
    public int level = 0;
    public int bartenderLevel = 0;
    public int challengeLevel = 0;
    public bool isModeBartenderSuguested = false;
    public bool isChallengePlayed = false;
    public bool isShopTutShowed = false;
    public bool isMaxLevelBartender = false;
    public bool isMaxLevelChallenge = false;

    [Header("Tut")]
    public bool tutBartenderDone = false;

    [Header("Reward Unlock")]
    private int levelChestPercent = 0;
    public int LevelChesPercent 
    {
        get
        {
            return levelChestPercent;
        }
        set
        {
            levelChestPercent = Mathf.Min(100,value);
        }
    }

    public DateTime lastdayClaimed = DateTime.MinValue;
    public int dailyRewardClaimCount = 0;
    public DateTime lastTimeBuyCoinWithAds = DateTime.MinValue;

    private string lastTimeUpdate = new DateTime(1999, 1, 1).ToString();
    public DateTime LastTimeUpdate
    {
        get => DateTimeConverter.ToDateTime(lastTimeUpdate);
        set => lastTimeUpdate = value.ToString();
    }
    private string fistTimeOpen = DateTime.Now.ToString();
    public DateTime FistTimeOpen
    {
        get => DateTimeConverter.ToDateTime(fistTimeOpen);
        set => fistTimeOpen = value.ToString();
    }
    private bool _isRemovedAds;
    public bool isRemovedAds
    {
        get => _isRemovedAds;
        set
        {
            if (value != _isRemovedAds)
            {
                _isRemovedAds = value; 
                SetUserProperty("is_removed_ads", _isRemovedAds);
                OnRemovedAdsChanged?.Invoke(_isRemovedAds);
            }
        }
    }
    [SerializeField]
    protected bool _isVip = false;
    public bool isVIP
    {
        get => _isVip;
        set
        {
            if (value != _isVip)
            {
                _isVip = value;
                SetUserProperty("is_VIP", _isVip);
                OnVIPChanged?.Invoke(_isVip);
            }
        }
    }

    private float _scaleCoin = 1;
    public float scaleCoin
    {
        get
        {
            if (_scaleCoin < 1 || _scaleCoin >= 2)
                _scaleCoin = 1;
            return _scaleCoin;
        }
        set
        {
            if (value != _scaleCoin)
            {
                _scaleCoin = value;
            }
        }
    }

    private long _limitedDateTime;

    public double limitedPassTimeCountDown;

    public long limitedDateTime
    {
        get
        {
#if !UNITY_EDITOR
            if (_limitedDateTime > DateTime.Now.AddDays(30).Ticks)
#else
            if (_limitedDateTime > DateTime.Now.AddDays(6).Ticks)
#endif
                _limitedDateTime = DateTime.Now.AddMinutes(1).Ticks;
            return _limitedDateTime;
        }
        set
        {
            if (_limitedDateTime != value && value > 0)
            {
                _limitedDateTime = value;
            }
        }
    }

    [Header("Money")]
    [SerializeField]
    private int coin = 0;
    public int totalCoin
    {
        get => coin;
        set
        {
            if (coin < 2000000000)
            {
                if (coin != value)
                {
                    int changed = 0;
                    if (coin > value)
                    {
                        changed = coin - value;
                        totalCoinSpend += changed;
                    }
                    else
                    {
                        changed = value - coin;
                        totalCoinEarn += changed;
                    }

                    coin = Mathf.Max(0, value);
                    OnCoinChanged?.Invoke(changed, coin);
                }
            }
            else
            {
                UIToast.ShowError("Don't do that!");
                coin = 100;
                totalCoinEarn = 0;
                totalCoinSpend = 0;
            }
        }
    }
    public int totalCoinEarn = 0;
    public int totalCoinSpend = 0;

    private int bankCoin = 0;
    public int totalBankCoin 
    { 
        get => bankCoin;
        set
        {
            bankCoin = Mathf.Min(value, DataManager.GameConfig.BankCoinStage.Last());
        }
    }

    [SerializeField]
    private int diamond;
    public int totalDiamond
    {
        get => diamond;
        set
        {
            if (diamond != value)
            {
                int changed = 0;
                if (diamond > value)
                {
                    changed = diamond - value;
                    totalDiamondSpend += changed;
                }
                else
                {
                    changed = value - diamond;
                    totalDiamondEarn += changed;
                }

                diamond = value;
                OnDiamondChanged?.Invoke(changed, diamond);
            }
        }
    }
    public int totalDiamondEarn = 0;
    public int totalDiamondSpend = 0;

    [SerializeField]
    private int star;
    public int totalStar
    {
        get
        {
            return star;
        }
        set
        {
            if (star != value)
            {
                int changed = 0;
                if (star > value)
                {
                    changed = star - value;
                    totalStarSpend += changed;
                }
                else
                {
                    changed = value - star;
                    totalStarEarn += changed;
                }

                star = value;
                OnStarChanged?.Invoke(changed, star);
            }
        }
    }
    public int totalStarEarn = 0;
    public int totalStarSpend = 0;

    [Header("Buff")]
    private int hintBuff = 0;
    public int totalHintBuff 
    {
        get
        {
            return hintBuff;
        }
        set
        {
            if(hintBuff != value)
            {
                int changed = 0;
                if (hintBuff > value)
                {
                    changed = hintBuff - value;
                    totalHintSpent += changed;
                }
                else
                {
                    changed = value - hintBuff;
                    totalHintEarn += changed;
                }

                hintBuff = value;
                OnHintBuffChanged?.Invoke(changed, hintBuff);
            }
        } 
    }
    public int totalHintEarn;
    public int totalHintSpent;

    private int swapBuff = 0;
    public int totalSwapBuff
    {
        get { return swapBuff; }
        set
        {
            if (swapBuff != value)
            {
                int changed = 0;
                if (swapBuff > value)
                {
                    changed = hintBuff - value;
                    totalSwapSpent += changed;
                }
                else
                {
                    changed = value - swapBuff;
                    totalSwapEarn += changed;
                }

                swapBuff = value;
                OnSwapBuffChanged?.Invoke(changed, swapBuff);
            }
        }
    }
    public int totalSwapEarn;
    public int totalSwapSpent;

    private int totalPurchased = 0;
    public int TotalPurchased
    {
        get => totalPurchased;
        set
        {
            if (totalPurchased != value && value > 0)
            {
                totalPurchased = value;
            }
        }
    }

    public delegate void MoneyChangedDelegate(int change, int current);
    public static event MoneyChangedDelegate OnCoinChanged;
    public static event MoneyChangedDelegate OnDiamondChanged;
    public static event MoneyChangedDelegate OnStarChanged;

    public static event MoneyChangedDelegate OnHintBuffChanged;
    public static event MoneyChangedDelegate OnSwapBuffChanged;
}

[Serializable]
public class UserAnalysic : UserBase
{
    [Header("Analysic")]
    private int versionInstall;
    public int VersionInstall
    {
        get => versionInstall;
        set
        {
            if (versionInstall != value)
            {
                versionInstall = value;
            }
        }
    }

    private int versionCurrent;
    public int VersionCurrent
    {
        get => versionCurrent;
        set
        {
            if (versionCurrent != value)
            {
                versionCurrent = value;
            }
        }
    }

    private int session = 0;
    public int Session
    {
        get => session;
        set
        {
            if (session != value && value > 0)
            {
                session = value;
            }
        }
    }

    private long totalPlay = 0;
    public long TotalPlay
    {
        get => totalPlay;
        set
        {
            if (totalPlay != value && value > 0)
            {
                totalPlay = value;
            }
        }
    }

    private int totalWin = 0;
    public int TotalWin
    {
        get => totalWin;
        set
        {
            if (totalWin != value && value > 0)
            {
                totalWin = value;
            }
        }
    }
    private int winStreak = 0;
    public int WinStreak
    {
        get => winStreak;
        set
        {
            if (winStreak != value)
            {
                winStreak = value;
            }
        }
    }
    private int loseStreak = 0;
    public int LoseStreak
    {
        get => loseStreak;
        set
        {
            if (loseStreak != value)
            {
                loseStreak = value;
            }
        }
    }

    private long totalTimePlay = 0;
    public long TotalTimePlay
    {
        get => totalTimePlay;
        set
        {
            if (totalTimePlay != value && value > 0)
            {
                totalTimePlay = value;
            }
        }
    }

    [Header("Ads")]
    private long totalAdInterstitial = 0;
    public long TotalAdInterstitial
    {
        get => totalAdInterstitial;
        set
        {
            if (totalAdInterstitial != value && value > 0)
            {
                totalAdInterstitial = value;
            }
        }
    }

    private long totalAdRewarded = 0;
    public long TotalAdRewarded
    {
        get => totalAdRewarded;
        set
        {
            if (totalAdRewarded != value && value > 0)
            {
                totalAdRewarded = value;
            }
        }
    }


    private string abTesting;
    public string ABTesting
    {
        get
        {
            if (string.IsNullOrEmpty(abTesting))
            {
                int randonAB = UnityEngine.Random.Range(0, 3);
                if (randonAB == 0)
                    abTesting = "A";
                else if (randonAB == 1)
                    abTesting = "B";
                else
                    abTesting = "C";
            }
            return abTesting;
        }
        set
        {
            if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(abTesting))
            {
                abTesting = value;
            }
        }
    }

    private string source;
    public string Source
    {
        get => source;
        set
        {
            if (!string.IsNullOrEmpty(value) && source != value)
            {
                source = value;
            }
        }
    }

    public void SetUserProperty(string title, object value)
    {
        try
        {
#if USE_FIREBASE
            Base.FirebaseManager.SetUser(title.ToLower(), value.ToString());
#endif
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

    }
    private int totalSpinTime = 0;
    public int TotalSpinTime
    {
        get => totalSpinTime;
        set
        {
            if (totalSpinTime != value)
            {
                totalSpinTime = value;
            }
        }
    }
    private int remainSpinTime = 0;
    public int RemainSpinTime
    {
        get => remainSpinTime;
        set
        {
            if (remainSpinTime != value)
            {
                remainSpinTime = value;
            }
        }
    }
    private DateTime timeOnQuitSpinner = DateTime.MinValue;
    public DateTime TimeOnQuitSpinner
    {
        get => timeOnQuitSpinner;
        set
        {
            if (timeOnQuitSpinner != value)
            {
                timeOnQuitSpinner = value;
            }
        }
    }

    private string isGiftOpenedString = ""; // Format: false_true_false
    public bool[] isGiftOpened = new bool[] { false, false, false };

    public bool GetLuckyWheelOpenedGiftState(int index)
    {
        index = Mathf.Clamp(index, 0, 2);
        if (string.IsNullOrEmpty(isGiftOpenedString))
            return false;

        var states = isGiftOpenedString.Split('_');
        var canParse = bool.TryParse(states[index], out bool state);
        return canParse && state;
    }

    public void SetLuckyWheelOpenedGiftState(int index, bool value)
    {
        index = Mathf.Clamp(index, 0, 2);
        bool[] states = new bool[3] { false, false, false };

        if (!string.IsNullOrEmpty(isGiftOpenedString))
        {
            var splittedStates = isGiftOpenedString.Split('_');
            for (int i = 0; i < splittedStates.Length; i++)
            {
                var canParse = bool.TryParse(splittedStates[i], out bool state);
                states[i] = canParse && state;
            }
        }

        states[index] = value;
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < states.Length; i++)
        {
            builder.Append(states[i].ToString());
            if (i < states.Length - 1)
                builder.Append("_");
        }

        isGiftOpenedString = builder.ToString();
    }
}

[Serializable]
public class UserBase
{
    [Header("Base")]
    public string id;
    public string email;
    public string name;
}
