using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StarManager : MonoBehaviour
{
    [SerializeField]
    private UITextNumber number = null;
    public static UITextNumber Number { get => instance?.number; }
    [SerializeField]
    private UITextNumber maxStar = null;
    public static UITextNumber MaxStar { get => instance?.maxStar; }

    [SerializeField]
    private ParticleLockAt particle = null;
    public static ParticleLockAt Particle { get => instance?.particle; }

    //public Transform testingTfFrom;
    public Transform defaultTarget;

    private static int[] _totalStar;
    public static int[] totalStar => DataManager.UserData.totalStar;
    //{
    //    get
    //    {
    //        return _totalStar;
    //    }
    //    private set
    //    {
    //        for(int i = 0; i < DataManager.UserData.totalStar.Length; i++)
    //        {
    //            _totalStar[i] = DataManager.UserData.totalStar[i];
    //        }
    //    }
    //}

    public static int CoinByAds => DataManager.GameConfig.goldByAds;

    private static StarManager instance;

    private void Awake()
    {
        instance = this;
        //DataManager.OnLoaded += DataManager_OnLoaded;
        this.RegisterListener((int)EventID.ChooseMap, DataManager_OnLoaded);
    }

    private void DataManager_OnLoaded(object obj)
    {
        if (instance == null || Number == null)
            return;
        Number.DOAnimation(0, totalStar[DataManager.mapSelect-1], 0);
    }

    public static void Add(int numb, Transform fromTrans = null, Transform toTrans = null)
    {
        var current = totalStar[DataManager.mapSelect-1];
        totalStar[DataManager.mapSelect-1] += numb;
        if (Number != null)
        {
            if (numb > 0)
            {
                SoundManager.Play("7. Star appear");
                if (fromTrans)
                {
                    Particle.Emit(Mathf.Clamp(numb + 1, 0, 10), fromTrans, toTrans ?? instance.defaultTarget);
                }
                Number.DOAnimation(current, totalStar[DataManager.mapSelect-1], Particle == null ? 0.5f : Particle.StartLifetime * 0.5f);
                //DOVirtual.DelayedCall(Particle.StartLifetime, () => SoundManager.Play("5. Star to target"));
            }
            else
            {
                Number.DOAnimation(current, totalStar[DataManager.mapSelect-1], 0);
            }
        }
    }
}
