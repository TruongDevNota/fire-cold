using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MyBox;
[CreateAssetMenu(fileName = "LampDataAsset", menuName = "DataAsset/LampDataAsset")]

public class LampsAsset : BaseAsset<LampData>
{
    [Header("Config")]
    [SerializeField] public List<GameObject> allPrefabs;
    [SerializeField] public Sprite[] allThumbsTier1;
    //[SerializeField] public Sprite[] allThumbsTier2;
    //[SerializeField] public Sprite[] allThumbsTier3;

    [ButtonMethod]
    public void ConfigAllCharacters()
    {
        list.Clear();
        int count = 0;
        System.Action<Sprite, UnlockType, int> createCD = (sprite, ut, price) =>
        {
            var c = new LampData();
            c.name = sprite.name;
            c.id = sprite.name;
            c.index = count;
            c._unlockType = ut;
            c._unlockPrice = price;
            c.unlockPay = 0;
            c.isUnlocked = count == 0;
            c.isSelected = count == 0;

            c.main = sprite;
            c.gameObject = allPrefabs.FirstOrDefault(x => x.name.Contains(sprite.name));

            list.Add(c);
            count++;
        };

        var max = Mathf.Max(allThumbsTier1.Length, allThumbsTier1.Length/* allThumbsTier2.Length, allThumbsTier3.Length*/);
        for (int i = 0; i < max; i++)
        {
            if (i < allThumbsTier1.Length)
                createCD(allThumbsTier1[i], UnlockType.Gold, 100 * (i + 1));
            //if (i < allThumbsTier2.Length)
            //    createCD(allThumbsTier2[i], UnlockType.Ads, 1);
            //if (i < allThumbsTier3.Length)
            //    createCD(allThumbsTier3[i], UnlockType.Gold, 1500 * (i + 1));
        }


        foreach (var p in allPrefabs)
        {
            if (!list.Any(x => p.name.Contains(x.name)))
                Debug.Log($"'{p.name}' is not configable");
        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public void UpdateUnlockPrice()
    {
        foreach (var d in list)
        {
            if (d._unlockType == UnlockType.Gold)
            {
                d.unlockPay = (int)DataManager.UserData.TotalPlay;
                d.isUnlocked = d.unlockPay >= d._unlockPrice;
            }
        }
    }
    [ButtonMethod]
    public List<LampData> GetNotUnlockedSkin()
    {
        List<LampData> allLamps = new List<LampData>();
        foreach (var item in list)
        {
            allLamps.Add(item);
        }
        allLamps.RemoveAt(0);
        return allLamps;
    }
    [ButtonMethod]
    public void ResetUnlockedAndSelectedStage()
    {
        int count = 0;
        foreach (var item in list)
        {
            item.isUnlocked = count == 0;
            item.isSelected = count == 0;
            count++;
        }
    }
}
[Serializable]
public class LampData : SaveData
{
    [Header("GameData")]
    public UnlockType _unlockType = UnlockType.Gold;
    public int _unlockPrice = 1;
    public Sprite main;
    public Sprite background;
    public GameObject gameObject;
}