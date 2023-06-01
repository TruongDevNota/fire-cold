using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CatHouse : MonoBehaviour
{
    [SerializeField] List<HouseFloor> floors = new List<HouseFloor>();

    public void Init()
    {
        for(int i = 0; i < floors.Count; i++)
        {
            var datum = DataManager.HouseAsset.allFloorData.FirstOrDefault(x => x.floorIndex == i + 1);
            if(datum == null)
            {
                Debug.LogError($"HouseData is null at floor {i + 1}");
                continue;
            }

            floors[i].Fill(datum);
        }
    }
}
