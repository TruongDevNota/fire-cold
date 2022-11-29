using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "ExercisesAsset", menuName = "DataAsset/ExercisesAsset")]
public class ExercisesAsset : BaseAsset<ExerciseData>
{
    [Header("Level Design")]
    [SerializeField] float defaultHealth = 5;
    [SerializeField] float defaultPower = 5;
    [SerializeField] int defaultPrice = 1000;
    [SerializeField] int upScale = 2;
    [SerializeField] public float upgradeStatsPercent = 0.1f;
    [SerializeField] public float upgradeTimePercent = 0.05f;
    [SerializeField] public int upgradeLevelMax = 5;

    [Header("Assets")]
    [SerializeField] Sprite[] spritesIconEx;

    public ExerciseData GetExerciseByIndex(int index)
    {
        var nextStage = list.FirstOrDefault(s => s.index == index);
        if (nextStage != null)
        {
            nextStage.isUnlocked = true;
        }
        return nextStage;
    }


    [ButtonMethod]
    public void AddAllExercises()
    {
        list.Clear();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    public override void ResetData()
    {
        base.ResetData();
        AddAllExercises();
    }

    private void AddAllExercises(AnimationClip[] clips, ExerciseType eType, float healthScale = 1, float powerScale = 1, float priceScale = 1, int incomeDefault = 1)
    {
        for (int i = 0; i < clips.Length; i++)
        {
            try
            {
                ExerciseData stage = new ExerciseData();
                stage.name = clips[i].name.Replace("_"," ");
                stage.id = clips[i].name;
                stage.index = list.Count;
                stage.isUnlocked = i == 0;
                stage.priceUpgrate = Mathf.FloorToInt(defaultPrice * (int)Mathf.Pow(upScale, i) * priceScale);
                stage.exerciseType = eType;
                stage.time = 30;
                stage.health = defaultHealth * Mathf.Pow(upScale, i) * healthScale;
                stage.power = defaultPower * (int)Mathf.Pow(upScale, i) * powerScale;
                stage.income = incomeDefault + upgradeLevelMax * i;
                stage.level = 1;
                stage.unlockType = UnlockType.Gold;
                stage.spIcon = GetIcon(clips[i].name);
                stage.isSelected = false;

                list.Add(stage);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("AddAllExercises: " + i + " " + ex.Message + " " + ex.StackTrace);
            }
        }
    }

    Sprite GetIcon(string id)
    {
        for (int i = 0; i < spritesIconEx.Length; i++)
        {
            if (spritesIconEx[i].name.Equals(id))
                return spritesIconEx[i];
        }
        return spritesIconEx[0];
    }


    public void UpgradeLevel(ExerciseData ex)
    {
        if (ex.level < upgradeLevelMax)
        {
            ex.level++;
        }
        else
        {
            var next = list.Where(x=>x.exerciseType == ex.exerciseType && !x.isUnlocked).OrderBy(x => x.index).FirstOrDefault();
            if (next != null)
            {
                next.isUnlocked = true;
                Current = next;
            }
        }

    }
}

[System.Serializable]
public class ExerciseData : SaveData
{
    [Header("Exercise Info")]
    public ExerciseType exerciseType;
    public float health;
    public float power;
    public float time;
    public int priceUpgrate;
    public int income;
    public int level;
    public Sprite spIcon;

    public float GetHealth()
    {
        return health * (1 + (level - 1) * DataManager.ExercisesAsset.upgradeStatsPercent);
    }
    public float GetPower()
    {
        return power * (1 + (level - 1) * DataManager.ExercisesAsset.upgradeStatsPercent);
    }
    public int GetPrice()
    {
        return Mathf.RoundToInt(priceUpgrate * (1 + (level - 1) * DataManager.ExercisesAsset.upgradeStatsPercent));
    }
    public int GetIncome()
    {
        return income + level - 1;
    }
}

[System.Serializable]
public enum ExerciseType
{
    Arm,
    Leg,
    Body
}
