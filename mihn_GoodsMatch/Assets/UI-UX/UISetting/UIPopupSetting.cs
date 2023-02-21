using Base.Ads;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupSetting : MonoBehaviour
{
    [SerializeField]
    UIAnimation anim;
    [SerializeField] 
    GameObject panelButtonsIngame;
    [SerializeField]
    Button homeBtn;
    [SerializeField]
    Button restartBtn;
    [SerializeField]
    Button continueBtn;
    [SerializeField]
    Text txt_version;
    [SerializeField]
    Text txt_header;

    private void Start()
    {
        homeBtn?.onClick.AddListener(OnHomeBtnClick);
        restartBtn?.onClick.AddListener(OnRestartBtnClick);
        continueBtn?.onClick.AddListener(OnContinueBtnClick);
    }

    public void OnShow()
    {
        panelButtonsIngame?.SetActive(GameStateManager.CurrentState != GameState.Idle);
        txt_version?.gameObject.SetActive(GameStateManager.CurrentState == GameState.Idle);
        txt_header.text = GameStateManager.CurrentState == GameState.Idle ? "SETTING" : "PAUSE";

        anim.Show();
    }

    public void OnHomeBtnClick()
    {
        SoundManager.Play("1. Click Button");
        GameStateManager.Idle(null);
        anim.Hide();
    }

    public void OnRestartBtnClick()
    {
        SoundManager.Play("1. Click Button");
        GameStateManager.LoadGame(null);
        anim.Hide();

#if USE_IRON || USE_MAX || USE_ADMOB
        AdsManager.ShowInterstitial((s, adType) =>
        {
            GameStateManager.LoadGame(null);
            UIToast.Hide();
        }, name, "RestartFromGameSetting");
#else
        UIToast.Hide();
#endif
    }

    public void OnContinueBtnClick()
    {
        SoundManager.Play("1. Click Button");
        if (GameStateManager.CurrentState == GameState.Pause)
            GameStateManager.Play(null);
        anim.Hide();
    }
}
