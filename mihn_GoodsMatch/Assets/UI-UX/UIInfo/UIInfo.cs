using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIInfo : MonoBehaviour
{
    [SerializeField] Text levelTxt = null;

    private void Awake()
    {
        GameStateManager.OnStateChanged += GameStateManager_OnStateChanged;
    }

    private void GameStateManager_OnStateChanged(GameState current, GameState last, object data)
    {
        if (current == GameState.Init)
        {
            levelTxt.text = $"{DataManager.UserData.level + 1}";
        }
        else if (current == GameState.Ready)
        {
        }
    }
}
