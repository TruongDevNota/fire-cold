using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EffectManager : MonoBehaviour
{
    [SerializeField]
    private HintCircle hintCirclePrefab;

    private static EffectManager instance;

    private void Awake()
    {
        instance = this;

        hintCirclePrefab.CreatePool(6);
    }

    public static void ShowHintCircle(Transform objTf, Vector3 positionOffset)
    {
        var circle = instance.hintCirclePrefab.Spawn(objTf);
        circle.transform.localPosition = Vector3.zero + positionOffset;
        circle.ShowAnim();
    }
}
