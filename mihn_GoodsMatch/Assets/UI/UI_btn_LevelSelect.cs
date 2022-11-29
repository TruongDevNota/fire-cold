using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_btn_LevelSelect : MonoBehaviour
{
    public void OnLevelSelect(int level)
    {
        BoardGame.instance.PrepareSceneLevel(level);
    }
}
