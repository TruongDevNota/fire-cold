using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AdsManager;

public class IronHelper : AdsBase<IronHelper>
{
    public static void Init(bool isDebug)
    {
        TAG = "[IRON] ";
#if USE_IRON
        if (instance)
        {
            try
            {
                Application.RequestAdvertisingIdentifierAsync((string advertisingId, bool trackingEnabled, string error) => Debug.Log(TAG + "advertisingId " + advertisingId + " " + trackingEnabled + " " + error));
                IronSource.Agent.init(AppKeyId, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
                IronSource.Agent.setAdaptersDebug(isDebug);
                string userId = IronSource.Agent.getAdvertiserId();
                if (string.IsNullOrEmpty(userId))
                    userId = SystemInfo.deviceUniqueIdentifier;
                IronSource.Agent.setUserId(userId);
                if (isDebug) IronSource.Agent.validateIntegration();
                Debug.Log(TAG + "Init: " + AppKeyId + " - UserId: " + SystemInfo.deviceUniqueIdentifier + " AdvertiserId: " + IronSource.Agent.getAdvertiserId());
                instance.RewardInit();
                instance.InterInit();
                instance.InitBanner();
            }
            catch (Exception ex)
            {
                Debug.LogError(TAG + "Init: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError(TAG + "instance NULL");
        }
#endif
    }

    #region VIDEO REWARDED
    public override void RewardInit()
    {
#if USE_IRON
        IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += (s) => RewardOnReady();
        IronSourceEvents.onRewardedVideoAdLoadFailedDemandOnlyEvent += (s, e) => RewardOnLoadFailed(s, e);
        IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardOnShowSuscess;
        IronSourceEvents.onRewardedVideoAdShowFailedEvent += (s) => RewardOnLoadFailed("", s);
        IronSourceEvents.onRewardedVideoAdClickedEvent += RewardOnClick;
        IronSourceEvents.onRewardedVideoAdClosedEvent += RewardOnClose;
        Debug.Log(TAG + "RewardInit");
        RewardLoad();
#endif
    }

    public override void RewardLoad()
    {
#if USE_IRON
        if (IsConnected)
        {
            SetStatus(AdType.VideoReward, AdEvent.Load);
            if (!RewardIsReady)
                RewardIsReady = IronSource.Agent.isRewardedVideoAvailable();
        }
        else
        {
            SetStatus(AdType.VideoReward, AdEvent.NotInternet);
        }
#endif
    }

    public override void RewardOnReady()
    {
#if USE_IRON
        RewardIsReady = IronSource.Agent.isRewardedVideoAvailable();
        RewardCountTry = 0;
        Debug.Log(TAG + "RewardOnReady " + RewardIsReady);
#endif
    }

    public static void ShowRewarded(Action<AdEvent> onSuccess, string placementName, string itemId, float waitAvaibale = 3f)
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
        {
            onSuccess?.Invoke(AdEvent.Success);
            return;
        }

        if (instance)
            instance.StartCoroutine(WaitToShowRewarded(() => ShowRewardedDoNotWait(onSuccess, placementName, itemId), waitAvaibale));
        else
            Debug.LogError(TAG + "instance NULL");
    }

    private static IEnumerator WaitToShowRewarded(Action onAvaiable, float waitAvaibale)
    {
        ShowNoticeOnLoading();
        float elapsedTime = waitAvaibale;
        while (!RewardIsReady && elapsedTime > 0)
        {
            elapsedTime -= 0.25f;
            yield return new WaitForSeconds(0.25f);
        }
        yield return new WaitForSeconds(0.25f);
        onAvaiable?.Invoke();
    }

    public static void ShowRewardedDoNotWait(Action<AdEvent> onSuccess, string placementName, string itemId)
    {
        if (instance)
        {
#if USE_IRON
            RewardIsReady = IronSource.Agent.isRewardedVideoAvailable();
            //if (TestAutoSwitch)
            //{
            //    RewardIsReady = false;
            //    InterIsReady = true;
            //}

            if (RewardIsReady)
            {
                Debug.Log(TAG + "ShowRewarded -> Ready");
                instance.RewardShow(onSuccess, placementName, itemId);
                IronSource.Agent.showRewardedVideo(RewardUnitId);
            }
            //else if (!RewardIsReady && InterIsReady && AutoRewardToInter)
            //{
            //    Debug.Log(TAG + "ShowRewarded -> Not Ready -> InterShow");
            //    instance.InterShow(onSuccess, placementName, itemId);
            //    IronSource.Agent.showInterstitial(InterUnitId);
            //    AnalyticsManager.LogEvent("Ad_RewardToInter", new Dictionary<string, object> { { "placementName", placementName }, { "itemId", itemId } });
            //}
            else
            {
                Debug.LogError(TAG + "ShowRewarded -> Not Ready");
                SetStatus(AdType.VideoReward, AdEvent.NotAvailable, placementName, itemId);
                onSuccess?.Invoke(AdEvent.NotAvailable);
                RewardCountTry = 0;
                instance.RewardLoad();
            }
#else
            onSuccess?.Invoke(AdEvent.Fail);
#endif
        }
        else
        {
            onSuccess?.Invoke(AdEvent.Fail);
            Debug.LogError(TAG + "instance NULL");
        }
    }
    #endregion

    #region INTERSTITIAL
    public override void InterInit()
    {
#if USE_IRON
        IronSourceEvents.onInterstitialAdReadyEvent += InterOnReady;
        IronSourceEvents.onInterstitialAdLoadFailedEvent += InterOnLoadFailed;
        IronSourceEvents.onInterstitialAdShowSucceededEvent += InterOnShowSuscess;
        IronSourceEvents.onInterstitialAdShowFailedEvent += InterOnLoadFailed;
        IronSourceEvents.onInterstitialAdClickedEvent += InterOnClick;
        IronSourceEvents.onInterstitialAdClosedEvent += InterOnClose;
        Debug.Log(TAG + "InterInit");
        InterLoad();
#endif
    }

    public override void InterLoad()
    {
#if USE_IRON
        if (IsConnected)
        {
            SetStatus(AdType.Interstitial, AdEvent.Load);
            InterIsReady = IronSource.Agent.isInterstitialReady();
            if (!InterIsReady)
                IronSource.Agent.loadInterstitial();
        }
        else
        {
            SetStatus(AdType.Interstitial, AdEvent.NotInternet);
        }
#endif
    }

    public override void InterOnReady()
    {
#if USE_IRON
        InterIsReady = IronSource.Agent.isInterstitialReady();
        InterCountTry = 0;
        Debug.Log(TAG + "InterOnReady " + InterIsReady);
#endif
    }

    public static void ShowInterstitial(Action<AdEvent> onSuccess, string placementName, string itemId, float waitAvaibale = 1f)
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
        {
            onSuccess?.Invoke(AdEvent.Success);
            return;
        }

        if (instance)
            instance.StartCoroutine(WaitToShowInterstitial(() => ShowInterstitialDoNotWait(onSuccess, placementName, itemId), waitAvaibale));
        else
            Debug.LogError(TAG + "instance NULL");
    }

    private static IEnumerator WaitToShowInterstitial(Action onAvaiable, float waitAvaibale)
    {
        ShowNoticeOnLoading();
        float elapsedTime = waitAvaibale;
        while (!InterIsReady && elapsedTime > 0)
        {
            elapsedTime -= 0.25f;
            yield return new WaitForSeconds(0.25f);
        }
        yield return new WaitForSeconds(0.25f);
        onAvaiable?.Invoke();
    }

    public static void ShowInterstitialDoNotWait(Action<AdEvent> onSuccess, string placementName, string itemId)
    {
        if (instance)
        {
#if USE_IRON
            InterIsReady = IronSource.Agent.isInterstitialReady();

            //if (TestAutoSwitch)
            //{
            //    RewardIsReady = true;
            //    InterIsReady = false;
            //}

            if (InterIsReady)
            {
                Debug.Log(TAG + "ShowInterstitial -> Ready");
                instance.InterShow(onSuccess, placementName, itemId);
                IronSource.Agent.showInterstitial(InterUnitId);
            }
            //else if (!InterIsReady && RewardIsReady && AutoInterToReward)
            //{
            //    Debug.Log(TAG + "ShowInterstitial -> Not Ready -> RewardShow");
            //    instance.RewardShow(onSuccess, placementName, itemId);
            //    IronSource.Agent.showRewardedVideo(RewardUnitId);
            //    AnalyticsManager.LogEvent("Ad_InterToReward", new Dictionary<string, object> { { "placementName", placementName }, { "itemId", itemId } });
            //}
            else
            {
                Debug.LogError(TAG + "ShowInterstitial -> Not Ready");
                SetStatus(AdType.Interstitial, AdEvent.NotAvailable, placementName, itemId);
                onSuccess?.Invoke(AdEvent.NotAvailable);
                InterCountTry = 0;
                instance.InterLoad();
            }
#else
            onSuccess?.Invoke(AdEvent.Fail);
#endif
        }
        else
        {
            onSuccess?.Invoke(AdEvent.Fail);
            Debug.LogError(TAG + "instance NULL");
        }
    }
    #endregion

    #region BANNER
    public override void InitBanner()
    {
        if (!IsRemoveAds && UseBanner)
        {
#if USE_IRON
            Debug.Log(TAG + "BannerInit");

            IronSourceEvents.onBannerAdLoadFailedEvent += OnBannerLoadFailed;
            IronSourceEvents.onBannerAdLoadedEvent += OnBannerAdLoadedEvent;

            LoadBanner();
#endif
        }
    }

    private bool bannerIsLoaded = false;
    public override void LoadBanner()
    {
        if (!bannerIsLoaded)
        {
#if USE_IRON
            SetStatus(AdType.Banner, AdEvent.Load);
            IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM, "Default");
#endif
        }
    }

        private void OnBannerAdLoadedEvent()
    {
        SetStatus(AdType.Banner, AdEvent.Success, "", "");
        bannerIsLoaded = true;
    }

#if USE_IRON
    private void OnBannerLoadFailed(IronSourceError obj)
    {
        bannerIsLoaded = false;
        if (BannerCountTry < bannerCountMax)
        {
            Debug.LogError(TAG + "OnBannerLoadFailed re-trying in 3 seconds count " + BannerCountTry + "/" + bannerCountMax);
            BannerCountTry++;
            Invoke("LoadBanner", 3);
        }
    }
#endif

    public static void DestroyBaner()
    {
#if USE_IRON
        IronSource.Agent.destroyBanner();
#endif
    }
    #endregion

    protected override void PauseApp(bool pause)
    {
        base.PauseApp(pause);
    }
}
