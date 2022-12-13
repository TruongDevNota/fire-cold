using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_btn_LevelSelect : MonoBehaviour
{
    public void OnLevelSelect(int level)
    {
        Debug.Log($"Level data load: {DataManager.UserData.level}");

        if (GameStateManager.CurrentState == GameState.Idle)
            GameStateManager.LoadGame(null);
    }
}
