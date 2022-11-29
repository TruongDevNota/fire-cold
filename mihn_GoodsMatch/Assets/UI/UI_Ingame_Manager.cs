using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Ingame_Manager : MonoBehaviour
{
    [SerializeField] UI_Home panelHome;
    [SerializeField] UIPanel_Playing panelPlaying;
    [SerializeField] UIPanel_TapToPlay panelTapToPlay;
    [SerializeField] UI_Panel_GameOver panelGameOver;

    public static UI_Ingame_Manager instance;

    private void Awake()
    {
        instance = this;
    }

    public void OnGameInitHandle()
    {
        panelTapToPlay.gameObject.SetActive(false);
        panelPlaying.gameObject.SetActive(false);
        panelGameOver.gameObject.SetActive(false);
        panelHome.gameObject.SetActive(true);
    }

    public void OnGamePrepareHandle(int level)
    {
        panelTapToPlay.gameObject.SetActive(true);
        panelTapToPlay.OnGamePrepare();
        panelPlaying.gameObject.SetActive(true);
        panelPlaying.OnGamePrepareHandler(level);
        panelHome.gameObject.SetActive(false);
        panelGameOver.gameObject.SetActive(false);
    }

    public void OnGamePrepareDone()
    {
        panelTapToPlay.OnGamePrepareDone();
    }

    public void OnGameStarted()
    {
        panelTapToPlay.gameObject.SetActive(false);
    }

    public void OnGameOverHandle(bool isWin)
    {
        panelGameOver.gameObject.SetActive(true);
        panelGameOver.Onshow(isWin);
    }

    public void OnGamePauseHandle()
    {
        panelTapToPlay.gameObject.SetActive(true);
        panelTapToPlay.OnGamePrepareDone();
    }
}
