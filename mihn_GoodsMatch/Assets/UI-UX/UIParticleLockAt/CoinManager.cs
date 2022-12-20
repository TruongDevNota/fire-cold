using System;
using UnityEngine;
using DG.Tweening;
using Base.Ads;

public class CoinManager : MonoBehaviour
{
    [SerializeField]
    private UITextNumber number = null;
    public static UITextNumber Number { get => instance?.number; }


    [SerializeField]
    private ParticleLockAt particle = null;
    public static ParticleLockAt Particle { get => instance?.particle; }

    //public Transform testingTfFrom;
    public Transform defaultTarget;
    public static int totalCoin
    {
        get => DataManager.UserData.totalCoin;
        private set => DataManager.UserData.totalCoin = value;
    }

    public static int CoinByAds => DataManager.GameConfig.goldByAds;

    private static CoinManager instance;

    private void Awake()
    {
        instance = this;
        DataManager.OnLoaded += DataManager_OnLoaded;
    }

    private void DataManager_OnLoaded(GameData gameData)
    {
        Number.DOAnimation(0, totalCoin, 0);
    }

    public static void Add(int numb, Transform fromTrans = null, Transform toTrans = null)
    {
        var current = totalCoin;
        totalCoin += numb;
        if (Number != null)
        {
            if (numb > 0)
            {
                if (fromTrans)
                {
                    Particle.Emit(Mathf.Clamp(numb + 1,0,10), fromTrans, toTrans ?? instance.defaultTarget);
                    SoundManager.Play("7. Star appear");
                }
                Number.DOAnimation(current, totalCoin, Particle == null ? 0.5f : Particle.StartLifetime * 0.5f);
                DOVirtual.DelayedCall(Particle.StartLifetime *0.8f, () => SoundManager.Play("5. Star to target"));
            }
            else
            {
                Number.DOAnimation(current, totalCoin, 0);
            }
        }
    }
    public static void GetByAds(Transform transform, string placement, Action<AdEvent> status = null)
    {
#if USE_IRON || USE_MAX || USE_ADMOB
        AdsManager.ShowVideoReward((onSuccess, adType) =>
        {
            if (onSuccess == AdEvent.ShowSuccess || DataManager.GameConfig.isAdsByPass)
            {
                Add(CoinByAds, transform);
            }
            else
            {
                AdsManager.ShowNotice(onSuccess);
            }
            status?.Invoke(onSuccess);
        }, placement, "coin");
#endif
    }
    public static string CoinByAdsFormat(int fontSize = 16)
    {
        return "<size=" + fontSize + ">+</size>" + CoinByAds;
    }

    public void Ins_GetByAds(Transform transform)
    {
        GetByAds(transform, name);
    }
    public void Test(int numb)
    {
        Add(numb);
    }

#if UNITY_EDITOR
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Test(100000);
        }
    }
#endif
}
