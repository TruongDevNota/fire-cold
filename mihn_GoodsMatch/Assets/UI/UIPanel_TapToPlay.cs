using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanel_TapToPlay : MonoBehaviour
{
    [SerializeField] Button btn_TapToPlay;
    [SerializeField] Button btn_Back;

    private void Start()
    {
        btn_TapToPlay.gameObject.SetActive(false);
    }

    public void OnGamePrepare()
    {
        btn_TapToPlay.gameObject.SetActive(false);
    }

    public void OnGamePrepareDone()
    {
        btn_TapToPlay.gameObject.SetActive(true);
        btn_Back.gameObject.SetActive(true);
    }

    public void OnButtonTapToPlayClieked()
    {
        if (GameStateManager.CurrentState == GameState.Ready || GameStateManager.CurrentState == GameState.Pause)
            return;
            //BoardGame.instance.StartGamePlay();
    }

    public void OnBack()
    {
        if (!BoardGame.instance.isPlayingGame)
            return;
        GameStateManager.Idle(null);
    }
}
