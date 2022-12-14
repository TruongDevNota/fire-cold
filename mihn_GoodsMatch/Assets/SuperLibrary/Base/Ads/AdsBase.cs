using System;
using UnityEngine;
using static Base.Ads.AdsManager;

namespace Base.Ads
{
    public abstract class AdsBase<T> : MonoBehaviour where T : AdsBase<T>
    {
        public static string TAG = "[ADS] ";
        public AdMediation mediation = AdMediation.NONE;

        [Header("Interstitial")]
        protected static string interPlacemenetName = "DefaultInterstitial";
        protected static string interItemName = "Default";
        private Action<AdEvent, AdType> onInterShowSuccess = null;


        public static bool IsInitInter = false;

        protected static int interCountMax = 3;
        public static int interCountTry = 0;

        [Header("VideoRewared")]
        protected static string rewardPlacementName = "DefaultRewardedVideo";
        protected static string rewardItemName = "Default";
        protected Action<AdEvent, AdType> onRewardShowSuccess = null;
        protected AdEvent rewardEvent = AdEvent.NotAvailable;

        public static bool IsInitReward = false;

        protected static int rewardCountMax = 3;
        protected static int rewardCountTry = 0;

        #region VIDEO REWARED
        public abstract void InitVideoRewarded();

        public abstract void RewardLoad();

        public abstract void RewardOnReady(bool avaiable);

        protected virtual void RewardOnLoadFailed(object obj)
        {
            PauseApp(false);
            SetStatus(AdType.VideoReward, AdEvent.LoadFailed, rewardPlacementName, rewardItemName, mediation);
            onRewardShowSuccess?.Invoke(AdEvent.LoadFailed, AdType.VideoReward);
            onRewardShowSuccess = null;

            if (rewardCountTry < rewardCountMax)
            {
                rewardCountTry++;
                Invoke("RewardLoad", 15);
            }
        }

        protected virtual void RewardOnShowFailed(object obj)
        {
            PauseApp(false);
            SetStatus(AdType.VideoReward, AdEvent.ShowFailed, rewardPlacementName, rewardItemName, mediation);
            onRewardShowSuccess?.Invoke(AdEvent.ShowFailed, AdType.VideoReward);
            onRewardShowSuccess = null;

            RewardLoad();
        }

        public virtual void RewardOnClick(object obj = null)
        {
            SetStatus(AdType.VideoReward, AdEvent.Click, rewardPlacementName, rewardItemName, mediation);
        }

        public virtual void RewardOnClose()
        {
            PauseApp(false);

            if (rewardEvent == AdEvent.Success)
            {
                if (UnityMainThreadDispatcher.Exists())
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        onRewardShowSuccess?.Invoke(AdEvent.Success, AdType.VideoReward);
                        onRewardShowSuccess = null;
                    });
                }
                else
                {
                    try
                    {
                        onRewardShowSuccess?.Invoke(AdEvent.Success, AdType.VideoReward);
                        onRewardShowSuccess = null;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }

            SetStatus(AdType.VideoReward, AdEvent.Close, rewardPlacementName, rewardItemName, mediation);

            rewardEvent = AdEvent.Load;
            RewardLoad();
        }

        public virtual void RewardOnShowSuscess(object obj)
        {
            rewardEvent = AdEvent.Success;
            SetStatus(AdType.VideoReward, AdEvent.Success, rewardPlacementName, rewardItemName, mediation);
        }

        protected void RewardShow(Action<AdEvent, AdType> status)
        {
            rewardEvent = AdEvent.ShowStart;
            rewardCountTry = 0;
            onRewardShowSuccess = status;
            SetStatus(AdType.VideoReward, AdEvent.ShowStart, rewardPlacementName, rewardItemName, mediation);
            PauseApp(true);
        }
        #endregion

        #region INTERSTITIAL
        public abstract void InitInterstitial();

        public abstract void InterLoad();

        public abstract void InterOnReady();

        protected virtual void InterOnLoadFailed(object obj = null)
        {
            PauseApp(false);
            SetStatus(AdType.Interstitial, AdEvent.LoadFailed, interPlacemenetName, interItemName, mediation);
            onInterShowSuccess?.Invoke(AdEvent.LoadFailed, AdType.Interstitial);
            onInterShowSuccess = null;

            if (interCountTry < interCountMax)
            {
                interCountTry++;
                Invoke("InterLoad", 15);
            }
        }

        protected virtual void InterOnShowFailed(object obj = null)
        {
            PauseApp(false);
            SetStatus(AdType.Interstitial, AdEvent.ShowFailed, interPlacemenetName, interItemName, mediation);
            onInterShowSuccess?.Invoke(AdEvent.ShowFailed, AdType.Interstitial);
            onInterShowSuccess = null;

            InterLoad();
        }

        public virtual void InterOnShowSuscess()
        {
            SetStatus(AdType.Interstitial, AdEvent.Success, interPlacemenetName, interItemName, mediation);
            onInterShowSuccess?.Invoke(AdEvent.Success, AdType.Interstitial);
            onInterShowSuccess = null;
        }

        public virtual void InterOnClick()
        {
            SetStatus(AdType.Interstitial, AdEvent.Click, interPlacemenetName, interItemName, mediation);
        }

        public virtual void InterOnClose()
        {
            PauseApp(false);
            SetStatus(AdType.Interstitial, AdEvent.Close, interPlacemenetName, interItemName, mediation);
            InterLoad();
        }

        protected void InterShow(Action<AdEvent, AdType> status)
        {
            PauseApp(true);
            onInterShowSuccess = status;
            SetStatus(AdType.Interstitial, AdEvent.ShowStart, interPlacemenetName, interItemName, mediation);
        }

        private float lastTimeScale = 1f;
        private void PauseApp(bool pause)
        {
            if (pause)
            {
                lastTimeScale = Time.timeScale;
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = lastTimeScale;
            }
        }
        #endregion
    }
}
public enum AdType
{
    Interstitial = 0,
    VideoReward = 1,
    Banner = 2,
    AppOpen = 3
}
public enum AdEvent
{
    Load = 0,
    Avaiable = 1,
    Offer = 2,
    LoadFailed = 3,
    Show = 4,
    ShowStart = 5,
    ShowFailed = 6,
    Success = 7,
    Click = 8,
    Close = 9,
    Cancel = 10,
    NotInternet = 11,
    NotAvailable = 12,
    NotTimeToShow = 13,
    Exception = 14
}
public enum AdMediation
{
    NONE = 0,
    IRON = 1,
    MAX = 2,
    ADMOD = 3,
    APPOPEN = 4
}
public enum BannerPos
{
    NONE = 0,
    TOP = 1,
    BOTTOM = 2
}