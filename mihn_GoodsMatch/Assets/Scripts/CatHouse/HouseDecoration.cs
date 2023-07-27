using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MewtonGames.Nonogram;

public class HouseDecoration : MonoBehaviour
{
    [SerializeField] List<HouseFloor> floors = new List<HouseFloor>();
    [SerializeField] Transform _roofTF;
    [SerializeField] float _roofPosYMin;
    [SerializeField] float _floorHeight;

    [Header("Camera Controller")]
    [SerializeField] MapInput mapInput;
    [SerializeField] CameraMapMoving cameraMoving;
    [SerializeField] private float _cameraPosMinY;
    [SerializeField] private float _cameraPosOffsetY;
    [SerializeField] public Vector3 pCurrentLevelNodeWorldPosition;
    private Vector3 startPressDownScreenPosition = Vector3.zero;

    private void Start()
    {
        this.RegisterListener((int)EventID.OnFloorUnlocked, Init);
    }

    private void OnDestroy()
    {
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnFloorUnlocked, Init);
        mapInput.Enable(false);
    }

    private void OnEnable()
    {
        
        Init(null);
    }

    private void Update()
    {
        mapInput.Tick();

        if (mapInput.pIsHolding)
            cameraMoving.MoveDistance(mapInput.pMovingWorldOffsetY, mapInput.pIsFlick);

        if (mapInput.pIsFlick)
            mapInput.Reset();

        cameraMoving.Tick();

        if (Input.GetMouseButtonDown(0))
        {
            startPressDownScreenPosition = Input.mousePosition;
        }
    }

    public void Init(object data)
    {
        LoadFloor();
        StartCoroutine(YieldInit(null));
    }
    public void LoadFloor()
    {

        for (int i = 0; i < DataManager.HouseAsset.allFloorData.Count; i++)
        {
            var datum = DataManager.HouseAsset.allFloorData.FirstOrDefault(x => x.floorIndex == i + 1);
            //floors[i].gameObject.SetActive(datum != null && DataManager.HouseAsset.CheckFoorUnlockable(i + 1));
            if (datum == null)
            {
                Debug.LogError($"HouseData is null at floor {i + 1}");
                continue;
            }
            else if (DataManager.HouseAsset.CheckFoorUnlockable(i + 1)&&i+1>floors.Count)
            {
                string path = $"PrefabCat/Floor {i + 1}";
                var prefab = Resources.Load<HouseFloor>(path);
                var floor = Instantiate(prefab, this.transform);
                floors.Add(floor);
            }

        }
    }
    public IEnumerator YieldInit(object data)
    {
        int roofCount = 0;
        for (int i = 0; i < floors.Count; i++)
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

        mapInput.Init();
        cameraMoving.Init();
        mapInput.Enable(true);
        cameraMoving.SetupBound(_cameraPosMinY, _cameraPosMinY + _cameraPosOffsetY + Mathf.Max(0, roofCount - 1) * _floorHeight);

        yield return new WaitForSeconds(0.1f);
        cameraMoving.Show(pCurrentLevelNodeWorldPosition.y);
    }
}