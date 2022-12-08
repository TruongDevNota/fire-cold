using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_btn_LevelSelect : MonoBehaviour
{
    public void OnLevelSelect(int level)
    {
        Debug.Log($"User level: {DataManager.UserData.level + 1}");
        DataManager.selectedLevel = DataManager.UserData.level < 30 ? DataManager.UserData.level + 1 : Random.Range(20, 31);
        Debug.Log($"Level data load: {DataManager.UserData.level + 1}");

        if (GameStateManager.CurrentState == GameState.Idle)
            GameStateManager.LoadGame(null);
    }
}
