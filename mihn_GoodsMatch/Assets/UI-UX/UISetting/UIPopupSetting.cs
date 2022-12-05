using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupSetting : MonoBehaviour
{
    [SerializeField] 
    GameObject panelButtonsIngame;
    [SerializeField]
    Button homeBtn;
    [SerializeField]
    Button restartBtn;
    [SerializeField]
    Button continueBtn;

    private void Start()
    {
        homeBtn?.onClick.AddListener(OnHomeBtnClick);
        restartBtn?.onClick.AddListener(OnRestartBtnClick);
        continueBtn?.onClick.AddListener(OnContinueBtnClick);
    }

    public void OnShow()
    {
        panelButtonsIngame?.SetActive(GameStateManager.CurrentState == GameState.Play);
    }

    public void OnHomeBtnClick()
    {
        GameStateManager.Idle(null);
    }

    public void OnRestartBtnClick()
    {
        if (GameStateManager.CurrentState == GameState.Pause)
            GameStateManager.Restart(null);
    }

    public void OnContinueBtnClick()
    {
        if (GameStateManager.CurrentState == GameState.Pause)
            GameStateManager.Play(null);
    }
}
