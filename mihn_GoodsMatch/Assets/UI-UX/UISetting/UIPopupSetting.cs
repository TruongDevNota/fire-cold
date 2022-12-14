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
    }

    public void OnHomeBtnClick()
    {
        GameStateManager.Idle(null);
        anim.Hide();
    }

    public void OnRestartBtnClick()
    {
        GameStateManager.LoadGame(null);
        anim.Hide();
    }

    public void OnContinueBtnClick()
    {
        if(GameStateManager.CurrentState != GameState.Idle)
            GameStateManager.Play(null);
        anim.Hide();
    }
}
