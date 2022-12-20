using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UILineRoullete : MonoBehaviour
{
    [SerializeField]
    float maxDistance;
    [SerializeField]
    float roulleteRoundTime;

    [SerializeField]
    RectTransform valueAnchor;
    [SerializeField]
    Vector2 anchorOriginPos;

    [SerializeField]
    float[] distanceStages;
    [SerializeField]
    int[] scaleValues;

    float anchorPos = 0;
    System.Action<int> scaleValueCallback;
    Coroutine RoulleteCoroutine;

    private void OnEnable()
    {
        valueAnchor.anchoredPosition = anchorOriginPos;
        scaleValueCallback = null;
    }

    public void StartRoullete(System.Action<int> callback = null)
    {
        valueAnchor.anchoredPosition = anchorOriginPos;
        RoulleteCoroutine = StartCoroutine(DoRoullete());
        scaleValueCallback = callback;
    }

    public void StopRoulelete()
    {
        if (RoulleteCoroutine != null)
            StopCoroutine(RoulleteCoroutine);
    }

    public IEnumerator DoRoullete()
    {
        float t = 0;
        while (true)
        {
            anchorPos = maxDistance * Mathf.Sin(Mathf.Deg2Rad * t * 360 / roulleteRoundTime);
            t += Time.deltaTime;
            valueAnchor.anchoredPosition = new Vector2(anchorPos, anchorOriginPos.y);
            scaleValueCallback?.Invoke(GetScaleValue(anchorPos));
            yield return new WaitForEndOfFrame();
        }
    }

    private int GetScaleValue(float anchorPosX)
    {
        for(int i = 0; i < distanceStages.Length; i++)
        {
            if (anchorPosX < distanceStages[i])
                return scaleValues[i];
        }
        return scaleValues[distanceStages.Length];
    }
}
