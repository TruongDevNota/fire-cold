using System;
using System.Linq;
using UnityEngine;


public class GameStatisticsManager : MonoBehaviour
{
    #region GameState
    private UserData userData => DataManager.UserData;

    private void Awake()
    {
        GameStateManager.OnStateChanged += OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState current, GameState last, object data)
    {
        if (current != GameState.LoadMain)
        {
            switch (current)
            {
                case GameState.Idle:
                    DebugMode.UpdateWinLose();
                    break;
                case GameState.Init:
                    UIManager.DelScreenShot();
                    break;
                case GameState.Play:
                    userData.TotalPlay++;
                    TimePlayInGameStart = DateTime.Now;

                    break;
                case GameState.RebornCheckPoint:
                    break;
                case GameState.RebornContinue:
                    break;
                case GameState.Restart:
                    break;
                case GameState.WaitGameOver:

                    if (userData.WinStreak > 0)
                    {
                        userData.LoseStreak = 0;
                        userData.WinStreak = 0;
                    }
                    userData.LoseStreak++;
                    goldEarn = 0;

                    DebugMode.UpdateWinLose();
                    break;
                case GameState.WaitComplete:
                    if (userData.LoseStreak > 0)
                    {
                        userData.LoseStreak = 0;
                        userData.WinStreak = 1;
                    }
                    userData.WinStreak++;

                    DebugMode.UpdateWinLose();
                    break;
                case GameState.Complete:
                    userData.TotalWin++;
                    userData.TotalTimePlay += TimePlayInGameEnd;
                    break;
                case GameState.GameOver:
                    userData.TotalTimePlay += TimePlayInGameEnd;
                    break;
            }
        }
    }
    #endregion

    #region RewardInGame
    public static int goldEarn = 0;
    public static int gemEarn = 0;
    public static int starEarn = 0;
    #endregion

    #region Score
    private static int score = -1;
    public static int Score
    {
        get
        {
            return score;
        }
        set
        {
            if (score != value)
            {
                score = value;
                OnScoreChanged?.Invoke(score);
            }
        }
    }

    public static event Action<int> OnScoreChanged = delegate { };
    #endregion

    #region TimePlay
    public static DateTime TimePlayInGameStart { get; set; }
    public static int TimePlayInGameEnd => (int)(DateTime.Now - TimePlayInGameStart).TotalSeconds;
    #endregion

    #region Perfect

    private static int perfect = -1;
    public static int Perfect
    {
        get => perfect;
        set
        {
            if (perfect != value)
            {
                perfect = value;
                OnPerfectChanged?.Invoke(perfect);
            }
        }
    }
    public static event Action<int> OnPerfectChanged = delegate { };
    #endregion
}
