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

    public void Explode()
    {
        canPick = false;
        StartCoroutine(YieldExplode());
    }

    private IEnumerator YieldExplode()
    {
        transform.DOShakePosition(exploreAnim_Duration, 0.15f, 5, 45f);
        yield return new WaitForSeconds(exploreAnim_Duration);
        DOTween.Kill(this);
        BoardGame.instance.ItemEarned++;
        GameObject.Destroy(gameObject);
    }
}
