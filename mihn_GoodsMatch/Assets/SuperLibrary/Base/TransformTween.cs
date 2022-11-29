using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformTween
{
    public static void DoScale(this Transform transform, float scaleFrom = 1f, float scaleTo = 1.05f, float timeAnimation = 0.25f, float delayTime = 0.01f, bool autoRevese = true)
    {
        if (transform == null)
        {
            Debug.LogError("[TransformExtend] DoScale: " + transform.name + " NULL");
            return;
        }

        transform.hasChanged = true;

        if (autoRevese)
        {
            transform.DOKill();
            transform.DOScale(scaleTo, timeAnimation * 0.35f).OnComplete(() =>
            {
                transform.DOScale(scaleFrom, timeAnimation * 0.65f).SetEase(Ease.OutCubic);
            })
            .SetEase(Ease.InCubic)
            .SetDelay(delayTime);
        }
        else
        {
            transform.DOScale(scaleTo, timeAnimation * 0.35f).SetEase(Ease.InCubic);
        }
    }

    public static void DoMoveZ(this Transform transform, float from = 0f, float to = -100f, float timeAnimation = 0.25f, float delayTime = 0.01f, bool autoRevese = false, TweenCallback onDone = null)
    {
        if (transform == null)
        {
            Debug.LogError("[TransformExtend] DoMoveZ: " + transform.name + " NULL");
            return;
        }

        if (autoRevese)
        {
            transform.DOKill();
            transform.localPosition.Set(transform.localPosition.x, transform.localPosition.y, from);
            transform.DOLocalMoveZ(to, timeAnimation * 0.7f).OnComplete(() =>
            {
                if (onDone != null)
                    transform.DOLocalMoveZ(from, timeAnimation * 0.3f).SetEase(Ease.OutCubic).OnComplete(onDone);
                else
                    transform.DOLocalMoveZ(from, timeAnimation * 0.3f).SetEase(Ease.OutCubic);
            })
            .SetEase(Ease.InCubic)
            .SetDelay(delayTime);
        }
        else
        {
            transform.DOKill();
            transform.localPosition.Set(transform.localPosition.x, transform.localPosition.y, from);
            if (onDone != null)
                transform.DOLocalMoveZ(to, timeAnimation).SetEase(Ease.OutCubic).SetDelay(delayTime).OnComplete(onDone);
            else
                transform.DOLocalMoveZ(to, timeAnimation).SetEase(Ease.OutCubic).SetDelay(delayTime);
        }
    }

    public static void DoRotateZ(this Transform transform, float to = 90f, float timeAnimation = 0.25f, float delayTime = 0.01f, TweenCallback onDone = null)
    {
        if (transform == null)
        {
            Debug.LogError("[TransformExtend] DoRotateZ: " + transform.name + " NULL");
            return;
        }

        transform.DOKill();
        transform.DORotate(new Vector3(0, 0, to), timeAnimation * 0.7f).OnComplete(() =>
        {
            if (onDone != null)
                onDone.Invoke();
        })
        .SetEase(Ease.InOutCubic)
        .SetDelay(delayTime);
    }

    public static void DoShakeScreen(this Transform transform, float timeAnimation, float strength, float timeDelay = 0.01f, TweenCallback onDone = null)
    {
        if (onDone != null)
            transform.DOShakePosition(timeAnimation, strength).SetDelay(timeDelay);
        else
            transform.DOShakePosition(timeAnimation, strength).SetDelay(timeDelay).OnComplete(onDone);
    }

    public static void DoRotate(this Transform transform, float timeAnimation = 0.5f, int loop = -1, TweenCallback onAnimationDone = null, bool resetLocal = false)
    {
        if (transform)
        {
            transform.DOKill();
            if (resetLocal)
            {
                transform.SetLocalY(0);
                transform.SetLocalRotation2D(0);
            }
            transform.DOLocalRotate(-1 * Vector3.forward * 360, timeAnimation, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear).SetLoops(loop, LoopType.Incremental)
                .OnComplete(() => onAnimationDone?.Invoke());
        }
    }

    public static void DoJump(this Transform transform, float timeAnimation = 0.5f, int loop = -1, float detalY = 0.35f, TweenCallback onAnimationDone = null, bool resetLocal = false)
    {
        if (transform)
        {
            transform.DOKill();
            if (resetLocal)
            {
                transform.SetLocalY(0);
                transform.SetLocalRotation2D(0);
            }
            transform.DOLocalMoveY(detalY, timeAnimation)
                .SetLoops(loop, LoopType.Yoyo)
                .OnComplete(() => onAnimationDone?.Invoke());
        }
    }

}
