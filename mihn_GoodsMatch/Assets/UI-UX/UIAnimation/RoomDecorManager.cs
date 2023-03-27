using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
    private void GoToShop()
    {
        roomCam.transform.DOLocalMoveY(-2.85f, timeMoveCam);
    }
    private void IdleGame()
    {
        roomCam.transform.DOLocalMoveY(0, timeMoveCam);
    }

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
        chairsImg.sprite = current.main;
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
