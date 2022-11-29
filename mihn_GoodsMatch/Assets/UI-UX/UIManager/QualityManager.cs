using UnityEngine;

public class QualityManager : MonoBehaviour
{
    [SerializeField]
    private UILocalizedText qualityStatus = null;

    [SerializeField]
    private UISlider graphicSlider = null;
    public static int QualityInt { get { return Mathf.Clamp((int)Quality, 0, 3); } }

    private static Quality quality = Quality.Unknown;
    public static Quality Quality
    {
        get => quality;
        set
        {
            if (quality != value)
            {
                quality = value;
                if (quality == Quality.Ultra)
                    QualitySettings.resolutionScalingFixedDPIFactor = 1.0f;
                else if (quality == Quality.High)
                    QualitySettings.resolutionScalingFixedDPIFactor = 0.95f;
                else if (quality == Quality.Normal)
                    QualitySettings.resolutionScalingFixedDPIFactor = 0.9f;
                else if (quality == Quality.Low)
                    QualitySettings.resolutionScalingFixedDPIFactor = 0.85f;
                else
                    QualitySettings.resolutionScalingFixedDPIFactor = 0.95f;

                Debug.Log("ResolutionScalingFixedDPIFactor: " + QualitySettings.resolutionScalingFixedDPIFactor);

                OnQualityChanged?.Invoke(quality);
            }
        }
    }

    public delegate void QualityDelegate(Quality quality);
    public static event QualityDelegate OnQualityChanged;

    #region Base
    private static QualityManager instance { get; set; }

    public static string TAG
    {
        get
        {
            if (instance != null)
                return "[" + instance.GetType().Name + "] ";
            return "";
        }
    }

    private void Awake()
    {
        instance = this;
    }
    #endregion

    private void Start()
    {
        Debug.Log("ResolutionScalingFixedDPIFactor Default: " + QualitySettings.resolutionScalingFixedDPIFactor);

        if (PlayerPrefs.GetInt("GraphicManager", -1) < 0)
            AutoCheckQuality();
        else
            Quality = (Quality)Mathf.Clamp(PlayerPrefs.GetInt("GraphicManager", -1), 0, 3);

        if (graphicSlider)
        {
            graphicSlider.OnChangedAction(ValueChanged);
            graphicSlider.Value = (int)Quality;
            UpdateStatus();
        }
        else
        {
            Debug.LogWarning("[QualityManager] " + "graphicSlider NULL");
        }
    }

    private void ValueChanged(float value)
    {
        Quality = (Quality)value;
        UpdateStatus();
        PlayerPrefs.SetInt("GraphicManager", Mathf.Clamp(QualityInt, 0, 3));
        PlayerPrefs.Save();
    }

    private void UpdateStatus()
    {
        if (LocalizedManager.localizedData.Count > 0)
            qualityStatus.Key = "base_Quality_" + QualityInt;
        else
            qualityStatus.Text = Quality.ToString();
    }

    public void AutoCheckQuality()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            if (SystemInfo.systemMemorySize >= 2048 && SystemInfo.graphicsMemorySize >= 512)
            {
                Quality = Quality.Ultra;
            }
            else if (SystemInfo.systemMemorySize >= 1024 && SystemInfo.graphicsMemorySize >= 256)
            {
                Quality = Quality.High;
            }
            else if (SystemInfo.systemMemorySize >= 512 && SystemInfo.graphicsMemorySize >= 128)
            {
                Quality = Quality.Normal;
            }
            else
            {
                Quality = Quality.Low;
            }
        }
        else
        {
            if (SystemInfo.systemMemorySize >= 3072 && SystemInfo.graphicsMemorySize >= 1024)
            {
                Quality = Quality.Ultra;
            }
            else if (SystemInfo.systemMemorySize >= 2048 && SystemInfo.graphicsMemorySize >= 768)
            {
                Quality = Quality.High;
            }
            else if (SystemInfo.systemMemorySize >= 1024 && SystemInfo.graphicsMemorySize >= 512)
            {
                Quality = Quality.Normal;
            }
            else
            {
                Quality = Quality.Low;
            }
        }
    }

    private void Reset()
    {
        if (graphicSlider)
        {
            graphicSlider.WholeNumbers = true;
            graphicSlider.MaxValue = 3;
            graphicSlider.MinValue = 0;
            graphicSlider.Value = 3;
        }
    }
}

public enum Quality
{
    Unknown = -1,
    Low = 0,
    Normal = 1,
    High = 2,
    Ultra = 3,
}
