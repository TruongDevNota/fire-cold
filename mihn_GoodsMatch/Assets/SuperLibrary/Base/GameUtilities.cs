using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUtilities
{
    public static bool IsShowAdsInter(int level)
    {
        return level >= 5 && (level >= 21 || level % 2 == 0);
    }

    public static bool GetRandomBool()
    {
        return Random.value > 0.5f;
    }
}
