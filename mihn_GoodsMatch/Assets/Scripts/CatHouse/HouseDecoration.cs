using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HouseDecoration : MonoBehaviour
{
    [SerializeField] List<HouseFloor> floors = new List<HouseFloor>();
    [SerializeField] Transform _roofTF;
    [SerializeField] float _roofPosYMin;
    [SerializeField] float _floorHeight;

    private void Start()
    {
        this.RegisterListener((int)EventID.OnFloorUnlocked, Init);
    }

    private void OnDestroy()
    {
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnFloorUnlocked, Init);
    }

    private void OnEnable()
    {
        Init(null);
    }

    public void Init(object data)
    {
        int roofCount = 0;
        for(int i = 0; i < floors.Count; i++)
        {
            var datum = DataManager.HouseAsset.allFloorData.FirstOrDefault(x => x.floorIndex == i + 1);
            floors[i].gameObject.SetActive(datum != null && DataManager.HouseAsset.CheckFoorUnlockable(i + 1));
            if (datum == null)
            {
                Debug.LogError($"HouseData is null at floor {i + 1}");
                continue;
            }
            else if (DataManager.HouseAsset.CheckFoorUnlockable(i + 1))
            {
                floors[i].gameObject.SetActive(true);
                floors[i].Fill(datum);
                roofCount++;
            }
            else
                floors[i].gameObject.SetActive(false);

        }
        _roofTF.position = new Vector2(0, _roofPosYMin + Mathf.Max(0, roofCount - 1) * _floorHeight);
    }
}