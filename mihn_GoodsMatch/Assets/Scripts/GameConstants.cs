using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameConstants
{
    public const string goodsItemTag = "Goods_Item";
    public const string cellTag = "Cell";

    #region MapData
    public static char[] shelfSplitChars = new char[] { ',', ';' };
    public static char itemSplittChar = '|';
    #endregion

    #region soundName
    public const string sound_Button_Clicked = "1. Click Button";

    public const string sound_ItemSpawn = "ItemSpawn";
    public const string sound_CatAngryLeave1 = "Cat_AngryAndLeave_1";
    public const string sound_CatAngryLeave2 = "Cat_AngryAndLeave_2";
    public const string sound_CatHappy1 = "Cat_Happy_1";
    public const string sound_CatHappy2 = "Cat_Happy_2";
    public const string sound_CatWaitlong1 = "Cat_WaitLong_1";
    public const string sound_CatWaitlong2 = "Cat_WaitLong_2";

    public const string sound_doorCloseDown = "Close door";
    public const string sound_doorOpenUp = "Open Door";

    public static string[] soundsCombo = new string[4] { "Combo 01", "Combo 02", "Combo 03", "Combo 04" };
    #endregion

}
