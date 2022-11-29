using Lean.Touch;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    public static Vector3 Director => instance.director;
    public static float Speed => instance == null ? 0 : instance.speed;
    public static bool IsMoving => Speed >= 0.15;

    public static System.Action OnTapHandle;
    public static System.Action OnTouchDown;
    public static System.Action OnTouchUp;

    [SerializeField]
    private Image dragAnchorImg = null, dragPointImg = null;
    private Vector2 dragAnchorOriginal = Vector2.zero;
    [SerializeField]
    private float dragDistance = 1.5f;

    public LeanScreenDepth ScreenDepth = new LeanScreenDepth(LeanScreenDepth.ConversionType.DepthIntercept);

    protected float fingerDelta { get; set; }
    protected Transform dragAnchorTf, dragPointTf;
    protected Vector3 director = Vector3.zero;
    protected float speed { get; set; }
    protected float dragAnchorImgAlpha { get; set; }
    protected float dragPointImgAlpha { get; set; }
    protected float lastFingerTapAge { get; set; }
    protected float minSpeed { get; set; }


    void Awake()
    {
        instance = this;
        dragAnchorTf = dragAnchorImg.transform;
        dragPointTf = dragPointImg.transform;
        dragAnchorImgAlpha = dragAnchorImg.color.a;
        dragPointImgAlpha = dragPointImg.color.a;
        //GameStateManager.OnStateChanged += GameStateManager_OnStateChanged;

        dragAnchorOriginal = dragAnchorImg.rectTransform.anchoredPosition.y * Vector2.up;
    }

    void Start()
    {
        Application.targetFrameRate = 60;

        LeanTouch.OnFingerTap += HandleFingerTap;
        LeanTouch.OnFingerSet += HandleFingerSet;
        LeanTouch.OnFingerUp += HandleFingerUp;
        LeanTouch.OnFingerDown += LeanTouch_OnFingerDown;

    }
    private void OnEnable()
    {
    }
    private void OnDisable()
    {
    }

    private void LeanTouch_OnFingerDown(LeanFinger obj)
    {
        OnTouchDown?.Invoke();
    }

    private void GameStateManager_OnStateChanged(GameState current, GameState last, object data)
    {
        if (current == GameState.Init || current == GameState.Idle)
        {
            HandleFingerUp(null);
            minSpeed = 0.2f;
            director = Vector3.zero;
        }
        else if (current == GameState.Pause)
        {
            HandleFingerUp(null);
        }

        if(current == GameState.LoadGame || current == GameState.Init)
        {
            dragAnchorImg.rectTransform.anchoredPosition = dragAnchorOriginal;
            dragPointImg.rectTransform.anchoredPosition = dragAnchorOriginal;
            dragAnchorImg.SetAlpha(0.2f);
            dragPointImg.SetAlpha(0.2f);
        }
    }

    private void HandleFingerUp(LeanFinger obj)
    {
        dragAnchorImg.rectTransform.anchoredPosition = dragAnchorOriginal;
        dragPointImg.rectTransform.anchoredPosition = dragAnchorOriginal;
        dragAnchorImg.SetAlpha(0);
        dragPointImg.SetAlpha(0);
        director = Vector3.zero;
        speed = 0;
        lastFingerTapAge = 0;
        OnTouchUp?.Invoke();
    }

    private void HandleFingerSet(LeanFinger finger)
    {
        if (GameStateManager.CurrentState != GameState.Play)
            return;

        var startPosition = ScreenDepth.Convert(finger.StartScreenPosition, gameObject);
        var endPosition = ScreenDepth.Convert(finger.ScreenPosition, gameObject);
        var fingerOffset = endPosition - startPosition;

        dragPointTf.DOKill();
        dragPointImg.SetAlpha(dragPointImgAlpha);

        var s = fingerOffset.magnitude;
        if (s < dragDistance)
            dragPointTf.position = endPosition;
        else
        {
            dragPointTf.position = startPosition + fingerOffset.normalized * dragDistance;
        }

        fingerOffset = dragPointTf.position - dragAnchorTf.position;
        if (s < minSpeed * dragDistance)
        {
            speed = 0;
        }
        else
        {
            speed = 1;
            director.Set(fingerOffset.x, 0, fingerOffset.y);
            dragAnchorTf.DOKill();
            dragAnchorTf.position = startPosition;
            dragAnchorImg.SetAlpha(dragAnchorImgAlpha);
        }
    }

    private void HandleFingerTap(LeanFinger finger)
    {
        if (GameStateManager.CurrentState != GameState.Play)
            return;
        if (finger.ScreenPosition.y < 80 && finger.ScreenPosition.x < 80)
            return;

        OnTapHandle?.Invoke();
    }
}
