using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ContentSizeHelper : MonoBehaviour
{
    [SerializeField]
    protected ContentSizeFitter contentSizeFitter = null;

    [SerializeField]
    protected Text contentText = null;

    private void Start()
    {
        contentText.RegisterDirtyLayoutCallback(() =>
        {
            //Debug.Log("RegisterDirtyLayoutCallback");
            UpdateLayout();
        });
    }

    public void UpdateLayout()
    {
        if (isActiveAndEnabled)
        {
            StartCoroutine(DOUpdateLayout());
        }
    }

    public IEnumerator DOUpdateLayout()
    {
        if (contentSizeFitter)
        {
            yield return new WaitForEndOfFrame();
            contentSizeFitter.enabled = false;
            yield return new WaitForEndOfFrame();
            contentSizeFitter.enabled = true;
        }
    }
}
