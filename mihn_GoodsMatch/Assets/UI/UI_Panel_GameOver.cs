using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Panel_GameOver : MonoBehaviour
{
    [SerializeField] Button btn_Home, btn_Replay, btn_Next;

    public void Onshow(bool isWin)
    {
        btn_Home.gameObject.SetActive(true);
        btn_Replay.gameObject.SetActive(!isWin);
        btn_Next.gameObject.SetActive(false);
    }

    public void OnBtn_HomeClicked()
    {
        UI_Ingame_Manager.instance.OnGameInitHandle();
    }

    public void OnBtn_RestartHandler()
    {
        BoardGame.instance.RestartGame();
    }
}
