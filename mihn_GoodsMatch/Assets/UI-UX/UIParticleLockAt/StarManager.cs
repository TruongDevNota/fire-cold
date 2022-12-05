using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarManager : MonoBehaviour
{
    [SerializeField]
    private UITextNumber number = null;
    public static UITextNumber Number { get => instance?.number; }

    [SerializeField]
    private ParticleLockAt particle = null;
    public static ParticleLockAt Particle { get => instance?.particle; }

    //public Transform testingTfFrom;
    public Transform defaultTarget;

    public static int totalStar
    {
        get => DataManager.UserData.totalStar;
        private set => DataManager.UserData.totalStar = value;
    }

    public static int CoinByAds => DataManager.GameConfig.goldByAds;

    private static StarManager instance;

    private void Awake()
    {
        instance = this;
        DataManager.OnLoaded += DataManager_OnLoaded;
    }

    private void DataManager_OnLoaded(GameData gameData)
    {
        Number.DOAnimation(0, totalStar, 0);
    }

    public static void Add(int numb, Transform fromTrans = null, Transform toTrans = null)
    {
        var current = totalStar;
        totalStar += numb;
        if (Number != null)
        {
            if (numb > 0)
            {
                if (fromTrans)
                {
                    Particle.Emit(Mathf.Clamp(numb + 1, 0, 10), fromTrans, toTrans ?? instance.defaultTarget);
                }
                Number.DOAnimation(current, totalStar, Particle == null ? 0.5f : Particle.StartLifetime * 0.5f);
            }
            else
            {
                Number.DOAnimation(current, totalStar, 0);
            }
        }
    }
}
