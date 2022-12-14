#if USE_ADSOPEN
using GoogleMobileAds.Api;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_IOS
using System.Collections.Generic;
using Unity.Advertisement.IosSupport;
#endif
using UnityEngine;
using static Base.Ads.AdsManager;

namespace Base.Ads
{
    public class AdsOpen : MonoBehaviour
    {
        public const string TAG = "ADSOPEN ";

        public AdMediation mediation = AdMediation.APPOPEN;

        public ScreenOrientation screenOrientation = ScreenOrientation.AutoRotation;

        protected static string adUnitId = "";

        protected static string placementName = "app_open";

        public static AdsOpen instance = null;

        private static bool IsInit = false;

        public bool autoShowAtStart = false;

        private static DateTime startLoadTime = DateTime.Now;

        private void Awake()
        {
            if (instance != null)
                Destroy(gameObject);
            instance = this;
            DontDestroyOnLoad(instance.gameObject);
        }

        private void Start()
        {
#if USE_ADSOPEN
            if (Settings.initAdsOpenAtStart)
                Init();
#endif
        }

        public static void Init()
        {
            if (instance == null)
            {
                throw new Exception("AdsManager could not find the AdsManager object. Please ensure you have added the AdsManager Prefab to your scene.");
            }

            try
            {
#if USE_ADSOPEN

                if (Application.platform == RuntimePlatform.Android)
                    adUnitId = Settings.openAnroidUnitId?.Trim();
                else
                    adUnitId = Settings.openIOSUnitId?.Trim();

                Log(TAG + "Load Start");
                instance.Load(instance.autoShowAtStart);

                startLoadTime = DateTime.Now;
#endif
                IsInit = true;
            }
            catch (Exception ex)
            {
                LogError(TAG + "Init: " + ex.Message);
#if USE_FIREBASE
                try
                {
                    LogEvent("ad_init_exception", new Dictionary<string, object>
                    {
                        { "errorDescription", ex.Message },
                        { "mediation", TAG }
                    });
                }
                catch (Exception e)
                {
                    LogError(TAG + "Log Firebase: " + e.Message);
                }
#endif
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (IsInit == true && pause == false && LastAdEvent != AdEvent.ShowStart && LastAdEvent != AdEvent.Success && LastAdEvent != AdEvent.Close)
            {
#if USE_ADSOPEN
                ShowAdIfAvailable("pause");
#endif
            }
        }

        public static IEnumerator WaitToShow(double time, string placementName = "app_open")
        {

            double loadAdsIn = (DateTime.Now - startLoadTime).TotalSeconds;
            while (loadAdsIn > time)
            {
                loadAdsIn = (DateTime.Now - startLoadTime).TotalSeconds;
                yield return new WaitForSeconds(0.125f);
            }

#if USE_ADSOPEN
            ShowAdIfAvailable(placementName);
#endif
            yield return new WaitForSeconds(0.125f);

            while (IsShowing)
            {
                yield return new WaitForSeconds(0.125f);
            }

            Cancel();

        }

        public static void Cancel()
        {
            IsTimeOut = true;
        }


        public static bool IsTimeOut = false;
        public static bool IsLoading = false;
        public static bool IsShowing = false;

#if USE_ADSOPEN
        private AppOpenAd ad;
        public static bool IsAdAvailable
        {
            get
            {
                return instance?.ad != null;
            }
        }
#endif

        public static bool IsTimeToShow
        {
            get
            {
                float timePlayToShowOpenAds = 5f;
                if (AdsManager.GameConfig != null)
                    timePlayToShowOpenAds = AdsManager.GameConfig.timePlayToShowAds;
                double lastTimeShowAd = (DateTime.Now - LastTimeShowAd).TotalSeconds;
                bool isTimeToShow = lastTimeShowAd >= timePlayToShowOpenAds;
                Log(TAG + "IsTimeToShowAds: " + isTimeToShow + " LastTimeShowAd: " + lastTimeShowAd.ToString("#0.0") + " timePlayToShowOpenAds: " + timePlayToShowOpenAds.ToString("#0.0"));
                return isTimeToShow;
            }
        }


#if USE_ADSOPEN
        protected int reloadTimeIfError = 15;
        protected int countAutoReload = 0;
        protected int maxAutoReload = 3;
        public void Load(bool showIfAvaible = false)
        {
            try
            {
                Log(TAG + "Load: ShowIfAvaible " + showIfAvaible + " ScreenOrientation: " + Screen.orientation.ToString());

                IsTimeOut = false;

                if (ad != null || IsLoading || IsShowing)
                    return;

                if (IsLoading == false)
                    IsLoading = true;


                var loadTime = DateTime.Now;

                LogEvent(AdType.AppOpen, AdEvent.Load, ParamsBase(placementName, placementName, mediation));

                AdRequest request = new AdRequest.Builder().Build();

                if (screenOrientation == ScreenOrientation.AutoRotation)
                    screenOrientation = Screen.orientation;

                // Load an app open ad for portrait orientation
                AppOpenAd.LoadAd(adUnitId, screenOrientation, request, (appOpenAd, error) =>
                {
                    var logParams = ParamsBase(placementName, placementName, mediation);
                    logParams.Add("load_time", (DateTime.Now - loadTime).TotalSeconds.ToString("#0.0"));

                    IsLoading = false;

                    if (error != null)
                    {
                        try
                        {
                            var loadError = error.LoadAdError;
                            if (loadError != null)
                            {                       // Handle the error.
                                LogError(string.Format(TAG + "Load: Failed to load the ad. (error: {0} - {1} in {2} id {3}, auto reload {4}/{5} in {6}s)", loadError.GetCode(), loadError.GetMessage(), (DateTime.Now - loadTime).TotalSeconds.ToString("#0.0"), adUnitId, countAutoReload, maxAutoReload, reloadTimeIfError));
                                logParams.Add("errorCode", loadError.GetCode().ToString());
                                logParams.Add("errorDescription", loadError.GetMessage());
                            }
                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }

                        LogEvent(AdType.AppOpen, AdEvent.LoadFailed, logParams);

                        if (countAutoReload < maxAutoReload)
                        {
                            countAutoReload++;
                            DG.Tweening.DOVirtual.DelayedCall(reloadTimeIfError, () => { Load(); });
                        }
                        return;
                    }

                    // App open ad is loaded.
                    ad = appOpenAd;

                    countAutoReload = 0;

                    LogEvent(AdType.AppOpen, AdEvent.Avaiable, logParams);

                    Log(TAG + "Load: isLoaded " + (DateTime.Now - loadTime).TotalSeconds.ToString("#0.0"));

                    if (showIfAvaible)
                        StartCoroutine(WaitRequest(() => ShowAdIfAvailable()));
                });
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                ad = null;
                IsShowing = false;
                IsLoading = false;
            }
        }


        private IEnumerator WaitRequest(Action onDone = null)
        {
#if UNITY_IOS
            while (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
                yield return null;
#endif
            yield return null;
            onDone?.Invoke();
        }

        public static void ShowAdIfAvailable(string placement = "app_open")
        {
            Log(TAG + "ShowAdIfAvailable: " + IsAdAvailable + " IsTimeOut: " + IsTimeOut + " isShowingAd: " + IsShowing + " IsTimeToShow: " + IsTimeToShow + " placement: " + placement);
            placementName = placement;

            if (IsShowing == true)
            {
                return;
            }

            if (IsTimeOut)
            {
                IsTimeOut = false;
                return;
            }

            if (IsAdAvailable == false)
            {
                if (IsLoading == false)
                    instance.Load();
                return;
            }

            if (IsTimeToShow == false)
            {
                return;
            }

            instance.Show(placement);
        }

        public void Show(string placement = "app_open")
        {
            try
            {
                ad.OnAdDidDismissFullScreenContent += OnClose;
                ad.OnAdFailedToPresentFullScreenContent += OnShowFailed;
                ad.OnAdDidPresentFullScreenContent += HandleAdDidPresentFullScreenContent;
                ad.OnAdDidRecordImpression += HandleAdDidRecordImpression;
                ad.OnPaidEvent += HandlePaidEvent;
                ad.Show();

                LogEvent(AdType.AppOpen, AdEvent.ShowStart, ParamsBase(placement, placementName, mediation));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                ad = null;
                IsShowing = false;
                IsLoading = false;
                LogEvent(AdType.AppOpen, AdEvent.Exception, ParamsBase(placement, placementName, mediation));
            }
        }

        private void OnClose(object sender, EventArgs args)
        {
            Log(TAG + "Closed app open ad");
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            ad = null;
            IsShowing = false;
            IsLoading = false;
            LastTimeShowAd = DateTime.Now;
            LogEvent(AdType.AppOpen, AdEvent.Close, ParamsBase(placementName, placementName, mediation));
            Load();
        }

        private void OnShowFailed(object sender, AdErrorEventArgs args)
        {
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.  
            Debug.LogFormat(TAG + "Failed to present the ad (reason: {0})", args.AdError.GetMessage());
            ad = null;
            IsShowing = false;
            IsLoading = false;
            LogEvent(AdType.AppOpen, AdEvent.ShowFailed, ParamsBase(placementName, placementName, mediation));
            Load();
        }

        private void HandleAdDidPresentFullScreenContent(object sender, EventArgs args)
        {
            Debug.Log(TAG + "Displayed app open ad Success");
            IsShowing = true;
            LogEvent(AdType.AppOpen, AdEvent.Success, ParamsBase(placementName, placementName, mediation));
        }

        private void HandleAdDidRecordImpression(object sender, EventArgs args)
        {
            Debug.Log(TAG + "Recorded ad impression");
        }

        private void HandlePaidEvent(object sender, AdValueEventArgs args)
        {
            Debug.LogFormat(TAG + "Received paid event. (currency: {0}, value: {1}", args.AdValue.CurrencyCode, args.AdValue.Value);
        }
#endif
    }
}

