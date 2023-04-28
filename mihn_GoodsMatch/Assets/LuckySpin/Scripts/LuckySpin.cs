using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using MyBox;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(UIAnimation))]
public class LuckySpin : MonoBehaviour
{
    [SerializeField]
    private UIAnimation anim = null;
    [SerializeField]
    private LuckySpinAsset spinAsset = null;
    [SerializeField]
    private SpinContent contentPrefab = null;
    [SerializeField]
    private GiftBoxContent giftBoxPrefab = null;
    [SerializeField]
    private Transform contentContainer = null;
    [SerializeField]
    private Transform giftBoxContainer = null;

    [SerializeField]
    private Image giftBoxProgressImg = null;
    [SerializeField]
    private RectTransform currentProgressImg = null;

    [SerializeField]
    private Text timerTxt = null;

    private float timeSpin = 15f;

    private float timeAnimateSliderValue = 0.25f;
    private List<GiftBoxContent> giftBoxContents = new List<GiftBoxContent>();
    float[] probabilities = new float[] { 0.2f, 0.2f, 0.2f, 0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.05f };
    // float[] probabilities = new float[] { 0.0f, 0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1f };
    private LuckySpinReward currentReward = null;
    private int targetAngle;

    private void OnEnable()
    {
        this.RegisterListener((int)EventID.OnSpinDone, OnSpinDone);
        this.RegisterListener((int)EventID.OnReceiverGift, OnReceiverGift);
    }
    private void OnDisable()
    {
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnSpinDone, OnSpinDone);
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnReceiverGift, OnReceiverGift);
    }

    private void OnReceiverGift(object obj)
    {
        this.PostEvent((int)EventID.OnShowRewardPopup, obj);
    }

    private void OnSpinDone(object obj)
    {
        DataManager.UserData.TotalSpinTime++;
        UpdateGiftBoxProgressImg();
        var listUnlockedGift = giftBoxContents.Where(x => x.gift.spinTimeRequiredToGetThisGift == DataManager.UserData.TotalSpinTime).ToList();
        if (listUnlockedGift.Count != 0)
        {
            var gift = listUnlockedGift.FirstOrDefault();
            gift.Unlock();
        }

        StartCoroutine(DelayShowRewardPopUp(obj));
    }
    private IEnumerator DelayShowRewardPopUp(object obj)
    {
        yield return new WaitForSeconds(1);
        this.PostEvent((int)EventID.OnShowRewardPopup, obj);
    }
    SkinData rewardWall = null;
    FloorData rewardFloor = null;
    CarpetData rewardCarpet = null;
    WindowsData rewardWindows = null;
    CeillingData rewardCeilling = null;
    ChairData rewardChair = null;
    TableData rewardTable = null;
    LampData rewardLamp = null;
    public void FillData()
    {
        if (giftBoxContents.Count == 0)
        {
            for (int i = 0; i < spinAsset.luckySpinRewards.Length; i++)
            {
                var rw = spinAsset.luckySpinRewards[i];
                var spineContent = contentPrefab.Spawn(contentContainer);
                spineContent.transform.SetAsFirstSibling();
                spineContent.transform.eulerAngles = new Vector3(0, 0, i * (-360 / spinAsset.luckySpinRewards.Length) + (360 / spinAsset.luckySpinRewards.Length) / 2);
                switch (rw.rewardsTypes)
                {
                    case LuckyRewardsTypes.Coins:
                        spineContent.Init(rw);
                        break;
                    case LuckyRewardsTypes.Wall:
                        var listWallAvailble = DataManager.SkinsAsset.GetNotUnlockedSkin();
                        rewardWall = listWallAvailble[UnityEngine.Random.Range(0, listWallAvailble.Count)];
                        spineContent.Init(rw, rewardWall);
                        break;
                    case LuckyRewardsTypes.Floor:
                        var listFloorAvailble = DataManager.FloorAsset.GetNotUnlockedSkin();
                        rewardFloor = listFloorAvailble[UnityEngine.Random.Range(0, listFloorAvailble.Count)];
                        spineContent.Init(rw, rewardFloor);
                        break;
                    case LuckyRewardsTypes.Windows:
                        var listWindowsAvailble = DataManager.WindowAsset.GetNotUnlockedSkin();
                        rewardWindows = listWindowsAvailble[UnityEngine.Random.Range(0, listWindowsAvailble.Count)];
                        spineContent.Init(rw, rewardWindows);
                        break;
                    case LuckyRewardsTypes.Carpet:
                        var listCarpetAvailble = DataManager.CarpetsAsset.GetNotUnlockedSkin();
                        rewardCarpet = listCarpetAvailble[UnityEngine.Random.Range(0, listCarpetAvailble.Count)];
                        spineContent.Init(rw, rewardCarpet);
                        break;
                    case LuckyRewardsTypes.Ceilling:
                        var listCeillingAvailble = DataManager.CeillingAsset.GetNotUnlockedSkin();
                        rewardCeilling = listCeillingAvailble[UnityEngine.Random.Range(0, listCeillingAvailble.Count)];
                        spineContent.Init(rw, rewardCeilling);
                        break;
                    case LuckyRewardsTypes.Chair:
                        var listChairAvailble = DataManager.ChairsAsset.GetNotUnlockedSkin();
                        rewardChair = listChairAvailble[UnityEngine.Random.Range(0, listChairAvailble.Count)];
                        spineContent.Init(rw, rewardChair);
                        break;
                    case LuckyRewardsTypes.Table:
                        var listTablesAvailble = DataManager.TableAssets.GetNotUnlockedSkin();
                        rewardTable = listTablesAvailble[UnityEngine.Random.Range(0, listTablesAvailble.Count)];
                        spineContent.Init(rw, rewardTable);
                        break;
                    case LuckyRewardsTypes.Lamp:
                        var listLampAvailble = DataManager.LampsAsset.GetNotUnlockedSkin();
                        rewardLamp = listLampAvailble[UnityEngine.Random.Range(0, listLampAvailble.Count)];
                        spineContent.Init(rw, rewardLamp);
                        break;
                }
                if (i == 0)
                    currentReward = rw;
            }
            contentContainer.transform.eulerAngles = new Vector3(0, 0, 180);

            var contentWidth = giftBoxContainer.GetComponent<RectTransform>().sizeDelta.x;
            int totalSpinRequired = spinAsset.giftBoxRewards.LastOrDefault().spinTimeRequiredToGetThisGift;
            var distance = contentWidth / totalSpinRequired;
            for (int i = 0; i < spinAsset.giftBoxRewards.Length; i++)
            {
                var gb = spinAsset.giftBoxRewards[i];
                var box = giftBoxPrefab.Spawn(giftBoxContainer);

                switch (gb.rewardsTypes)
                {
                    case LuckyRewardsTypes.Coins:
                        box.Init(gb);
                        break;
                    case LuckyRewardsTypes.Wall:
                        //var listWallAvailble = DataManager.SkinsAsset.GetNotUnlockedSkin();
                        //rewardWall = listWallAvailble[UnityEngine.Random.Range(0, listWallAvailble.Count)];
                        //box.Init(gb);
                        break;
                    case LuckyRewardsTypes.Floor:
                        //var listFloorAvailble = DataManager.FloorAsset.GetNotUnlockedSkin();
                        //rewardFloor = listFloorAvailble[UnityEngine.Random.Range(0, listFloorAvailble.Count)];
                        //box.Init(gb);
                        break;
                    case LuckyRewardsTypes.Windows:
                        var listWindowsAvailble = DataManager.WindowAsset.GetNotUnlockedSkin();
                        rewardWindows = listWindowsAvailble[UnityEngine.Random.Range(0, listWindowsAvailble.Count)];
                        box.Init(gb, rewardWindows);
                        break;
                    case LuckyRewardsTypes.Carpet:
                        //var listCarpetAvailble = DataManager.CarpetsAsset.GetNotUnlockedSkin();
                        //rewardCarpet = listCarpetAvailble[UnityEngine.Random.Range(0, listCarpetAvailble.Count)];
                        //box.Init(gb);
                        break;
                    case LuckyRewardsTypes.Ceilling:
                        //var listCeillingAvailble = DataManager.CeillingAsset.GetNotUnlockedSkin();
                        //rewardCeilling = listCeillingAvailble[UnityEngine.Random.Range(0, listCeillingAvailble.Count)];
                        //box.Init(gb);
                        break;
                    case LuckyRewardsTypes.Chair:
                        //var listChairAvailble = DataManager.ChairsAsset.GetNotUnlockedSkin();
                        //rewardChair = listChairAvailble[UnityEngine.Random.Range(0, listChairAvailble.Count)];
                        //box.Init(gb);
                        break;
                    case LuckyRewardsTypes.Table:
                        var listTablesAvailble = DataManager.TableAssets.GetNotUnlockedSkin();
                        rewardTable = listTablesAvailble[UnityEngine.Random.Range(0, listTablesAvailble.Count)];
                        box.Init(gb, rewardTable);
                        break;
                    case LuckyRewardsTypes.Lamp:
                        var listLampAvailble = DataManager.LampsAsset.GetNotUnlockedSkin();
                        rewardLamp = listLampAvailble[UnityEngine.Random.Range(0, listLampAvailble.Count)];
                        box.Init(gb, rewardLamp);
                        break;
                }

                //box.Init(gb);
                var rect = box.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(distance * spinAsset.giftBoxRewards[i].spinTimeRequiredToGetThisGift, rect.anchoredPosition.y);

                giftBoxContents.Add(box);
            }

            UpdateGiftBoxProgressImg();

            if (DataManager.UserData.RemainSpinTime == 0)
                DataManager.UserData.RemainSpinTime = spinAsset.timeBetweenTwoFreeSpinInSeconds;
        }

        if (DataManager.UserData.TimeOnQuitSpinner != DateTime.MinValue)
        {
            var elapsedTimeSinceLastOpen = (DateTime.Now - DataManager.UserData.TimeOnQuitSpinner).TotalSeconds;

            DataManager.UserData.RemainSpinTime -= (int)elapsedTimeSinceLastOpen;
            if (DataManager.UserData.RemainSpinTime >= 0)
            {
                StartCoroutine(CountDown());
            }
            else
            {
                timerTxt.transform.GetChild(0).gameObject.SetActive(false);//disactive the stop watch icon
                timerTxt.text = "Spin Now";
                DataManager.UserData.RemainSpinTime = -1;
            }
        }
        else
        {
            DataManager.UserData.RemainSpinTime = -1;
            timerTxt.transform.GetChild(0).gameObject.SetActive(false);//disactive the stop watch icon
            timerTxt.text = "Spin Now";
        }
    }
    private IEnumerator CountDown()
    {
        timerTxt.transform.GetChild(0).gameObject.SetActive(true);//active the stop watch icon
        while (DataManager.UserData.RemainSpinTime > -1)
        {
            Timer(DataManager.UserData.RemainSpinTime);
            yield return new WaitForSeconds(1);
            DataManager.UserData.RemainSpinTime--;
            if (DataManager.UserData.RemainSpinTime == -1)
            {
                timerTxt.transform.GetChild(0).gameObject.SetActive(false);//disactive the stop watch icon
                timerTxt.text = "Spin Now";
            }
        }
    }
    private void Timer(int timer)
    {
        int hours = Mathf.FloorToInt(timer / 3600);
        int minutes = Mathf.FloorToInt((timer - hours * 3600) / 60);
        int seconds = Mathf.FloorToInt(timer - hours * 3600 - minutes * 60);
        timerTxt.text = $"{minutes}m : {seconds}s";
    }
    public LuckySpinReward GetRandomGift()
    {
        float random = UnityEngine.Random.Range(0f, 1f);
        float cumulativeProbability = 0f;
        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulativeProbability += probabilities[i];
            if (random <= cumulativeProbability)
            {
                Debug.Log($"Your reward is: {spinAsset.luckySpinRewards[i].rewardsTypes} with amount: {spinAsset.luckySpinRewards[i].rewardAmount}");
                var reward = spinAsset.luckySpinRewards[i];
                return reward;
            }
        }
        return null;
    }
    private void UpdateGiftBoxProgressImg()
    {
        var currentValue = giftBoxProgressImg.fillAmount;
        var targetValue = 1f / spinAsset.giftBoxRewards.LastOrDefault().spinTimeRequiredToGetThisGift * DataManager.UserData.TotalSpinTime;
        var contentWidth = giftBoxContainer.GetComponent<RectTransform>().sizeDelta.x;
        DOVirtual.Float(currentValue, targetValue, timeAnimateSliderValue, (f) =>
          {
              giftBoxProgressImg.fillAmount = f;
              currentProgressImg.anchoredPosition = new Vector2(contentWidth * f, currentProgressImg.anchoredPosition.y);
          });
    }
    private void GetRandomRewardThenSpin()
    {
        //Select reward
        var gift = GetRandomGift();
        if (gift.index > currentReward.index)
        {
            targetAngle = (gift.index - currentReward.index) * 45;
        }
        else if (gift.index == currentReward.index)
        {
            targetAngle = (int)transform.eulerAngles.z;
        }
        else
        {
            targetAngle = (spinAsset.luckySpinRewards.Length - currentReward.index + gift.index) * 45;
        }

        targetAngle = (360 - targetAngle) + 3600;
        //Debug.Log("Target Angle " + targetAngle);
        Spin();
        currentReward = gift;
    }
    public void Ins_BtnSpinClick()
    {
        if (!isSpinning)
        {
            if (DataManager.UserData.RemainSpinTime == -1)
            {
                GetRandomRewardThenSpin();
                DataManager.UserData.RemainSpinTime = spinAsset.timeBetweenTwoFreeSpinInSeconds;
                StartCoroutine(CountDown());
            }
            else
            {
                //Debug.Log();
                UIToast.ShowNotice($"Free spin will be availble in: {DataManager.UserData.RemainSpinTime} seconds");
            }
        }
        else
        {
            UIToast.ShowNotice($"Please Wait...!!!");
        }
    }
    public void Ins_BtnAdsSpinClick()
    {
        if (!isSpinning)
        {
            Base.Ads.AdsManager.ShowVideoReward((e, t) =>
            {
                if (e == AdEvent.ShowSuccess)
                {
                    GetRandomRewardThenSpin();
                }
                Base.Ads.AdsManager.ShowNotice(e);
            }, "lucky_spin");
        }
        else
        {
            UIToast.ShowNotice($"Please Wait...!!!");
        }
    }
    private bool isSpinning = false;
    private void Spin()
    {
        isSpinning = true;
        //Debug.Log($"Angle on spine: {contentContainer.transform.eulerAngles.z}");
        contentContainer.transform.DORotate(new Vector3(0, 0, -targetAngle), timeSpin, RotateMode.LocalAxisAdd).SetEase(Ease.OutExpo).OnComplete(() =>
        {
            isSpinning = false;
            this.PostEvent((int)EventID.OnSpinDone, currentReward);
        });
    }
    public void Show()
    {
        anim.Show();
    }
    public void Hide()
    {
        if (!isSpinning)
        {
            if (anim.Status == UIAnimStatus.IsShow)
            {
                GameStateManager.Idle(null);
                anim.Hide();
                StopCoroutine(CountDown());
                DataManager.UserData.TimeOnQuitSpinner = DateTime.Now;
            }
        }
        else
        {
            UIToast.ShowNotice($"Please Wait...!!!");
        }
    }
    private void OnApplicationQuit()
    {
        if (anim.Status == UIAnimStatus.IsShow)
        {
            DataManager.UserData.TimeOnQuitSpinner = DateTime.Now;
        }
    }
#if UNITY_EDITOR
    [ButtonMethod]
    public void SpinTest()
    {
        Spin();
    }
#endif
}

[System.Serializable]
public class LuckySpinReward
{
    public int index;
    public LuckyRewardsTypes rewardsTypes;
    public int rewardAmount;
    public Color color;
    public Sprite rewardSpriteIcon;
    [NonSerialized] public SkinData tmpRewardSkin;
    [NonSerialized] public FloorData tmpRewardFloor;
    [NonSerialized] public WindowsData tmpRewardWindows;
    [NonSerialized] public CarpetData tmpRewardCarpet;
    [NonSerialized] public CeillingData tmpRewardCeilling;
    [NonSerialized] public ChairData tmpRewardChair;
    [NonSerialized] public TableData tmpRewardTable;
    [NonSerialized] public LampData tmpRewardLamp;
}

[System.Serializable]
public class GiftBoxReward
{
    public int index;
    public int spinTimeRequiredToGetThisGift;
    public LuckyRewardsTypes rewardsTypes;
    public int rewardAmount;
    public Sprite rewardSpriteIcon;
    [NonSerialized] public SkinData tmpRewardSkin;
    [NonSerialized] public FloorData tmpRewardFloor;
    [NonSerialized] public WindowsData tmpRewardWindows;
    [NonSerialized] public CarpetData tmpRewardCarpet;
    [NonSerialized] public CeillingData tmpRewardCeilling;
    [NonSerialized] public ChairData tmpRewardChair;
    [NonSerialized] public TableData tmpRewardTable;
    [NonSerialized] public LampData tmpRewardLamp;
}

public enum LuckyRewardsTypes
{
    Coins,
    Wall,
    Floor,
    Windows,
    Carpet,
    Ceilling,
    Chair,
    Table,
    Lamp
}