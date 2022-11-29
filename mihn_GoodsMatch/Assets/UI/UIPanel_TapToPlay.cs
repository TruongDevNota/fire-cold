using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanel_TapToPlay : MonoBehaviour
{
    [SerializeField] Button btn_TapToPlay;

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
    }

    public void OnButtonTapToPlayClieked()
    {
        BoardGame.instance.StartGamePlay();
    }
}
