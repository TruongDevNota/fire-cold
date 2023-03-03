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
        anim.Initialize(true);
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
        ChangeAnim(GuestAnimations.moveAnims[Random.Range(0, GuestAnimations.moveAnims.Length)]);
        yield return this.transform.DOMove(endPos, duration).OnComplete(() =>
        {
            callback?.Invoke();
            if (autoHide)
                this.Recycle();
        }).SetId($"guest_{name}_Move") .WaitForCompletion();
    }
    
    [ButtonMethod]
    public void AddSkinIndexes()
    {
        ingameSkinIndex = new int[13] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
