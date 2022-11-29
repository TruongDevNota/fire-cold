using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SampleMapAsset", menuName = "DataAsset/SampleMapAsset")]
public class SampleMapAsset : ScriptableObject
{
    [SerializeField] public MapDatum sampleMap;
}
