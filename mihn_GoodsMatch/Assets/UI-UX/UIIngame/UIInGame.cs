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
    private PopupBuyBuff popupBuyBuff = null;

    [Header("UIInformation Bartender")]
    [SerializeField] UIInfo uiInforNormal = null;
    [SerializeField] UIAnimation uiBottomAnim = null;

    [Header("UIInformation Bartender")]
    [SerializeField] UIInfo_Bartender uiInfor_Bartender = null;

    [Header("Buff")]
    [SerializeField]
    private Button buffHintButton = null;
    [SerializeField]
    private Text hintCountText = null;
    [SerializeField]
    private Button buffRestartButton = null;
    [SerializeField]
    private Text restartCountText = null;
    [SerializeField] GameObject obj_IdleIcon;
    [SerializeField] GameObject obj_IdleLockIcon;
    [SerializeField] GameObject obj_SwapIcon;
    [SerializeField] GameObject obj_SwapLockIcon;

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

    [SerializeField] UIPopupSetting popupSetting;

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
            if(GameStateManager.CurrentState == GameState.Play)
            {
                GameStateManager.Pause(null);
                popupSetting.OnShow();
            }
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
        bool hintUnlocked = DataManager.UserData.level >= 4;
        bool swapUnlocked = DataManager.UserData.level >= 9;
        switch (current)
        {
            case GameState.Init:
            case GameState.Restart:
                playButton?.gameObject.SetActive(true);
                ingameBG.sprite = backgroundSprites[Random.Range(0, backgroundSprites.Count)];
                if(DataManager.currGameMode == eGameMode.Normal)
                {
                    levelTxt.text = $"LEVEL {DataManager.levelSelect}";
                }else if(DataManager.currGameMode == eGameMode.Bartender)
                {
                    if(DataManager.UserData.bartenderLevel % 2 == 0)
                    {
                        levelTxt.text = $"DAY {DataManager.UserData.bartenderLevel / 2 + 1}";
                    }else
                        levelTxt.text = $"NIGHT {DataManager.UserData.bartenderLevel / 2 + 1}";
                }else
                    levelTxt.text = $"LEVEL {DataManager.UserData.challengeLevel+1}";
                //levelTxt.text = DataManager.currGameMode == eGameMode.Normal ? $"LEVEL {DataManager.levelSelect}" 
                //    : DataManager.UserData.bartenderLevel % 2 == 0 ? $"DAY {DataManager.UserData.bartenderLevel/2 + 1}" : $"NIGHT {DataManager.UserData.bartenderLevel / 2 + 1}";
                uiInforNormal.Hide();
                uiBottomAnim?.Hide();
                uiInfor_Bartender?.Hide();
                img_Alert?.gameObject.SetActive(false);
                buffHintButton.interactable = hintUnlocked;
                buffRestartButton.interactable = swapUnlocked;
                obj_IdleIcon?.SetActive(hintUnlocked);
                obj_IdleLockIcon?.SetActive(!hintUnlocked);
                obj_SwapIcon?.SetActive(swapUnlocked);
                obj_SwapLockIcon?.SetActive(!swapUnlocked);
                hintCountText.text = !hintUnlocked ? "lv.5" : DataManager.UserData.totalHintBuff > 0 ? DataManager.UserData.totalHintBuff.ToString() : "+";
                restartCountText.text = !swapUnlocked ? "lv.10" : DataManager.UserData.totalSwapBuff > 0 ? DataManager.UserData.totalSwapBuff.ToString() : "+";
                break;
            case GameState.Ready:
                playButton?.gameObject.SetActive(true);
                anim.Show();
                break;
            case GameState.Play:
                playButton?.gameObject.SetActive(false);
                resumeButton?.gameObject.SetActive(false);
                backButton?.gameObject.SetActive(false);
                if(DataManager.currGameMode == eGameMode.Normal|| DataManager.currGameMode == eGameMode.Challenge)
                {
                    uiBottomAnim.Show();
                    uiInforNormal.Show();
                }
                else if(last != GameState.Pause)
                {
                    uiInfor_Bartender.Show();
                }
                break;
            case GameState.Pause:
                popupSetting.OnShow();
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
