using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_btn_LevelSelect : MonoBehaviour
{
    public void OnLevelSelect(int level)
    {
        DataManager.demoLevel = level;

        if (GameStateManager.CurrentState == GameState.Idle)
            GameStateManager.LoadGame(null);

        //BoardGame.instance.PrepareSceneLevel(level);
    }
}
