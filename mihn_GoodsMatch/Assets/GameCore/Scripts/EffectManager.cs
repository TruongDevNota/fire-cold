using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EffectManager : MonoBehaviour
{
    [SerializeField] private HintCircle hintCirclePrefab;
    
    [Header("Combo")]
    [SerializeField] private ComboFX comboFXPrefab;
    [SerializeField] Sprite[] comboSprites;
    [SerializeField] Vector3 comboStartPos;
    [SerializeField] Vector3 comboEndPos;
    [SerializeField] float comboFXShowTime = 1f;
    [SerializeField] float startScale = 0.75f;
    [SerializeField] float endScale = 1f;

    private static EffectManager instance;

    private void Awake()
    {
        instance = this;

        hintCirclePrefab.CreatePool(6);
        comboFXPrefab.CreatePool(4);
    }
    private void Start()
    {
        this.RegisterListener((int)EventID.OnNewCombo, ShowComboFX);
    }
    
    public static void ShowHintCircle(Transform objTf, Vector3 positionOffset)
    {
        var circle = instance.hintCirclePrefab.Spawn(objTf);
        circle.transform.localPosition = Vector3.zero + positionOffset;
        circle.ShowAnim();
    }

    private void ShowComboFX(object obj)
    {
        int comboCount = (int)obj;
        var sprite = comboSprites[Mathf.Min(comboCount-1, comboSprites.Length -1)];
        var comboOB = comboFXPrefab.Spawn(transform);
        comboOB.ShowFX(comboStartPos, comboEndPos, sprite, comboFXShowTime, true, 0.75f, 1f);
    }
}
