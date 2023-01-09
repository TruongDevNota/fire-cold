using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using Base.Ads;
using System.Collections;

[RequireComponent(typeof(UIAnimation))]
public class UIInGame : MonoBehaviour
{
    [Header("Base")]
    [SerializeField]
    private UIAnimation anim = null;
    public UIAnimStatus Status => anim.Status;

    [SerializeField]
    List<Sprite> backgroundSprites;
    [SerializeField]
    Image ingameBG;
    [SerializeField] 
    Text levelTxt = null;
    [SerializeField]
    private UIAnimation uiTopAnim = null;
    [SerializeField]
    private UIAnimation uiBottomAnim = null;
    [SerializeField]
    private PopupBuyBuff popupBuyBuff = null;

    [Header("Buff")]
    [SerializeField]
    private Button buffHintButton = null;
    [SerializeField]
    private Text hintCountText = null;
    [SerializeField]
    private Button buffRestartButton = null;
    [SerializeField]
    private Text restartCountText = null;

    [SerializeField]
    private GameObject touchPanel = null;
    [SerializeField]
    private Button settingButton = null;
    [SerializeField]
    private Button resumeButton = null;
    [SerializeField]
    private Button playButton = null;
    [SerializeField]
    private Button backButton = null;

    [Header("PerfectToast")]
    [SerializeField]
    private UIPerfectToast perfectToast = null;

    [Header("Alert")]
    [SerializeField] Image img_Alert;
    [SerializeField] float fadeTime;

    private void Awake()
    {
        if (anim == null)
            anim = GetComponent<UIAnimation>();
    }

    private void OnEnable()
    {
        UserData.OnHintBuffChanged += OnHintBuffChange;
        UserData.OnSwapBuffChanged += OnSwapBuffChange;
        this.RegisterListener((int)EventID.OnAlertTimeout, DoAlert);
    }
    private void OnDisable()
    {
        UserData.OnHintBuffChanged -= OnHintBuffChange;
        UserData.OnSwapBuffChanged -= OnSwapBuffChange;
        EventDispatcher.Instance?.RegisterListener((int)EventID.OnAlertTimeout, DoAlert);
    }

    private void Start()
    {
        settingButton?.onClick.AddListener(() =>
        {
            GameStateManager.Pause(null);
        });

        backButton?.onClick.AddListener(backButtonOnClick);

        resumeButton?.onClick.AddListener(() =>
        {
            GameStateManager.Play(null);
        });

        playButton?.onClick.AddListener(PlayButtonOnClick);
        buffHintButton?.onClick.AddListener(BuffHintButtonOnclick);
        buffRestartButton?.onClick.AddListener(BuffSwapClick);
        
        GameStateManager.OnStateChanged += GameStateManager_OnStateChanged;
    }

    private void OnDestroy()
    {
        GameStateManager.OnStateChanged -= GameStateManager_OnStateChanged;
    }
    private void backButtonOnClick()
    {
        if (GameStateManager.CurrentState == GameState.Pause)
        {
            //PopupMes.Show("BACK!?", "Do you really want back to main!?",
            //    "No", null,
            //    "Ok", () => GameStateManager.Idle(null));
            GameStateManager.Idle(null);
        }
    }

    private void PlayButtonOnClick()
    {
        GameStateManager.Play(null);
    }

    private void BuffHintButtonOnclick()
    {
        SoundManager.Play("1. Click Button");

        if (DataManager.UserData.totalHintBuff > 0)
        {
            this.PostEvent((int)EventID.OnBuffHint);
            DataManager.UserData.totalHintBuff--;
            DataManager.Save();
        }
        else
        {
            popupBuyBuff.Onshow(BuffType.Hint);
        }
    }

    private void BuffSwapClick()
    {
        SoundManager.Play("1. Click Button");
        if (DataManager.UserData.totalSwapBuff > 0)
        {
            this.PostEvent((int)EventID.OnBuffSwap);
            DataManager.UserData.totalSwapBuff--;
        }
        else
        {
            popupBuyBuff.Onshow(BuffType.Swap);
        }
    }

    private void GameStateManager_OnStateChanged(GameState current, GameState last, object data)
    {
        switch (current)
        {
            case GameState.Init:
            case GameState.Restart:
                playButton?.gameObject.SetActive(true);
                ingameBG.sprite = backgroundSprites[Random.Range(0, backgroundSprites.Count)];
                levelTxt.text = $"LEVEL {DataManager.levelSelect}";
                uiTopAnim.Hide();
                uiBottomAnim.Hide();
                img_Alert?.gameObject.SetActive(false);
                break;
            case GameState.Ready:
                playButton?.gameObject.SetActive(true);
                anim.Show();
                CountDown();
                break;
            case GameState.Play:
                hintCountText.text = DataManager.UserData.totalHintBuff > 0 ? DataManager.UserData.totalHintBuff.ToString() : "+";
                restartCountText.text = DataManager.UserData.totalSwapBuff > 0 ? DataManager.UserData.totalSwapBuff.ToString() : "+";
                playButton?.gameObject.SetActive(false);
                resumeButton?.gameObject.SetActive(false);
                backButton?.gameObject.SetActive(false);
                uiBottomAnim.Show();
                uiTopAnim.Show();
                break;
            case GameState.Pause:
                playButton?.gameObject.SetActive(false);
                resumeButton?.gameObject.SetActive(false);
                backButton?.gameObject.SetActive(true);
                break;
            case GameState.GameOver:
                resumeButton?.gameObject.SetActive(false);
                backButton?.gameObject.SetActive(false);
                break;
            case GameState.Complete:
                break;
            case GameState.RebornContinue:
                img_Alert?.gameObject.SetActive(false);
                break;
        }
    }

    public void Show()
    {
        anim.Show();
    }

    public void Hide()
    {
        anim.Hide();
    }

    #region Ready
    public void CountDown()
    {
        StartCoroutine(DOStartCountDown());
    }

    public IEnumerator DOStartCountDown()
    {
        if (GameStateManager.CurrentState == GameState.Ready)
        {
            var wait1 = new WaitForSeconds(1);
            DoPlaySoundCount();
            ShowToastPerfect("3", 0.7f, false);
            yield return wait1;
            DoPlaySoundCount();
            ShowToastPerfect("2", 0.7f, false);
            yield return wait1;
            DoPlaySoundCount();
            ShowToastPerfect("1", 0.7f, false);
            yield return wait1;
            //DoPlaySoundCount();
            //ShowToastPerfect("go", 0.8f, true);
        }
        GameStateManager.Play(null);
    }

    private void DoPlaySoundCount()
    {
        SoundManager.Play("sfx_task");
    }

    public void ShowToastPerfect(string second, float timeAutoHide, bool waitAnimation)
    {
        if (perfectToast != null)
            perfectToast.Show(second.ToUpper(), timeAutoHide, waitAnimation);
    }
    #endregion

    public void ShowTapToPlay()
    {
        playButton?.gameObject.SetActive(true);
    }

    public void Ins_BtnBack()
    {
        GameStateManager.Idle(null);
    }

    public void Ins_Toast()
    {
        UIToast.ShowNotice("This is toast!!");
    }

    public void Ins_Popup()
    {
        PopupMes.Show("This is Popup", "This is popup description",
            "OK", () =>
            {
                Debug.Log("Popup -> OK");
            },
            "Cancel", () =>
            {
                Debug.Log("Popup -> Cancel");
            });
    }

    private void OnHintBuffChange(int change, int current)
    {
        hintCountText.text = current > 0 ? current.ToString() : "+";
    }

    private void OnSwapBuffChange(int change, int current)
    {
        restartCountText.text = current > 0 ? current.ToString() : "+";
    }

    private void DoAlert(object obj)
    {
        StartCoroutine(YieldShowAlert());
    }

    private IEnumerator YieldShowAlert()
    {
        img_Alert.DOFade(0, 0);
        img_Alert.gameObject.SetActive(true);
        int count = 0;
        while(GameStateManager.CurrentState == GameState.Play)
        {
            string soundName = count % 2 == 0 ? "12.Alert_chan" : "13.Alert_le";
            SoundManager.Play(soundName);
            yield return img_Alert.DOFade(1f, fadeTime*0.5f).WaitForCompletion();
            yield return img_Alert.DOFade(0f, fadeTime * 0.5f).WaitForCompletion();
            count++;
        }
        img_Alert.gameObject.SetActive(false);
    }
}
