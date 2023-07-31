using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MewtonGames.Nonogram;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class HouseDecoration : MonoBehaviour
{
    [SerializeField] List<HouseFloor> floors = new List<HouseFloor>();
    [SerializeField] private AssetReference[] assetReference;
    [SerializeField] Transform _roofTF;
    [SerializeField] float _roofPosYMin;
    [SerializeField] float _floorHeight;
    private int floorCount;

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
        floorCount = 0;
        StartCoroutine(LoadFloor());

    }
    public IEnumerator LoadFloor()
    {
        int loadedFloorCount = 0;
        for (int i = 0; i < DataManager.HouseAsset.allFloorData.Count; i++)
        {
            if (DataManager.HouseAsset.CheckFoorUnlockable(i + 1) && i + 1 > floors.Count)
            {
                floorCount++;
                float startTime = Time.time;
                var handle = assetReference[i].LoadAssetAsync<GameObject>();
                yield return handle;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log("Load sucess");
                    var floor = Instantiate(handle.Result, this.transform);
                    floors.Add(floor.GetComponent<HouseFloor>());
                    loadedFloorCount++;
                    float loadTime = Time.time - startTime;
                    Debug.LogError("load prefab time: " + loadTime);
                }
                else
                {
                    Debug.Log("Failed to load!");
                }


            }
        }
        yield return new WaitForSeconds(.1f);

        yield return new WaitUntil(() => loadedFloorCount >= floorCount);
        yield return YieldInit(null);
    }
    public IEnumerator YieldInit(object data)
    {
        int roofCount = 0;
        for (int i = 0; i < floors.Count; i++)
        {
            var datum = DataManager.HouseAsset.allFloorData.FirstOrDefault(x => x.floorIndex == i + 1);
            floors[i].Fill(datum);
            roofCount++;
        }
        _roofTF.position = new Vector2(0, _roofPosYMin + Mathf.Max(0, roofCount - 1) * _floorHeight);

        mapInput.Init();
        cameraMoving.Init();
        mapInput.Enable(true);
        cameraMoving.SetupBound(_cameraPosMinY, _cameraPosMinY + _cameraPosOffsetY + Mathf.Max(0, roofCount - 1) * _floorHeight);

        yield return new WaitForSeconds(0.1f);
        cameraMoving.Show(pCurrentLevelNodeWorldPosition.y);
        UILoadGame.Hide();
    }
}