#if USE_FIREBASE
using Firebase.Analytics;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Globalization;

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
        protected List<GameObject> bannerGameObjectList = null;

        private static bool IsDebug => DebugMode.IsDebugMode;

        [Header("Options")]
        [SerializeField]
        protected Button interstitialButton = null;
        [SerializeField]
        protected Button videoRewarded = null;
        [SerializeField]
        protected Toggle removeAdsToggle = null;

        [Header("Backup")]
        [SerializeField]
        protected bool loadBackupOnInitDone = false;
        public static bool LoadBackupOnInitDone => instance.loadBackupOnInitDone;
        [SerializeField]
        protected bool testForceBackup = false;
        public static bool TestForceBackup => instance.testForceBackup;
        [SerializeField]
        protected string testDeviceId = "";
        public static string TestDeviceId => instance.testDeviceId;

        [Header("Banner Safe Area")]
        [SerializeField]
        protected float bannerHeight = 68f;
        protected RectTransform parentTransform;
        public static BannerPos BannerPos => Settings.bannerPosition;
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

        protected static GameConfig gameConfig = new GameConfig { timePlayToShowAd = 15, timePlayToShowAdReduce = 0, timePlayToShowOpenAd = 15, timeToWaitOpenAd = 5 };
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
                bool isReady = false;
#if USE_IRON || USE_MAX || USE_ADMOB
                if (IronHelper.RewardIsReady)
                    isReady = true;
                else if (MaxHelper.RewardIsReady)
                    isReady = true;
                else if (AdmobHelper.RewardIsReady)
                    isReady = true;
#else
                isReady = true;
#endif
                return isReady;
            }
        }

        public static bool InterIsReady
        {
            get
            {
                bool isReady = false;
#if USE_IRON || USE_MAX || USE_ADMOB
                if (IronHelper.InterIsReady)
                    isReady = true;
                else if (MaxHelper.InterIsReady)
                    isReady = true;
                else if (AdmobHelper.InterIsReady)
                    isReady = true;
#else
                isReady = true;
#endif
                return isReady;
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
                    isTimeToShowAds = false;

                    if (IsNotShowAds)
                    {
                        Log("isVIP: " + UserData.isVIP + " isRemovedAds: " + UserData.isRemovedAds);
                        return isTimeToShowAds;
                    }

                    if ((LastAdType == AdType.Reward || LastAdType == AdType.Inter || LastAdType == AdType.AppOpen) && (LastAdEvent == AdEvent.ShowStart || LastAdEvent == AdEvent.ShowSuccess || LastAdEvent == AdEvent.Close))
                    {
                        Debug.LogWarning("[AdsManager] is " + LastAdType + " is " + LastAdEvent + " --> return");
                        return isTimeToShowAdOpen;
                    }

                    float timePlayToShowAds = GameConfig.timePlayToShowAd + (GameConfig.timePlayToShowAdReduce * Mathf.Min(totalSuccess, 2));
                    float totalTimePlay = (float)(DateTime.Now - LastTimeShowAd).TotalSeconds;

                    if (Mathf.FloorToInt(totalTimePlay) >= timePlayToShowAds)
                        isTimeToShowAds = true;

                    Log("[AdsManager] isTimeToShowAds:  " + isTimeToShowAds + " - totalTimePlay: " + totalTimePlay.ToString("#0.0") + " - timePlayToShowAds: " + timePlayToShowAds + " - totalSuccess: " + totalSuccess);
                }
                return isTimeToShowAds;
            }
        }

        protected static bool isTimeToShowAdOpen = true;
        /// <summary>
        /// Check time play more than game config to show ads
        /// </summary>
        public static bool IsTimeToShowAdOpen
        {
            get
            {
                if (UserData != null && GameConfig != null)
                {
                    isTimeToShowAdOpen = false;

                    if (IsNotShowAds)
                    {
                        Log("isVIP: " + UserData.isVIP + " isRemovedAds: " + UserData.isRemovedAds);
                        return isTimeToShowAdOpen;
                    }

                    if ((LastAdType == AdType.Reward || LastAdType == AdType.Inter || LastAdType == AdType.AppOpen) && (LastAdEvent == AdEvent.ShowStart || LastAdEvent == AdEvent.ShowSuccess || LastAdEvent == AdEvent.Close))
                    {
                        Debug.LogWarning("[AdsManager] is " + LastAdType + " is " + LastAdEvent + " --> return");
                        return isTimeToShowAdOpen;
                    }

                    float timePlayToShowAds = GameConfig.timePlayToShowOpenAd;
                    float totalTimePlay = (float)(DateTime.Now - LastTimeShowAd).TotalSeconds;

                    if (Mathf.FloorToInt(totalTimePlay) >= timePlayToShowAds)
                        isTimeToShowAdOpen = true;

                    Log("[AdsManager] isTimeToShowAds:  " + isTimeToShowAdOpen + " - totalTimePlay: " + totalTimePlay.ToString("#0.0") + " - timePlayToShowAds: " + timePlayToShowAds);
                }
                return isTimeToShowAdOpen;
            }
        }

        public delegate void AdsDelegate(AdType currentType, AdEvent currentEvent, string currentPlacement, string currentItem);
        public static AdsDelegate OnStateChanged;

        protected static AdsManager instance = null;

        private void Awake()
        {
            try
            {
                if (instance != null)
                    Destroy(gameObject);
                if (instance == null)
                    instance = this;
                DontDestroyOnLoad(gameObject);
            }
            catch (Exception ex)
            {
                Debug.LogError("[AdsManager] Exception: " + ex.Message);
            }
        }

        public static void CheckInstance()
        {
            if (instance == null)
            {
                var prefab = Resources.Load<AdsManager>("AdsManager");
                if (prefab != null)
                    instance = Instantiate(prefab);
            }

            if (instance == null)
                throw new Exception("AdsManager could not find the AdsManager object. Please ensure you have added the Base/Plugins/Resources/AdsManager Prefab to your scene.");
        }

        private void Start()
        {
            interstitialButton?.onClick.RemoveAllListeners();
            interstitialButton?.onClick.AddListener(TestInterstitial);


            videoRewarded?.onClick.RemoveAllListeners();
            videoRewarded?.onClick.AddListener(TestVideoReward);


            removeAdsToggle?.onValueChanged.RemoveAllListeners();
            removeAdsToggle?.onValueChanged.AddListener((isOn) =>
            {
                //UIToast.ShowNotice("Not show ads is " + isOn);
                UpdateBannerArea();
            });

            UserData.OnVIPChanged += OnVIPChanged;
            UserData.OnRemovedAdsChanged += OnRemovedAdsChanged;
        }

        private void OnDestroy()
        {
            UserData.OnVIPChanged -= OnVIPChanged;
            UserData.OnRemovedAdsChanged -= OnRemovedAdsChanged;
        }

        private void OnVIPChanged(bool isVip)
        {
            Debug.Log("UserData_OnVIPChanged");
            UpdateBannerArea();
        }

        private void OnRemovedAdsChanged(bool isRemoveAds)
        {
            Debug.Log("OnRemovedAdsChanged");
            UpdateBannerArea();
        }

        public static IEnumerator DOInit()
        {
            CheckInstance();

            if (IsInit)
                yield break;

            yield return new WaitForEndOfFrame();

            yield return ATTHelper.DOCheckATT();


#if !USE_MAX && !USE_IRON && !USE_ADMOB && !USE_UNITY
            if (!instance)
            {
                throw new Exception("AdsManager could not find the AdsManager object. Please ensure you have added the AdsManager Prefab to your scene.");
                yield break;
            }
#else

            if (GameConfig.timeToWaitOpenAd > 0)
                yield return AdOpen.DOInit();

            if (GameConfig.adUseBackup)
            {
                foreach (var network in AdsNetwork)
                {
                    if (network == AdMediation.IRON)
                    {
                        Log("[AdsManager] Init " + network);
                        IronHelper.Init(IsDebug);
                        yield return new WaitForSeconds(0.25f);
                    }
                    else if (network == AdMediation.MAX)
                    {
                        Log("[AdsManager] Init " + network);
                        MaxHelper.Init(IsDebug);
                        yield return new WaitForSeconds(0.25f);
                    }
                    else if (network == AdMediation.ADMOD)
                    {
                        Log("[AdsManager] Init " + network);
                        yield return AdmobHelper.DOInit(IsDebug, () => Debug.Log("AdmobHelper Init DONE but DON NOT AUTO LOAD"), LoadBackupOnInitDone || TestForceBackup);
                        yield return new WaitForSeconds(0.25f);
                    }
                    else
                    {
                        LogError("[AdsManager] Init " + network + " NOT SUPPORT");
                    }
                }
            }
            else
            {
                if (DefaultMediation == AdMediation.MAX)
                {
                    Log("[AdsManager] Init " + DefaultMediation);
                    MaxHelper.Init(IsDebug);
                    yield return new WaitForSeconds(0.25f);
                }
                else if (DefaultMediation == AdMediation.IRON)
                {
                    Log("[AdsManager] Init " + DefaultMediation);
                    IronHelper.Init(IsDebug);
                    yield return new WaitForSeconds(0.25f);
                }
                else if (DefaultMediation == AdMediation.ADMOD)
                {
                    Log("[AdsManager] Init " + DefaultMediation);
                    yield return AdmobHelper.DOInit(IsDebug, () => Debug.Log("AdmobHelper Init DONE --> AUTO LOAD"), true);
                    yield return new WaitForSeconds(0.25f);
                }
                else
                {
                    LogError("[AdsManager] Init " + DefaultMediation + " NOT SUPPORT");
                }
            }

            UpdateBannerArea();

            yield return AdvertyHelper.DOInit();
#endif
            IsInit = true;
        }

        /// <summary>
        /// Should call show then FirebaseManager initiated. Flow game monetization on Gameover
        /// </summary>
        /// <returns></returns>
        public static IEnumerator ShowAdOpen()
        {
            string itemName = "app_open";
            if (GameConfig != null)
            {
#if UNITY_IOS && (USE_ADOPEN || USE_MAXOPEN) && !UNITY_EDITOR
                while (Unity.Advertisement.IosSupport.ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == Unity.Advertisement.IosSupport.ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
                    yield return null;
#endif
                DateTime startLoad = DateTime.Now;

                Log(AdOpen.TAG + "WaitToShow: " + GameConfig.timeToWaitOpenAd + " is " + AdOpen.Status.ToString());
                if (GameConfig.timeToWaitOpenAd > 0)
                {
                    double loadAdsIn = (DateTime.Now - startLoad).TotalSeconds;
                    while (loadAdsIn < GameConfig.timeToWaitOpenAd && (AdOpen.Status == AdEvent.Offer || AdOpen.Status == AdEvent.Load))
                    {
                        loadAdsIn = (DateTime.Now - startLoad).TotalSeconds;
                        Log(AdOpen.TAG + "WaitToShow: " + AdOpen.Status.ToString() + " in " + loadAdsIn.ToString("#0.00"));
                        yield return new WaitForSeconds(0.125f);
                    }

                    if (AdOpen.IsReady)
                    {
                        Log(AdOpen.TAG + "WaitToShow: " + AdOpen.Status.ToString() + " IsReady " + AdOpen.IsReady);
                        AdOpen.ShowOpenAdIfAvailable(itemName);
                        yield break;
                    }
                    else
                    {
                        Log(AdOpen.TAG + "WaitToShow: " + AdOpen.Status.ToString() + " " + AdOpen.IsReady + " ---> Check Open MAX");
                    }
                }

                startLoad = DateTime.Now;
                Log(MaxHelper.TAG + "WaitToShow: " + GameConfig.adUseOpenBackup + " is " + MaxHelper.StatusOpen.ToString());
#if USE_MAXOPEN
                if (GameConfig.adUseOpenBackup)
                {
                    double loadAdsIn = (DateTime.Now - startLoad).TotalSeconds;
                    while (loadAdsIn < GameConfig.timeToWaitOpenAd && (MaxHelper.StatusOpen == AdEvent.Offer || MaxHelper.StatusOpen == AdEvent.Load))
                    {
                        loadAdsIn = (DateTime.Now - startLoad).TotalSeconds;
                        Log(MaxHelper.TAG + "WaitToShow: " + MaxHelper.StatusOpen.ToString() + " in " + loadAdsIn.ToString("#0.00"));
                        yield return new WaitForSeconds(0.125f);
                    }

                    if (MaxHelper.OpenIsReady)
                    {
                        Log(MaxHelper.TAG + "WaitToShow: " + GameConfig.adUseOpenBackup + " is " + MaxHelper.StatusOpen.ToString() + " IsReady " + MaxHelper.OpenIsReady);
                        MaxHelper.ShowOpenAdIfAvailable(itemName);
                        yield break;
                    }
                    else
                    {
                        Log(MaxHelper.TAG + "WaitToShow: " + GameConfig.adUseOpenBackup + " is " + MaxHelper.StatusOpen.ToString() + " IsReady " + MaxHelper.OpenIsReady + " ---> Check Iner DEFAULT");
                    }
                }
#endif
                Log("DEFAULT " + "WaitToShow: " + GameConfig.timeToWaitOpenInter + " is " + InterIsReady);
                if (GameConfig.timeToWaitOpenInter > 0)
                {
                    double loadAdsIn = (DateTime.Now - startLoad).TotalSeconds;
                    while (loadAdsIn < GameConfig.timeToWaitOpenInter && InterIsReady)
                    {
                        loadAdsIn = (DateTime.Now - startLoad).TotalSeconds;
                        Log(MaxHelper.TAG + "WaitToShow: InterIsReady " + InterIsReady + " in " + loadAdsIn.ToString("#0.00"));
                        yield return new WaitForSeconds(0.125f);
                    }

                    //ShowInterstitial((status, type) => { }, "app_open_inter", itemName);
                }
            }
        }

        private static void Log(object p)
        {
            throw new NotImplementedException();
        }

        public void TestInterstitial()
        {
            LastTimeShowAd = DateTime.Now.AddSeconds(-180);

            ShowInterstitial((s, a) =>
            {
                LogWarning("Test Interstitial: " + s);
            }, "default");
        }

        public void ShowInterstitial(string item)
        {
            if (GameConfig.forceInterEverywhere)
            {
                ShowInterstitial((s, a) =>
                {

                }, "forceInterEverywhere", item);
            }
        }

        /// <summary>
        /// Befor call should show loading then check status to do something. Flow game monetization on Gameover
        /// <para/>
        /// A.on RESULT SCREEN: 
        /// if (timePlayInGame >= timePlayToShowAds)
        /// {
        ///     ShowInterstitial((status) =>
        ///     {
        ///         do something
        ///     }
        ///}
        ///<para/>
        ///B.on CONTINUE SCREEN: 
        ///if (userClickRebornByAds) 
        ///     => reset timePlayInGame
        /// else 
        ///     => flow on RESULT SCREEN
        /// </summary>
        public static void ShowInterstitial(Action<AdEvent, AdType> onSuccess, string placementName, string itemName = "default", bool forceToShow = false)
        {
            try
            {
                if (instance == null)
                {
                    Debug.LogError("[AdsManager] could not find the AdsManager object. Please ensure you have added the AdsManager Prefab to your scene.");
                    onSuccess?.Invoke(AdEvent.ShowNotAvailable, AdType.Inter);
                    return;
                }

                instance.StartCoroutine(DOInit());

                interToReward = GameConfig.forceInterToReward;

                Debug.Log(string.Format("[AdsManager] ShowInterstitial IRON {0} MAX {1} ADMOB {2} LastAdEvent {3} IsTime {4} TotalSuccess {5} ", IronHelper.InterIsReady, MaxHelper.InterIsReady, AdmobHelper.InterIsReady, LastAdEvent, IsTimeToShowAds, totalSuccess));

                if (UserData.TotalWin < GameConfig.adShowFromLevel)
                {
                    Debug.LogWarning("[AdsManager] adShowFromLevel " + UserData.TotalWin + "/" + GameConfig.adShowFromLevel + " --> return");
                    return;
                }

                if (!IsTimeToShowAds && !forceToShow || IsNotShowAds)
                {
                    onSuccess?.Invoke(AdEvent.ShowNotTime, AdType.Inter);
                    return;
                }

                if (Settings.ratioInterPerReward > 0 && (totalInterSuccess + 0.001) / (totalRewardSuccess + 1) >= Settings.ratioInterPerReward)
                {
                    Log(string.Format("[AdsManager] ShowInterstitial RatioInterPerReward {0}/{1}", (totalInterSuccess + 0.001) / (totalRewardSuccess + 1), Settings.ratioInterPerReward));
                    ShowVideoReward(onSuccess, placementName, itemName);
                    return;
                }

                SetStatus(AdType.Inter, AdEvent.Show, placementName, itemName, DefaultMediation);

#if UNITY_EDITOR && !USE_ADMOB
                if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
                {
                    onSuccess?.Invoke(AdEvent.ShowSuccess, AdType.Inter);
                    SetStatus(AdType.Inter, AdEvent.ShowSuccess, placementName, itemName, DefaultMediation);
                    return;
                }
#endif

#if !USE_MAX && !USE_IRON && !USE_ADMOB && !USE_UNITY
                onSuccess?.Invoke(AdEvent.ShowSuccess, AdType.Inter);
                SetStatus(AdType.Inter, AdEvent.ShowSuccess, placementName, itemName, DefaultMediation);
                return;
#else

                if (string.IsNullOrEmpty(placementName))
                    placementName = "default";

                if (string.IsNullOrEmpty(itemName))
                    itemName = "default";

                if (DefaultMediation == AdMediation.IRON)
                {
                    if (IronHelper.InterIsReady && !instance.testForceBackup)
                    {
                        Debug.Log("[AdsManager] ShowInterstitial IRON Show");

                        IronHelper.ShowInterstitial(onSuccess, placementName, itemName);
                    }
                    else if (AdsNetwork.Contains(AdMediation.MAX) && MaxHelper.InterIsReady)
                    {
                        Debug.Log("[AdsManager] ShowInterstitial IRON --> MAX Show");

                        LogEvent("ad_" + AdType.Inter + "_iron_max", ParamsBase(placementName, itemName, AdMediation.MAX));

                        MaxHelper.ShowInterstitial(onSuccess, placementName, itemName);
                    }
                    else if (AdsNetwork.Contains(AdMediation.ADMOD) && AdmobHelper.InterIsReady)
                    {
                        Debug.Log("[AdsManager] ShowInterstitial IRON --> ADMOD Show");

                        LogEvent("ad_" + AdType.Inter + "_iron_admob", ParamsBase(placementName, itemName, AdMediation.ADMOD));

                        AdmobHelper.ShowInterstitial(onSuccess, placementName, itemName);
                    }
                    else
                    {
                        if (interToReward && IronHelper.RewardIsReady)
                        {
                            Debug.Log("[AdsManager] ShowInterstitial IRON Inter --> IRON Reward");

                            LogEvent("ad_" + AdType.Inter + "_" + AdType.Reward, ParamsBase(placementName, itemName, DefaultMediation));

                            IronHelper.ShowRewarded(onSuccess, placementName, itemName);
                        }
                        else
                        {
                            Debug.Log("[AdsManager] ShowInterstitial IRON Inter --> NOT AVAIABLE --> Try show AdUseBackup: " + GameConfig.adUseBackup + " " + AdmobHelper.IsInitInter);

                            IronHelper.ShowInterstitial(onSuccess, placementName, itemName);

                            if (GameConfig.adUseBackup && AdsNetwork.Contains(AdMediation.ADMOD) && AdmobHelper.IsInitInter == false)
                            {
                                Debug.Log("[AdsManager] ShowInterstitial IRON Inter --> NOT AVAIABLE --> ADMOB Init Inter");
                                instance.StartCoroutine(AdmobHelper.DOInit(IsDebug, AdmobHelper.InitInter));
                            }
                        }
                    }
                }
                else if (DefaultMediation == AdMediation.MAX)
                {
                    if (MaxHelper.InterIsReady && !instance.testForceBackup)
                    {
                        Debug.Log("[AdsManager] ShowInterstitial MAX Show");

                        MaxHelper.ShowInterstitial(onSuccess, placementName, itemName);
                    }
                    else if (AdsNetwork.Contains(AdMediation.IRON) && IronHelper.InterIsReady)
                    {
                        Debug.Log("[AdsManager] ShowInterstitial MAX --> IRON Show");

                        LogEvent("ad_" + AdType.Inter + "_max_iron", ParamsBase(placementName, itemName, AdMediation.IRON));

                        IronHelper.ShowInterstitial(onSuccess, placementName, itemName);
                    }
                    else if (AdsNetwork.Contains(AdMediation.ADMOD) && AdmobHelper.InterIsReady)
                    {
                        Debug.Log("[AdsManager] ShowInterstitial MAX --> ADMOD Show");

                        LogEvent("ad_" + AdType.Inter + "_max_admob", ParamsBase(placementName, itemName, AdMediation.ADMOD));

                        AdmobHelper.ShowInterstitial(onSuccess, placementName, itemName);
                    }
                    else
                    {
                        if (interToReward && IronHelper.RewardIsReady)
                        {
                            Debug.Log("[AdsManager] ShowInterstitial MAX Inter --> MAX Reward");

                            LogEvent("ad_" + AdType.Inter + "_" + AdType.Reward, ParamsBase(placementName, itemName, DefaultMediation));

                            MaxHelper.ShowRewarded(onSuccess, placementName, itemName);
                        }
                        else
                        {
                            Debug.Log("[AdsManager] ShowInterstitial MAX Inter --> NOT AVAIABLE --> Try show AdUseBackup: " + GameConfig.adUseBackup + " " + AdmobHelper.IsInitInter);

                            MaxHelper.ShowInterstitial(onSuccess, placementName, itemName);

                            if (GameConfig.adUseBackup && AdsNetwork.Contains(AdMediation.ADMOD) && AdmobHelper.IsInitInter == false)
                            {
                                Debug.Log("[AdsManager] ShowInterstitial MAX Inter --> NOT AVAIABLE --> ADMOB Init Inter");
                                instance.StartCoroutine(AdmobHelper.DOInit(IsDebug, AdmobHelper.InitInter));
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("[AdsManager] ShowInterstitial Show --> NOT AVAIABLE ------------------------------");
                    onSuccess?.Invoke(AdEvent.ShowNotAvailable, AdType.Inter);
                    SetStatus(AdType.Inter, AdEvent.ShowNotAvailable, placementName, itemName, DefaultMediation);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                onSuccess?.Invoke(AdEvent.ShowFailed, AdType.Inter);
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
        ///     if (status == AdEvent.ShowSuccess)
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
                    onSuccess?.Invoke(AdEvent.ShowNotAvailable, AdType.Reward);
                    return;
                }

                instance.StartCoroutine(DOInit());

                Debug.Log(string.Format("[AdsManager] ShowVideoReward IRON {0} MAX {1} ADMOB {2} LastAdEvent {3} IsTime {4}", IronHelper.RewardIsReady, MaxHelper.RewardIsReady, AdmobHelper.RewardIsReady, LastAdEvent, IsTimeToShowAds));

                rewardToInter = GameConfig.forceRewardToInter;

                SetStatus(AdType.Reward, AdEvent.Show, placementName, itemName, DefaultMediation);

#if UNITY_EDITOR && !USE_ADMOB
                if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
                {
                    onSuccess?.Invoke(AdEvent.ShowSuccess, AdType.Reward);
                    SetStatus(AdType.Reward, AdEvent.ShowSuccess, placementName, itemName, DefaultMediation);
                    return;
                }
#endif

#if !USE_MAX && !USE_IRON && !USE_ADMOB && !USE_UNITY
                onSuccess?.Invoke(AdEvent.ShowSuccess, AdType.Reward);
                SetStatus(AdType.Reward, AdEvent.ShowSuccess, placementName, itemName, DefaultMediation);
                return;
#else

                if (string.IsNullOrEmpty(placementName))
                    placementName = "default";

                if (string.IsNullOrEmpty(itemName))
                    itemName = "default";

                if (DefaultMediation == AdMediation.IRON)
                {
                    if (IronHelper.RewardIsReady && !instance.testForceBackup)
                    {
                        IronHelper.ShowRewarded(onSuccess, placementName, itemName);
                    }
                    else if (AdsNetwork.Contains(AdMediation.MAX) && MaxHelper.RewardIsReady)
                    {
                        Debug.Log("[AdsManager] ShowVideoReward IRON --> MAX");

                        LogEvent("ad_" + AdType.Reward + "_iron_max", ParamsBase(placementName, itemName, AdMediation.MAX));

                        MaxHelper.ShowRewarded(onSuccess, placementName, itemName);
                    }
                    else if (AdsNetwork.Contains(AdMediation.ADMOD) && AdmobHelper.RewardIsReady)
                    {
                        Debug.Log("[AdsManager] ShowVideoReward IRON --> ADMOD");

                        LogEvent("ad_" + AdType.Reward + "_iron_admob", ParamsBase(placementName, itemName, AdMediation.ADMOD));

                        AdmobHelper.ShowRewarded(onSuccess, placementName, itemName);
                    }
                    else
                    {
                        if (rewardToInter && IronHelper.InterIsReady)
                        {
                            Debug.Log("[AdsManager] ShowVideoReward IRON Reward --> IRON Inter");

                            LogEvent("ad_" + AdType.Reward + "_" + AdType.Inter, ParamsBase(placementName, itemName, DefaultMediation));

                            IronHelper.ShowInterstitial(onSuccess, placementName, itemName);
                        }
                        else
                        {
                            Debug.Log("[AdsManager] ShowVideoReward IRON Reward --> NOT AVAIABLE --> Try show AdUseBackup: " + GameConfig.adUseBackup + " " + AdmobHelper.IsInitInter);

                            IronHelper.ShowRewarded(onSuccess, placementName, itemName);

                            if (GameConfig.adUseBackup && AdsNetwork.Contains(AdMediation.ADMOD) && AdmobHelper.IsInitReward == false)
                            {
                                Debug.Log("[AdsManager] ShowInterstitial IRON Reward --> NOT AVAIABLE --> ADMOB Init Reward");
                                instance.StartCoroutine(AdmobHelper.DOInit(IsDebug, AdmobHelper.InitReward));
                            }
                        }
                    }
                }
                else if (DefaultMediation == AdMediation.MAX)
                {
                    if (MaxHelper.RewardIsReady && !instance.testForceBackup)
                    {
                        Debug.Log("[AdsManager] ShowVideoReward MAX --> MAX");
                        MaxHelper.ShowRewarded(onSuccess, placementName, itemName);
                    }
                    else if (AdsNetwork.Contains(AdMediation.IRON) && IronHelper.RewardIsReady)
                    {
                        Debug.Log("[AdsManager] ShowVideoReward MAX --> IRON");

                        LogEvent("ad_" + AdType.Reward + "_max_iron", ParamsBase(placementName, itemName, AdMediation.IRON));

                        IronHelper.ShowRewarded(onSuccess, placementName, itemName);
                    }
                    else if (AdsNetwork.Contains(AdMediation.ADMOD) && AdmobHelper.RewardIsReady)
                    {
                        Debug.Log("[AdsManager] ShowVideoReward MAX --> ADMOD");

                        LogEvent("ad_" + AdType.Reward + "_max_admob", ParamsBase(placementName, itemName, AdMediation.ADMOD));

                        AdmobHelper.ShowRewarded(onSuccess, placementName, itemName);
                    }
                    else
                    {
                        if (rewardToInter)
                        {
                            if (IronHelper.InterIsReady)
                            {
                                Debug.Log("[AdsManager] ShowVideoReward MAX Reward --> MAX Inter");

                                LogEvent("ad_" + AdType.Reward + "_" + AdType.Inter, ParamsBase(placementName, itemName, DefaultMediation));

                                MaxHelper.ShowInterstitial(onSuccess, placementName, itemName);
                            }
                            else
                            {
                                Debug.Log("[AdsManager] ShowVideoReward MAX Inter --> NOT AVAIABLE --> Try show AdUseBackup: " + GameConfig.adUseBackup + " " + AdmobHelper.IsInitInter);

                                MaxHelper.ShowRewarded(onSuccess, placementName, itemName);

                                if (GameConfig.adUseBackup && AdsNetwork.Contains(AdMediation.ADMOD) && AdmobHelper.IsInitReward == false)
                                {
                                    Debug.Log("[AdsManager] ShowInterstitial MAX Reward --> NOT AVAIABLE --> ADMOB Init Reward");
                                    instance.StartCoroutine(AdmobHelper.DOInit(IsDebug, AdmobHelper.InitReward));
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning("[AdsManager] ShowVideoReward --> NOT AVAIABLE ------------------------------");

                            onSuccess?.Invoke(AdEvent.ShowNotAvailable, AdType.Reward);

                            SetStatus(AdType.Reward, AdEvent.ShowNotAvailable, placementName, itemName, DefaultMediation);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("[AdsManager] ShowVideoReward --> NOT DefaultMediation ------------------------------");

                    onSuccess?.Invoke(AdEvent.ShowNotAvailable, AdType.Reward);

                    SetStatus(AdType.Reward, AdEvent.ShowNotAvailable, placementName, itemName, DefaultMediation);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                onSuccess?.Invoke(AdEvent.Exception, AdType.Reward);
            }
        }

        public static void InitBanner(BannerPos bannerPos)
        {
            if (instance == null)
            {
                Debug.LogError("[AdsManager] could not find the AdsManager object. Please ensure you have added the AdsManager Prefab to your scene.");
                return;
            }

            if (Settings.useBanner == AdMediation.IRON)
            {
                IronHelper.InitBanner(bannerPos);
            }
            else if (Settings.useBanner == AdMediation.MAX)
            {
                MaxHelper.InitBanner(bannerPos);
            }
            else if (Settings.useBanner == AdMediation.ADMOD)
            {
                AdmobHelper.InitBanner(bannerPos);
            }
        }

        public static void DestroyBanner()
        {
            if (Settings.useBanner == AdMediation.IRON)
            {
                IronHelper.DestroyBanner();
            }
            else if (Settings.useBanner == AdMediation.MAX)
            {
                MaxHelper.DestroyBanner();
            }
            else if (Settings.useBanner == AdMediation.ADMOD)
            {
                AdmobHelper.DestroyBanner();
            }
        }

        public static void ShowBanner()
        {
            if (Settings.useBanner == AdMediation.IRON)
            {
                IronHelper.ShowBanner();
            }
            else if (Settings.useBanner == AdMediation.MAX)
            {
                MaxHelper.ShowBanner();
            }
            else if (Settings.useBanner == AdMediation.ADMOD)
            {
                AdmobHelper.ShowBanner();
            }
        }

        public static void HideBanner()
        {
            if (Settings.useBanner == AdMediation.IRON)
            {
                IronHelper.HideBanner();
            }
            else if (Settings.useBanner == AdMediation.MAX)
            {
                MaxHelper.HideBanner();
            }
            else if (Settings.useBanner == AdMediation.ADMOD)
            {
                AdmobHelper.HideBanner();
            }
        }

        public static void ShowBannerMediumRectangle(string placementName, Vector2 customPosition = default)
        {
            AdmobHelper.ShowBannerMediumRectangle(placementName, customPosition);
        }

        public static void HideBannerMediumRectangle()
        {
            AdmobHelper.HideBannerMediumRectangle();
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

                    if (adType != AdType.Banner)
                    {
                        if (adEvent == AdEvent.Close)
                        {
                            LastTimeShowAd = DateTime.Now;
                        }
                        else if (adEvent == AdEvent.ShowSuccess)
                        {
                            if (adType == AdType.Inter)
                            {
                                LastTimeShowAd = DateTime.Now;
                                totalSuccess++;
                                totalInterSuccess++;
                                UserData.TotalAdInterstitial++;
                                LogEvent("ad_success", ParamsBase(placementName, itemName, mediation));
                                DataManager.Save();
                                ShowNotice(adEvent);
                            }

                            if (adType == AdType.Reward)
                            {
                                LastTimeShowAd = DateTime.Now;
                                totalSuccess++;
                                totalRewardSuccess++;
                                UserData.TotalAdRewarded++;
                                LogEvent("ad_success", ParamsBase(placementName, itemName, mediation));
                                DataManager.Save();
                                ShowNotice(adEvent);
                            }
                        }
                        else if (adType == AdType.Reward && (adEvent == AdEvent.ShowStart || adEvent == AdEvent.ShowNotAvailable || adEvent == AdEvent.ShowNoInternet))
                        {
                            ShowNotice(adEvent);
                        }
                        else if (adEvent == AdEvent.LoadNotAvaiable)
                        {
                            if (GameConfig.adUseBackup && mediation != AdMediation.ADMOD && AdsNetwork.Contains(AdMediation.ADMOD))
                            {
                                if (AdmobHelper.IsInitInter == false && adType == AdType.Inter)
                                    instance.StartCoroutine(AdmobHelper.DOInit(IsDebug, AdmobHelper.InitInter));
                                if (AdmobHelper.IsInitReward == false && adType == AdType.Reward)
                                    instance.StartCoroutine(AdmobHelper.DOInit(IsDebug, AdmobHelper.InitReward));
                            }
                        }

                        LastAdEvent = adEvent;
                        LastAdType = adType;
                        LastMediation = mediation;

                        if (adEvent == AdEvent.Load)
                            Debug.Log(mediation.ToString() + " " + adType.ToString() + " " + adEvent.ToString());
                    }

                    OnStateChanged?.Invoke(adType, adEvent, placementName, itemName);

                    LogEvent(adType, adEvent, ParamsBase(placementName, itemName, mediation));
                    LogAppsFlyer(adType, adEvent, placementName, itemName);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public static bool IsNotShowAds
        {
            get
            {
                if (instance != null && instance.removeAdsToggle != null && instance.removeAdsToggle.isOn)
                    return true;
                if (UserData != null)
                    return UserData.isVIP || UserData.isRemovedAds;
                return false;
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

        public static string InternetStatus
        {
            get
            {
                switch (Application.internetReachability)
                {
                    case NetworkReachability.ReachableViaLocalAreaNetwork:
                        return "Wifi";
                    case NetworkReachability.ReachableViaCarrierDataNetwork:
                        return "Carrier";
                    default:
                        return "None";
                }
            }
        }

        public static Dictionary<string, object> ParamsBase(string placementName = "default", string itemName = "default", AdMediation mediation = AdMediation.NONE)
        {
            return new Dictionary<string, object>
            {
                { "internet", InternetStatus},
                { "in_session", totalSuccess.ToString() },
                { "session", UserData.Session.ToString() },
                { "ad_interstitial", UserData.TotalAdInterstitial.ToString() },
                { "ad_rewarded", UserData.TotalAdRewarded.ToString() },
                { "ad_platform", mediation.ToString() },
                { "placement", placementName.ToLower() },
                { "item", itemName.ToLower() },
                { "total", (UserData.TotalAdRewarded + UserData.TotalAdInterstitial).ToString()}
            };
        }

        public static void ShowNotice(AdEvent onSuccess)
        {
            try
            {
                if (onSuccess == AdEvent.ShowNoInternet)
                    UIToast.ShowNotice("Please check your internet connection...!");
                else if (onSuccess == AdEvent.ShowNotAvailable)
                    UIToast.ShowNotice("Video not ready, please try again...!");
                else if (onSuccess == AdEvent.Exception)
                    UIToast.ShowNotice("Something wrong, please try again...!");
                else if (onSuccess == AdEvent.ShowSuccess)
                    Debug.Log("Show Ads success!");
                //UIToast.ShowNotice("Thanks for watching video...!");
                else if (onSuccess == AdEvent.ShowStart)
                    Debug.Log("Time to show ADs!");
                //UIToast.ShowLoading("Time to show ADs... please wait!");
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
                var bannerObjs = FindObjectsOfType<AdsBannerObj>();
                foreach (var i in bannerObjs)
                {
                    if (i != null)
                        i.SetActive(!IsNotShowAds);
                }

                if (instance.parentTransform == null)
                    instance.parentTransform = UIAnimManager.RootRectTransform;

                if (instance.parentTransform != null && AdsBannerAreaList != null)
                {
                    foreach (var i in AdsBannerAreaList)
                        i.SetArea(instance.bannerHeight / instance.parentTransform.rect.height, IsNotShowAds ? BannerPos.NONE : BannerPos);
                }

                if (IsNotShowAds)
                    DestroyBanner();
            }
        }

        public static void Log(string value)
        {
            if (IsDebug)
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

        public static CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
        public static void LogImpressionData(AdMediation mediation, object data, string adUnitId = null)
        {
            try
            {
                double revenue = 0;
                string ad_network = "";
                string ad_unit_name = "";
                string ad_format = "";
                string country = "";
                double lifetime_revenue = 0;
                string currency = "USD";

#if USE_MAX
                if (data != null && data is MaxSdkBase.AdInfo)
                {
                    var impressionData = data as MaxSdkBase.AdInfo;

                    ad_network = impressionData.NetworkName;
                    ad_unit_name = impressionData.AdUnitIdentifier;
                    ad_format = impressionData.Placement;
                    revenue = impressionData.Revenue;
                }
#endif

#if USE_IRON
                if (data != null && data is IronSourceImpressionData)
                {
                    var impressionData = data as IronSourceImpressionData;
                    ad_network = impressionData.adNetwork;
                    ad_unit_name = impressionData.adUnit;
                    ad_format = impressionData.instanceName;
                    revenue = (double)(impressionData.revenue != null ? impressionData.revenue : 0.0f);
                    country = impressionData.country;
                    lifetime_revenue = (double)(impressionData.lifetimeRevenue != null ? impressionData.lifetimeRevenue : 0.0f);
                }
#endif

#if USE_ADSOPEN || USE_ADMOB
                if (data != null && data is GoogleMobileAds.Api.AdValueEventArgs)
                {
                    var impressionData = data as GoogleMobileAds.Api.AdValueEventArgs;
                    if (impressionData != null || impressionData.AdValue != null)
                    {
                        revenue = impressionData.AdValue != null ? impressionData.AdValue.Value : 0.0f;
                        currency = impressionData.AdValue.CurrencyCode;
                    }
                }
#endif

#if USE_FIREBASE
                if (FirebaseManager.AnalyticStatus == FirebaseStatus.Initialized)
                {
                    FirebaseAnalytics.LogEvent("ad_impression",
                        new Parameter[] {
                        new Parameter("is_connected", RegexString(InternetStatus)),
                        new Parameter("ad_platform", RegexString(mediation)),
                        new Parameter("ad_source",RegexString( ad_network)),
                        new Parameter("ad_unit_name",RegexString( ad_unit_name)),
                        new Parameter("ad_format", RegexString(ad_format)),
                        new Parameter("value", revenue ),
                        new Parameter("currency", currency),
                        new Parameter("country", country),
                        new Parameter("lifetime_revenue", lifetime_revenue)
                    });

                    FirebaseAnalytics.LogEvent("paid_ad_impression",
                        new Parameter[] {
                        new Parameter("is_connected", RegexString(InternetStatus)),
                        new Parameter("ad_platform", RegexString(mediation)),
                        new Parameter("ad_source",RegexString( ad_network)),
                        new Parameter("ad_unit_name",RegexString( ad_unit_name)),
                        new Parameter("ad_format", RegexString(ad_format)),
                        new Parameter("value", revenue ),
                        new Parameter("currency", currency),
                        new Parameter("country", country),
                        new Parameter("lifetime_revenue", lifetime_revenue)
                    });
                }
#endif


                //#if USE_APPSFLYER
                //var afParameters = new Dictionary<string, string>
                //{
                //{ "af_currency", currency },
                //{ "af_content_id", ad_format },
                //{ "af_revenue", revenue.ToString("#0.00000", culture) },
                //{ "af_level", DataManager.UserData.totalWin.ToString() }
                //};

                //AppsFlyerSDK.AppsFlyer.sendEvent(ad_unit_name.ToLower(), afParameters);
                //#endif

                string stringLog = "ad_impression" + " " + mediation.ToString() + " " + ad_network + " " + ad_unit_name + " " + ad_format + " " + revenue.ToString("#0.00000", culture) + " " + currency.ToString();

                Debug.Log("--------> " + stringLog);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static string RegexString(object value)
        {
            if (value != null)
                return Regex.Replace(value.ToString(), @"\B[A-Z]", m => "_" + m.ToString()).ToLower().Replace("ı", "i");
            return "";
        }

        private void OnApplicationPause(bool isPaused)
        {
            Debug.Log("OnApplicationPause " + isPaused);
#if USE_IRON
            IronSource.Agent.onApplicationPause(isPaused);
#endif

            StopCoroutine(ShowWarningLostConnection());

            if (IsTimeToShowAdOpen && isPaused == false)
            {
                string itemName = "app_pause";
                if (GameConfig != null)
                {
                    if (GameConfig.timeToWaitOpenAd > 0 && AdOpen.IsReady)
                    {
                        AdOpen.ShowOpenAdIfAvailable(itemName);
                        return;
                    }
                    else
                    {
#if USE_MAXOPEN
                        Log(AdOpen.TAG + "WaitToShow: " + AdOpen.Status.ToString() + " " + AdOpen.IsReady + " ---> Check Open MAX");
#endif
                    }

#if USE_MAXOPEN
                    if (GameConfig.adUseOpenBackup && MaxHelper.OpenIsReady)
                    {
                        MaxHelper.ShowOpenAdIfAvailable(itemName);
                        return;
                    }
                    else
                    {
                        Log(MaxHelper.TAG + "WaitToShow: " + MaxHelper.StatusOpen.ToString() + " " + MaxHelper.OpenIsReady + " ---> Check Inter DEFAULT");
                    }
#endif

                    if (GameConfig.timeToWaitOpenInter > 0 && InterIsReady)
                    {
                        //ShowInterstitial((status, type) => { }, "app_pause_inter", itemName);
                    }

                    CheckLostConnection();
                }
            }
        }

        private void OnApplicationFocus(bool isFocus)
        {
            Debug.Log("OnApplicationFocus " + isFocus);
        }

        public static void CheckLostConnection()
        {
            if (instance != null && (GameStateManager.CurrentState == GameState.Ready || GameStateManager.CurrentState == GameState.Play) && UserData.TotalWin > GameConfig.isNeedInternet)
            {
                if (isCheckingLostConnection == false)
                {
                    instance.StopCoroutine(instance.ShowWarningLostConnection());
                    instance.StartCoroutine(instance.ShowWarningLostConnection());
                }
            }
        }

        protected static bool isCheckingLostConnection = false;
        public IEnumerator ShowWarningLostConnection()
        {
            if (IsConnected)
            {
                isCheckingLostConnection = false;
                yield break;
            }

            int countTimeOut = 1;
            while (IsConnected == false && countTimeOut <= 5)
            {
                isCheckingLostConnection = true;
                UIToast.ShowLoading("Connection lost, trying to reconnect.. " + countTimeOut + " time(s)", 6);
                yield return new WaitForSeconds(0.5f);
                if (IsConnected)
                {
                    isCheckingLostConnection = false;
                    UIToast.ShowNotice("Connecting to server... ");
                    yield break;
                }
                yield return new WaitForSeconds(0.5f);
                countTimeOut++;
            }

            yield return new WaitForSeconds(1f);
            isCheckingLostConnection = false;
            UIToast.ShowError("Failed connection timed out... ");

            //GameStateManager.Main();

            //PopupMessage.Show("NO INTERNET",
            //        "Some actions are"
            //        + "\n" + "NOT available OFFLINE"
            //        + "\n" + "Please check your internet connection",
            //        "OKIE", null, null);
            ;
        }

        public static void LogAppsFlyer(AdType adType, AdEvent adEvent, string placementName, string itemName)
        {
#if USE_APPSFLYER
            var param = new Dictionary<string, string>()
            {
                { "placement", placementName.ToLower() },
                { "item", itemName.ToLower() },
                { "internet", InternetStatus.ToLower()}
            };
            if (adType == AdType.Inter)
            {
                if (adEvent == AdEvent.Show)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_inters_ad_eligible", param); //Bắn lên khi gọi hàm show quảng cáo inter của game (bắn lên khi ấn nút show ads theo logic của game)
                else if (adEvent == AdEvent.ShowStart)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_inters_api_called", param); //Bắn lên nếu quảng cáo có sẵn trong game khi gọi hàm show quảng cáo (bắn lên khi ads available)
                else if (adEvent == AdEvent.ShowSuccess)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_inters_displayed", param); //Bắn lên khi ad hiện lên màn hình cho user xem (open inter)
            }
            else if (adType == AdType.Reward)
            {
                if (adEvent == AdEvent.Show)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_rewarded_ad_eligible", param); //Bắn lên khi gọi hàm show quảng cáo reward của game(bắn lên khi ấn nút show ads theo logic của game)
                else if (adEvent == AdEvent.ShowStart)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_rewarded_api_called", param); //Bắn lên nếu quảng cáo có sẵn trong game khi gọi hàm show quảng cáo (bắn lên khi ads available)
                else if (adEvent == AdEvent.ShowSuccess)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_rewarded_ad_displayed", param); //Bắn lên khi ad hiện lên màn hình cho user xem(open reward)
            }
            else if (adType == AdType.AppOpen)
            {
                if (adEvent == AdEvent.Show)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_appopen_ad_eligible", param); //Bắn lên khi gọi hàm show quảng cáo appopen của game(bắn lên khi ấn nút show ads theo logic của game)
                else if (adEvent == AdEvent.ShowStart)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_appopen_api_called", param); //Bắn lên nếu quảng cáo có sẵn trong game khi gọi hàm show quảng cáo (bắn lên khi ads available)
                else if (adEvent == AdEvent.ShowSuccess)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_appopen_ad_displayed", param); //Bắn lên khi ad hiện lên màn hình cho user xem (open appopen)
            }
            else if (adType == AdType.Banner)
            {
                if (adEvent == AdEvent.Show)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_banner_ad_eligible", param); //Bắn lên khi gọi hàm show quảng cáo small banner của game(bắn lên khi ấn nút show ads theo logic của game)
                else if (adEvent == AdEvent.ShowStart)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_banner_api_called", param); //Bắn lên nếu quảng cáo có sẵn trong game khi gọi hàm show quảng cáo (bắn lên khi ads available)
                else if (adEvent == AdEvent.ShowSuccess)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_banner_ad_displayed", param); //Bắn lên khi ad hiện lên màn hình cho user xem (open small banner)
            }
            else if (adType == AdType.Rectangle)
            {
                if (adEvent == AdEvent.Show)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_rectanggle_ad_eligible", param); //Bắn lên khi gọi hàm show quảng cáo rectangle banner của game(bắn lên khi ấn nút show ads theo logic của game)
                else if (adEvent == AdEvent.ShowStart)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_rectanggle_api_called", param); //Bắn lên nếu quảng cáo có sẵn trong game khi gọi hàm show quảng cáo (bắn lên khi ads available)
                else if (adEvent == AdEvent.ShowSuccess)
                    AppsFlyerSDK.AppsFlyer.sendEvent("af_rectanggle_ad_displayed", param); //Bắn lên khi ad hiện lên màn hình cho user xem (open rectangle banner)
            }

            if (adEvent == AdEvent.ShowSuccess)
            {
                AppsFlyerSDK.AppsFlyer.sendEvent("ad_" + adType.ToString().ToLower(), param);
            }
#endif
        }

        public static void PauseApp(bool pause)
        {
            if (pause)
            {
                SetVolumeFade(0, 0.25f);
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
                SetVolumeFade(1, 1f);
            }
        }

        public static void SetVolumeFade(float endValue, float duration)
        {
            DOTween.Kill("SetVolumeFade", true);
            var volume = AudioListener.volume;
            DOVirtual.Float(volume, endValue, duration, (v) =>
            {
                AudioListener.volume = v;
            })
            .SetId("SetVolumeFade")
            .SetUpdate(true);
        }
    }
}
