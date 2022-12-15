using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using Base.Ads;

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

    private void Awake()
    {
        if (anim == null)
            anim = GetComponent<UIAnimation>();
    }

    private void OnEnable()
    {
        UserData.OnHintBuffChanged += OnHintBuffChange;
        UserData.OnSwapBuffChanged += OnSwapBuffChange;
    }
    private void OnDisable()
    {
        UserData.OnHintBuffChanged -= OnHintBuffChange;
        UserData.OnSwapBuffChanged -= OnSwapBuffChange;
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
        if(DataManager.UserData.totalHintBuff > 0)
        {
            this.PostEvent((int)EventID.OnBuffHint);
            DataManager.UserData.totalHintBuff--;
        }
        else
        {
            //Show Popup message

            AdsManager.ShowVideoReward((e, t) =>
            {
                if(e == AdEvent.Success)
                {
                    DataManager.UserData.totalHintBuff++;
                }
            }, "BuffHintButtonOnclick");
        }
    }

    private void BuffSwapClick()
    {
        if (DataManager.UserData.totalSwapBuff > 0)
        {
            this.PostEvent((int)EventID.OnBuffSwap);
            DataManager.UserData.totalSwapBuff--;
        }
        else
        {
            //Show Popup message

            AdsManager.ShowVideoReward((e, t) =>
            {
                if (e == AdEvent.Success)
                {
                    DataManager.UserData.totalSwapBuff++;
                }
            }, "BuffSwapButtonOnclick");
        }
    }

    private void GameStateManager_OnStateChanged(GameState current, GameState last, object data)
    {
        switch (current)
        {
            case GameState.Init:
            case GameState.Restart:
            case GameState.Ready:
                playButton?.gameObject.SetActive(true);
                hintCountText.text = DataManager.UserData.totalHintBuff > 0 ? DataManager.UserData.totalHintBuff.ToString() : "+";
                buffHintButton.gameObject.SetActive(false);
                buffRestartButton.gameObject.SetActive(false);
                ingameBG.sprite = backgroundSprites[Random.Range(0, backgroundSprites.Count)];
                break;
            case GameState.Play:
                hintCountText.text = DataManager.UserData.totalHintBuff > 0 ? DataManager.UserData.totalHintBuff.ToString() : "+";
                playButton?.gameObject.SetActive(false);
                resumeButton?.gameObject.SetActive(false);
                backButton?.gameObject.SetActive(false);
                buffHintButton.gameObject.SetActive(true);
                buffRestartButton.gameObject.SetActive(true);
                //touchPanel?.SetActive(true);
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
        }
    }

    public void Show()
    {
        anim.Show(()=> {
            playButton?.gameObject.SetActive(true);
            //touchPanel?.SetActive(false);
        }, ()=> {
            ShowTapToPlay();
            //touchPanel?.SetActive(true);
        });
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
}
