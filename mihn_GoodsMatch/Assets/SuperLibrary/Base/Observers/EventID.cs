
public enum EventID
{
    None = 0,

    OnFSMStateEnter = 1,
    OnFSMStateUpdate = 2,
    OnFSMStateExit = 3,
    OnAnimatorTrigger = 4,
    OnAttack = 5,
    OnDeath = 6,
    OnInit = 7,
    OnDamage = 8,

    OnBuffHint = 20,
    OnBuffSwap = 21,

    OnPlayMusic = 40,

    OnGameReady = 50,

    OnGoToChallengeLevel = 60,
    OnModeBartenderUnlocked = 61,

    OnAlertTimeout = 70,

    OnNewMatchSuccess = 100,
    OnMatchedWrongRequest = 101,
    OnMatchedRightRequest = 102,
    OnNewCombo = 103,

    OnNewRequestCreated = 200,
    OnRequestTimeout = 201,
    
    OnClearLastLevel = 300,

    OnTutStepDone = 400,

    OnPauseAppByAds = 1000,
    OnDecorTutorialComplete = 1001,
    BuySuccess = 1002,
    OnSpinDone = 1003,
    OnReceiverGift = 1004,
    OnShowRewardPopup = 1005,
    OnClaimReward = 1006,
    OnClaimGiftBox = 1007,
    OnX2Coin = 1008,
    ChooseMap = 1009,

    ShowItemPreview = 2000,
}
