using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXObjectBase : MonoBehaviour
{
    protected SpriteRenderer objectSR;

    protected Coroutine showCoroutine;
    protected void Awake()
    {
        objectSR = GetComponent<SpriteRenderer>();
    }
    protected void OnEnable()
    {
        
    }
    protected void OnDisable()
    {
        StopAllCoroutines();
    }
    public void ShowFX(Vector2 startPos, Vector2 endPos, Sprite sprite = null, float duration = 1.5f, bool hideAtEnd = true, float startScale = 1f, float endScale = 1f, float startFade = 1f, float endFade = 1f, System.Action callback = null)
    {
        if (showCoroutine != null)
            StopCoroutine(showCoroutine);
        showCoroutine = StartCoroutine(YieldShow(startPos, endPos, sprite, duration, hideAtEnd, startScale, endScale, startFade, endFade, callback));
    }
    public IEnumerator YieldShow(Vector2 startPos, Vector2 endPos, Sprite sprite = null, float duration = 1.5f, bool hideAtEnd = true, float startScale = 1f, float endScale = 1f, float startFade = 1f, float endFade = 1f, System.Action callback = null)
    {
        if(objectSR == null)
            yield break;
        if(sprite != null)
            objectSR.sprite = sprite;

        transform.position = startPos;
        var fromScale = Vector3.one * startScale;
        var toScale = Vector3.one * endScale;
        transform.localScale = fromScale;
        float t = 0;
        while(t < duration)
        {
            var process = Mathf.Clamp01(t / duration);
            transform.localScale = Vector3.Lerp(fromScale, toScale, process);
            transform.position = Vector3.Lerp(startPos, endPos, process);
            if(endFade != startFade)
            {
                var color = objectSR.GetColor();
                color.a = startFade + (endFade - startFade) * process;
                objectSR.color = color;
            }
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        callback?.Invoke();
        if (hideAtEnd)
            this.Recycle();
    }
}
