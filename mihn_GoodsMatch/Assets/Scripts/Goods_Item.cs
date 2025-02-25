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
    public SpriteRenderer spriteRenderer;
    [SerializeField]
    protected SpriteRenderer outlineSR;
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

    [Header("JumpAnim")]
    [SerializeField] protected float jumpAnim_Duration = 0.5f;
    [SerializeField] protected float deltaY = 0.35f;
    private Coroutine jumpCoroutine;
    private Vector3 defaultPos;

    public Sprite itemIcon => spriteRenderer.sprite;
    public eItemType Type { get => type; }

    [SerializeField] private ShelfUnit currentShelf;
    public ShelfUnit pCurrentShelf
    {
        get => currentShelf;
        set { currentShelf = value; }
    }

    [SerializeField] private int firstLeftCellIndex;
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
    private void OnEnable()
    {
        outlineSR?.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        transform.DOKill();
        //DOTween.Kill($"DOScaleHightlight_{this.GetInstanceID()}");
        //DOTween.Kill($"{this.GetInstanceID()}_scaleExplode");
        //DOTween.Kill($"{this.GetInstanceID()}_FaddInit");
        transform.localScale = Vector3.one;
        StopAllCoroutines();
    }
    public void OnPutUpShelf()
    {
        SoundManager.Play("2. Item Puton");
        StopRotate();
        //StopJumping();
    }

    public void OnPickUp()
    {
        StopRotate();
        StopJumping();
        SoundManager.Play("1. Item Pickup");
        rotateCoroutine = StartCoroutine(YieldRotate());
    }
    private void StopJumping()
    {
        if (jumpCoroutine != null)
        {
            StopCoroutine(jumpCoroutine);
            //transform.localPosition = defaultPos;
        }
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
        canPick = false;
        //BoardGame.instance.CheckGameComplete();
        StartCoroutine(YieldExplode(index));
    }

    public IEnumerator YieldExplode(int index = 0)
    {
        StopRotate();
        yield return new WaitForSeconds(index * 0.2f);
        yield return transform.DOScale(1.1f, exploreAnim_Duration * 0.5f).SetId($"{this.GetInstanceID()}_scaleExplode").WaitForCompletion();
        DOTween.Kill($"{this.GetInstanceID()}_scaleExplode");
        yield return new WaitForEndOfFrame();
        yield return transform.DOScale(0f, exploreAnim_Duration * 0.5f).SetId($"{this.GetInstanceID()}_scaleExplode").WaitForCompletion();
        //To-do: Post event collect gold?
        
        //UIInfo.CollectStars(1,this.transform);
        this.Recycle();
    }

    public void jump(int index)
    {
        jumpCoroutine = StartCoroutine(YieldJumping(index));
    }
    private IEnumerator YieldJumping(int index)
    {
        defaultPos = transform.localPosition;
        var worldPos = transform.position;
        yield return new WaitForSeconds(index * 0.2f);
        for (int i = 0; i < 2 ; i++)
        {
            yield return transform.DOLocalMoveY(defaultPos.y + deltaY, jumpAnim_Duration * 0.25f).WaitForCompletion();
            yield return new WaitForEndOfFrame();
            yield return transform.DOLocalMoveY(defaultPos.y, jumpAnim_Duration * 0.25f).WaitForCompletion();
        }
        transform.localPosition = defaultPos;
    }

    public void OnInit(float dur = 0.5f, bool active = true, float delay= 0)
    {
        StartCoroutine(YieldInitShow(dur, active, delay));
    }
    private IEnumerator YieldInitShow(float dur, bool active, float delay=0)
    {
        canPick = false;
        spriteRenderer.SetAlpha(0);
        transform.SetScale(0.25f);
        if(delay > 0)
            yield return new WaitForSeconds(delay);
        transform.DOScale(1f, dur).SetUpdate(true);
        yield return spriteRenderer.DOFade(1f, dur + 0.1f).SetUpdate(true).SetId($"{this.GetInstanceID()}_FaddInit").WaitForCompletion();
        canPick = active;
    }

    public IEnumerator YieldMoveThenHide(Vector3 desPos, float dur = 0.25f)
    {
        yield return tfMoving.YieldMoveToWorldPosition(desPos, dur);
        this.Recycle();
    }

    public void OnHighLight(float scale = 1.4f, float duration = 1f, float delay = 0f)
    {
        StartCoroutine(YieldHightLightShow(scale, duration, delay));
    }

    public IEnumerator YieldHightLightShow(float scale, float duration, float delay)
    {
        yield return new WaitForSeconds(delay);
        outlineSR?.gameObject.SetActive(true);
        transform.DOLocalMoveY(0.5f, duration * 0.5f);
        transform.DOScale(scale, duration * 0.5f).SetId($"DOScaleHightlight_{this.GetInstanceID()}");
    }
}
