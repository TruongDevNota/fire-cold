using UnityEngine;
using DG.Tweening;
using System.Collections;

public class HintCircle : MonoBehaviour
{
    [SerializeField]
    private int sparkleCount;
    [SerializeField]
    private float sparkleDuration;
    [SerializeField]
    private float sparkleWait;

    private SpriteRenderer sr;
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void ShowAnim(System.Action callback = null)
    {
        StartCoroutine(YieldShowAnim(callback));
    }

    private IEnumerator YieldShowAnim(System.Action callback)
    {
        if(sr == null)
            sr = GetComponent<SpriteRenderer>();
        for(int i = 0; i < sparkleCount; i++)
        {
            sr.SetAlpha(1f);
            transform.DOScale(1f, 0f);
            yield return transform.DOScale(1.2f, sparkleDuration).OnComplete(() =>
            {
                sr.SetAlpha(0f);
            });
            yield return new WaitForSeconds(sparkleWait);
        }
        callback?.Invoke();
        this.Recycle();
    }
}
