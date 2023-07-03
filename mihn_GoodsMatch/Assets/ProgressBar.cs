using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class ProgressBar : MonoBehaviour
{
    public Image progressBar;
    public Text ptcText;
    public float IncreaseRate = 0.2f;
    int to;
    int from;

    private void Awake()
    {
        //this.RegisterListener((int)EventID.ReInit,ReInit);
        //this.RegisterListener((int)EventID.OnWinLevel, ActivateBar);
    }
    public void Init(int levelprogress, int stepCount)
    {
        //Debug.Log($"level progression { DataManager.UserData.levelProgress}");

        to = levelprogress;
        from = levelprogress - 100 / stepCount;
        Debug.Log($"{DataManager.UserData.level}-{from}%-{to}%");

        if (GameStateManager.CurrentState == GameState.GameOver)
        {
            if (to == 100)
            {
                progressBar.fillAmount = 0;
                ptcText.text = $"{0}%";
                return;
            }
            progressBar.fillAmount = (float)to / 100;
            ptcText.text = $"{to}%";
            return;
        }
        //if (!DataManager.IsPlayingHighestLevel)
        //{
        //    if (to == 100)
        //    {
        //        progressBar.fillAmount = 0;
        //        ptcText.text = $"{0}%";
        //    }
        //    else
        //    {
        //        progressBar.fillAmount = (float)to / 100;
        //        ptcText.text = $"{to}%";
        //    }

        //}
        //else
        //{

        //    //progressBar.DOFillAmount((float)from / 100, 2);
        //    progressBar.fillAmount = (float)from / 100;
        //    ptcText.text = $"{from}%";
        //}
    }
    

    private void OnDestroy()
    {
        //EventDispatcher.Instance.RemoveListener((int)EventID.OnWinLevel, ActivateBar);
    }

    public void AnimateProgressBar(object obj, string progress = null)
    {
        if (obj != null)
        {
            to = (int)obj;
        }
        if (!gameObject.activeInHierarchy)
            return;
        StartCoroutine(StartIncrease(from / 100f, (float)to / 100, progress));
    }
    IEnumerator StartIncrease(float from, float to, string progress)
    {
        float ptc = from;
        while (ptc < to)
        {
            ptc += Time.deltaTime * IncreaseRate;
            progressBar.fillAmount = ptc;
            ptcText.text = $"{Math.Round(ptc * 100, 0)}%";
            yield return null;
        }
        if (ptc >= to)
        {
            progressBar.fillAmount = to;
            // progressBar.DOFillAmount(to, 2);
            ptcText.text = $"{Math.Round(to * 100, 0)}%";
        }
        //if (DataManager.UserData.levelProgress == 100 & progress == "level")
        //{
        //    Debug.Log("Send Instruction");
        //    DataManager.UserData.levelProgress = 0;
        //    yield return new WaitForSeconds(2.5f);
        //    DataManager.UserData.rankLevel++;
        //    this.PostEvent((int)EventID.ShowNewThemePopup);
        //}

    }
}