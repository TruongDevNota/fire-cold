using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_btn_LevelSelect : MonoBehaviour
{
    public void OnLevelSelect(int level)
    {
        Debug.Log($"User level: {DataManager.UserData.level}");
        DataManager.selectedLevel = DataManager.UserData.level <= 30 ? DataManager.UserData.level : Random.Range(20, 31);
        Debug.Log($"Level data load: {DataManager.selectedLevel}");

        if (GameStateManager.CurrentState == GameState.Idle)
            GameStateManager.LoadGame(null);
    }
}
