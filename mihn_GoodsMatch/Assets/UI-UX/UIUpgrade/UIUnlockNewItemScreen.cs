using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUnlockNewItemScreen : MonoBehaviour
{
    [SerializeField] UIAnimation anim;
    [SerializeField] Button closeBtn;
    [SerializeField] UINewItemUnlock[] uiNewItem;

    private System.Action OnCloseHandle = null;

    private void Awake()
    {
        closeBtn?.onClick.AddListener(OnCloseClicked);
    }

    private void Onshow()
    {
        
    }

    private void OnCloseClicked()
    {
        anim.Hide(onCompleted: () => OnCloseHandle?.Invoke());
    }
}
