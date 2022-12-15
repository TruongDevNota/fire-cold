using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Goods_Item : MonoBehaviour
{
    [SerializeField]
    public int matchAmount = 3;
    [SerializeField]
    public Vector3 size = Vector3.one;
    [SerializeField]
    protected SpriteRenderer spriteRenderer;
    [SerializeField]
    public MovingTF tfMoving;
    [SerializeField] 
    protected eItemType type;

    [Header("Explode Anim")]
    [SerializeField] protected float exploreAnim_Duration;
    [SerializeField] Ease exploreAnim_Ease;

    [Header("Rotate Anim")]
    [SerializeField] protected float rotateAnim_Duration = 0.5f;
    [SerializeField] protected float rotateAngle = 20f;
    private Coroutine rotateCoroutine;

    public eItemType Type { get => type; }

    private ShelfUnit currentShelf;
    public ShelfUnit pCurrentShelf
    {
        get => currentShelf;
        set { currentShelf = value; }
    }

    private int firstLeftCellIndex;
    public int pFirstLeftCellIndex
    {
        get => firstLeftCellIndex;
        set { firstLeftCellIndex = value; }
    }

    public bool canPick = true;

    private void Start()
    {
        canPick = true;
    }

    public void OnPutUpShelf()
    {
        SoundManager.Play("2. Item Puton");
        StopRotate();
    }

    public void OnPickUp()
    {
        StopRotate();
        SoundManager.Play("1. Item Pickup");
        rotateCoroutine = StartCoroutine(YieldRotate());
    }

    private void StopRotate()
    {
        if (rotateCoroutine != null)
            StopCoroutine(rotateCoroutine);
        transform.DoRotateZ(0f, 0f);
    }

    private IEnumerator YieldRotate()
    {
        while (true)
        {
            transform.DoRotateZ(rotateAngle, 0.25f);
            yield return new WaitForSeconds(rotateAnim_Duration * 0.5f);
            transform.DoRotateZ(-rotateAngle, 0.25f);
            yield return new WaitForSeconds(rotateAnim_Duration * 0.5f);
            transform.DoRotateZ(0f, 0f);
        }
    }

    public void Explode(int index = 0)
    {
        StopRotate();
        canPick = false;
        BoardGame.instance.CheckGameComplete();
        StartCoroutine(YieldExplode(index));
    }

    private IEnumerator YieldExplode(int index = 0)
    {
        var waitDelay = new WaitForSeconds(index * 0.2f);
        yield return waitDelay;
        yield return transform.DOScale(1.1f, exploreAnim_Duration * 0.5f).WaitForCompletion();
        yield return transform.DOScale(0f, exploreAnim_Duration * 0.5f).WaitForCompletion();
        UIInfo.CollectStars(1,this.transform);
        GameObject.Destroy(gameObject);
    }
}
