using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Base.Ads
{
    public class AdsManager : MonoBehaviour
    {
        [SerializeField]
        private List<AdMediation> adsNetWork = new List<AdMediation> { AdMediation.IRON };
        public static List<AdMediation> AdsNetwork { get => instance?.adsNetWork; }
        public static AdMediation DefaultMediation { get { return AdsNetwork.FirstOrDefault(); } }

        [Tooltip("Interstitial NOT READY --> switch to VideoReward")]
        public static bool interToReward = false;
        [Tooltip("VideoReward NOT READY --> switch to Interstitial")]
        public static bool rewardToInter = false;

        protected static AdsSettings adsSettings = null;
        public static AdsSettings Settings
        {
            get
            {
                if (adsSettings == null)
                    adsSettings = Resources.Load<AdsSettings>(AdsSettings.fileName);
                if (adsSettings == null)
                    throw new Exception("[AdsManager]" + "AdsSettings NULL --> Please creat from Base SDK/Ads Settings");
                else
                    return adsSettings;
            }
            private set => adsSettings = value;
        }

        [SerializeField]
        protected GameObject adsNoticePrefab = null;

        public bool isDebug = false;
        private static bool IsDebug
        {
            get
            {
                if (instance != null)
                    return instance.isDebug;
                return false;
            }
            set
            {
                if (instance != null)
                {
                    instance.isDebug = value;
                }
            }
        }

        [Header("Options")]
        [SerializeField]
        protected Button interstitialButton = null;
        [SerializeField]
        protected Button videoRewarded = null;
        [SerializeField]
        protected Toggle removeAdsToggle = null;

        [Header("Banner Safe Area")]
        protected RectTransform parentTransform;
        [SerializeField]
        protected float bannerHeight = 68f;
        [SerializeField]
        protected BannerPos bannerPos = BannerPos.BOTTOM;
        public static BannerPos BannerPos => instance.bannerPos;
        public static List<AdsBannerArea> AdsBannerAreaList = new List<AdsBannerArea>();

        private static int totalInterSuccess { get; set; }
        private static int totalRewardSuccess { get; set; }
        private static int totalSuccess { get; set; }

        public static bool IsInit { get; private set; }

        private static DateTime lastTimeShowAd = DateTime.Now.AddSeconds(-600);
        public static DateTime LastTimeShowAd { get => lastTimeShowAd; set => lastTimeShowAd = value; }

        protected static UserData userData = new UserData();
        public static UserData UserData
        {
            get
            {
                if (DataManager.UserData != null)
                    userData = DataManager.UserData;
                return userData;
            }
        }

        protected static GameConfig gameConfig = new GameConfig { timePlayToShowAds = 6, timePlayReduceToShowAds = 2 };
        public static GameConfig GameConfig
        {
            get
            {
                if (DataManager.GameConfig != null)
                    gameConfig = DataManager.GameConfig;
                return gameConfig;
            }
        }

        public static bool RewardIsReady
        {
            get
            {
#if USE_IRON || USE_MAX
                return IronHelper.RewardIsReady || MaxHelper.RewardIsReady;
#endif
                return false;
            }
        }

        protected static bool isTimeToShowAds = true;
        /// <summary>
        /// Check time play more than game config to show ads
        /// </summary>
        public static bool IsTimeToShowAds
        {
            get
            {
                if (UserData != null && GameConfig != null)
                {
                    float timePlayToShowAds = GameConfig.timePlayToShowAds + (GameConfig.timePlayReduceToShowAds * Mathf.Min(totalSuccess, 2));
                    float totalTimePlay = (float)(DateTime.Now - LastTimeShowAd).TotalSeconds;

                    if (GameStateManager.CurrentState == GameState.Complete)
                        isTimeToShowAds = true;
                    if (Mathf.FloorToInt(totalTimePlay) >= timePlayToShowAds)
                        isTimeToShowAds = true;

                    Log("[AdsManager] isTimeToShowAds:  " + isTimeToShowAds + " - totalTimePlay: " + totalTimePlay.ToString("#0.0") + " - timePlayToShowAds: " + timePlayToShowAds + " - totalSuccess: " + totalSuccess);
                }
                return isTimeToShowAds;
            }
        }

        public delegate void AdsDelegate(AdType currentType, AdEvent currentEvent, string currentPlacement, string currentItem);
        public static AdsDelegate OnStateChanged;

        private static AdsManager instance;

        private void Awake()
        {
            if (instance != null)
                Destroy(gameObject);
            instance = this;
            DontDestroyOnLoad(instance.gameObject);
        }

        private void Start()
        {
            interstitialButton?.onClick.AddListener(TestInterstitial);
            videoRewarded?.onClick.AddListener(TestVideoReward);
            removeAdsToggle?.onValueChanged.AddListener((isOn) =>
            {
                ShowNotice("Not show ads is " + isOn);
                UpdateBannerArea();
            });
        }

        public static void Init()
        {
            if (IsInit)
                return;

#if !USE_MAX && !USE_IRON && !USE_ADMOB && !USE_UNITY
            if (!instance)
            {
                throw new Exception("AdsManager could not find the AdsManager object. Please ensure you have added the AdsManager Prefab to your scene.");
            }
#else
            foreach (var network in AdsNetwork)
            {
                if (network == AdMediation.MAX)
                {
                    Log("[AdsManager] Init " + network);
                    MaxHelper.Init(IsDebug);
                }
                else if (network == AdMediation.IRON)
                {
                    Log("[AdsManager] Init " + network);
                    IronHelper.Init(IsDebug);
                }
                else
                {
                    LogError("[AdsManager] Init " + network + " NOT SUPPORT");
                }
            }

            if (Settings.useBanner == AdMediation.MAX)
            {
                Log("[AdsManager] Init Banner " + DefaultMediation);
                MaxHelper.InitBanner();
                UpdateBannerArea();
            }
            else if (Settings.useBanner == AdMediation.IRON)
            {
                Log("[AdsManager] Init Banner " + DefaultMediation);
                IronHelper.InitBanner();
                UpdateBannerArea();
            }
            else if (Settings.useBanner == AdMediation.ADMOD)
            {
                LogError("[AdsManager] Init Banner " + Settings.useBanner + " NOT implement!");
            }
#endif
            IsInit = true;
        }

        public void TestInterstitial()
        {
            LastTimeShowAd = DateTime.Now.AddSeconds(-60);

            ShowInterstitial((s, a) =>
            {
                LogWarning("Test Interstitial: " + s);
            }, "default");
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
        public static void ShowInterstitial(Action<AdEvent, AdType> onSuccess, string placementName, string itemName = "default")
        {
            try
            {
                if (!instance)
                {
                    Debug.LogError("[AdsManager] could not find the AdsManager object. Please ensure you have added the AdsManager Prefab to your scene.");
                    onSuccess?.Invoke(AdEvent.NotAvailable, AdType.Interstitial);
                    return;
                }

                Init();

                interToReward = false;

                Log(string.Format("[AdsManager] ShowInterstitial IRON {0} MAX {1}", IronHelper.InterIsReady, MaxHelper.InterIsReady));

                if (!IsTimeToShowAds)
                {
                    onSuccess?.Invoke(AdEvent.NotTimeToShow, AdType.Interstitial);
                    return;
                }

                if (Settings.ratioInterPerReward > 0 && (totalInterSuccess + 0.001) / (totalRewardSuccess + 1) >= Settings.ratioInterPerReward)
                {
                    Log(string.Format("[AdsManager] RatioInterPerReward {0}/{1}", (totalInterSuccess + 0.001) / (totalRewardSuccess + 1), Settings.ratioInterPerReward));
                    ShowVideoReward(onSuccess, placementName, itemName);
                    return;
                }

                ShowNotice(AdEvent.Show);

                SetStatus(AdType.Interstitial, AdEvent.Show, placementName, itemName, DefaultMediation);

                if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
                {
                    onSuccess?.Invoke(AdEvent.Success, AdType.Interstitial);
                    SetStatus(AdType.Interstitial, AdEvent.Success, placementName, itemName, DefaultMediation);
                    return;
                }

#if !USE_MAX && !USE_IRON && !USE_ADMOB && !USE_UNITY
                onSuccess?.Invoke(AdEvent.Success, AdType.Interstitial);
                SetStatus(AdType.Interstitial, AdEvent.Success, placementName, itemName, DefaultMediation);
                return;
#else

                if (string.IsNullOrEmpty(placementName))
                    placementName = "default";

                if (string.IsNullOrEmpty(itemName))
                    itemName = "default";

                if (DefaultMediation == AdMediation.IRON)
                {
                    if (IronHelper.InterIsReady)
                    {
                        Log("[AdsManager] IRON Show");

                        IronHelper.ShowInterstitial(onSuccess, placementName, itemName);
                    }
                    else if (AdsNetwork.Contains(AdMediation.MAX) && MaxHelper.InterIsReady)
                    {
                        Log("[AdsManager] IRON --> MAX Show");

                        LogEvent("ad_" + AdType.Interstitial + "_iron_max", ParamsBase(placementName, itemName, AdMediation.MAX));

                        MaxHelper.ShowInterstitial(onSuccess, placementName, itemName);
                    }
                    else
                    {
                        if (interToReward && IronHelper.RewardIsReady)
                        {
                            Log("[AdsManager] IRON Inter --> IRON Reward");
                            LogEvent("ad_" + AdType.Interstitial + "_" + AdType.VideoReward, ParamsBase(placementName, itemName, DefaultMediation));
                            IronHelper.ShowRewarded(onSuccess, placementName, itemName);
                        }
                        else
                        {
                            LogWarning("[AdsManager] IRON --> MAX --> NOT AVAIABLE ------------------------------");
                            onSuccess?.Invoke(AdEvent.NotAvailable, AdType.Interstitial);
                            SetStatus(AdType.Interstitial, AdEvent.NotAvailable, placementName, itemName, DefaultMediation);
                        }
                    }
                }
                else if (DefaultMediation == AdMediation.MAX)
                {
                    if (MaxHelper.InterIsReady)
                    {
                        Log("[AdsManager] MAX Show");

                        MaxHelper.ShowInterstitial(onSuccess, placementName, itemName);
                    }
                    else if (AdsNetwork.Contains(AdMediation.IRON) && IronHelper.InterIsReady)
                    {
                        Log("[AdsManager] MAX --> IRON Show");

                        LogEvent("ad_" + AdType.Interstitial + "_max_iron", ParamsBase(placementName, itemName, AdMediation.IRON));

                        IronHelper.ShowInterstitial(onSuccess, placementName, itemName);
                    }
                    else
                    {
                        if (interToReward && MaxHelper.RewardIsReady)
                        {
                            Log("[AdsManager] MAX Inter --> MAX Reward");
                            LogEvent("ad_" + AdType.Interstitial + "_" + AdType.VideoReward, ParamsBase(placementName, itemName, DefaultMediation));
                            MaxHelper.ShowRewarded(onSuccess, placementName, itemName);
                        }
                        else
                        {
                            LogWarning("[AdsManager] MAX --> IRON --> NOT AVAIABLE ------------------------------");
                            onSuccess?.Invoke(AdEvent.NotAvailable, AdType.Interstitial);
                            SetStatus(AdType.Interstitial, AdEvent.NotAvailable, placementName, itemName, DefaultMediation);
                        }
                    }
                }
                else
                {
                    LogWarning("[AdsManager] Show --> NOT AVAIABLE ------------------------------");
                    onSuccess?.Invoke(AdEvent.NotAvailable, AdType.Interstitial);
                    SetStatus(AdType.Interstitial, AdEvent.NotAvailable, placementName, itemName, DefaultMediation);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                onSuccess?.Invoke(AdEvent.ShowFailed, AdType.Interstitial);
            }
        }

        public void TestVideoReward()
        {
            LastTimeShowAd = DateTime.Now.AddSeconds(-60);

            ShowVideoReward((s, a) =>
            {
                LogWarning("Test VideoReward: " + s);
            }, "default");
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
        public static void ShowVideoReward(Action<AdEvent, AdType> onSuccess, string placementName, string itemName = "default")
        {
            try
            {
                if (!instance)
                {
                    Debug.LogError("[AdsManager] could not find the AdsManager object. Please ensure you have added the AdsManager Prefab to your scene.");
                    onSuccess?.Invoke(AdEvent.NotAvailable, AdType.VideoReward);
                    return;
                }

                Init();

                rewardToInter = false;

                SetStatus(AdType.VideoReward, AdEvent.Show, placementName, itemName, DefaultMediation);

                Log(string.Format("[AdsManager] IRON {0} MAX {1}", IronHelper.RewardIsReady, MaxHelper.RewardIsReady));

                ShowNotice(AdEvent.Show);

                if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
                {
                    onSuccess?.Invoke(AdEvent.Success, AdType.VideoReward);
                    SetStatus(AdType.VideoReward, AdEvent.Success, placementName, itemName, DefaultMediation);
                    return;
                }

#if !USE_MAX && !USE_IRON && !USE_ADMOB && !USE_UNITY
                onSuccess?.Invoke(AdEvent.Success, AdType.VideoReward);
                SetStatus(AdType.VideoReward, AdEvent.Success, placementName, itemName, DefaultMediation);
                return;
#else

                if (string.IsNullOrEmpty(placementName))
                    placementName = "default";

                if (string.IsNullOrEmpty(itemName))
                    itemName = "default";

                if (DefaultMediation == AdMediation.IRON)
                {
                    if (IronHelper.RewardIsReady)
                    {
                        IronHelper.ShowRewarded(onSuccess, placementName, itemName);
                    }
                    else if (AdsNetwork.Contains(AdMediation.MAX) && MaxHelper.RewardIsReady)
                    {
                        Log("[AdsManager] IRON --> MAX");
                        LogEvent("ad_" + AdType.VideoReward + "_iron_max", ParamsBase(placementName, itemName, AdMediation.MAX));
                        MaxHelper.ShowRewarded(onSuccess, placementName, itemName);
                    }
                    else
                    {
                        if (rewardToInter && IronHelper.InterIsReady)
                        {
                            Log("[AdsManager] IRON Reward --> IRON Inter");
                            LogEvent("ad_" + AdType.VideoReward + "_" + AdType.Interstitial, ParamsBase(placementName, itemName, DefaultMediation));
                            IronHelper.ShowInterstitial(onSuccess, placementName, itemName);
                        }
                        else
                        {
                            LogWarning("[AdsManager] IRON --> MAX --> NOT AVAIABLE ------------------------------");
                            onSuccess?.Invoke(AdEvent.NotAvailable, AdType.VideoReward);
                            SetStatus(AdType.VideoReward, AdEvent.NotAvailable, placementName, itemName, DefaultMediation);
                        }
                    }
                }
                else if (DefaultMediation == AdMediation.MAX)
                {
                    if (MaxHelper.RewardIsReady)
                    {
                        MaxHelper.ShowRewarded(onSuccess, placementName, itemName);
                    }
                    else if (AdsNetwork.Contains(AdMediation.IRON) && IronHelper.RewardIsReady)
                    {
                        Log("[AdsManager] MAX --> IRON");
                        LogEvent("ad_" + AdType.VideoReward + "_max_iron", ParamsBase(placementName, itemName, AdMediation.IRON));

                        IronHelper.ShowRewarded(onSuccess, placementName, itemName);
                    }
                    else
                    {
                        if (rewardToInter && MaxHelper.InterIsReady)
                        {
                            Log("[AdsManager] MAX Reward --> MAX Inter");
                            LogEvent("ad_" + AdType.VideoReward + "_" + AdType.Interstitial, ParamsBase(placementName, itemName, DefaultMediation));
                            MaxHelper.ShowInterstitial(onSuccess, placementName, itemName);
                        }
                        else
                        {
                            LogWarning("[AdsManager] MAX --> IRON --> NOT AVAIABLE ------------------------------");
                            onSuccess?.Invoke(AdEvent.NotAvailable, AdType.VideoReward);
                            SetStatus(AdType.VideoReward, AdEvent.NotAvailable, placementName, itemName, DefaultMediation);
                        }
                    }
                }
                else
                {
                    LogWarning("[AdsManager] Show --> NOT AVAIABLE ------------------------------");
                    onSuccess?.Invoke(AdEvent.NotAvailable, AdType.VideoReward);
                    SetStatus(AdType.VideoReward, AdEvent.NotAvailable, placementName, itemName, DefaultMediation);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                onSuccess?.Invoke(AdEvent.Exception, AdType.VideoReward);
            }
        }

        public static void DestroyBanner()
        {
            if (DefaultMediation == AdMediation.IRON)
            {
                IronHelper.DestroyBanner();
            }
            else if (DefaultMediation == AdMediation.MAX)
            {
                MaxHelper.DestroyBaner();
            }
        }

        public static void ShowBanner()
        {
            if (DefaultMediation == AdMediation.IRON)
            {
                IronHelper.ShowBanner();
            }
            else if (DefaultMediation == AdMediation.MAX)
            {
                MaxHelper.ShowBanner();
            }
        }

        public static void HideBanner()
        {
            if (DefaultMediation == AdMediation.IRON)
            {
                IronHelper.HideBanner();
            }
            else if (DefaultMediation == AdMediation.MAX)
            {
                MaxHelper.HideBanner();
            }
        }

        public static AdEvent LastAdEvent { get; set; }
        public static AdType LastAdType { get; set; }
        public static AdMediation LastMediation { get; set; }

        public static void SetStatus(AdType adType, AdEvent adEvent, string placementName = "default", string itemName = "default", AdMediation mediation = AdMediation.NONE)
        {
            if (instance)
            {
                try
                {
                    if (UserData == null)
                        return;

                    if (adEvent == AdEvent.Close)
                        LastTimeShowAd = DateTime.Now;

                    if (adEvent == AdEvent.Success)
                    {
                        LastTimeShowAd = DateTime.Now;

                        if (adType == AdType.Interstitial)
                        {
                            totalSuccess++;
                            totalInterSuccess++;
                            UserData.TotalAdInterstitial++;
                            LogEvent("ad_success", ParamsBase(placementName, itemName, mediation));
                            DataManager.Save();
                            ShowNotice(adEvent);
                        }

                        if (adType == AdType.VideoReward)
                        {
                            totalSuccess++;
                            totalRewardSuccess++;
                            UserData.TotalAdRewarded++;
                            LogEvent("ad_success", ParamsBase(placementName, itemName, mediation));
                            DataManager.Save();
                            ShowNotice(adEvent);
                        }
                    }
                    else
                    {
                        if (adType == AdType.VideoReward)
                        {
                            ShowNotice(adEvent);
                        }
                        else
                        {
                            HideNotice();
                        }
                    }

                    OnStateChanged?.Invoke(adType, adEvent, placementName, itemName);

                    if (adEvent == LastAdEvent && adType == LastAdType && LastMediation == mediation)
                        return;

                    LastAdEvent = adEvent;
                    LastAdType = adType;
                    LastMediation = mediation;

                    LogEvent(adType, adEvent, ParamsBase(placementName, itemName, mediation));
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public static bool IsConnected
        {
            get
            {
                switch (Application.internetReachability)
                {
                    case NetworkReachability.ReachableViaLocalAreaNetwork:
                        return true;
                    case NetworkReachability.ReachableViaCarrierDataNetwork:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public static Dictionary<string, object> ParamsBase(string placementName = "default", string itemName = "default", AdMediation mediation = AdMediation.NONE)
        {
            return new Dictionary<string, object>
            {
                { "in_session", totalSuccess.ToString() },
                { "session", UserData.Session.ToString() },
                { "ad_interstitial", UserData.TotalAdInterstitial.ToString() },
                { "ad_rewarded", UserData.TotalAdRewarded.ToString() },
                { "ad_platform", mediation.ToString() },
                { "placement", placementName.ToLower() },
                { "item", itemName.ToLower() },
                { "total", (UserData.TotalAdInterstitial + UserData.TotalAdRewarded).ToString()}
            };
        }

        public static void ShowNotice(AdEvent onSuccess)
        {
            try
            {
                if (onSuccess == AdEvent.NotInternet)
                    ShowNotice("Please check your internet connection...!");
                else if (onSuccess == AdEvent.NotAvailable)
                    ShowNotice("Video not ready, please try again...!");
                else if (onSuccess == AdEvent.Exception)
                    ShowNotice("Something wrong, please try again...!");
                else if (onSuccess == AdEvent.Success)
                    ShowNotice("Thanks for watching video...!");
                else if (onSuccess == AdEvent.Show)
                    ShowNotice("Time to show ADs... please wait!");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static void UpdateBannerArea()
        {
            if (instance != null && Settings.useBanner != AdMediation.NONE)
            {
                if (instance.parentTransform == null)
                    instance.parentTransform = UIAnimManager.RootRectTransform;
                if (instance.parentTransform != null && AdsBannerAreaList != null)
                {
                    foreach (var i in AdsBannerAreaList)
                        i.SetArea(instance.bannerHeight / instance.parentTransform.rect.height, IsNotShowAds ? BannerPos.NONE : instance.bannerPos);
                }

                if (IsNotShowAds)
                    DestroyBanner();
            }
        }

        public static bool IsNotShowAds
        {
            get
            {
                if (instance != null && instance.removeAdsToggle != null && instance.removeAdsToggle.isOn)
                    return true;
                if (UserData != null)
                    return UserData.isRemovedAds;
                return false;
            }
        }

        private static void ShowNotice(string value)
        {
            //UIToast.ShowNotice(value);
        }

        public static void HideNotice()
        {
            //UIToast.Hide();
        }

        public static void Log(string value)
        {
                Debug.Log(value);
        }

        public static void LogWarning(string value)
        {
            if (IsDebug)
                Debug.LogWarning(value);
        }

        public static void LogError(string value)
        {
            Debug.LogError(value);
        }

        public static void LogException(Exception ex)
        {
            Debug.LogException(ex);
        }

        public static void LogEvent(AdType adType, AdEvent adEvent, Dictionary<string, object> eventParams)
        {
            string eventName = "ad_" + adType.ToString().ToLower() + "_" + adEvent.ToString().ToLower();
            LogEvent(eventName, eventParams);

            if (adEvent == AdEvent.Success)
            {
#if USE_APPSFLYER
                var appParam = new Dictionary<string, string>();
                foreach (var x in appParam)
                {
                    if (x.Key != null && x.Value != null)
                        appParam.Add(x.Key.ToLower(), !x.Value.ToString().Contains("_") ? System.Text.RegularExpressions.Regex.Replace(x.Value.ToString(), @"\B[A-Z]", m => "_" + m.ToString()).ToLower() : x.Value.ToString());
                }
                AppsFlyerSDK.AppsFlyer.sendEvent("ad_" + adType.ToString().ToLower(), appParam);
#endif
            }
        }

        public static void LogEvent(string eventName, Dictionary<string, object> eventParams)
        {
            if (eventParams == null)
                eventParams = ParamsBase();

#if USE_FIREBASE
            FirebaseManager.LogEvent(eventName, eventParams);
#else
            string stringLog = eventName + "\n";
            foreach (var k in eventParams)
            {
                stringLog += string.Format("{0}: {1} \n", k.Key, k.Value);
            }
            Debug.Log("--------> LogEvent " + stringLog);
#endif
        }

        public static void LogImpressionData(AdMediation mediation, object data, string adUnitId = null)
        {
            try
            {
                double revenue = 0;
                string ad_source = "";
                string ad_unit_name = "";
                string ad_format = "";
                string country = "";
                double lifetime_revenue = 0;
#if USE_MAX
                if (data != null && data is MaxSdkBase.AdInfo)
                {
                    var impressionData = data as MaxSdkBase.AdInfo;

                    ad_source = impressionData.NetworkName;
                    ad_unit_name = impressionData.AdUnitIdentifier;
                    ad_format = impressionData.Placement;
                    revenue = impressionData.Revenue;
                }
#endif

#if USE_IRON
                if (data != null && data is IronSourceImpressionData)
                {
                    var impressionData = data as IronSourceImpressionData;
                    ad_source = impressionData.adNetwork;
                    ad_unit_name = impressionData.adUnit;
                    ad_format = impressionData.instanceName;
                    revenue = (double)(impressionData.revenue != null ? impressionData.revenue : 0.0f);
                    country = impressionData.country;
                    lifetime_revenue = (double)(impressionData.lifetimeRevenue != null ? impressionData.lifetimeRevenue : 0.0f);
                }
#endif


                var impressionParameters = new Dictionary<string, object>
                {
                    { "ad_platform", mediation.ToString() },
                    { "ad_source", ad_source },
                    { "ad_unit_name", ad_unit_name },
                    { "ad_format", ad_format.ToLower() },
                    { "value", revenue },
                    { "currency", "USD" },

                    { "country", country },
                    { "lifetime_revenue", lifetime_revenue }
                };
#if USE_FIREBASE
                FirebaseManager.LogEvent("ad_impression", impressionParameters);
#endif

                string stringLog = "ad_impression" + " " + mediation.ToString() + " " + ad_source + " " + ad_unit_name + " " + ad_format;

                if (IsDebug)
                    Debug.Log("--------> LogEvent " + stringLog);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void OnApplicationPause(bool isPaused)
        {
#if USE_IRON
            IronSource.Agent.onApplicationPause(isPaused);
#endif
        }

        public static void SetArea()
        {}
    }
}
