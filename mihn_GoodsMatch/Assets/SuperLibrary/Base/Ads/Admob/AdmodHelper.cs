using System;
using UnityEngine;
using static Base.Ads.AdsManager;

namespace Base.Ads
{
    public class AdmodHelper : AdsBase<AdmodHelper>
    {
        protected static string appKey = "UnSupport";

        public static AdmodHelper instance = null;

        public static bool InterIsReady
        {
            get
            {
#if USE_ADMOB
                return IronSource.Agent.isInterstitialReady();
#else
                return false;
#endif
            }
        }

        public static bool RewardIsReady
        {
            get
            {
#if USE_ADMOB
                return IronSource.Agent.isRewardedVideoAvailable();
#else
                return false;
#endif
            }
        }

        protected void Awake()
        {
            instance = this;
            TAG = "ADMOB ";
        }

        public static void Init(bool isDebug)
        {
#if USE_ADMOB
            if (instance)
            {
                try
                {
                    if (Application.platform == RuntimePlatform.Android)
                        appKey = Settings.ironAndroidAppKey?.Trim();
                    else if (Application.platform == RuntimePlatform.IPhonePlayer)
                        appKey = Settings.ironIOSAppKey?.Trim();
                    else
                        appKey = Settings.ironAndroidAppKey?.Trim();

                    interCountMax = Settings.autoRetryMax;
                    rewardCountMax = Settings.autoRetryMax;

                    if (isDebug)
                    {
                        IronSource.Agent.validateIntegration();
                        IronSource.Agent.setAdaptersDebug(isDebug);
                    }

                    IronSource.Agent.shouldTrackNetworkState(true);

                    IronSourceEvents.onImpressionSuccessEvent += ImpressionSuccessEvent;

                    if (Settings.useBanner == AdMediation.IRON)
                    {
                        IronSource.Agent.init(appKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
                        IronSourceEvents.onBannerAdLoadedEvent += OnBannerAdLoadedEvent;
                        IronSourceEvents.onBannerAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
                    }
                    else
                        IronSource.Agent.init(appKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL);

                    Log(TAG + "Init: " + appKey + " - deviceUniqueIdentifier: " + SystemInfo.deviceUniqueIdentifier + " AdvertiserId: " + IronSource.Agent.getAdvertiserId());

                    DG.Tweening.DOVirtual.DelayedCall(1, () =>
                    {
                        instance.InitVideoRewarded();
                    });

                    DG.Tweening.DOVirtual.DelayedCall(2, () =>
                    {
                        instance.InitInterstitial();
                    });


                    Log(TAG + "Init DONE");
                }
                catch (Exception ex)
                {
                    LogError(TAG + "Init: " + ex.Message);
#if USE_FIREBASE
                    try
                    {
                        Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_init_exception", new Firebase.Analytics.Parameter[]
                        {
                            new Firebase.Analytics.Parameter("errorDescription", ex.Message),
                            new Firebase.Analytics.Parameter("mediation", TAG)
                        });
                    }
                    catch (Exception e)
                    {
                        LogError(TAG + "Log Firebase: " + e.Message);
                    }
#endif
                }
            }
            else
            {
                LogError(TAG + "instance NULL");
            }
#endif
        }

#if USE_ADMOB
        private static void ImpressionSuccessEvent(IronSourceImpressionData impressionData)
        {
            Log(TAG + "ImpressionSuccessEvent allData: " + impressionData.allData);

#if USE_FIREBASE
            try
            {
                var eventParams = new Firebase.Analytics.Parameter[]
                {
                new Firebase.Analytics.Parameter("adNetwork", impressionData.adNetwork),
                new Firebase.Analytics.Parameter("adUnit", impressionData.adUnit),
                new Firebase.Analytics.Parameter("country", impressionData.country),
                new Firebase.Analytics.Parameter("revenue", impressionData.revenue?.ToString("#0.000")),
                new Firebase.Analytics.Parameter("lifetimeRevenue", impressionData.lifetimeRevenue?.ToString("#0.000")),
                new Firebase.Analytics.Parameter("placement", impressionData.placement)
                };

                Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", eventParams);
            }
            catch (Exception ex)
            {
                Firebase.Crashlytics.Crashlytics.LogException(ex);
            }
#endif
        }
#endif

        #region VIDEO REWARDED
        public override void InitVideoRewarded()
        {
#if USE_ADMOB
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardOnReady;
            IronSourceEvents.onRewardedVideoAdLoadFailedEvent += RewardOnLoadFailed;
            IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardOnShowSuscess;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardOnShowFailed;
            IronSourceEvents.onRewardedVideoAdClickedEvent += RewardOnClick;
            IronSourceEvents.onRewardedVideoAdClosedEvent += RewardOnClose;
            IronSource.Agent.loadRewardedVideo();
            IsInitReward = true;
            Log(TAG + "InitVideoRewarded " + appKey);
#endif
        }

        public override void RewardLoad()
        {
#if USE_ADMOB
            if (!RewardIsReady)
            {
                SetStatus(AdType.VideoReward, AdEvent.Load, rewardPlacementName, rewardItemName, mediation);
                IronSource.Agent.loadRewardedVideo();
            }
#endif
        }

        public override void RewardOnReady(bool avaiable)
        {
#if USE_ADMOB
            rewardCountTry = 0;
            Debug.Log(TAG + " RewardOnReady " + RewardIsReady);
#endif
        }

        public static void ShowRewarded(Action<AdEvent, AdType> onSuccess, string placementName, string itemName = "")
        {
            if (instance)
            {
                try
                {
#if USE_ADMOB
                    if (!IsInitReward) instance.InitVideoRewarded();

                    rewardPlacementName = placementName;
                    rewardItemName = itemName;

                    if (RewardIsReady)
                    {
                        Log(TAG + "ShowRewarded -> Ready");
                        instance.RewardShow(onSuccess);
                        IronSource.Agent.showRewardedVideo(rewardPlacementName);
                    }
                    else
                    {
                        LogError(TAG + "ShowRewarded -> Not Ready");
                        SetStatus(AdType.VideoReward, AdEvent.NotAvailable, placementName, itemName, instance.mediation);
                        onSuccess?.Invoke(AdEvent.NotAvailable, AdType.VideoReward);
                        rewardCountTry = 0;
                        instance.RewardLoad();
                    }
#else
                    onSuccess?.Invoke(AdEvent.ShowFailed, AdType.VideoReward);
#endif
                }
                catch (Exception ex)
                {
                    instance.RewardOnShowFailed(null);
                    LogException(ex);
                }
            }
            else
            {
                instance.RewardOnShowFailed(null);
                LogError(TAG + "instance NULL");
            }
        }

        protected override void RewardOnLoadFailed(object obj)
        {
            base.RewardOnLoadFailed(obj);

            var logParams = ParamsBase(interPlacemenetName, interItemName, mediation);

            try
            {
#if USE_ADMOB
                if (obj != null && obj is IronSourceError errorIron)
                {
                    logParams.Add("errorCode", errorIron.getErrorCode().ToString());
                    logParams.Add("errorDescription", errorIron.getDescription());
                    logParams.Add("error", errorIron.ToString());

                    LogError(TAG + "InterOnLoadFailed Error: " + errorIron.ToString() + " re-trying in 15 seconds " + interCountTry + "/" + interCountMax);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            LogEvent(AdType.VideoReward, AdEvent.LoadFailed, logParams);
        }

        protected override void RewardOnShowFailed(object obj)
        {
            var logParams = ParamsBase(interPlacemenetName, interItemName, mediation);
            try
            {
#if USE_ADMOB
                if (obj != null && obj is IronSourceError errorIron)
                {
                    logParams.Add("errorCode", errorIron.getErrorCode().ToString());
                    logParams.Add("errorDescription", errorIron.getDescription());
                    logParams.Add("error", errorIron.ToString());

                    LogError(TAG + "InterOnShowFailed Error: " + errorIron.ToString());

                    if (errorIron.getCode() == 520)
                    {
                        onRewardShowSuccess?.Invoke(AdEvent.NotInternet, AdType.VideoReward);
                    }
                    else
                    {
                        onRewardShowSuccess?.Invoke(AdEvent.ShowFailed, AdType.VideoReward);
                    }
                }
                else
                {
                    onRewardShowSuccess?.Invoke(AdEvent.ShowFailed, AdType.VideoReward);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            LogEvent(AdType.VideoReward, AdEvent.ShowFailed, logParams);

            base.RewardOnShowFailed(obj);
        }
        #endregion

        #region INTERSTITIAL
        public override void InitInterstitial()
        {
#if USE_ADMOB
            IronSourceEvents.onInterstitialAdReadyEvent += InterOnReady;
            IronSourceEvents.onInterstitialAdLoadFailedEvent += InterOnLoadFailed;
            IronSourceEvents.onInterstitialAdShowSucceededEvent += InterOnShowSuscess;
            IronSourceEvents.onInterstitialAdShowFailedEvent += InterOnShowFailed;
            IronSourceEvents.onInterstitialAdClickedEvent += InterOnClick;
            IronSourceEvents.onInterstitialAdClosedEvent += InterOnClose;
            IronSource.Agent.loadInterstitial();
            IsInitInter = true;
            Log(TAG + "InitInterstitial " + appKey);
#endif
        }

        public override void InterLoad()
        {
#if USE_ADMOB
            if (!InterIsReady)
            {
                SetStatus(AdType.Interstitial, AdEvent.Load, interPlacemenetName, interItemName, mediation);
                IronSource.Agent.loadInterstitial();
            }
#endif
        }

        public override void InterOnReady()
        {
#if USE_ADMOB
            SetStatus(AdType.Interstitial, AdEvent.Avaiable, interPlacemenetName, interItemName, mediation);
            interCountTry = 0;
#endif
        }

        public static void ShowInterstitial(Action<AdEvent, AdType> onSuccess, string placementName, string itemName)
        {
            if (instance)
            {
                try
                {
                    interPlacemenetName = placementName;
                    interItemName = itemName;

                    if (!IsInitInter) instance.InitInterstitial();

#if USE_ADMOB
                    if (IronSource.Agent.isInterstitialReady())
                    {
                        Log(TAG + "ShowInterstitial -> Ready");
                        instance.InterShow(onSuccess);
                        IronSource.Agent.showInterstitial(interPlacemenetName);
                    }
                    else
                    {
                        LogError(TAG + "ShowInterstitial -> Not Ready");
                        SetStatus(AdType.Interstitial, AdEvent.NotAvailable, interPlacemenetName, interItemName, instance.mediation);
                        onSuccess?.Invoke(AdEvent.NotAvailable, AdType.Interstitial);
                        interCountTry = 0;
                        instance.InterLoad();
                    }
#else
                    onSuccess?.Invoke(AdEvent.ShowFailed, AdType.VideoReward);
#endif
                }
                catch (Exception ex)
                {
                    instance.InterOnShowFailed(null);
                    LogException(ex);
                }
            }
            else
            {
                instance.InterOnShowFailed(null);
                LogError(TAG + "instance NULL");
            }
        }

        protected override void InterOnLoadFailed(object obj = null)
        {
            var logParams = ParamsBase(interPlacemenetName, interItemName, mediation);

            try
            {
#if USE_ADMOB
                if (obj != null && obj is IronSourceError errorIron)
                {
                    logParams.Add("errorCode", errorIron.getErrorCode().ToString());
                    logParams.Add("errorDescription", errorIron.getDescription());
                    logParams.Add("error", errorIron.ToString());

                    LogError(TAG + "InterOnLoadFailed Error: " + errorIron.ToString() + " re-trying in 15 seconds " + interCountTry + "/" + interCountMax);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            LogEvent(AdType.Interstitial, AdEvent.LoadFailed, logParams);

            base.InterOnLoadFailed(obj);
        }

        protected override void InterOnShowFailed(object obj = null)
        {
            var logParams = ParamsBase(interPlacemenetName, interItemName, mediation);

            try
            {
#if USE_ADMOB
                if (obj != null && obj is IronSourceError errorIron)
                {
                    logParams.Add("errorCode", errorIron.getErrorCode().ToString());
                    logParams.Add("errorDescription", errorIron.getDescription());
                    logParams.Add("error", errorIron.ToString());

                    LogError(TAG + "InterOnShowFailed Error: " + errorIron.ToString());
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            LogEvent("ad_" + AdType.Interstitial + "show_error", logParams);

            base.InterOnShowFailed(obj);
        }
        #endregion

        #region BANNER
        public static void InitBanner()
        {
#if USE_ADMOB
            SetStatus(AdType.Banner, AdEvent.Load, "default", "default", instance.mediation);
            if (AdsManager.BannerPos == BannerPos.BOTTOM)
                IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
            else if (AdsManager.BannerPos == BannerPos.TOP)
                IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.TOP);
            Log(TAG + "InitBanner " + appKey);
#endif
        }

        public static void DestroyBanner()
        {
#if USE_ADMOB
            IronSource.Agent.destroyBanner();
#endif
        }

        public static void HideBanner()
        {
#if USE_ADMOB
            IronSource.Agent.hideBanner();
#endif
        }

        public static void ShowBanner()
        {
#if USE_ADMOB
            IronSource.Agent.displayBanner();
#endif
        }

        protected static bool tryLoadBanner = false;
        protected static void OnBannerAdLoadFailedEvent(object obj)
        {
#if USE_ADMOB
            if (instance != null)
                SetStatus(AdType.Banner, AdEvent.LoadFailed, "default", "default", instance.mediation);
            if (tryLoadBanner == false)
            {
                tryLoadBanner = true;
                InitBanner();
            }
#endif
        }

        private static void OnBannerAdLoadedEvent()
        {
#if USE_ADMOB
            IronSource.Agent.displayBanner();
            if (instance != null)
                SetStatus(AdType.Banner, AdEvent.Success, "default", "default", instance.mediation);
#endif
        }
        #endregion
    }
}
