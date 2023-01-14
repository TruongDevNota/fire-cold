using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUtilities
{
    public static bool IsShowAdsInter(int level)
    {
        return level >= 21 || level % 2 == 0;
    }
}
