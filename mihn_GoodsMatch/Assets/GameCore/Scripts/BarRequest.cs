using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;
using DG.Tweening;

public class BarRequest : MonoBehaviour
{
    [SerializeField] GameItemAsset gameItemAsset;
    [SerializeField] RequestManager requestManager;

    [Header("Items on Bar")]
    [SerializeField] Vector3 startPosOffset;
    [SerializeField] float itemDistance = 1.2f;

    [Header("TimeLine")]
    [SerializeField] float waitMatchedFXTime = 1f;
    [SerializeField] float waitMissedFXTime = 1f;

    [Header("TimeProcess")]
    [SerializeField] SpriteRenderer bgSR;
    [SerializeField] SpriteRenderer processBG;
    [SerializeField] SpriteRenderer waitTimeProcess;
    [SerializeField] float fillProcessFullWidth;

    public List<Goods_Item> requestingItems = new List<Goods_Item>();
    public bool IsRequesting;
    private RequestDatum currDatum = null;
    public RequestDatum CurrRequest { get => currDatum; }
    private float elapsedTime;

    [Header("Debug")]
    [ReadOnly, SerializeField] int currId;

    private void Awake()
    {
        if(requestManager == null)
            requestManager = GetComponentInParent<RequestManager>();
    }
    private void OnEnable()
    {
        this.RegisterListener((int)EventID.OnMatchedRightRequest, OnMatchedRequest);
    }
    private void OnDisable()
    {
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnMatchedRightRequest, OnMatchedRequest);
        StopAllCoroutines();
    }
    private void LateUpdate()
    {
        if(!IsRequesting)
            return;
        elapsedTime += Time.deltaTime;
        waitTimeProcess.SetSizeX(fillProcessFullWidth * Mathf.Max(0,  (currDatum.waitTime - elapsedTime)/ currDatum.waitTime));
        if(elapsedTime >= currDatum.waitTime && requestingItems.Count > 0)
        {
            OnTimeOut();
        }
    }
    public void ShowRequestItem(RequestDatum datum)
    {
        gameObject.SetActive(true);
        waitTimeProcess.SetSizeX(fillProcessFullWidth);
        currDatum = datum;
        currId = datum.id;
        StartCoroutine(YieldInitItems(datum.types));
    }
    private IEnumerator YieldInitItems(List<eItemType> types)
    {
        yield return bgSR.DOFade(1f, 0.25f).SetUpdate(false).WaitForCompletion();
        for (int i = 0; i < types.Count; i++)
        {
            var item = gameItemAsset.GetItemByType(types[i]).itemProp.Spawn();
            item.transform.parent = transform;
            item.transform.localPosition = GetPosOnBar(types.Count, i);
            item.OnInit(active: false);
            requestingItems.Add(item);
            yield return new WaitForSeconds(0.3f);
        }
        elapsedTime = 0;
        IsRequesting = true;
        processBG.SetAlpha(1f);
        waitTimeProcess.SetAlpha(1f);
        this.PostEvent((int)EventID.OnNewRequestCreated, requestingItems);
    }
    private void OnMatchedRequest(object obj)
    {
        if (!IsRequesting)
            return;
        var item = (Goods_Item)obj;
        if (requestingItems.Contains(item))
        {
            requestingItems.Remove(item);
        }
        if(requestingItems.Count == 0)
        {
            IsRequesting=false;
            StartCoroutine(YieldLeave(true));
        }
    }
    private void OnTimeOut()
    {
        this.PostEvent((int)EventID.OnRequestTimeout, this.currDatum);
        BoardGame_Bartender.instance?.OnRequestTimeout(this.requestingItems);
        StartCoroutine(YieldLeave(false));
        IsRequesting = false;
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
            yield return new WaitForSeconds(waitMissedFXTime);
        }
        else
        {
            //Show happy anim then leave
            yield return new WaitForSeconds(waitMatchedFXTime);
        }
        ClearAll();
        yield return new WaitForEndOfFrame();
        gameObject.SetActive(false);
    }
    private void HideAll()
    {
        bgSR.SetAlpha(0f);
        processBG.SetAlpha(0f);
        waitTimeProcess.SetAlpha(0f);
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
        IsRequesting = false;
        foreach (var item in requestingItems)
        {
            if(item.gameObject != null)
            item.Recycle();
        }
        requestingItems.Clear();
        HideAll();
    }
}
