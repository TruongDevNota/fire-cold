using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Slider))]
[DisallowMultipleComponent]
public class UISlider : MonoBehaviour
{
    [SerializeField]
    private float scaleValue = 1;

    //[SerializeField]
    //private Text titleText = null;
    [SerializeField]
    private Text valueText = null;
    [SerializeField]
    private string valueFormat = "#0.0";
    [SerializeField]
    private string valueName = "";
    [SerializeField]
    private bool toUpper = true;
    [SerializeField]
    private bool autoUpdateStatus = true;
    [SerializeField]
    private bool autoSaveLoad = true;

    [SerializeField]
    private Slider slider = null;

    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (slider != null)
        {
            if (autoSaveLoad)
                slider.value = PlayerPrefs.GetFloat(name, slider.value);
            if (string.IsNullOrEmpty(valueFormat))
                valueFormat = "";
        }
    }

    private void Start()
    {
        slider.onValueChanged.AddListener(OnValueChanged);
        if (autoUpdateStatus)
            UpdateStatus();
    }

    public void OnChangedAction(UnityAction<float> action)
    {
        slider.onValueChanged.AddListener(action);
    }

    private void OnValueChanged(float value)
    {
        if (autoUpdateStatus)
            UpdateStatus();

        if (autoSaveLoad)
        {
            PlayerPrefs.SetFloat(name, value);
            PlayerPrefs.Save();
        }

    }

    public void SetValueAnimation(float nextValue, float timeAnimation = 0.025f)
    {
        if (QualityManager.QualityInt > 0)
        {
            slider.DOValue(nextValue, timeAnimation * QualityManager.QualityInt)
            .OnComplete(() =>
            {
                slider.value = nextValue;
                UpdateStatus();
            });
        }
        else
        {
            slider.value = nextValue;
            UpdateStatus();
        }
    }

    private void UpdateStatus()
    {
        if (valueText)
        {
            if (toUpper)
                valueText.text = ((slider.value * scaleValue).ToString(valueFormat) + valueName).ToUpper();
            else
                valueText.text = ((slider.value * scaleValue).ToString(valueFormat) + valueName);
        }
    }

    public float ScaleValue { get { return slider.value * scaleValue; } set { slider.value = value / scaleValue; } }

    public float Value { get { return slider.value; } set { slider.value = value; } }

    public float MaxValue { get { return slider.maxValue; } set { slider.maxValue = value; } }

    public float MinValue { get { return slider.minValue; } set { slider.minValue = value; } }

    public bool WholeNumbers { get { return slider.wholeNumbers; } set { slider.wholeNumbers = value; } }

    public string StringValue
    {
        set
        {
            if (toUpper)
                valueText.text = value.ToUpper();
            else
                valueText.text = value;
        }
    }

    private void Reset()
    {
        slider = GetComponent<Slider>();
    }
}