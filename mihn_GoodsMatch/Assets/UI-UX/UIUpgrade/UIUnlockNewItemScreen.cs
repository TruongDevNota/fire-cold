using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUnlockNewItemScreen : MonoBehaviour
{
    [SerializeField] UIAnimation anim;
    [SerializeField] Button closeBtn;
    [SerializeField] UINewItemUnlock[] uiNewItem;

    GameItemAsset itemData => DataManager.ItemsAsset;

    private System.Action OnCloseHandle = null;
    private Coroutine showCoroutine = null;
    private void Awake()
    {
        closeBtn?.onClick.AddListener(Hide);
    }

    public void Show(System.Action onCloseAction = null)
    {
        this.gameObject.SetActive(true);
        OnCloseHandle = onCloseAction;
        if(showCoroutine != null)
            StopCoroutine(showCoroutine);
        showCoroutine = StartCoroutine(YieldShow());
    }
    public void Hide()
    {
        anim.Hide(onCompleted: () => OnCloseHandle?.Invoke());
    }
    private IEnumerator YieldShow()
    {
        var itemsToUnlock = itemData.lockedList;
        if(itemsToUnlock.Count <= 0)
        {
            yield return new WaitForSeconds(1f);
            OnCloseHandle?.Invoke();
            yield break;
        }

        for (int i = 0; i < uiNewItem.Length; i++)
        {
            uiNewItem[i].gameObject.SetActive(i < itemsToUnlock.Count);
            if (i < itemsToUnlock.Count)
                uiNewItem[i].Init(itemsToUnlock[i]);
            yield return new WaitForEndOfFrame();
        }
        anim.Show();
    }
}
