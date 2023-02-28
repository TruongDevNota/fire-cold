using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovingTF : MonoBehaviour
{
    [SerializeField] float stepDuration = 0.1f;
    [SerializeField] float stepDistant = 1f;

    private void OnDisable()
    {
        StopAllCoroutines();
        DOTween.Kill(this);
    }

    public IEnumerator YiledMovingLocalPosition(Vector3 targetPos, float duration = 0.1f, System.Action onDone = null)
    {
        float t = 0;
        var starPos = transform.localPosition;
        SoundManager.Play("4. Moving item");
        while(t < duration)
        {
            transform.localPosition = Vector3.Lerp(starPos, targetPos, t/duration);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = targetPos;
        onDone?.Invoke();
    }

    public IEnumerator YieldMoveToWorldPosition(Vector3 targetPos, float duration = 0.1f, bool unScaleTime = true, System.Action onDone = null)
    {
        SoundManager.Play("4. Moving item");
        yield return transform.DOMove(targetPos, duration).SetUpdate(unScaleTime).WaitForCompletion();
        transform.position = targetPos;
        onDone?.Invoke();
    }
}
