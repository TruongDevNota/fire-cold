using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_btn_LevelSelect : MonoBehaviour
{
    public void OnLevelSelect(int level)
    {
        Debug.Log($"Level data load: {DataManager.UserData.level}");
        DataManager.levelSelect = DataManager.UserData.level + 1;
        //if ((DataManager.UserData.level + 1) % DataManager.GameConfig.starsToNextChallenge == 0)
        //    this.PostEvent((int)EventID.OnGoToChallengeLevel);
        //else
            GameStateManager.LoadGame(null);
    }
}
