using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

#if USE_APPSFLYER
using AppsFlyerSDK;
#endif


public class AdsManager : MonoBehaviour
{

    [Header("Main Network")]
    [SerializeField]
    private AdMobile adNetWork = AdMobile.ADMOD;
    public static AdMobile AdNetwork { get => instance.adNetWork; }

    [Header("Options")]
    [SerializeField]
    private bool initAtStart = true;
    [SerializeField]
    protected bool useBanner = true;
    public static bool UseBanner => (bool)instance?.useBanner;
    public static int VideoRewardedCount { get; private set; } = 0;
    public static int AdInterCount { get; private set; } = 0;

    public static bool IsDebugMode => DebugMode.IsDebugMode;
    [SerializeField]
    protected Button interstitialButton = null;
    [SerializeField]
    protected Button videoRewarded = null;
    //[SerializeField]
    //protected Toggle testAutoSwitch = null;
    //public static bool TestAutoSwitch => (bool)(instance?.testAutoSwitch?.isOn) && IsDebugMode;

    [Header("Banner Safe Area")]
    [SerializeField]
    protected RectTransform parentTransform;
    [SerializeField]
    protected float bannerHeight = 68f;
    [SerializeField]
    protected BannerPos bannerPos = BannerPos.BOTTOM;
    public static List<AdsBannerArea> AdsBannerAreaList = new List<AdsBannerArea>();

    public static float TotalTimePlay { get; set; }
    public static int TotalSuccess { get; private set; }
    public static DateTime LastTimeShowAd { get; private set; }

    private static UserData userData => DataManager.UserData;
    private static GameConfig gameConfig => DataManager.GameConfig;

    [SerializeField]
    protected UIToggle isRemoveAds;
    public static bool IsRemoveAds
    {
        get
        {
            if (instance != null && userData != null)
            {
                if (instance.isRemoveAds && !userData.isRemovedAds)
                    userData.isRemovedAds = instance.isRemoveAds.isOn;
                return userData.limitedDateTime > DateTime.Now.Ticks || userData.isRemovedAds;
            }
            return false;
        }
    }

    /// <summary>
    /// Check time play more than game config to show ads
    /// </summary>
    public static bool IsTimeToShowAds
    {
        get
        {
            if (userData != null && gameConfig != null)
            {
                if (IsRemoveAds)
                {
                    Debug.Log("limitedDateTime: " + userData.limitedDateTime + " isRemovedAds: " + userData.isRemovedAds);
                    return false;
                }

                float reduceTime = Mathf.Min(TotalSuccess, 2) * gameConfig.timePlayReduceToShowAds;
                float timePlayToShowAds = gameConfig.timePlayToShowAds + reduceTime;
                if (LastTimeShowAd.AddSeconds(timePlayToShowAds) < DateTime.Now)
                {
                    DebugMode.Log("totalTimePlay: " + TotalTimePlay.ToString("#0.0") + " timePlayToShowAds: " + timePlayToShowAds + " showSuscess: " + Mathf.Min(TotalSuccess, 2));
                    return true;
                }
                else
                {
                    Debug.Log("Not time to show Ads!");
                    return false;
                }
            }
            return false;
        }
    }

    public delegate void AdsDelegate(AdType currentType, AdEvent currentEvent, string currentPlacement, string currentItemId);
    public static AdsDelegate OnStateChanged;

    private static AdsManager instance;

    public static AdType currentType { get; private set; }
    public static AdEvent currentEvent { get; private set; }
    public static string currentPlacement { get; private set; }
    public static string currentItem { get; private set; }

    public static bool InterstitialIsReady
    {
        get
        {
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
            {
                return true;
            }
#if !USE_MAX && !USE_IRON && !USE_ADMOB && !USE_UNITY
            return true;
#else
            if (AdNetwork == AdMobile.ADMOD)
            {
                return AdmobHelper.InterIsReady;
            }
            else if (AdNetwork == AdMobile.IRONSOURCE)
            {
                return IronHelper.InterIsReady;
            }
            else if (AdNetwork == AdMobile.MAX)
            {
                return false;
            }
            else
                return false;
#endif
        }
    }

    public static bool VideoRewaredIsReady
    {
        get
        {
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
            {
                return true;
            }
#if !USE_MAX && !USE_IRON && !USE_ADMOB && !USE_UNITY
            return true;
#else
            if (AdNetwork == AdMobile.ADMOD)
            {
                return AdmobHelper.RewardIsReady;
            }
            else if (AdNetwork == AdMobile.IRONSOURCE)
            {
                return IronHelper.RewardIsReady;
            }
            else if (AdNetwork == AdMobile.MAX)
            {
                return false;
            }
            else
                return false;
#endif
        }
    }

    private void Awake()
    {
        instance = this;

        if (isRemoveAds)
        {
            isRemoveAds.OnChangedAction((isOn) =>
            {
                UpdateArea();
            });
        }


        DataManager.OnLoaded += DataManager_OnLoaded;
    }

    private void DataManager_OnLoaded(GameData gameData)
    {
        if (!initAtStart)
            Init();
        UpdateArea();
    }

    private void Start()
    {
        interstitialButton?.onClick.AddListener(TestInterstitial);
        videoRewarded?.onClick.AddListener(TestVideoReward);

        if (initAtStart)
            Init();
    }

    public static void Init()
    {
        if (!instance)
        {
            Debug.LogError("[AdsManager] NULL");
            return;
        }

        if (AdNetwork == AdMobile.ADMOD)
        {
            AdmobHelper.SetStatus = SetStatus;
            AdmobHelper.Init(IsDebugMode);
        }
        else if (AdNetwork == AdMobile.IRONSOURCE)
        {
            IronHelper.SetStatus = SetStatus;
            IronHelper.Init(IsDebugMode);
        }
        else if (AdNetwork == AdMobile.MAX)
        {
            Debug.LogError("MaxHelper.Init(); Not implement!!");
        }
    }

    public void TestInterstitial()
    {
        LastTimeShowAd = new DateTime(1999, 1, 1);
        TotalTimePlay = 9999;
        ShowInterstitial((s) =>
        {
            Debug.Log("Test Interstitial: " + s);
        }, "TestInterstitialName", "TestInterstitialId");
    }
    /// <summary>
    /// Befor call should show loading then check status to do something. Flow game monetization on Gameover
    /// <para/>
    /// A.on RESULT SCREEN: 
    /// if (timePlayInGame >= timePlayToShowAds)
    /// {
    ///     ShowInterstitial((status) =>
    ///     {
    ///         if (status == AdEvent.Success)
    ///         {
    ///             resetTimePlayInGame();
    ///             do something
    ///         }
    ///         else
    ///         {
    ///             do something
    ///         }
    ///     }
    ///}
    ///<para/>
    ///B.on CONTINUE SCREEN: 
    ///if (userClickRebornByAds) 
    ///     => reset timePlayInGame
    /// else 
    ///     => flow on RESULT SCREEN
    /// </summary>
    public static void ShowInterstitial(Action<AdEvent> onSuccess, string placementName, string itemId)
    {
        if (!instance)
        {
            Debug.LogError("[AdsManager] NULL");
            onSuccess(AdEvent.NotAvailable);
            return;
        }

        if (!IsTimeToShowAds)
        {
            onSuccess(AdEvent.NotTimeToShow);
            return;
        }

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
        {
            if (string.IsNullOrEmpty(placementName))
                Debug.LogWarning("placementName IsNullOrEmpty");
            onSuccess(AdEvent.Success);
            return;
        }


#if !USE_MAX && !USE_IRON && !USE_ADMOB && !USE_UNITY
        onSuccess(AdEvent.Success);
#else
        userData.TotalAdInterstitial++;
        if (AdNetwork == AdMobile.ADMOD)
        {
            //if (AutoInterViewReward > 3 && AdInterCount >= AutoInterViewReward && VideoRewaredIsReady && VideoRewardedCount <= 3)
            //{
            //    Debug.Log("AutoInterViewReward: " + AdInterCount + "/" + AutoInterViewReward + " VideoRewaredIsReady " + VideoRewaredIsReady);
            //    AdmobHelper.ShowRewarded(onSuccess, placementName, itemId);
            //    AdInterCount = 0;
            //}
            //else
                AdmobHelper.ShowInterstitial(onSuccess, placementName, itemId);
        }
        else if (AdNetwork == AdMobile.IRONSOURCE)
        {
            //if (AutoInterViewReward > 3 && AdInterCount >= AutoInterViewReward && VideoRewaredIsReady && VideoRewardedCount <= 3)
            //{
            //    Debug.Log("AutoInterViewReward: " + AdInterCount + "/" + AutoInterViewReward + " VideoRewaredIsReady " + VideoRewaredIsReady);
            //    IronHelper.ShowRewarded(onSuccess, placementName, itemId);
            //    AdInterCount = 0;
            //}
            //else
                IronHelper.ShowInterstitial(onSuccess, placementName, itemId);
        }
        else if (AdNetwork == AdMobile.MAX)
        {
            Debug.LogError("MaxInterstitial.Show(onSuccess, placementName); Not implement!!");
        }
        else
        {
            onSuccess?.Invoke(AdEvent.NotAvailable);
        }
#endif
    }

    public void TestVideoReward()
    {
        LastTimeShowAd = new DateTime(1999, 1, 1);
        TotalTimePlay = 9999;
        ShowVideoReward((s) =>
        {
            Debug.Log("Test VideoReward: " + s);
        }, "TestVideoRewardName", "TestVideoRewardId");
    }

    /// <summary>
    /// Befor call should show loading then check status to do something. Flow game monetization on Gameover
    /// <para/>
    /// LOGIC:
    /// ShowVideoReward((status) =>
    /// {
    ///     if (status == AdEvent.Success)
    ///     {
    ///         resetTimePlayInGame();
    ///         do something
    ///     }
    ///     else
    ///     {
    ///        do something
    ///     }
    /// }
    /// </summary>
    public static void ShowVideoReward(Action<AdEvent> onSuccess, string placementName = "", string itemId = "")
    {
        if (!instance)
        {
            Debug.LogError("[AdsManager] NULL");
            onSuccess(AdEvent.NotAvailable);
            return;
        }

        if (!IsConnected)
        {
            PopupMes.Show("Connection Error", "Failed to connect to server. Please check your internet connection and try again!", "Okie");
            SetStatus(AdType.VideoReward, AdEvent.NotInternet, placementName, itemId);
            onSuccess(AdEvent.NotInternet);
            return;
        }

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
        {
            if (string.IsNullOrEmpty(placementName))
                Debug.LogWarning("placementName IsNullOrEmpty");
            onSuccess(AdEvent.Success);
            return;
        }
#if !USE_MAX && !USE_IRON && !USE_ADMOB && !USE_UNITY
        onSuccess(AdEvent.Success);
#else
        userData.TotalAdRewarded++;
        if (AdNetwork == AdMobile.ADMOD)
        {
            AdmobHelper.ShowRewarded(onSuccess, placementName, itemId);
        }
        else if (AdNetwork == AdMobile.IRONSOURCE)
        {
            IronHelper.ShowRewarded(onSuccess, placementName, itemId);
        }
        else if (AdNetwork == AdMobile.MAX)
        {
            Debug.LogError("MaxVideoReward.Show(onSuccess, placement);  Not implement!!");
        }
        else
        {
            onSuccess?.Invoke(AdEvent.NotAvailable);
        }
#endif
    }

    public static void DestroyBanner()
    {
        if (AdNetwork == AdMobile.ADMOD)
        {
            AdmobHelper.DestroyBaner();
        }
        else if (AdNetwork == AdMobile.IRONSOURCE)
        {
            IronHelper.DestroyBaner();
        }
        else if (AdNetwork == AdMobile.MAX)
        {
            Debug.LogError("Max DestroyBanner  Not implement!!");
        }
    }

    public static void SetStatus(AdType adType, AdEvent adEvent, string placementName = "", string itemId = "")
    {
        if (instance)
        {
            currentType = adType;
            currentEvent = adEvent;
            currentPlacement = placementName;
            currentItem = itemId;

            if (adEvent == AdEvent.Success)
            {
                TotalTimePlay = 0;
                LastTimeShowAd = DateTime.Now;
                TotalSuccess++;
                if (adType == AdType.VideoReward)
                    VideoRewardedCount++;
                else if (adType == AdType.Interstitial)
                    AdInterCount++;
            }

#if USE_APPSFLYER
            if (adEvent == AdEvent.Success || adEvent == AdEvent.Show)
                LogAppsFlyer(adType, adEvent, placementName);
#endif

            GameStateManager.isBusy = adEvent == AdEvent.Show;

            OnStateChanged?.Invoke(currentType, currentEvent, currentPlacement, currentItem);

            DebugMode.Log(currentType.ToString() + " " + currentEvent.ToString() + " " + currentPlacement + " " + currentItem);
        }
    }

    public static bool IsConnected
    {
        get
        {
            return UnityNetworkHelper.IsConnected;
        }
    }

    public static void ShowNoticeOnLoading()
    {
        try
        {
            UIToast.ShowLoading("Time to show ads... please wait!");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public static void ShowNotice(AdEvent onSuccess)
    {
        try
        {
            if (onSuccess == AdEvent.NotInternet)
                UIToast.ShowError("Please check your internet connection...!");
            else if (onSuccess == AdEvent.NotAvailable)
                UIToast.ShowError("Video Reward not ready, please try again...!");
            else if (onSuccess == AdEvent.Fail)
                UIToast.ShowError("Something wrong, please try again...!");
            else if (onSuccess == AdEvent.Success)
                UIToast.ShowError("Thanks for watching!");
            else
                UIToast.Hide();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public static void SetArea()
    {
        instance?.UpdateArea();
    }

    protected void UpdateArea()
    {
        if (parentTransform != null && AdsBannerAreaList != null)
        {
            foreach (var i in AdsBannerAreaList)
                i.SetArea(bannerHeight / parentTransform.rect.height, IsRemoveAds ? BannerPos.NONE : bannerPos);
        }

        if (IsRemoveAds)
            DestroyBanner();
    }

    private void OnApplicationPause(bool isPaused)
    {
#if USE_IRON
        IronSource.Agent.onApplicationPause(isPaused);
#endif
    }


#if USE_APPSFLYER
    private static void LogAppsFlyer(AdType ad_type, AdEvent adEvent, string placement)
    {
        var adWatchedEvent = new Dictionary<string, string> { { "af_type", placement } };
        AppsFlyer.sendEvent("af_ad_" + "_" + ad_type.ToString().ToLower() + adEvent.ToString().ToLower(), adWatchedEvent);
    }
#endif
}
