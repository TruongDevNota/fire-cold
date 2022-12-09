using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System.IO;

[CreateAssetMenu(fileName = "SampleMapAsset", menuName = "DataAsset/SampleMapAsset")]
public class SampleMapAsset : ScriptableObject
{
    [SerializeField] public MapDatum sampleMap;
    [SerializeField] public LevelConfig sampleLevelConfig;

    [ButtonMethod]
    public void WriteSampleConfigFile()
    {
        string configText = JsonUtility.ToJson(sampleLevelConfig);
        Debug.Log(configText);
        //File.WriteAllText(Application.dataPath + "sampleLevelConfig", configText);
    }
}
