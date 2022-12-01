using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Ingame_Manager : MonoBehaviour
{
    [SerializeField] UIPanel_Playing panelPlaying;
    [SerializeField] UIPanel_TapToPlay panelTapToPlay;

    //public static UI_Ingame_Manager instance;

    private void Awake()
    {
        //instance = this;
    }

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += OnGameStateChangeHandler;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= OnGameStateChangeHandler;
    }

    private void OnGameStateChangeHandler(GameState current, GameState last, object data)
    {
        switch (current)
        {
            case GameState.None:
                break;
            case GameState.LoadMain:
                break;
            case GameState.Login:
                break;
            case GameState.Idle:
                break;
            case GameState.LoadGame:
                break;
            case GameState.Init:
                break;
            case GameState.Ready:
                break;
            case GameState.Play:
                OnGameStarted();
                break;
            case GameState.Pause:
                break;
            case GameState.RebornContinue:
                break;
            case GameState.RebornCheckPoint:
                break;
            case GameState.WaitGameOver:
                OnGameOverHandle(false);
                break;
            case GameState.GameOver:
                break;
            case GameState.WaitComplete:
                OnGameOverHandle(true);
                break;
            case GameState.Complete:
                break;
            case GameState.Restart:
                break;
            case GameState.Next:
                break;
            case GameState.InShop:
                break;
        }
    }

    public void OnGameInitHandle()
    {
        panelTapToPlay.gameObject.SetActive(false);
        panelPlaying.gameObject.SetActive(false);
    }

    public void OnGamePrepareHandle(int level)
    {
        panelTapToPlay.gameObject.SetActive(true);
        panelTapToPlay.OnGamePrepareDone();
        panelPlaying.gameObject.SetActive(true);
        panelPlaying.OnGamePrepareHandler(level);
    }

    public void OnGameStarted()
    {
        panelTapToPlay.gameObject.SetActive(false);
    }

    public void OnGameOverHandle(bool isWin)
    {
        //panelPlaying.gameObject.SetActive(false);
        panelTapToPlay.gameObject.SetActive(false);
    }

    public void OnGamePauseHandle()
    {
        panelTapToPlay.gameObject.SetActive(true);
        panelTapToPlay.OnGamePrepareDone();
    }

    public void OnGameEndHandle()
    {

    }
}
