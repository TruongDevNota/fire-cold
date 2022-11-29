#if USE_ADMOB
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
#endif
using System;
using System.Collections;
using UnityEngine;
using static AdsManager;

public class AdmobHelper : AdsBase<AdmobHelper>
{
    public static void Init(bool isDebug)
    {
        TAG = "[ADMOB] ";
#if USE_ADMOB
        if (instance)
        {
            try
            {
                Application.RequestAdvertisingIdentifierAsync((string advertisingId, bool trackingEnabled, string error) => Debug.Log(TAG + "advertisingId " + advertisingId + " " + trackingEnabled + " " + error));
              
                RequestConfiguration requestConfiguration =
                    new RequestConfiguration.Builder()
                    .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.Unspecified)
                    .build();
                    
                MobileAds.SetiOSAppPauseOnBackground(true);
                MobileAds.SetRequestConfiguration(requestConfiguration);
                MobileAds.Initialize(initStatus => {
                    
                    MobileAdsEventExecutor.ExecuteInUpdate(() => {
                        instance.RewardInit();
                        instance.InterInit();
                        instance.InitBanner();
                        Debug.Log(TAG + "Init: Completed - UserId: " + SystemInfo.deviceUniqueIdentifier);
                    });
                });
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        else
        {
            Debug.LogError(TAG + "instance NULL");
        }
#endif
    }

#if USE_ADMOB
    private AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder()
            .AddTestDevice(AdRequest.TestDeviceSimulator)
            .AddKeyword("game")
            //.SetGender(Gender.Male)
            //.SetBirthday(new DateTime(1985, 1, 1))
            //.TagForChildDirectedTreatment(false)
            //.AddExtra("color_bg", "9B30FF")
            .Build();
    }
#endif

    #region VIDEO REWARDED
#if USE_ADMOB
    private RewardedAd rewardedAd;
#endif
    public override void RewardInit()
    {
#if USE_ADMOB
        try
        {
            rewardedAd = new RewardedAd(RewardUnitId);
            rewardedAd.OnAdLoaded += (s, e) => RewardOnReady();
            rewardedAd.OnAdFailedToLoad += (s, e) => RewardOnLoadFailed(e.Message, s);
            rewardedAd.OnAdFailedToShow += (s, e) => RewardOnLoadFailed(e.Message, s);
            rewardedAd.OnUserEarnedReward += (s, e) => RewardOnShowSuscess(e.Type);
            rewardedAd.OnAdClosed += (s, e) => RewardOnClose();

            Debug.Log(TAG + "RewardInit: " + RewardUnitId);
            RewardLoad();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
#endif
    }

    public override void RewardLoad()
    {
#if USE_ADMOB
        if (IsConnected)
        {
            SetStatus(AdType.VideoReward, AdEvent.Load);
            RewardIsReady = rewardedAd.IsLoaded();
            if (!RewardIsReady)
                rewardedAd.LoadAd(CreateAdRequest());
        }
        else
        {
            SetStatus(AdType.VideoReward, AdEvent.NotInternet);
        }
#endif
    }

    public override void RewardOnReady()
    {
#if USE_ADMOB
        RewardIsReady = rewardedAd.IsLoaded();
        RewardCountTry = 0;
        Debug.Log(TAG + "RewardOnReady " + RewardIsReady);
#endif
    }

    public static void ShowRewarded(Action<AdEvent> onSuccess, string placementName, string itemId, float waitAvaibale = 1f)
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
        {
            onSuccess?.Invoke(AdEvent.Success);
            return;
        }

        if (instance)
            instance.StartCoroutine(WaitToShowRewarded(() => ShowRewardedDoNotWait(onSuccess, placementName, itemId), waitAvaibale));
        else
            DebugMode.LogError(TAG + "instance NULL");
    }

    private static IEnumerator WaitToShowRewarded(Action onAvaiable, float waitAvaibale)
    {
        float elapsedTime = waitAvaibale;
        while (!RewardIsReady && elapsedTime > 0)
        {
            elapsedTime -= 0.25f;
            yield return new WaitForSeconds(0.25f);
        }
        onAvaiable?.Invoke();
    }

    public static void ShowRewardedDoNotWait(Action<AdEvent> onSuccess, string placementName, string itemId)
    {
#if USE_ADMOB
        if (instance)
        {
            RewardIsReady = instance.rewardedAd.IsLoaded();
            if (RewardIsReady)
            {
                Debug.Log(TAG + "ShowRewarded -> Ready");
                instance.RewardShow(onSuccess, placementName, itemId);
                instance.rewardedAd.Show();
            }
            else
            {
                SetStatus(AdType.VideoReward, AdEvent.NotAvailable, placementName, itemId);
                onSuccess?.Invoke(AdEvent.NotAvailable);
                RewardCountTry = 0;
                instance.RewardLoad();
            }
        }
        else
        {
            DebugMode.LogError(TAG + "instance NULL");
        }
#endif
    }
    #endregion

    #region INTERSTITIAL
#if USE_ADMOB
    private InterstitialAd interstitial;
#endif
    public override void InterInit()
    {
#if USE_ADMOB
        try
        {
            interstitial = new InterstitialAd(InterUnitId);
            interstitial.OnAdLoaded += (s, e) => InterOnReady();
            interstitial.OnAdFailedToLoad += (s, e) => InterOnLoadFailed(s);
            interstitial.OnAdOpening += (s, e) => InterOnShowSuscess();
            interstitial.OnAdLeavingApplication += (s, e) => InterOnClick();
            interstitial.OnAdClosed += (s, e) => InterOnClose();
            Debug.Log(TAG + "InterInit: " + InterUnitId);
            InterLoad();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
#endif
    }

    public override void InterLoad()
    {
#if USE_ADMOB
        if (IsConnected)
        {
            SetStatus(AdType.Interstitial, AdEvent.Load);
            InterIsReady = interstitial.IsLoaded();
            if (!InterIsReady)
                interstitial.LoadAd(CreateAdRequest());
        }
        else
        {
            SetStatus(AdType.Interstitial, AdEvent.NotInternet);
        }
#endif
    }

    public override void InterOnReady()
    {
#if USE_ADMOB
        InterIsReady = interstitial.IsLoaded();
        InterCountTry = 0;
        Debug.Log(TAG + "InterOnReady " + InterIsReady);
#endif
    }

    public static void ShowInterstitial(Action<AdEvent> onSuccess, string placementName, string itemId, float waitAvaibale = 0.25f)
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
        float elapsedTime = waitAvaibale;
        while (!InterIsReady && elapsedTime > 0)
        {
            elapsedTime -= 0.25f;
            yield return new WaitForSeconds(0.25f);
        }
        onAvaiable?.Invoke();
    }

    public static void ShowInterstitialDoNotWait(Action<AdEvent> onSuccess, string placementName, string itemId)
    {
#if USE_ADMOB
        if (instance)
        {
            InterIsReady = instance.interstitial.IsLoaded();
            if (InterIsReady)
            {
                Debug.Log(TAG + "ShowInterstitial -> Ready");
                instance.InterShow(onSuccess, placementName, itemId);
                instance.interstitial.Show();
            }
            else
            {
                SetStatus(AdType.Interstitial, AdEvent.NotAvailable, placementName, itemId);
                onSuccess?.Invoke(AdEvent.NotAvailable);
                InterCountTry = 0;
                instance.InterLoad();
            }
        }
        else
        {
            onSuccess?.Invoke(AdEvent.Fail);
            DebugMode.LogError(TAG + "instance NULL");
        }
#endif
    }
    #endregion



    #region BANNER
#if USE_ADMOB
    private BannerView bannerView;
#endif
    public override void InitBanner()
    {
#if USE_ADMOB
        bannerView = new BannerView(BannerUnitId, AdSize.SmartBanner, AdPosition.Bottom);

        bannerView.OnAdLoaded += (sender, args) => {
            Debug.Log(TAG + "BannerOnReady ");
        };
        bannerView.OnAdFailedToLoad += (sender, args) => {
            Debug.Log(TAG + "BannerOnLoadFailed");
        };
#endif
    }

    public override void LoadBanner()
    {
#if USE_ADMOB
        bannerView.LoadAd(CreateAdRequest());
#endif
    }
    public static void DestroyBaner()
    {
#if USE_ADMOB
        if (instance.bannerView != null)
        {
            instance.bannerView.Destroy();
        }
#endif
    }
    #endregion

    protected override void PauseApp(bool pause)
    {
        base.PauseApp(pause);
    }
}
