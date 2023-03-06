using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupBartender : MonoBehaviour
{
    [SerializeField] UIAnimation anim;
    [SerializeField] Button btn_Next;
    [SerializeField] Button btn_Skip;

    private void Start()
    {
        btn_Next?.onClick.AddListener(BtnNextHandle);
        btn_Skip?.onClick.AddListener(OnSkip);
        this.RegisterListener((int)EventID.OnModeBartenderUnlocked, OnShow);
    }

    public void OnShow(object obj)
    {
        btn_Skip.gameObject.SetActive(false);
        anim.Show(null, onCompleted: () =>
        {
            DOVirtual.DelayedCall(2f, () => btn_Skip.gameObject.SetActive(true));
        });
    }

    private void BtnNextHandle()
    {
        SoundManager.Play(GameConstants.sound_Button_Clicked);
        GameStateManager.Idle(true);
        anim.Hide();
    }

    private void OnSkip()
    {
        GameStateManager.LoadGame(null);
        anim.Hide();
    }
}
