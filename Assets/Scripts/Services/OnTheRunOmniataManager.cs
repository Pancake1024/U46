using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class OnTheRunOmniataManager : Manager<OnTheRunOmniataManager>
{
    #region Singleton instance
    public static OnTheRunOmniataManager Instance
    {
        get
        {
            return Manager<OnTheRunOmniataManager>.Get();
        }
    }
    #endregion
    
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport("__Internal")]
	private static extern bool _isJailbroken();
	
	[DllImport("__Internal")]
	private static extern string getIdfa();
#endif

    static bool IsJailbroken
    {
        get
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            return _isJailbroken();
#else
            return false;
#endif
        }
    }

	public static string UserId
	{
		get
		{
#if UNITY_EDITOR || UNITY_WEBPLAYER
            return string.Empty;
#elif UNITY_IPHONE && !UNITY_EDITOR
			return getIdfa();
#elif UNITY_ANDROID && !UNITY_KINDLE
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.ontherun.OnTheRunActivity"))
                return jc.CallStatic<string>("getAdvertisingId");
#elif UNITY_ANDROID && UNITY_KINDLE
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.ontherun.OnTheRunActivity"))
                return jc.CallStatic<string>("getAndroidId");
#endif
        }
	}

    OmniataImplementation implementation;
    string apiKey;
    bool isJailbroken;

    public string developmentApiKey = "9ff71e8f";
    public string iOsApiKey = "fd60cf2f";
    public string androidApiKey = "32b94fd2";
    public string amazonApiKey = "148221eb";

    OnTheRunEnvironment environmentManager;
    //public string userId = "";

    bool isRunning = false;
    float runTime = 0.0f;

    bool sessionHasBegun = false;

    new void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);
		
		DetectJailbroken();

        SetApiKey();
        SetImplementation();
        Initialize();

        trackUserWasRequested = false;
        trackMcInfoWasRequested = false;
	}
	
	void DetectJailbroken()
	{
		isJailbroken = IsJailbroken;
	}

    void SetApiKey()
    {
#if UNITY_IPHONE && !UNITY_EDITOR
		if (SBS.Miniclip.MCUtilsBindings.isMassiveTestBuild())
            apiKey = developmentApiKey;
        else
            apiKey = iOsApiKey;
#elif UNITY_ANDROID
#   if DEVELOPMENT_BUILD
        apiKey = developmentApiKey;
#   elif !UNITY_KINDLE && !UNITY_EDITOR
        apiKey = androidApiKey;
#   elif UNITY_KINDLE && !UNITY_EDITOR
        apiKey = amazonApiKey;
#   endif
#else
        apiKey = developmentApiKey;
#endif
    }

    void SetImplementation()
    {
#if UNITY_EDITOR
        implementation = gameObject.AddComponent<EditorOmniataImplementation>();
#elif UNITY_WEBPLAYER || UNITY_WP8
        implementation = gameObject.AddComponent<VoidOmniataImplementation>();
#elif UNITY_ANDROID
        implementation = gameObject.AddComponent<UnitySdkOmniataImplementation>();
#else
        implementation = gameObject.AddComponent<UnitySdkOmniataImplementation>();
#endif
    }

    void Initialize()
    {
#if UNITY_WEBPLAYER
        return;
#endif
        //if (McSocialApiManager.Instance != null)
        //    userId = McSocialApiManager.Instance.UserLoginData.Id;

        implementation.Initialize(apiKey, UserId);
    }

    void OmniataSessionBegin()
    {
#if UNITY_WEBPLAYER
        return;
#endif
#if UNITY_IPHONE
        implementation.TrackSessionBegin(OmniataIds.Param_iOS_Idfa, UserId);
#else
        implementation.TrackSessionBegin();
#endif

        //TrackMcInfo();

        if (trackUserWasRequested)
            TrackUser();

        if (trackMcInfoWasRequested)
            TrackMcInfo();

        sessionHasBegun = true;
    }

    void OmniataSessionEnd()
    {
#if UNITY_WEBPLAYER
        return;
#endif
        sessionHasBegun = false;
        trackUserWasRequested = false;
        trackMcInfoWasRequested = false;

        TimeSpan durationTimeSpan;
        OnTheRunSessionsManager.Instance.GetSessionTimeSpan(out durationTimeSpan);
        string durationInSeconds = Math.Truncate(durationTimeSpan.TotalSeconds).ToString();

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add(OmniataIds.Param_SessionLength, durationInSeconds);
        parameters.Add(OmniataIds.Param_LastMenu, Manager<UIManager>.Get().ActivePageName);
        parameters.Add(OmniataIds.Param_Coins, PlayerPersistentData.Instance.Coins.ToString());
        parameters.Add(OmniataIds.Param_Gems, PlayerPersistentData.Instance.Diamonds.ToString());
        parameters.Add(OmniataIds.Param_PlayerLevel, PlayerPersistentData.Instance.Level.ToString());
        
        implementation.TrackEvent(OmniataIds.Evt_SessionEnd, parameters);
    }

    public void TrackPurchase(string product_id, string currencyCode, float expense, bool hackedPurchase)
    {
#if UNITY_WEBPLAYER
        return;
#endif

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add(OmniataIds.Param_PromotionId, string.Empty);
        parameters.Add(OmniataIds.Param_ProductId, product_id);
        parameters.Add(OmniataIds.Param_Total, expense.ToString());
        parameters.Add(OmniataIds.Param_CurrencyCode, currencyCode);
        parameters.Add(OmniataIds.Param_Coins, PlayerPersistentData.Instance.Coins.ToString());
        parameters.Add(OmniataIds.Param_Gems, PlayerPersistentData.Instance.Diamonds.ToString());
        parameters.Add(OmniataIds.Param_PlayerLevel, PlayerPersistentData.Instance.Level.ToString());
        parameters.Add(OmniataIds.Param_GameVersion, GetVersionString());

		if (hackedPurchase)
			implementation.TrackEvent(OmniataIds.Evt_Hacker_Purchase, parameters);
		else
			implementation.TrackEvent(OmniataIds.Evt_Purchase, parameters);
    }

    public bool trackUserWasRequested = false;

    public void RequestTrackUser()
    {
        //Debug.Log("TRACK USER - sessionHasBegun: " + sessionHasBegun + " - trackUserWasRequested: " + trackUserWasRequested);
        if (sessionHasBegun)
            TrackUser();
        else
            trackUserWasRequested = true;
    }

    public void TrackUser()
    {
#if UNITY_WEBPLAYER
        return;
#endif

        string gender = "u";

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        
        if (OnTheRunFacebookManager.Instance.IsInitialized && OnTheRunFacebookManager.Instance.IsLoggedIn)
        {
            parameters.Add(OmniataIds.Param_FacebookId, OnTheRunFacebookManager.Instance.UserId);

            if (!string.IsNullOrEmpty(OnTheRunFacebookManager.Instance.Birthday))
                parameters.Add(OmniataIds.Param_Dob, OnTheRunFacebookManager.Instance.Birthday);

            string fb_gender = OnTheRunFacebookManager.Instance.Gender;
            if (fb_gender.Equals("male"))
                gender = "m";
            else if (fb_gender.Equals("female"))
                gender = "f";
        }

        parameters.Add(OmniataIds.Param_Gender, gender);

        implementation.TrackEvent(OmniataIds.Evt_User, parameters);

        trackUserWasRequested = false;
    }

    public bool trackMcInfoWasRequested = false;

    public void RequestTrackMcInfo()
    {
        if (sessionHasBegun)
            TrackMcInfo();
        else
            trackMcInfoWasRequested = true;
    }

    public void TrackMcInfo()//string loginType, int friendsNumber)
    {
#if UNITY_WEBPLAYER
        return;
#endif

        string loginType = string.Empty;
        int friendsNumber = 0;

        if (McSocialApiManager.Instance != null)
            loginType = McSocialApiManager.Instance.UserLoginData.Type.ToString();

        if (OnTheRunFacebookManager.Instance != null)
            friendsNumber = OnTheRunFacebookManager.Instance.FriendsDictionary.Count;

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add(OmniataIds.Param_LoginType, loginType);
        parameters.Add(OmniataIds.Param_FacebookFriendsNumber, friendsNumber.ToString());
		parameters.Add(OmniataIds.Param_Jailbroken, isJailbroken ? "1" : "0");
        parameters.Add(OmniataIds.Param_UserIsHacker, OnTheRunInAppManager.Instance.UserHasHackedPurchases ? "1" : "0");
        parameters.Add(OmniataIds.Param_GameVersion, GetVersionString());
        parameters.Add(OmniataIds.Param_Coins, PlayerPersistentData.Instance.Coins.ToString());
        parameters.Add(OmniataIds.Param_Gems, PlayerPersistentData.Instance.Diamonds.ToString());
        parameters.Add(OmniataIds.Param_PlayerLevel, PlayerPersistentData.Instance.Level.ToString());
        parameters.Add(OmniataIds.Param_TotalPlayTime, OnTheRunPlayTimeCounter.Instance.GetPlayTime().ToString());

        implementation.TrackEvent(OmniataIds.Evt_Info, parameters);

        trackMcInfoWasRequested = false;
    }

    public void TrackVirtualPurchase(string product_id, PriceData.CurrencyType currencyType, string total, string type)
    {
        TrackVirtualPurchase(product_id, currencyType, total, string.Empty, type);
    }

    public void TrackVirtualPurchase(string product_id, PriceData.CurrencyType currencyType, string total, string promotionId, string type)
    {
#if UNITY_WEBPLAYER
        return;
#endif

        string currency = "";
        if (currencyType == PriceData.CurrencyType.FirstCurrency)
            currency = OmniataIds.Currency_Coins;
        else if (currencyType == PriceData.CurrencyType.SecondCurrency)
            currency = OmniataIds.Currency_Diamonds;

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add(OmniataIds.Param_PromotionId, promotionId);
        parameters.Add(OmniataIds.Param_ProductId, product_id);
        parameters.Add(OmniataIds.Param_Type, type);
        parameters.Add(OmniataIds.Param_SoftCurrency, currency);
        parameters.Add(OmniataIds.Param_Total, total);
        parameters.Add(OmniataIds.Param_Coins, PlayerPersistentData.Instance.Coins.ToString());
        parameters.Add(OmniataIds.Param_Gems, PlayerPersistentData.Instance.Diamonds.ToString());
        parameters.Add(OmniataIds.Param_PlayerLevel, PlayerPersistentData.Instance.Level.ToString());

        implementation.TrackEvent(OmniataIds.Evt_VirtualPurchase, parameters);
    }

    public void OnRunStarted()
    {
#if UNITY_WEBPLAYER
        return;
#endif

        implementation.ProcessEvents = false;
        ResetAndStartTakingRunTime();
    }

    public void OnRunFinished(int coinsWon, int distance, string vehicle, int slipstreamCount, int maxCombo, int hitsCount, bool[] initBoosters)
    {
#if UNITY_WEBPLAYER
        return;
#endif
        implementation.ProcessEvents = true;
        StopTakingRunTime();

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add(OmniataIds.Param_Coins, PlayerPersistentData.Instance.Coins.ToString());
        parameters.Add(OmniataIds.Param_CoinsWon, coinsWon.ToString());
        parameters.Add(OmniataIds.Param_Distance, distance.ToString());
        parameters.Add(OmniataIds.Param_PlayerLevel, PlayerPersistentData.Instance.Level.ToString());
        parameters.Add(OmniataIds.Param_Vehicle, vehicle);
        parameters.Add(OmniataIds.Param_RunTime, Mathf.RoundToInt(runTime).ToString());

        if(environmentManager==null)
            environmentManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>().GetComponent<OnTheRunEnvironment>();

        string tierId = "none";
        switch (environmentManager.currentEnvironment)
        {
            case OnTheRunEnvironment.Environments.Europe: tierId = OnTheRunDataLoader.Instance.GetLocaleString("europeanCarsTitle"); break;
            case OnTheRunEnvironment.Environments.Asia: tierId = OnTheRunDataLoader.Instance.GetLocaleString("orientalCarsTitle"); break;
            case OnTheRunEnvironment.Environments.NY: tierId = OnTheRunDataLoader.Instance.GetLocaleString("americanCarsTitle"); break;
            case OnTheRunEnvironment.Environments.USA: tierId = OnTheRunDataLoader.Instance.GetLocaleString("muscleCarsTitle"); break;
        }
        parameters.Add(OmniataIds.Param_Tier, tierId);
        //parameters.Add(OmniataIds.Param_Tier, environmentManager.currentEnvironment.ToString());

        parameters.Add(OmniataIds.Param_SlipstreamCount, slipstreamCount.ToString());
        parameters.Add(OmniataIds.Param_MaxCombo, maxCombo.ToString());
        parameters.Add(OmniataIds.Param_HitsCount, hitsCount.ToString());

        //for (int i = 0; i < initBoosters.Length; ++i)
        //   parameters.Add(((OnTheRunBooster.BoosterType)i).ToString(), initBoosters[i] ? "1" : "0");

        implementation.TrackEvent(OmniataIds.Evt_Run, parameters);
    }

    public void TrackTutorialStepFinished(OnTheRunTutorialManager.TutorialType finishedStep)
    {
#if UNITY_WEBPLAYER
        return;
#endif

        string finishedStepParam = string.Empty;
        switch(finishedStep)
        {
            case OnTheRunTutorialManager.TutorialType.AvoidFirstCar:
                finishedStepParam = OmniataIds.TutorialStep_AvoidTraffic;
                break;

            case OnTheRunTutorialManager.TutorialType.SingleSlipstream:
                finishedStepParam = OmniataIds.TutorialStep_Slipstream;
                break;

            case OnTheRunTutorialManager.TutorialType.Turbo:
                finishedStepParam = OmniataIds.TutorialStep_Bolts;
                break;

            case OnTheRunTutorialManager.TutorialType.Trucks:
                finishedStepParam = OmniataIds.TutorialStep_Trucks;
                break;
        }

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add(OmniataIds.Param_TutorialStep, finishedStepParam);

        implementation.TrackEvent(OmniataIds.Evt_Tutorial, parameters);
	}
	
	public void TrackFacebookShare(string shareType)
	{
#if UNITY_WEBPLAYER
		return;
#endif
		
		Dictionary<string, string> parameters = new Dictionary<string, string>();

		parameters.Add(OmniataIds.Param_FacebookShareType, shareType);
		parameters.Add(OmniataIds.Param_Coins, PlayerPersistentData.Instance.Coins.ToString());
		parameters.Add(OmniataIds.Param_Gems, PlayerPersistentData.Instance.Diamonds.ToString());
		parameters.Add(OmniataIds.Param_PlayerLevel, PlayerPersistentData.Instance.Level.ToString());
		
		implementation.TrackEvent(OmniataIds.Evt_FacebookShare, parameters);
	}
	
	public void TrackFacebookInvite()
	{
#if UNITY_WEBPLAYER
		return;
#endif
		
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		parameters.Add(OmniataIds.Param_Coins, PlayerPersistentData.Instance.Coins.ToString());
		parameters.Add(OmniataIds.Param_Gems, PlayerPersistentData.Instance.Diamonds.ToString());
        parameters.Add(OmniataIds.Param_PlayerLevel, PlayerPersistentData.Instance.Level.ToString());
        parameters.Add(OmniataIds.Param_TotalPlayTime, OnTheRunPlayTimeCounter.Instance.GetPlayTime().ToString());
		
		implementation.TrackEvent(OmniataIds.Evt_FacebookInvite, parameters);
	}
	
	public void TrackFuelSent()
	{
#if UNITY_WEBPLAYER
		return;
#endif

		Dictionary<string, string> parameters = new Dictionary<string, string>();
		parameters.Add(OmniataIds.Param_Coins, PlayerPersistentData.Instance.Coins.ToString());
		parameters.Add(OmniataIds.Param_Gems, PlayerPersistentData.Instance.Diamonds.ToString());
        parameters.Add(OmniataIds.Param_PlayerLevel, PlayerPersistentData.Instance.Level.ToString());
        parameters.Add(OmniataIds.Param_TotalPlayTime, OnTheRunPlayTimeCounter.Instance.GetPlayTime().ToString());

		implementation.TrackEvent(OmniataIds.Evt_FuelSent, parameters);
	}
	
	public void TrackFuelReceived()
	{
#if UNITY_WEBPLAYER
		return;
#endif

		Dictionary<string, string> parameters = new Dictionary<string, string>();
		parameters.Add(OmniataIds.Param_Coins, PlayerPersistentData.Instance.Coins.ToString());
		parameters.Add(OmniataIds.Param_Gems, PlayerPersistentData.Instance.Diamonds.ToString());
        parameters.Add(OmniataIds.Param_PlayerLevel, PlayerPersistentData.Instance.Level.ToString());
        parameters.Add(OmniataIds.Param_TotalPlayTime, OnTheRunPlayTimeCounter.Instance.GetPlayTime().ToString());

		implementation.TrackEvent(OmniataIds.Evt_FuelReceived, parameters);
	}

    public void TrackMissionCompleted(string missionId, int missionCounter, int playerReach, bool completed, int lastFor, string popupType) //OnTheRunMissions.MissionDifficulty difficulty, 
    {
#if UNITY_WEBPLAYER
		return;
#endif
        string tierId = "none";
        switch (environmentManager.currentEnvironment)
        {
            case OnTheRunEnvironment.Environments.Europe: tierId = OnTheRunDataLoader.Instance.GetLocaleString("europeanCarsTitle"); break;
            case OnTheRunEnvironment.Environments.Asia: tierId = OnTheRunDataLoader.Instance.GetLocaleString("orientalCarsTitle"); break;
            case OnTheRunEnvironment.Environments.NY: tierId = OnTheRunDataLoader.Instance.GetLocaleString("americanCarsTitle"); break;
            case OnTheRunEnvironment.Environments.USA: tierId = OnTheRunDataLoader.Instance.GetLocaleString("muscleCarsTitle"); break;
        }

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add(OmniataIds.Param_MissionId, missionId);
        parameters.Add(OmniataIds.Param_MissionThreshold, missionCounter.ToString());
        parameters.Add(OmniataIds.Param_MissionPlayerReach, playerReach.ToString());
        parameters.Add(OmniataIds.Param_Tier, tierId);
        parameters.Add(OmniataIds.Param_MissionCompleted, completed ? "1" : "0");
        parameters.Add(OmniataIds.Param_MissionLast, lastFor.ToString());
        parameters.Add(OmniataIds.Param_MissionHelperPopup, popupType);
        
        implementation.TrackEvent(OmniataIds.Evt_EndRun_Mission, parameters);
    }

    public void TrackSkipMissionPopup(string missionId, int missionCounter, string btnClicked) //OnTheRunMissions.MissionDifficulty difficulty, 
    {
#if UNITY_WEBPLAYER
		return;
#endif
        string tierId = "none";
        switch (environmentManager.currentEnvironment)
        {
            case OnTheRunEnvironment.Environments.Europe: tierId = OnTheRunDataLoader.Instance.GetLocaleString("europeanCarsTitle"); break;
            case OnTheRunEnvironment.Environments.Asia: tierId = OnTheRunDataLoader.Instance.GetLocaleString("orientalCarsTitle"); break;
            case OnTheRunEnvironment.Environments.NY: tierId = OnTheRunDataLoader.Instance.GetLocaleString("americanCarsTitle"); break;
            case OnTheRunEnvironment.Environments.USA: tierId = OnTheRunDataLoader.Instance.GetLocaleString("muscleCarsTitle"); break;
        }

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add(OmniataIds.Param_MissionId, missionId);
        parameters.Add(OmniataIds.Param_MissionThreshold, missionCounter.ToString());
        parameters.Add(OmniataIds.Param_Tier, tierId);
        parameters.Add(OmniataIds.Param_MissionSkipBtnClicked, btnClicked);

        implementation.TrackEvent(OmniataIds.Evt_EndRun_Mission_Skip, parameters);
    }

    public void TrackDailyBonus(int daily_completed_count, string streak, int streak_days)
    {
#if UNITY_WEBPLAYER
		return;
#endif
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add(OmniataIds.Param_Day, daily_completed_count.ToString());
        parameters.Add(OmniataIds.Param_Streak, streak);
        parameters.Add(OmniataIds.Param_StreakDays, streak_days.ToString());
        
        implementation.TrackEvent(OmniataIds.Evt_DailyBonus, parameters);
    }

    public void TrackSpinGame(string prizeId, int quantity)
    {
#if UNITY_WEBPLAYER
		return;
#endif

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add(OmniataIds.Param_SpinPrize, prizeId);
        parameters.Add(OmniataIds.Param_Total, quantity.ToString());

        implementation.TrackEvent(OmniataIds.Evt_SpinGame, parameters);
    }

    public void TrackSaveMe(bool clicked, string type, int counter, double timePassed, double timeRemaining)
    {
#if UNITY_WEBPLAYER
		return;
#endif

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add(OmniataIds.Param_SaveMeCounter, counter.ToString());
        parameters.Add(OmniataIds.Param_SaveMeClicked, clicked ? "true" : "false");
        parameters.Add(OmniataIds.Param_SaveMeType, type);
        parameters.Add(OmniataIds.Param_SaveMeTimePassed, timePassed.ToString());
        parameters.Add(OmniataIds.Param_SaveMeTimeRemaining, timeRemaining.ToString());

        implementation.TrackEvent(OmniataIds.Evt_SaveMe, parameters);
    }

    public void TrackLevelUp()
    {
#if UNITY_WEBPLAYER
		return;
#endif

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add(OmniataIds.Param_PlayerLevel, PlayerPersistentData.Instance.Level.ToString());
        parameters.Add(OmniataIds.Param_TotalPlayTime, OnTheRunPlayTimeCounter.Instance.GetPlayTime().ToString());

        implementation.TrackEvent(OmniataIds.Evt_LevelUp, parameters);
    }

    public void TrackWatchVideoAds(OnTheRunCoinsService.WatchVideoPlacement placement)
    {
#if UNITY_WEBPLAYER
		return;
#endif

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add(OmniataIds.Param_VideoAdsPlacement, placement.ToString());

        implementation.TrackEvent(OmniataIds.Evt_VideoAds, parameters);
    }

    public void TrackCarUpdate(string carId, string carType, string updgradeType, int upgradeLevel, int tierIndex)
    {
#if UNITY_WEBPLAYER
		return;
#endif
        string tierId = "none";
        switch (tierIndex)
        {
            case 0: tierId = OnTheRunDataLoader.Instance.GetLocaleString("europeanCarsTitle"); break;
            case 1: tierId = OnTheRunDataLoader.Instance.GetLocaleString("orientalCarsTitle"); break;
            case 2: tierId = OnTheRunDataLoader.Instance.GetLocaleString("americanCarsTitle"); break;
            case 3: tierId = OnTheRunDataLoader.Instance.GetLocaleString("muscleCarsTitle"); break;
        }

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add(OmniataIds.Param_CardId, carId);
        parameters.Add(OmniataIds.Param_Type, carType);
        parameters.Add(OmniataIds.Param_UpgradeType, updgradeType);
        parameters.Add(OmniataIds.Param_UpgradeLevel, upgradeLevel.ToString());
        parameters.Add(OmniataIds.Param_Tier, tierId);

        implementation.TrackEvent(OmniataIds.Evt_CarUpgrade, parameters);
    }

    void Update()
    {
        if (isRunning)
        {
            runTime += TimeManager.Instance.MasterSource.DeltaTime;
        }
    }

    void ResetAndStartTakingRunTime()
    {
        runTime = 0.0f;
        isRunning = true;
    }

    void StopTakingRunTime()
    {
        isRunning = false;
    }

    string GetVersionString()
    {
        string versionStr = SBS.Core.iOSUtils.GetAppVersion();
#if UNITY_ANDROID
        versionStr = AndroidUtils.GetVersionName();
#elif UNITY_WP8
        versionStr = SBS.Miniclip.WP8Bindings.VersionNumber;
#endif

        return versionStr;
    }
}