using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;
using DG.Tweening;
using Random = System.Random;

public class BarRequest : MonoBehaviour
{
    [SerializeField] GameItemAsset gameItemAsset;
    [SerializeField] RequestManager requestManager;
    [SerializeField] BoardGame_Bartender boardGame;

    [Header("Visual")]
    [SerializeField] Sprite[] outsideSprs;
    [SerializeField] Sprite[] alertSprs;
    [SerializeField] float[] processPosition;
    [SerializeField] Transform processContainer;
    [SerializeField] SpriteRenderer outsideSR;
    [SerializeField] SpriteRenderer alertSR;

    [Header("Items on Bar")]
    [SerializeField] Vector3 startPosOffset;
    [SerializeField] float itemDistance = 1.2f;

    [Header("TimeLine")]
    [SerializeField] float waitMatchedFXTime = 1f;
    [SerializeField] float waitMissedFXTime = 1f;

    [Header("TimeProcess")]
    [SerializeField] SpriteRenderer processBG;
    [SerializeField] SpriteRenderer waitTimeProcess;
    [SerializeField] float fillProcessFullHeight;
    [SerializeField] Color bgMissedColor;
    [SerializeField] Color bgNormalColor;
    [SerializeField] SpriteRenderer iconTick;
    [SerializeField] SpriteRenderer iconMiss;

    [Header("VFX")]
    [SerializeField] ParticleSystem completeParticle;
    [SerializeField] float alertTime = 5f;
    [SerializeField] float alertSingleTime = 1f;

    [Header("Guest Config")]
    [SerializeField] GuestController guestPrefab;
    [SerializeField] Vector3 posOutSide;
    [SerializeField] Vector3 postInside;
    [SerializeField] float guestMovingTime = 1f;
    private GuestController currGuestController = null;

    [Header("Debug")]
    [ReadOnly, SerializeField] int currId;

    public List<Goods_Item> requestingItems = new List<Goods_Item>();
    public bool IsRequesting;
    private RequestDatum currDatum = null;
    public RequestDatum CurrRequest { get => currDatum; }
    private float elapsedTime;

    private void Awake()
    {
        if(requestManager == null)
            requestManager = GetComponentInParent<RequestManager>();
        if (boardGame == null)
            boardGame = requestManager.boardGame;
        guestPrefab.CreatePool(2);
    }
    private void Start()
    {
        iconTick.gameObject.SetActive(false);
        iconMiss.gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        this.RegisterListener((int)EventID.OnMatchedRightRequest, OnMatchedRequest);
    }
    private void OnDisable()
    {
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnMatchedRightRequest, OnMatchedRequest);
        StopAllCoroutines();
        HideAll();
    }
    private void LateUpdate()
    {
        if(!IsRequesting)
            return;
        elapsedTime += Time.deltaTime;
        waitTimeProcess.SetSizeY(fillProcessFullHeight * Mathf.Max(0,  (currDatum.waitTime - elapsedTime)/ currDatum.waitTime));
        if (elapsedTime >= currDatum.waitTime - alertTime && !alertSR.gameObject.activeSelf)
            StartCoroutine(YieldAlert());
        if(elapsedTime >= currDatum.waitTime && requestingItems.Count > 0)
        {
            OnTimeOut();
        }
    }
    public void ShowRequestItem()
    {
        gameObject.SetActive(true);
        alertSR.gameObject.SetActive(false);
        processBG.SetAlpha(0);
        waitTimeProcess.SetAlpha(0);
        SetElemetsActive(false);
        StartCoroutine(YieldInitItems());
    }
    private IEnumerator YieldInitItems()
    {
        outsideSR.DOColor(bgNormalColor, 0);

        //Create request datum
        var datum = requestManager.CreateRequest();
        currDatum = datum;
        currId = datum.id;
        outsideSR.sprite = outsideSprs[datum.types.Count - 1];
        alertSR.sprite = alertSprs[datum.types.Count - 1];
        alertSR.gameObject.SetActive(false);
        processContainer.SetLocalX(processPosition[datum.types.Count - 1]);
        waitTimeProcess.SetSizeY(fillProcessFullHeight);

        yield return YieldInitGuest();
        SetElemetsActive(true);

        yield return outsideSR.DOFade(1f, 0.25f).SetUpdate(false).WaitForCompletion();
        for (int i = 0; i < datum.types.Count; i++)
        {
            var item = gameItemAsset.GetItemByType(datum.types[i]).itemProp.Spawn();
            item.transform.parent = transform;
            item.transform.localPosition = GetPosOnBar(datum.types.Count, i);
            item.OnInit(active: false);
            SoundManager.Play(GameConstants.sound_ItemSpawn);
            requestingItems.Add(item);
            yield return new WaitForSeconds(0.1f);
        }
        elapsedTime = 0;
        IsRequesting = true;
        processBG.SetAlpha(1f);
        waitTimeProcess.SetAlpha(1f);
        this.PostEvent((int)EventID.OnNewRequestCreated, requestingItems);
    }
    private void SetElemetsActive(bool isActive)
    {
        outsideSR.gameObject.SetActive(isActive);
        processContainer.gameObject.SetActive(isActive);
    }
    private void OnMatchedRequest(object obj)
    {
        if (!IsRequesting)
            return;
        var item = (Goods_Item)obj;
        if (requestingItems.Contains(item))
        {
            if (currGuestController != null)
            {
                currGuestController.ChangeAnim(GuestAnimations.happy);
                string soundName = GameUtilities.GetRandomBool() ? GameConstants.sound_CatHappy1 : GameConstants.sound_CatHappy2;
                SoundManager.Play(soundName);
            }
            requestingItems.Remove(item);
        }
        if(requestingItems.Count == 0)
        {
            IsRequesting = false;
            StopCoroutine(YieldAlert());
            alertSR.gameObject.SetActive(false);
            StartCoroutine(YieldLeave(true));
        }
    }
    private void OnTimeOut()
    {
        IsRequesting = false;
        this.PostEvent((int)EventID.OnRequestTimeout, this.currDatum);
        BoardGame_Bartender.instance?.OnRequestTimeout(this.requestingItems);
        if (currGuestController != null)
        {
            currGuestController.ChangeAnim(GuestAnimations.angryLeaveAnim);
            string soundName = GameUtilities.GetRandomBool() ? GameConstants.sound_CatAngryLeave1 : GameConstants.sound_CatAngryLeave2;
            SoundManager.Play(soundName);
        }
            
        StartCoroutine(YieldLeave(false));
    }
    public void OnLevelEnd(bool isWin)
    {
        IsRequesting = false;
        StartCoroutine(YieldLeave(isWin));
    }
    public IEnumerator YieldLeave(bool winRequest)
    {
        if (!winRequest)
        {
            //Show angry anim then leave
            outsideSR.DOColor(bgMissedColor, 0.25f);
            iconMiss.gameObject.SetActive(true);
            iconMiss.transform.localScale = Vector3.one;
            yield return iconMiss.transform.DOScale(1.2f, waitMatchedFXTime).OnComplete(() =>
            {
                iconTick.gameObject.SetActive(false);
            }).WaitForCompletion();

            HideAll();

            if (currGuestController != null)
            {
                currGuestController.Move(currGuestController.transform.position, posOutSide + transform.position, true);
                currGuestController.transform.SetParent(null, true);
            }
        }
        else
        {
            //Show happy anim then leave
            completeParticle.Play();
            yield return new WaitForSeconds(waitMatchedFXTime * 0.5f);
            if (currGuestController != null)
            {
                currGuestController.Move(currGuestController.transform.position, posOutSide + transform.position, true);
                currGuestController.transform.SetParent(null, true);
            }
            yield return new WaitForSeconds(waitMatchedFXTime * 0.25f);
            HideAll();
            yield return new WaitForSeconds(waitMatchedFXTime * 0.25f);
        }
        ClearAll();
        yield return new WaitForEndOfFrame();
        gameObject.SetActive(false);
    }
    private IEnumerator YieldAlert()
    {
        alertSR.gameObject.SetActive(true);
        if (currGuestController != null)
            currGuestController.ChangeAnim(GuestAnimations.angryWaitAnim);
        int count = Mathf.FloorToInt(alertTime / alertSingleTime);
        string soundName = GameUtilities.GetRandomBool() ? GameConstants.sound_CatWaitlong1 : GameConstants.sound_CatWaitlong2;
        SoundManager.Play(soundName);
        for (int i = 0; i < count-1; i++)
        {
            yield return alertSR.DOFade(1f, alertSingleTime*0.5f).WaitForCompletion();
            yield return alertSR.DOFade(0.25f, alertSingleTime * 0.5f).WaitForCompletion();
        }
        yield return alertSR.DOFade(1f, alertSingleTime * 0.5f).WaitForCompletion();
    }

    private void HideAll()
    {
        outsideSR.SetAlpha(0f);
        processBG.SetAlpha(0f);
        waitTimeProcess.SetAlpha(0f);
        iconTick.gameObject.SetActive(false);
        iconMiss.gameObject.SetActive(false);
        completeParticle.Stop();
        alertSR.gameObject.SetActive(false);
    }
    private Vector3 GetPosOnBar(int totalItems, int index)
    {
        var startPos = startPosOffset;

        if (totalItems == 2)
            startPos.x -= (itemDistance*0.5f);
        if (index == 1)
            return startPos + Vector3.right.Multi(itemDistance, 0, 0);
        else if (index == 2)
            return startPos + Vector3.left.Multi(itemDistance, 0, 0);
        else
            return startPos;
    }
    public void ClearAll()
    {
        if(currGuestController)
            currGuestController = null;
        foreach (var item in requestingItems)
        {
            if(item.gameObject != null)
            item.Recycle();
        }
        requestingItems.Clear();
        IsRequesting = false;
        HideAll();
    }

    #region Guest Controller
    private IEnumerator YieldInitGuest()
    {
        currGuestController = null;
        currGuestController = guestPrefab.Spawn(this.transform);
        currGuestController.ChangeSkin();
        yield return currGuestController.YieldMove(posOutSide + transform.position, postInside + transform.position, false, guestMovingTime, callback: () => {
            currGuestController.ChangeAnim(GuestAnimations.idleAnims[UnityEngine.Random.Range(0, GuestAnimations.idleAnims.Length)]);
        });
    }
    #endregion
}
