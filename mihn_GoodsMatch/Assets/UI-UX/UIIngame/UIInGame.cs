using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(UIAnimation))]
public class UIInGame : MonoBehaviour
{
    [Header("Base")]
    [SerializeField]
    private UIAnimation anim = null;
    public UIAnimStatus Status => anim.Status;

    [SerializeField]
    private GameObject touchPanel = null;
    [SerializeField]
    private Button pauseButton = null;
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
        GameStateManager.OnStateChanged += GameStateManager_OnStateChanged;
    }
    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= GameStateManager_OnStateChanged;
    }

    private void Start()
    {
        pauseButton?.onClick.AddListener(() =>
        {
            GameStateManager.Pause(null);
        });

        backButton?.onClick.AddListener(backButtonOnClick);

        resumeButton?.onClick.AddListener(() =>
        {
        });
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

    private void GameStateManager_OnStateChanged(GameState current, GameState last, object data)
    {
        switch (current)
        {
            case GameState.Init:
                break;
            case GameState.Play:

                playButton?.gameObject.SetActive(false);
                pauseButton?.gameObject.SetActive(true);
                resumeButton?.gameObject.SetActive(false);
                backButton?.gameObject.SetActive(false);

                touchPanel?.SetActive(true);
                break;
            case GameState.Pause:
                playButton?.gameObject.SetActive(false);
                pauseButton?.gameObject.SetActive(false);
                resumeButton?.gameObject.SetActive(true);
                backButton?.gameObject.SetActive(true);
                break;
            case GameState.GameOver:
                resumeButton?.gameObject.SetActive(false);
                pauseButton?.gameObject.SetActive(false);
                backButton?.gameObject.SetActive(false);
                break;
            case GameState.Complete:
                break;
        }
    }

    public void Show()
    {
        anim.Show(()=> {
            pauseButton?.gameObject.SetActive(false);
            resumeButton?.gameObject.SetActive(false);
            backButton?.gameObject.SetActive(false);
            playButton?.gameObject.SetActive(false);
            touchPanel?.SetActive(false);
        }, ()=> {
            playButton?.gameObject.SetActive(true);
            touchPanel?.SetActive(true);
        });
    }

    public void Hide()
    {
        anim.Hide();
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
}
