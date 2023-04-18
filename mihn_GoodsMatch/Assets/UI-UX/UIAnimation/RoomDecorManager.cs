using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Spine.Unity;
using System;

public class RoomDecorManager : MonoBehaviour
{
    [SerializeField]
    Camera roomCam = null;

    [SerializeField]
    private SpriteRenderer wallImg = null;
    [SerializeField]
    private SpriteRenderer windowsImg = null;
    [SerializeField]
    private SpriteRenderer floorImg = null;
    [SerializeField]
    private SpriteRenderer ceillingImg = null;
    [SerializeField]
    private SpriteRenderer carpetImg = null;
    [SerializeField]
    private SpriteRenderer chairsImg = null;
    [SerializeField]
    private SpriteRenderer tableImg = null;
    [SerializeField]
    private SpriteRenderer lampImg = null;


    [SerializeField] private SkeletonAnimation cat1;
    [SerializeField] private SkeletonAnimation cat2;

    [SerializeField] private float translateCam = -2.85f;
    [SerializeField] private float fieldOfViewShop = 88f;
    [SerializeField] private float fieldOfViewIdle = 60f;

    GameObject chairObject;
    private void OnEnable()
    {
        GameStateManager.OnStateChanged += GameStateManager_OnStateChanged;
        DataManager_OnLoaded(null);
        SkinsAsset.OnChanged += SkinsAsset_OnChanged;
        WindowsAsset.OnChanged += WindowsAsset_OnChanged;
        FloorsAsset.OnChanged += FloorsAsset_OnChanged;
        CeillingAsset.OnChanged += CeillingAsset_OnChanged;
        CarpetsAsset.OnChanged += CarpetsAsset_OnChanged;
        ChairsAsset.OnChanged += ChairsAsset_OnChanged;
        TablesAsset.OnChanged += TablesAsset_OnChanged;
        LampsAsset.OnChanged += LampsAsset_OnChanged;
        cat1.AnimationName = "idle1";
        cat2.AnimationName = "idle1";
        this.RegisterListener((int)EventID.BuySuccess, ActiveAnim);
    }



    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= GameStateManager_OnStateChanged;
        SkinsAsset.OnChanged -= SkinsAsset_OnChanged;
        WindowsAsset.OnChanged -= WindowsAsset_OnChanged;
        FloorsAsset.OnChanged -= FloorsAsset_OnChanged;
        CeillingAsset.OnChanged -= CeillingAsset_OnChanged;
        CarpetsAsset.OnChanged -= CarpetsAsset_OnChanged;
        ChairsAsset.OnChanged -= ChairsAsset_OnChanged;
        TablesAsset.OnChanged -= TablesAsset_OnChanged;
        LampsAsset.OnChanged -= LampsAsset_OnChanged;
    }
    private void GameStateManager_OnStateChanged(GameState current, GameState last, object data)
    {
        switch (current)
        {
            case GameState.None:
                break;
            case GameState.LoadMain:
                break;
            case GameState.Login:
                break;
            case GameState.Idle:
                IdleGame();
                break;
            case GameState.LoadGame:
                break;
            case GameState.Init:
                break;
            case GameState.Ready:
                break;
            case GameState.Play:
                break;
            case GameState.Pause:
                break;
            case GameState.RebornContinue:
                break;
            case GameState.RebornCheckPoint:
                break;
            case GameState.WaitGameOver:
                break;
            case GameState.GameOver:
                break;
            case GameState.WaitComplete:
                break;
            case GameState.Complete:
                break;
            case GameState.Restart:
                break;
            case GameState.Next:
                break;
            case GameState.InShop:
                GoToShop();
                break;
            //case GameState.LuckySpin:
            //    break;
            //case GameState.DailyReward:
            //    break;
            default:
                break;
        }
    }
    private float timeMoveCam = 0.35f;
    [SerializeField] private float delayAnim = 1f;

    private void GoToShop()
    {
        roomCam.transform.DOLocalMoveY(translateCam, timeMoveCam);

    }
    private void IdleGame()
    {
        // roomCam.transform.DOLocalMoveY(0, timeMoveCam);
    }
    private void ActiveAnim(object obj)
    {
        cat1.AnimationName = "happy";
        cat2.AnimationName = "happy";
        StartCoroutine(DelayAnim());
    }

    private IEnumerator DelayAnim()
    {
        yield return new WaitForSeconds(delayAnim);
        cat1.AnimationName = "idle1";
        cat2.AnimationName = "idle1";
    }

    //private IEnumerator YieldMoveCameraIdle()
    //{
    //    currentFieldOfView = roomCam.fieldOfView;
    //    Debug.Log("cameraIdle");
    //    float t = 0;
    //    while(t< timeMoveCam)
    //    {
    //        yield return new WaitForEndOfFrame();
    //        t += Time.deltaTime;
    //        roomCam.fieldOfView = currentFieldOfView + (fieldOfViewIdle - currentFieldOfView) * Mathf.Clamp01(t / timeMoveCam);
    //    }
    //}

    private void LampsAsset_OnChanged(LampData current, List<LampData> list)
    {
        SetLampDecorSprite(current);
    }

    private void TablesAsset_OnChanged(TableData current, List<TableData> list)
    {
        SetTableDecorSprite(current);
    }

    private void ChairsAsset_OnChanged(ChairData current, List<ChairData> list)
    {
        SetChairDecorSprite(current);
    }

    private void CarpetsAsset_OnChanged(CarpetData current, List<CarpetData> list)
    {
        SetCarpetDocorSprite(current);
    }

    private void CeillingAsset_OnChanged(CeillingData current, List<CeillingData> list)
    {
        SetCeillingDecorSprite(current);
    }

    private void FloorsAsset_OnChanged(FloorData current, List<FloorData> list)
    {
        floorImg.sprite = current.main;
    }

    private void WindowsAsset_OnChanged(WindowsData current, List<WindowsData> list)
    {
        SetWindowsDecorSprite(current);
    }

    private void DataManager_OnLoaded(GameData gameData)
    {
        wallImg.sprite = DataManager.SkinsAsset.Current.main;

        SetWindowsDecorSprite(DataManager.WindowAsset.Current);

        floorImg.sprite = DataManager.FloorAsset.Current.main;

        SetCeillingDecorSprite(DataManager.CeillingAsset.Current);

        SetCarpetDocorSprite(DataManager.CarpetsAsset.Current);

        SetChairDecorSprite(DataManager.ChairsAsset.Current);

        SetTableDecorSprite(DataManager.TableAssets.Current);

        SetLampDecorSprite(DataManager.LampsAsset.Current);
    }
    private void SetChairDecorSprite(ChairData current)
    {
        if (chairObject != null)
        {
            Destroy(chairObject.gameObject);
        }
        if (current.gameObject.TryGetComponent<SkeletonAnimation>(out SkeletonAnimation animFlag))
        {
            chairObject = Instantiate(current.gameObject, chairsImg.transform);
            chairsImg.sprite = null;
            chairsImg.gameObject.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            chairsImg.sprite = current.main;
            chairsImg.gameObject.transform.localScale = new Vector3(-1, 1, 1);
        }
            
    }
    private void SetTableDecorSprite(TableData current)
    {
        tableImg.sprite = current.main;
    }
    private void SetLampDecorSprite(LampData current)
    {
        lampImg.sprite = current.main;
    }
    private void SetCarpetDocorSprite(CarpetData current)
    {
        carpetImg.sprite = current.main;
    }
    private void SetWindowsDecorSprite(WindowsData current)
    {
        windowsImg.sprite = current.main;
    }
    private void SetCeillingDecorSprite(CeillingData current)
    {
        ceillingImg.sprite = current.main;
    }

    private void SkinsAsset_OnChanged(SkinData current, List<SkinData> list)
    {
        wallImg.sprite = current.main;
    }
}
