using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIPopupRemoveAds : MonoBehaviour
{
    [SerializeField]
    Text txt_CountDown;
    [SerializeField]
    Button btn_RemoveAds;

    DateTime endTime;
    TimeSpan remainTime;
    Coroutine countDownCoroutine;
    private bool isCountDown = false;

    private void OnDisable()
    {
        if(countDownCoroutine != null)
            StopCoroutine(countDownCoroutine);
    }

    public void OnShow()
    {
        DateTime nextDay = DateTime.Now.AddDays(1);
        endTime = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, 0, 0, 0);
        remainTime = endTime.Subtract(DateTime.Now);
        btn_RemoveAds.interactable = true;
        isCountDown = true;
        if (countDownCoroutine != null)
            StopCoroutine(countDownCoroutine);
        countDownCoroutine = StartCoroutine(YieldCountDown());
        if (GameStateManager.CurrentState == GameState.Play)
            GameStateManager.Pause(null);
    }

    public void OnHide()
    {
        if (countDownCoroutine != null)
            StopCoroutine(countDownCoroutine);
        if (GameStateManager.CurrentState == GameState.Pause)
            GameStateManager.Play(null);
    }

    private IEnumerator YieldCountDown()
    {
        while (isCountDown)
        {
            txt_CountDown.text = remainTime.ToString("hh':'mm':'ss");
            yield return new WaitForSeconds(1);
            remainTime = remainTime.Subtract(TimeSpan.FromSeconds(1d));
            if(remainTime <= TimeSpan.Zero)
            {
                OnStopCountDown();
            }
        }
    }

    public void OnBtnRemoveAds()
    {
        //Shop ingame handle
        //OnStopCountDown();
        CoinManager.Add(DataManager.GameConfig.coinRewardByRemoveAds);
        DataManager.UserData.isRemovedAds = true;
    }

    private void OnStopCountDown()
    {
        isCountDown = false;
        txt_CountDown.text = "--:--:--";
        btn_RemoveAds.interactable = false;
    }
}
