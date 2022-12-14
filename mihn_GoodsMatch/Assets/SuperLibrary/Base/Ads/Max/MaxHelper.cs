using System;
using UnityEngine;
using static Base.Ads.AdsManager;

namespace Base.Ads
{
    public class MaxHelper : AdsBase<MaxHelper>
    {
        protected static string SDK_KEY = "M4GLwqezVT2WDo75OWFGOV873pVg6-3S3Kpz8Rxe_-9CnHI9oXPB2TI5LpnRnqvr8hpH8kw7i4KTMcc891KCad";
        protected static string interId = "e5b40675256cfb7a";
        protected static string rewardId = "e80642ae408db2eb";
        protected static string bannerId = "6c47f4926533ad4a";

        public static bool InterIsReady
        {
            get
            {
#if USE_MAX
                return MaxSdk.IsInterstitialReady(interId);
#else
                return false;
#endif
            }
        }

        public static bool RewardIsReady
        {
            get
            {
#if USE_MAX
                return MaxSdk.IsRewardedAdReady(rewardId);
#else
                return false;
#endif
            }
        }

        protected static MaxHelper instance = null;

        public void Awake()
        {
            instance = this;
            TAG = "MAX ";
        }

        public static void Init(bool isDebug)
        {
#if USE_MAX
            if (instance)
            {
                try
                {
                    SDK_KEY = Settings.maxSdkKey?.Trim();

                    if (Application.platform == RuntimePlatform.Android)
                    {
                        bannerId = Settings.maxAndroidBannerUnitId?.Trim();
                        interId = Settings.maxAndroidInterUnitId?.Trim();
                        rewardId = Settings.maxAndroidRewaredUnitId?.Trim();
                    }
                    else
                    {
                        bannerId = Settings.maxIOSBannerUnitId?.Trim();
                        interId = Settings.maxIOSInterUnitId?.Trim();
                        rewardId = Settings.maxIOSRewaredUnitId?.Trim();
                    }

                    if (isDebug)
                    {
                        MaxSdk.ShowMediationDebugger();
                    }

                    MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
                    {

                        DG.Tweening.DOVirtual.DelayedCall(1, () =>
                        {
                            instance.InitVideoRewarded();
                        });

                        DG.Tweening.DOVirtual.DelayedCall(2, () =>
                        {
                            instance.InitInterstitial();
                        });

                        if (Settings.useBanner == AdMediation.MAX)
                        {
                            MaxSdkCallbacks.Banner.OnAdLoadedEvent += (s, o) => OnBannerAdLoadedEvent();
                            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += (s, e) => OnBannerAdLoadFailedEvent();
                        }

                        Log(TAG + "Init DONE " + SDK_KEY);
                    };

                    MaxSdk.SetSdkKey(SDK_KEY);
                    MaxSdk.InitializeSdk();
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

        #region VIDEO REWARDED
        public override void InitVideoRewarded()
        {
#if USE_MAX
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += (s, o) => RewardOnReady(true);
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += (s, e) => RewardOnLoadFailed(e);
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += (s, r, o) => RewardOnShowSuscess(o);
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += (s, e, o) => RewardOnShowFailed(e);
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += (s, o) => RewardOnClick();
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += (s, o) => RewardOnClose();
            MaxSdk.LoadRewardedAd(rewardId);
            IsInitReward = true;
            Log(TAG + "InitVideoRewarded " + rewardId);
#endif
        }

        public override void RewardLoad()
        {
#if USE_MAX
            if (!RewardIsReady)
            {
                SetStatus(AdType.VideoReward, AdEvent.Load, rewardPlacementName, rewardItemName, mediation);
                MaxSdk.LoadRewardedAd(rewardId);
            }
#endif
        }

        public override void RewardOnReady(bool avaiable)
        {
#if USE_MAX
            SetStatus(AdType.VideoReward, AdEvent.Avaiable, rewardPlacementName, rewardItemName, mediation);
            rewardCountTry = 0;
#endif
        }

        public static void ShowRewarded(Action<AdEvent, AdType> onSuccess, string placementName, string itemName = "")
        {
            if (instance)
            {
                try
                {
#if USE_MAX
                    if (!IsInitReward) instance.InitVideoRewarded();

                    rewardPlacementName = placementName;
                    rewardItemName = itemName;

                    if (RewardIsReady)
                    {
                        Log(TAG + "ShowRewarded -> Ready");
                        instance.RewardShow(onSuccess);
                        MaxSdk.ShowRewardedAd(rewardId, rewardPlacementName);
                    }
                    else
                    {
                        LogError(TAG + "ShowRewarded -> Not Ready");
                        SetStatus(AdType.VideoReward, AdEvent.NotAvailable, rewardPlacementName, rewardItemName, instance.mediation);
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
            var logParams = ParamsBase(interPlacemenetName, interItemName, mediation);
            try
            {
#if USE_MAX
                if (obj != null && obj is MaxSdkBase.ErrorInfo errorMax)
                {
                    logParams.Add("errorCode", errorMax.Code.ToString());
                    logParams.Add("errorDescription", errorMax.Message);
                    logParams.Add("error", errorMax.ToString());


                    LogError(TAG + "InterOnLoadFailed Error: " + errorMax.ToString() + " re-trying in 15 seconds " + interCountTry + "/" + interCountMax);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            LogEvent(AdType.VideoReward, AdEvent.LoadFailed, logParams);

            base.RewardOnLoadFailed(obj);
        }

        protected override void RewardOnShowFailed(object obj)
        {
            var logParams = ParamsBase(interPlacemenetName, interItemName, mediation);
            try
            {
#if USE_MAX
                if (obj != null && obj is MaxSdkBase.ErrorInfo errorMax)
                {
                    logParams.Add("errorCode", errorMax.Code.ToString());
                    logParams.Add("errorDescription", errorMax.Message);
                    logParams.Add("error", errorMax.ToString());


                    LogError(TAG + "InterOnShowFailed Error: " + errorMax.ToString());

                    if (errorMax.Code == MaxSdkBase.ErrorCode.NoNetwork)
                    {
                        onRewardShowSuccess?.Invoke(AdEvent.NotInternet, AdType.Interstitial);
                    }
                    else
                    {
                        onRewardShowSuccess?.Invoke(AdEvent.ShowFailed, AdType.Interstitial);
                    }
                }
                else
                {
                    onRewardShowSuccess?.Invoke(AdEvent.ShowFailed, AdType.Interstitial);
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
#if USE_MAX
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += (s, o) => InterOnReady();
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += (s, e) => InterOnLoadFailed(e);
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += (s, o) => InterOnShowSuscess();
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += (s, e, o) => InterOnShowFailed(e);
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += (s, o) => InterOnClick();
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += (s, o) => InterOnClose();
            MaxSdk.LoadInterstitial(interId);
            IsInitInter = true;
            Log(TAG + "InitInterstitial " + interId);
#endif
        }

        public override void InterLoad()
        {
#if USE_MAX
            if (!InterIsReady)
            {
                SetStatus(AdType.Interstitial, AdEvent.Load, interPlacemenetName, interItemName, mediation);
                MaxSdk.LoadInterstitial(interId);
            }
#endif
        }

        public override void InterOnReady()
        {
#if USE_MAX           
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
                    if (!IsInitInter) instance.InitInterstitial();

                    interPlacemenetName = placementName;
                    interItemName = itemName;

#if USE_MAX
                    if (InterIsReady)
                    {
                        Log(TAG + "ShowInterstitial -> Ready");
                        instance.InterShow(onSuccess);
                        MaxSdk.ShowInterstitial(interId, interPlacemenetName);
                    }
                    else
                    {
                        LogError(TAG + "ShowInterstitial -> Not Ready");
                        SetStatus(AdType.Interstitial, AdEvent.NotAvailable, rewardPlacementName, rewardItemName, instance.mediation);
                        onSuccess?.Invoke(AdEvent.NotAvailable, AdType.Interstitial);
                        interCountTry = 0;
                        instance.InterLoad();
                    }
#else
                    onSuccess?.Invoke(AdEvent.ShowFailed, AdType.Interstitial);
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
#if USE_MAX
                if (obj != null && obj is MaxSdkBase.ErrorInfo errorMax)
                {
                    logParams.Add("errorCode", errorMax.Code.ToString());
                    logParams.Add("errorDescription", errorMax.Message);
                    logParams.Add("error", errorMax.ToString());


                    LogError(TAG + "InterOnLoadFailed Error: " + errorMax.ToString() + " re-trying in 15 seconds " + interCountTry + "/" + interCountMax);
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
#if USE_MAX
                if (obj != null && obj is MaxSdkBase.ErrorInfo errorMax)
                {
                    logParams.Add("errorCode", errorMax.Code.ToString());
                    logParams.Add("errorDescription", errorMax.Message);
                    logParams.Add("error", errorMax.ToString());


                    LogError(TAG + "InterOnShowFailed Error: " + errorMax.ToString());
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
#if USE_MAX
            if (AdsManager.BannerPos == BannerPos.BOTTOM)
                MaxSdk.CreateBanner(bannerId, MaxSdkBase.BannerPosition.BottomCenter);
            else if (AdsManager.BannerPos == BannerPos.TOP)
                MaxSdk.CreateBanner(bannerId, MaxSdkBase.BannerPosition.TopCenter);

            Log(TAG + "InitBanner " + interId);
#endif
        }

        public static void DestroyBaner()
        {
#if USE_MAX
            MaxSdk.DestroyBanner(bannerId);
#endif
        }

        public static void HideBanner()
        {
#if USE_MAX
            MaxSdk.HideBanner(bannerId);
#endif
        }

        public static void ShowBanner()
        {
#if USE_MAX
            MaxSdk.ShowBanner(bannerId);
#endif
        }

        protected static bool tryLoadBanner = false;
        protected static void OnBannerAdLoadFailedEvent()
        {
#if USE_MAX
            if (instance != null)
                SetStatus(AdType.Banner, AdEvent.LoadFailed, "default", "default", instance.mediation);
            if (tryLoadBanner == false)
            {
                tryLoadBanner = true;
                InitBanner();
            }
#endif
        }

        protected static void OnBannerAdLoadedEvent()
        {
#if USE_MAX
            MaxSdk.ShowBanner(bannerId);
            if (instance != null)
                SetStatus(AdType.Banner, AdEvent.Success, "default", "default", instance.mediation);
#endif
        }
        #endregion
    }
}
