#if USE_ADOPEN
using GoogleMobileAds.Api;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Base.Ads.AdsManager;

namespace Base.Ads
{
    public class AdOpen : MonoBehaviour
    {
        public const string TAG = "ADOPEN ";

        public AdMediation mediation = AdMediation.APPOPEN;

        public ScreenOrientation screenOrientation = ScreenOrientation.AutoRotation;

        protected static string adUnitId = "";

        protected static string placementName = "app_open";

        public static AdOpen instance = null;

        public static bool IsInit = false;

        private void Awake()
        {
            if (instance != null)
                Destroy(gameObject);
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
#if USE_ADOPEN
            MobileAds.SetiOSAppPauseOnBackground(true);
#endif
        }

        public static IEnumerator DOInit()
        {
            CheckInstance();

            if (instance == null)
            {
                throw new Exception("AdOpen could not find the AdOpen object. Please ensure you have added the AdsManager Prefab to your scene.");
            }

            bool isHasError = false;
            float timeOut = 0f;

            try
            {
#if USE_ADOPEN
                if (TestForceBackup && !string.IsNullOrEmpty(TestDeviceId))
                {
                    List<string> deviceIds = new List<string>() { AdRequest.TestDeviceSimulator, TestDeviceId };
                    RequestConfiguration requestConfiguration = new RequestConfiguration.Builder()
                       .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.True)
                       .SetTestDeviceIds(deviceIds).build();
                    MobileAds.SetRequestConfiguration(requestConfiguration);
                }

                MobileAds.Initialize((initStatus) =>
                {
                    Log(TAG + "Initialize: DONE " + initStatus.ToString());

                    if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
                        adUnitId = Settings.openIOSUnitId?.Trim();
                    else
                        adUnitId = Settings.openAnroidUnitId?.Trim();

                    if (TestForceBackup || DebugMode.IsDebugMode)
                    {
                        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
                            adUnitId = "ca-app-pub-3940256099942544/5662855259";
                        else
                            adUnitId = "ca-app-pub-3940256099942544/3419835294";
                    }

                    Log(TAG + "AppOpenLoad: " + adUnitId + " TestForceBackup: " + TestForceBackup);

                    Load();

                    IsInit = true;
                });
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
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

                isHasError = true;
#endif
            }

            while (IsInit == false && isHasError == false && timeOut < 3)
            {
                timeOut += Time.deltaTime;
                yield return null;
            }
        }

        protected static DateTime LastTimeLoaded = new DateTime();

#if USE_ADOPEN
        private AppOpenAd ad;
#endif

        protected int reloadTimeIfError = 15;
        protected int countAutoReload = 0;
        protected int maxAutoReload = 3;

        public static AdEvent Status = AdEvent.Offer;

        public static bool IsReady
        {
            get
            {
#if USE_ADOPEN
                return instance?.ad != null;
#else
                return false;
#endif
            }
        }

        protected static void Load()
        {
#if USE_ADOPEN
            try
            {
                if (instance == null)
                {
                    LogError(TAG + "instance NULL --> return");
                    return;
                }

                if (instance.ad != null)
                {
                    Log(TAG + "Load: is Loaded --> return");
                    return;
                }

                if (string.IsNullOrEmpty(adUnitId))
                {
                    Log(TAG + "Load: adUnitId NULL or EMPTY --> return");
                    return;
                }

                Log(TAG + "Load: Status: " + Status.ToString());

                if (Status == AdEvent.Load)
                {
                    Log(TAG + "Load: is loading --> return");
                    return;
                }

                Status = AdEvent.Load;

                var loadTime = DateTime.Now;

                SetStatus(AdType.AppOpen, AdEvent.Load, placementName, placementName, instance.mediation);

                AdRequest request = new AdRequest.Builder().Build();

                if (instance.screenOrientation == ScreenOrientation.AutoRotation)
                    instance.screenOrientation = Screen.orientation;

#if UNITY_EDITOR
                System.Threading.Thread.Sleep(1000);
#endif

                // Load an app open ad for portrait orientation
                AppOpenAd.LoadAd(adUnitId, instance.screenOrientation, request, (appOpenAd, args) =>
                {
                    var logParams = ParamsBase(placementName, placementName, instance.mediation);
                    logParams.Add("load_time", (DateTime.Now - loadTime).TotalSeconds.ToString("#0.00"));

                    if (args != null && args.LoadAdError != null)
                    {
                        try
                        {
                            if (IsConnected)
                            {
                                Status = AdEvent.LoadNotAvaiable;
                                LogEvent(AdType.AppOpen, AdEvent.LoadNotAvaiable, logParams);
                            }
                            else
                            {
                                Status = AdEvent.LoadNoInternet;
                                LogEvent(AdType.AppOpen, AdEvent.LoadNoInternet, logParams);
                            }

                            if (DebugMode.IsDebugMode)
                            {
                                var error = args.LoadAdError;
                                var errorDescription = error.GetMessage();
                                LogError(TAG + "Load Failed Error: " + errorDescription);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                        finally
                        {
                            if (instance.countAutoReload < instance.maxAutoReload)
                            {
                                instance.countAutoReload++;
                                instance.Invoke("Load", instance.reloadTimeIfError);
                            }
                        }
                        return;
                    }

                    // App open ad is loaded.
                    instance.ad = appOpenAd;
                    LastTimeLoaded = DateTime.Now;
                    instance.countAutoReload = 0;
                    Status = AdEvent.LoadAvaiable;

                    SetStatus(AdType.AppOpen, AdEvent.LoadAvaiable, placementName, placementName, instance.mediation);

                    Log(TAG + "Load: isLoaded " + (DateTime.Now - loadTime).TotalSeconds.ToString("#0.00"));
                });
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                DestroyAd();
            }
#endif
        }

        public static void ShowOpenAdIfAvailable(string placement = "app_open")
        {
#if USE_ADOPEN
            placementName = placement;

            if (instance.ad != null && (DateTime.Now - LastTimeLoaded).TotalHours >= 3.5)
            {
                Log(TAG + "ShowAdIfAvailable: OutTime: " + (DateTime.Now - LastTimeLoaded).TotalHours.ToString("0.0") + " --> return");
                DestroyAd();
            }

            if (IsInit == false)
            {
                Log(TAG + "ShowAdIfAvailable: IsInit: " + IsInit + " --> return");
            }

            if (IsTimeToShowAdOpen == false)
            {
                Log(TAG + "ShowAdIfAvailable: IsTimeToShowAdOpen: " + IsTimeToShowAdOpen + " --> return");
            }

            if (LastAdEvent == AdEvent.ShowStart || LastAdEvent == AdEvent.Show || LastAdEvent == AdEvent.Close || LastAdEvent == AdEvent.ShowSuccess)
            {
                Log(TAG + "ShowAdIfAvailable: LastAdEvent: " + LastAdEvent.ToString() + " --> return");
            }

            Status = AdEvent.Show;
            SetStatus(AdType.AppOpen, AdEvent.Show, placementName, placementName, instance.mediation);

            if (instance.ad != null)
            {
                SetStatus(AdType.AppOpen, AdEvent.ShowStart, placementName, placementName, instance.mediation);
                instance.Show(placement);
            }
            else
            {
                if (IsConnected)
                {
                    Status = AdEvent.ShowNotAvailable;
                    SetStatus(AdType.AppOpen, AdEvent.ShowNotAvailable, placementName, placementName, instance.mediation);
                }
                else
                {
                    Status = AdEvent.ShowNoInternet;
                    SetStatus(AdType.AppOpen, AdEvent.ShowNoInternet, placementName, placementName, instance.mediation);
                }

                Log(TAG + "ShowAdIfAvailable: Status: " + Status.ToString() + " --> Load");
                instance.countAutoReload = 0;
                Load();
            }
#endif
        }

        protected void Show(string placement = "app_open")
        {
#if USE_ADOPEN
            try
            {
                placementName = placement;
                Status = AdEvent.ShowStart;
                SetStatus(AdType.AppOpen, AdEvent.ShowStart, placementName, placementName, instance.mediation);
                ad.OnAdDidDismissFullScreenContent += OnClose;
                ad.OnAdFailedToPresentFullScreenContent += OnShowFailed;
                ad.OnAdDidPresentFullScreenContent += HandleAdDidPresentFullScreenContent;
                ad.OnAdDidRecordImpression += HandleAdDidRecordImpression;
                ad.OnPaidEvent += HandlePaidEvent;

                PauseApp(true);
                ad.Show();
            }
            catch (Exception ex)
            {
                LogException(ex);

                SetStatus(AdType.AppOpen, AdEvent.Exception, placementName, placementName, instance.mediation);

                DestroyAd();
                Load();
            }
#endif
        }

#if USE_ADOPEN
        protected static void OnClose(object sender, EventArgs args)
        {
            Status = AdEvent.Close;
            PauseApp(false);
            Log(TAG + "Closed app open ad");
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.

            LastTimeShowAd = DateTime.Now;
            SetStatus(AdType.AppOpen, AdEvent.Close, placementName, placementName, instance.mediation);

            DestroyAd();
            Load();
        }

        protected static void OnShowFailed(object sender, AdErrorEventArgs args)
        {
            Status = AdEvent.ShowFailed;
            PauseApp(false);
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.

            if (args != null && args.AdError != null)
            {
                var errorMessage = args.AdError.GetMessage();
                LogError(TAG + "Failed to present the ad " + errorMessage);
            }

            SetStatus(AdType.AppOpen, AdEvent.ShowFailed, placementName, placementName, instance.mediation);

            DestroyAd();
            Load();
        }

        protected static void HandleAdDidPresentFullScreenContent(object sender, EventArgs args)
        {
            Status = AdEvent.ShowSuccess;
            Debug.Log(TAG + "Displayed app open ad Success");

            SetStatus(AdType.AppOpen, AdEvent.ShowSuccess, placementName, placementName, instance.mediation);
        }

        protected static void HandleAdDidRecordImpression(object sender, EventArgs args)
        {
            Debug.Log(TAG + "Recorded ad impression");
        }

        protected static void HandlePaidEvent(object sender, AdValueEventArgs args)
        {
            var adValue = args.AdValue;
            if (adValue != null)
            {
                var currency = adValue.CurrencyCode;
                var revenue = adValue.Value;

                if (UnityMainThreadDispatcher.Exists())
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        Debug.LogFormat(TAG + "Received paid event. (currency: {0}, value: {1}", currency, revenue);
                        LogImpressionData(AdMediation.APPOPEN, args, placementName);
                    });
                }
            }
        }

        protected static void DestroyAd()
        {
            try
            {
                if (instance != null && instance.ad != null)
                {
                    instance.ad.OnAdDidDismissFullScreenContent -= OnClose;
                    instance.ad.OnAdFailedToPresentFullScreenContent -= OnShowFailed;
                    instance.ad.OnAdDidPresentFullScreenContent -= HandleAdDidPresentFullScreenContent;
                    instance.ad.OnAdDidRecordImpression -= HandleAdDidRecordImpression;
                    instance.ad.OnPaidEvent -= HandlePaidEvent;
                    instance.ad.Destroy();
                    instance.ad = null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
#endif
    }
}

