using Base;
using Base.Ads;
using DG.Tweening;
using MyBox;
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
    private UIPopupChallenge popupChallenge = null;
    [SerializeField]
    UILevelSelect popupLevelSelect = null;

    public static UIMainScreen MainScreen => instance?.mainScreen;

    [SerializeField]
    private UIInGame inGameScreen = null;

    [SerializeField] private UIGameOver gameOverScreen = null;
    [SerializeField] private UIGameOver_Bartender gameOver_Bar_Screen = null;
    public static UIGameOver GameOverScreen => instance?.gameOverScreen;
    public static UIGameOver_Bartender GameOver_Bar_Screen => instance?.gameOver_Bar_Screen;

    private DateTime startLoadTime = DateTime.Now;

    private GameConfig gameConfig => DataManager.GameConfig;
    private UserData user => DataManager.UserData;

    [Header("Hide Object in Game")]
    [SerializeField]
    protected Toggle hideObjectInGameToggle = null;
    [SerializeField]
    protected List<GameObject> hideObjectInGame = new List<GameObject>();

    public static bool IsHideObjectInGame
    {
        get
        {
            if (instance != null && instance.hideObjectInGameToggle != null)
                return instance.hideObjectInGameToggle.isOn;
            return false;
        }
    }
    public void CheckHideObject(bool isOn)
    {
        foreach (var i in hideObjectInGame)
        {
            if (i != null && i.gameObject != null)
            {
                var c = i.gameObject.GetOrAddComponent<CanvasGroup>();
                c.alpha = isOn ? 0 : 1;
            }
        }

    }

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

        if (hideObjectInGameToggle)
        {
            CheckHideObject(hideObjectInGameToggle.isOn);
            hideObjectInGameToggle.onValueChanged.AddListener((isOn) => CheckHideObject(isOn));
        }
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

        yield return AdsManager.DOInit();

#if USE_FIREBASE
        var remote = new GameConfig();
        var defaultRemoteConfig = new Dictionary<string, object>
        {
            {"suggestUpdateVersion" , gameConfig.suggestUpdateVersion },
            {"timePlayToShowAd" , gameConfig.timePlayToShowAd },
            {"timePlayToShowAdReduce" , gameConfig.timePlayToShowAdReduce },
            {"timePlayToShowOpenAd" , gameConfig.timePlayToShowOpenAd },
            {"timeToWaitOpenAd" , gameConfig.timeToWaitOpenAd },
            {"adShowFromLevel" , gameConfig.adShowFromLevel },
            {"adInterOnComplete" , gameConfig.adInterOnComplete },
            {"adUseBackup" , gameConfig.adUseBackup },
            {"adUseOpenBackup" , gameConfig.adUseOpenBackup },
            {"adUseNative" , gameConfig.adUseNative },
            {"adUseInPlay" , gameConfig.adUseInPlay },
            {"forceInterToReward" , gameConfig.forceInterToReward },
            {"forceRewardToInter" , gameConfig.forceRewardToInter },
            {"forceInterEverywhere" , gameConfig.forceInterEverywhere }
        };

        yield return FirebaseManager.DoCheckStatus(defaultRemoteConfig);

        var cacheExpirationHours = 12;
#if UNITY_EDITOR
        cacheExpirationHours = 0;
#endif
        yield return FirebaseManager.DoFetchRemoteData((status) =>
        {
            if (status == FirebaseStatus.Success && user != null && gameConfig != null)
            {
                gameConfig.suggestUpdateVersion = FirebaseManager.RemoteGetValueInt("suggestUpdateVersion");
                gameConfig.timePlayToShowAd = FirebaseManager.RemoteGetValueInt("timePlayToShowAd");
                gameConfig.timePlayToShowAdReduce = FirebaseManager.RemoteGetValueInt("timePlayToShowAdReduce");
                gameConfig.timePlayToShowOpenAd = FirebaseManager.RemoteGetValueInt("timePlayToShowOpenAd");
                gameConfig.timeToWaitOpenAd = FirebaseManager.RemoteGetValueInt("timeToWaitOpenAd");
                gameConfig.adInterOnComplete = FirebaseManager.RemoteGetValueBoolean("adInterOnComplete");
                gameConfig.adShowFromLevel = FirebaseManager.RemoteGetValueInt("adShowFromLevel");
                gameConfig.adUseBackup = FirebaseManager.RemoteGetValueBoolean("adUseBackup");
                gameConfig.adUseOpenBackup = FirebaseManager.RemoteGetValueBoolean("adUseOpenBackup");
                gameConfig.adUseNative = FirebaseManager.RemoteGetValueBoolean("adUseNative");
                gameConfig.adUseInPlay = FirebaseManager.RemoteGetValueBoolean("adUseInPlay");
                gameConfig.forceInterToReward = FirebaseManager.RemoteGetValueBoolean("forceInterToReward");
                gameConfig.forceRewardToInter = FirebaseManager.RemoteGetValueBoolean("forceRewardToInter");
                gameConfig.forceInterEverywhere = FirebaseManager.RemoteGetValueBoolean("forceInterEverywhere");
            }

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("DoFetchRemoteData_" + status.ToString());
                DebugMode.Log(JsonUtility.ToJson(gameConfig));
            });
        }, cacheExpirationHours);
#endif

        if (user.VersionInstall == 0)
        {
            user.VersionInstall = UIManager.BundleVersion;
#if USE_FIREBASE
            FirebaseManager.SetUser("Type", "New");
#endif
        }
        else if (user.VersionInstall != UIManager.BundleVersion)
        {
#if USE_FIREBASE
            FirebaseManager.SetUser("Type", "Update");
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
    }

    public void ForeUpdate()
    {
        string title = "New version avaiable!";
        string body = "We are trying to improve the game quality by updating it regularly.\nPlease update new version for the best experience!";
        PopupMes.Show(title, body,
            "Update", () =>
            {
                if (!string.IsNullOrEmpty(UIManager.UrlAndroid))
                    Application.OpenURL(UIManager.UrlAndroid);
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
            gameOver_Bar_Screen.Hide();
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
        LoadGameContent.PrepairDataToPlay(data);
        popupLevelSelect.OnHide();
        coinScreen.Hide();
        gameOverScreen.Hide();
        gameOver_Bar_Screen.Hide();
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

        yield return new WaitForSeconds(1f);

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
        if (DataManager.currGameMode == eGameMode.Normal)
            gameOverScreen.Show(GameState.GameOver, data);
        else
            gameOver_Bar_Screen.Show(GameState.GameOver, data);
    }

    protected override void CompleteGame(object data)
    {
        //inGameScreen.Hide();
        if(DataManager.currGameMode == eGameMode.Normal)
            gameOverScreen.Show(GameState.Complete, data);
        else
            gameOver_Bar_Screen.Show(GameState.Complete, data);
    }

    protected override void ReadyGame(object data)
    {
        this.PostEvent((int)EventID.OnPlayMusic, "Bgm_Gameplay_loop_MP3");
        mainScreen.Hide();
        StartCoroutine(WaitForLoading(() =>
        {
            UILoadGame.Hide();
            GameStateManager.Play(data);
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
        gameOver_Bar_Screen.Hide();
        this.PostEvent((int)EventID.OnPlayMusic, "Bgm_Gameplay_loop_MP3");
        //inGameScreen.ShowTapToPlay();
        //GameStateManager.Init(null);
        //StartCoroutine(WaitToAutoPlay());
    }

    protected override void RebornCheckPointGame(object data)
    {
        gameOverScreen.Hide();
        gameOver_Bar_Screen.Hide();
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