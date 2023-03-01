using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using DG.Tweening;
using MyBox;

public class GuestController : MonoBehaviour
{
    [SerializeField] SkeletonAnimation anim;

    [Header("Skin config")]
    [SerializeField] int[] ingameSkinIndex;
    [SerializeField] string gameoverSkinName;

    private void OnEnable()
    {
        
    }
    private void OnDisable()
    {
        DOTween.Kill(this);
        StopAllCoroutines();
    }

    public void ChangeSkin(string newskin = null)
    {
        if (string.IsNullOrEmpty(newskin))
            newskin = $"skin{ingameSkinIndex[Random.Range(0, ingameSkinIndex.Length)]}";
        anim.initialSkinName = newskin;
        anim.Initialize(true);
    }
    public void ChangeAnim(string animName)
    {
        anim.AnimationName = animName;
    }
    public void Move(Vector2 startPos, Vector2 endPos, bool autoHide = false, float duration = 1f, System.Action callback = null)
    {
        StartCoroutine(YieldMove(startPos, endPos, autoHide, duration, callback));
    }
    public IEnumerator YieldMove(Vector2 startPos, Vector2 endPos, bool autoHide = false, float duration = 1f, System.Action callback = null)
    {
        var dirX = endPos.x - startPos.x;
        anim.initialFlipX = dirX < 0;
        anim.Initialize(true);
        this.transform.position = startPos;
        ChangeAnim(GuestAnimations.moveAnims[0]);
        yield return this.transform.DOMove(endPos, duration).OnComplete(() =>
        {
            callback?.Invoke();
            if (autoHide)
                this.Recycle();
        }).SetId($"guest_{name}_Move") .WaitForCompletion();
    }
}
