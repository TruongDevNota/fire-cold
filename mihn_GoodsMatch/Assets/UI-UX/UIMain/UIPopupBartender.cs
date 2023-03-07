using Base.Ads;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupBartender : MonoBehaviour
{
    [SerializeField] UIAnimation anim;
    [SerializeField] Button btn_Next;
    [SerializeField] Button btn_Skip;
    [SerializeField] Button btn_UnlockByAds;

    private void Awake()
    {
        this.RegisterListener((int)EventID.OnModeBartenderUnlocked, OnShow);
    }

    private void Start()
    {
        btn_Next?.onClick.AddListener(BtnNextHandle);
        btn_Skip?.onClick.AddListener(OnSkip);
        btn_UnlockByAds?.onClick.AddListener(BtnUnlockByAdsHandle);
    }

    public void OnShow(object obj)
    {
        btn_Skip.gameObject.SetActive(false);
        btn_Next.gameObject.SetActive(DataManager.UserData.level >= DataManager.GameConfig.levelsToUnlockBartender - 1);
        btn_UnlockByAds.gameObject.SetActive(DataManager.UserData.level < DataManager.GameConfig.levelsToUnlockBartender - 1);
        anim.Show(null, onCompleted: () =>
        {
            DOVirtual.DelayedCall(2f, () => btn_Skip.gameObject.SetActive(true));
        });
    }

    private void BtnNextHandle()
    {
        SoundManager.Play(GameConstants.sound_Button_Clicked);
        GameStateManager.Idle(true);
        anim.Hide();
    }

    private void BtnUnlockByAdsHandle()
    {
        SoundManager.Play(GameConstants.sound_Button_Clicked);
        AdsManager.ShowVideoReward((e, t) =>
        {
            if (e == AdEvent.ShowSuccess || DataManager.GameConfig.isAdsByPass)
            {
                DataManager.UserData.isModeBartenderSuguested = true;
                DataManager.Save();
                anim.Hide(() =>
                {
                    DataManager.currGameMode = eGameMode.Bartender;
                    GameStateManager.LoadGame(null);
                });
            }
        }, "Unlock_Bartender_with_ads");
    }

    private void OnSkip()
    {
        if(GameStateManager.CurrentState != GameState.Idle)
            GameStateManager.LoadGame(null);
        anim.Hide();
    }
}
