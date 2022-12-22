using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingTF : MonoBehaviour
{
    [SerializeField] float stepDuration = 0.1f;
    [SerializeField] float stepDistant = 1f;

    public IEnumerator YiledMovingInSameTime(Vector3 targetPos, float duration = 0.1f, System.Action onDone = null)
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
    }
}
