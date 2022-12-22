using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupChallenge : MonoBehaviour
{
    [SerializeField] UIAnimation anim;
    [SerializeField] Button btn_PlayWithCoin;

    private void Start()
    {
        btn_PlayWithCoin?.onClick.AddListener(BtnPlayWithCoinClick);
    }

    public void OnShow()
    {
        anim.Show();
    }

    private void BtnPlayWithCoinClick()
    {
        GameStateManager.LoadGame(true);
        anim.Hide();
    }

    
}
