using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIScore : MonoBehaviour
{
    [SerializeField]
    private Text scoreTxt = null;
    [SerializeField]
    private Transform scoreTxtTransform = null;

    [SerializeField]
    private Text comboTxt = null;
    [SerializeField]
    private Transform comboTxtTransform = null;

    [Header("PerfectToast")]
    [SerializeField]
    private UIPerfectToast perfectToast = null;

    //[SerializeField]
    private UIAnimType type = UIAnimType.Scale;

    private static UIScore instance = null;

    private bool isShowHighScore;
    private int combo;
    private void Awake()
    {
        instance = this;
        GameStatisticsManager.OnScoreChanged += OnScoreChanged;
        GameStatisticsManager.OnPerfectChanged += GameStatistics_OnPerfectChanged;
    }

    private void GameStatistics_OnPerfectChanged(int perfect)
    {
        if (type == UIAnimType.Scale)
        {
            SetComboScaleAnimation(perfect);
        }
        else
        {
            SetComboZoomAnimation(perfect);
        }
    }

    private void OnScoreChanged(int value)
    {
        scoreTxt.text = value == 0 ? "0" : value.ToString();
        if (QualityManager.Quality != Quality.Low)
            scoreTxtTransform.DoScale(1.0f, 1.15f);

        if (!isShowHighScore && value > 50)
        {
            //SoundManager.Play("sfx_high_score");
            ShowToastPerfect("high score", 1.5f, true);
            isShowHighScore = true;
        }
    }

    public void ShowToastPerfect(string rank, float timeAutoHide, bool waitAnimation)
    {
        if (perfectToast != null)
            perfectToast.Show(rank.ToUpper(), timeAutoHide, waitAnimation);
    }

    public void SetComboScaleAnimation(int currentCombo, float delay = 0.025f)
    {
        if (comboTxt)
        {
            if (currentCombo > 0)
            {
                combo++;
                comboTxt.gameObject.SetActive(true);
                comboTxt.text = currentCombo.ToString();
                UIAnimation.DoScale(comboTxtTransform, 1.5f, 0.25f, delay);
            }
            else
            {
                combo = 0;
                comboTxt.gameObject.SetActive(false);
                comboTxt.text = "1";
            }
        }
    }

    public void SetComboZoomAnimation(int currentCombo, float delay = 0.025f)
    {
        if (comboTxt)
        {
            if (currentCombo > 0)
            {
                comboTxt.gameObject.SetActive(true);
                comboTxt.text = currentCombo.ToString();
                UIAnimation.DoMoveZ(comboTxtTransform, -150, 0, 0.25f, delay, true);
            }
            else
            {
                comboTxt.gameObject.SetActive(false);
                comboTxt.text = "-";
            }
        }
    }

}
