using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : GameManagerBase<GameUIManager>
{
    [SerializeField]
    private float waitTimeForLoadAd = 1;
    [SerializeField]
    private UIAnimation splashScreen = null;
    [SerializeField]
    private UIMainScreen mainScreen = null;
    [SerializeField]
    private UIAnimation coinScreen;
    [SerializeField]
    private UIAnimation starScreen;

    public static UIMainScreen MainScreen => instance?.mainScreen;

    [SerializeField]
    private UIInGame inGameScreen = null;

    [SerializeField]
    private UIGameOver gameOverScreen = null;
    public static UIGameOver GameOverScreen => instance?.gameOverScreen;

    private DateTime startLoadTime = DateTime.Now;

    private GameConfig gameConfig => DataManager.GameConfig;
    private UserData user => DataManager.UserData;

    protected override void Awake()
    {
        base.Awake();
        if (splashScreen)
            splashScreen.gameObject.SetActive(true);
        startLoadTime = DateTime.Now;

#if UNITY_EDITOR
        DOTween.useSafeMode = false;
#endif
    }

    protected override void Start()
    {
        base.Start();
        Input.multiTouchEnabled = false;
        StartCoroutine(LoadGameData());
    }


    public IEnumerator LoadGameData()
    {
        yield return DataManager.DoLoad();
        UIToast.ShowLoading("Loading... please wait!!");

        while (user == null)
        {
            DebugMode.Log("Load game data...");
            yield return null;
        }

        SoundManager.LoadAllSounds();

#if USE_FIREBASE
        yield return FirebaseHelper.DoCheckStatus(null, true);
#endif


        if (user.VersionInstall == 0)
        {
            user.VersionInstall = UIManager.BundleVersion;
#if USE_FIREBASE
            FirebaseHelper.SetUser("Type", "New");
            AnalyticsManager.LogEvent("User_New");
#endif
        }
        else if (user.VersionInstall != UIManager.BundleVersion)
        {
#if USE_FIREBASE
            FirebaseHelper.SetUser("Type", "Update");
            AnalyticsManager.LogEvent("User_Update");
#endif
        }
        user.VersionCurrent = UIManager.BundleVersion;

        while ((int)(DateTime.Now - startLoadTime).TotalSeconds < waitTimeForLoadAd)
        {
            yield return null;
        }


        GameStateManager.LoadMain(null);

        if (gameConfig.suggestUpdateVersion > user.VersionCurrent)
        {
            ForeUpdate();
        }
        else
        {
            GameStateManager.Idle(null);
            yield return new WaitForSeconds(0.5f);
            splashScreen?.Hide();
        }

        int loadGameIn = (int)(DateTime.Now - startLoadTime).TotalSeconds;
        Debug.Log("loadGameIn: " + loadGameIn + "s");

        AnalyticsManager.LogEvent("start_session", AnalyticsManager.logUser);
    }

    public void ForeUpdate()
    {
        string title = "New version avaiable!";
        string body = "We are trying to improve the game quality by updating it regularly.\nPlease update new version for the best experience!";
        PopupMes.Show(title, body,
            "Update", () =>
            {
                if (!string.IsNullOrEmpty(UIManager.shareUrl))
                    Application.OpenURL(UIManager.shareUrl);
            },
            "Later", () =>
            {
                splashScreen?.Hide();
                mainScreen.Show(null, () =>
                {
                    GameStateManager.Idle(null);
                });
            });
    }

    protected override void LoadMain(object data)
    {
        base.LoadMain(data);
    }

    public override void IdleGame(object data)
    {
        SceneHelper.DoLoadScene("2_Idle");
        inGameScreen.Hide();
        this.PostEvent((int)EventID.OnPlayMusic, "Bgm Menu");
        Action callback = () => {
            mainScreen.Show(()=> {
            }, () => {
                UILoadGame.Hide();
            });
            inGameScreen.Hide();
            gameOverScreen.Hide();

        };

        if(GameStateManager.LastState == GameState.Complete 
            || GameStateManager.LastState == GameState.GameOver
            || GameStateManager.LastState == GameState.Next)
        {
            UILoadGame.Init(true, null);
            StartCoroutine(WaitForLoading(callback, 0.5f));
        }
        else
        {
            callback.Invoke();
        }
    }

    protected override void GoToShop(object data)
    {
        base.GoToShop(data);
        mainScreen.Hide();
    }

    public override void LoadGame(object data)
    {
        Time.timeScale = 1;
        LoadGameContent.PrepairDataToPlay(); 
        coinScreen.Hide();
        starScreen.Hide();
    }

    public override void InitGame(object data)
    {
        foreach (var i in UIManager.listPopup)
            i.Hide();
        DOTween.Kill(this);
    }

    IEnumerator WaitForLoading(Action onComplete, float time = 0)
    {
        if (time > 0)
            yield return new WaitForSeconds(time);

        while (UILoadGame.currentProcess < 1)
        {
            UILoadGame.Process();
            yield return null;
        }
        inGameScreen.Show();
        yield return new WaitForSeconds(0.2f);
        UILoadGame.Hide();
        onComplete?.Invoke();
    }

    public override void PlayGame(object data)
    {
        MusicManager.UnPause();
    }

    public override void PauseGame(object data)
    {
        MusicManager.Pause();
    }

    protected override void GameOver(object data)
    {
        //inGameScreen.Hide();
        gameOverScreen.Show(GameState.GameOver, data);
    }

    protected override void CompleteGame(object data)
    {
        //inGameScreen.Hide();
        gameOverScreen.Show(GameState.Complete, data);
    }

    protected override void ReadyGame(object data)
    {
        this.PostEvent((int)EventID.OnPlayMusic, "Bgm Gamepaly");
        StartCoroutine(WaitForLoading(() =>
        {
            mainScreen.Hide();
            
            //StartCoroutine(WaitToAutoPlay());
        }));
    }

    public override void ResumeGame(object data)
    {
        MusicManager.UnPause();
    }

    public override void RestartGame(object data)
    {
        //GameStateManager.Idle(null);
    }

    public override void NextGame(object data)
    {
        GameStateManager.Idle(null);
    }

    protected override void WaitingGameOver(object data)
    {
        MusicManager.Stop(null);

        DOVirtual.Float(1.0f, 0.25f, 0.5f, (t) => Time.timeScale = t).SetDelay(0.25f)
            .OnComplete(() => Time.timeScale = 1);

        float timeWaitDie = 0f;

        DOVirtual.DelayedCall(timeWaitDie, () =>
        {
            if (GameStateManager.CurrentState == GameState.WaitGameOver)
                GameStateManager.GameOver(data);
        }).SetUpdate(false).SetId(this);
    }

    protected override void WaitingGameComplete(object data)
    {
        MusicManager.Stop(null);
    }

    protected override void RebornContinueGame(object data)
    {
        gameOverScreen.Hide();
        inGameScreen.ShowTapToPlay();
        //GameStateManager.Init(null);
        //StartCoroutine(WaitToAutoPlay());
    }

    protected override void RebornCheckPointGame(object data)
    {
        gameOverScreen.Hide();
        GameStateManager.Init(null);
        StartCoroutine(WaitToAutoPlay());
    }
    IEnumerator WaitToAutoPlay()
    {
        var wait01s = new WaitForSeconds(0.1f);
        var wait05s = new WaitForSeconds(1.5f);
        while (GameStateManager.CurrentState != GameState.Ready)
            yield return wait01s;
        yield return wait05s;
        GameStateManager.Play(null);
    }

    private void LateUpdate()
    {
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    if (GameStateManager.CurrentState == GameState.
        //        WaitGameOver ||
        //        GameStateManager.CurrentState == GameState.WaitComplete)
        //        return;

        //    var checkPopup = UIManager.listPopup.FirstOrDefault(x => x.Status == UIAnimStatus.IsShow);
        //    if (checkPopup != null)
        //    {
        //        checkPopup.Hide();
        //        return;
        //    }

        //    if (UIManager.listScreen.Any(x => x.IsAnimation))
        //        return;

        //    if (UIManager.listPopup.Any(x => x.IsAnimation))
        //        return;

        //    if (gameOverScreen.Status == UIAnimStatus.IsShow
        //           && (GameStateManager.CurrentState == GameState.Complete || GameStateManager.CurrentState == GameState.GameOver))
        //    {
        //        gameOverScreen.Back();
        //    }
        //    else if (inGameScreen.Status == UIAnimStatus.IsShow)
        //    {
        //        if (GameStateManager.CurrentState == GameState.Play)
        //        {
        //            GameStateManager.Pause(null);
        //        }
        //        else if (GameStateManager.CurrentState == GameState.Pause)
        //        {
        //            GameStateManager.Play(null);
        //        }
        //    }
        //    else if (mainScreen.Status == UIAnimStatus.IsShow && GameStateManager.CurrentState == GameState.Idle)
        //    {
        //        PopupMes.Show("QUIT!?", "Do you realy want to quit!?",
        //            "Cancel", null,
        //            "Quit", () =>
        //            {
        //                DataManager.Save(true);
        //                Application.Quit();
        //            });
        //    }
        //}
    }
}