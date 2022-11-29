using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Analytics;

#if USE_GA
using GameAnalyticsSDK;
#endif

public class AnalyticsManager : MonoBehaviour
{
#region Properties static
    private static UserData userData => DataManager.UserData;

    public static string TAG
    {
        get
        {
            if (instance != null)
                return "[" + instance.GetType().Name + "] ";
            return "";
        }
    }

    private static AnalyticsManager instance { get; set; }
#endregion

    protected void Awake()
    {
        instance = this;
        //AdsManager.OnStateChanged += OnAdStateChanged;
        GameStateManager.OnStateChanged += OnGameStateChanged;
    }


    private void OnGameStateChanged(GameState current, GameState last, object data)
    {
        switch (current)
        {
            case GameState.Ready:
                LogEvent("start_level", logLevel);
                LogEvent($"start_{userData.level + 1}");
                break;
            case GameState.GameOver:
                LogEvent("lose_level", logLevel);
                LogEvent($"lose_{userData.level + 1}");
                break;
            case GameState.Complete:
                LogEvent("win_level", logLevel);
                var key = $"win_{userData.level + 1}";
                var totalMatch = PlayerPrefs.GetInt(key, 0);
                if (totalMatch == 0)
                {
                    LogEvent($"first_win", logLevel);
                }
                totalMatch++;

                PlayerPrefs.SetInt(key, totalMatch);

                LogEvent($"{key}");

                break;
        }
    }




    private void OnAdStateChanged(AdType currentType, AdEvent currentEvent, string currentPlacement, string currentItemId)
    {
        LogAds(currentType, currentEvent, currentPlacement, currentItemId);
    }

    public static void LogInApp(string productId, InAppPurchaseEvent action, string failureReason = "")
    {
        if (userData != null)
        {
            if (action == InAppPurchaseEvent.Succeeded)
                userData.TotalPurchased++;

            var log = new Dictionary<string, object>
            {
                { "total_level", userData.level },
                { "total_play", userData.TotalPlay },
                { "total_timePlay", userData.TotalTimePlay },
                { "total_purchased", userData.TotalPurchased },
                { "product_id", productId }
            };

            if (!string.IsNullOrEmpty(failureReason))
                log.Add("failure", failureReason);

            LogEvent("in_app_" + action.ToString(), log);
            LogEvent("in_app_" + productId, log);
        }
    }

    public static Dictionary<string, object> logUser
    {
        get
        {
            if (userData != null)
            {
                var log = new Dictionary<string, object>
                {
                    { "day", (DateTime.Now - userData.FistTimeOpen).Days },

                    { "total_play", userData.TotalPlay },
                    { "total_time_play", userData.TotalTimePlay },

                    { "total_gold_earn", userData.totalCoin },
                    { "total_gem_earn", userData.totalDiamond },

                    { "total_ad_interstitial", userData.TotalAdInterstitial },
                    { "total_ad_rewarded", userData.TotalAdRewarded },
                    { "total_in_app", userData.TotalPurchased },

                    { "level", userData.level },
                };
                return log;
            }
            else
            {
                return new Dictionary<string, object>();
            }
        }
    }

    public static Dictionary<string, object> logLevel
    {
        get
        {
            if (userData != null)
            {
                var log = new Dictionary<string, object> {
                    { "level", userData.level + 1},
                };
                return log;
            }
            else
            {
                return new Dictionary<string, object>();
            }
        }
    }

    public static void LogEvent(string eventName, Dictionary<string, object> eventData = null)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogWarning("eventName IsNullOrEmpty");
            return;
        }

        if (eventName.Length >= 32)
            eventName = eventName.Substring(0, 32);
        if (!eventName.Contains("_"))
            eventName = Regex.Replace(eventName, @"\B[A-Z]", m => "_" + m.ToString()).Replace("__", "_").ToLower();
        else
            eventName = eventName.ToLower();

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
        {
            string debugLog = eventName;
            if (eventData != null)
            {
                var entries = eventData.Select(d => string.Format("\"{0}\": [{1}]", d.Key, string.Join(",", d.Value)));
                debugLog += "\n" + "{" + string.Join(",", entries) + "}";
            }
            Debug.LogWarning(debugLog);

            return;
        }

        if (eventData == null)
        {
            Analytics.CustomEvent(eventName, null);
            FirebaseHelper.LogEvent(eventName, null);
        }
        else
        {
            Analytics.CustomEvent(eventName, eventData);
            FirebaseHelper.LogEvent(eventName, eventData);
        }
    }

    public static void LogAds(AdType adType, AdEvent adEvent, string placementName, string itemId)
    {
        if (string.IsNullOrEmpty(placementName))
            placementName = "Unknow";
        if (string.IsNullOrEmpty(itemId))
            itemId = "Unknow";

        var log = new Dictionary<string, object>();
        log.Add("source", $"{placementName}_{itemId}");
        log.Add("placementName", $"{placementName}");
        log.Add("itemId", $"{itemId}");
        LogEvent(adType + "_" + adEvent, log);
    }

    public static void LogEarnMoney(MoneyType moneyType, string placementName, string itemId, int value)
    {
        if (string.IsNullOrEmpty(placementName))
            placementName = "Unknow";
        if (string.IsNullOrEmpty(itemId))
            itemId = "Unknow";

        var log = new Dictionary<string, object>();
        log.Add("source", $"{placementName}_{itemId}");
        log.Add("amount", value);

        if (moneyType == MoneyType.Gold)
            LogEvent("earn_gold", log);
        if (moneyType == MoneyType.Gem)
            LogEvent("earn_crystal", log);
    }

    public static void LogSpendMoney(MoneyType moneyType, string placementName, string itemId, int value)
    {
        if (string.IsNullOrEmpty(placementName))
            placementName = "Unknow";
        if (string.IsNullOrEmpty(itemId))
            itemId = "Unknow";

        var log = new Dictionary<string, object>();
        log.Add("source", $"{placementName}_{itemId}");
        log.Add("amount", value);

        if (moneyType == MoneyType.Gold)
            LogEvent("spend_gold", log);
        if (moneyType == MoneyType.Gem)
            LogEvent("spend_crystal", log);
    }
}

public enum MoneyType
{
    Gold,
    Gem
}