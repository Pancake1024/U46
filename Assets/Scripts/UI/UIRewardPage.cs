using UnityEngine;
using SBS.Core;
using System.Collections.Generic;
using System.Collections;
using System;
//-------------------------------------------------------//
[AddComponentMenu("OnTheRun/UI/UIRewardPage")]
public class UIRewardPage : MonoBehaviour
{
    [NonSerialized]
    public bool backToCarBase = false;
    public GameObject bottomBar;
    public GameObject SubmitScoreButton;
    public UIButton shareButton;
    public GameObject FBIcon;
    public GameObject FBIconGhost;
    public UINewMissionsPanel missionRewardPanel;
    public UIRankScroller rankPanel;

    private float overTapTime = 5.0f;

	private UISharedData uiSharedData;
    private OnTheRunGameplay gameplayManager;
    //protected OnTheRunWeeklyChallenges weeklyChallengesManager;
	private OnTheRunInterfaceSounds interfaceSounds;
	//private GameObject metersCollectButton;
    //private GameObject coinsCollectButton;
    private UIRoot m_pUIRoot = null;
	private UITextField m_pBestMetersText = null;
    private UITextField weeklyTextfield;
    private bool canAnimationBegin = true;
    
	private UITextField m_pCurrentMetersText = null;
    private UITextField m_pBestMetersLabel = null;
    private GameObject newRecord;
    private float newRecordInitX;

    private bool hasBeenBestScore = false;
    private bool willBeRankUp = false;

    private OnTheRunSingleRunMissions.DriverMission tierMission;

    protected bool waitForNoPopups = false;

    static public bool alreadyEntered = false;
    static public bool firstStep = true;
    static public bool canSkip = false;

    static public int skipMissionCounter = -1;
    string skipMissionCounterID = "skp_mss_cnt_id";

    protected string[] oldMissionId = {"", "", "", ""};
    protected int[] sameMissionCounter = {0, 0, 0, 0};

    public bool WillRankUp
    {
        set { willBeRankUp = value; }
        get { return willBeRankUp; }
    }

	//-------------------------------------------------------//
    void Awake()
    {
        bottomBar = Manager<UIRoot>.Get().transform.FindChild("RewarBottomBar").gameObject;
        skipMissionCounter = PlayerPrefs.GetInt(skipMissionCounterID, -1);
    }

    void OnEnable()
    {
        UIRewardPage.firstStep = true;
        UIRewardPage.canSkip = false;

        rankPanel.gameObject.SetActive(true);

        if (Manager<UIManager>.Get().ActivePage.gameObject != gameObject)
            return;

		if( m_pUIRoot == null )
			m_pUIRoot = Manager<UIRoot>.Get();

		m_pUIRoot.ShowCurrenciesItem(true);

        Transform centralRoot = transform.FindChild("CenterScreenAnchor");
        transform.FindChild("CenterScreenAnchorWeb").gameObject.SetActive(false);

        if (FBIcon != null)
        {
            float xPos = shareButton.transform.localPosition.x - shareButton.GetComponent<BoxCollider2D>().size.x * 0.2f;
            Vector3 newPos = new Vector3(xPos, FBIcon.transform.localPosition.y, FBIcon.transform.localPosition.z);
            FBIcon.transform.localPosition = newPos;
            FBIconGhost.transform.localPosition = newPos;
        }

#if UNITY_WEBPLAYER
        shareButton.gameObject.SetActive(false);
        if (FBIcon != null)
            FBIcon.SetActive(false);
        if (FBIconGhost != null)
            FBIconGhost.SetActive(false);
#else
        shareButton.transform.FindChild("tfTextfield").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("share");
        shareButton.transform.FindChild("tfTextfield").GetComponent<UITextField>().ApplyParameters();
#endif

        //#if UNITY_WEBPLAYER
//        centralRoot.gameObject.SetActive(false);
//        transform.FindChild("CenterScreenAnchorWeb").gameObject.SetActive(true);
//        centralRoot = transform.FindChild("CenterScreenAnchorWeb");
//#else
        //centralRoot.FindChild("Goals/TitleTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("weekly_goals");
        //weeklyTextfield = centralRoot.FindChild("Goals/EndInTextField").GetComponent<UITextField>();
//#endif

        centralRoot.FindChild("AnimAnchor/Coins/LabelTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("coins");
//#if UNITY_WEBPLAYER
//        centralRoot.FindChild("AnimAnchor/Meters/LabelTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_distance");
//        centralRoot.FindChild("AnimAnchor/Meters/BestLabelTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_best_distance");
//#else
        UITextField metersTextfield = centralRoot.FindChild("AnimAnchor/Meters/LabelTextField").GetComponent<UITextField>();
        metersTextfield.text = OnTheRunDataLoader.Instance.GetLocaleString("your_meters"); //"meters"
        metersTextfield.wordWrap = true; 
        metersTextfield.lineSpacing = 0.7f;
        metersTextfield.ApplyParameters();
        centralRoot.FindChild("AnimAnchor/Meters/BestLabelTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("best_meters"); //"best"
//#endif
        //transform.FindChild("BottomRightAnchor/PlayButton/TextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("btPlay");

        if (gameplayManager == null)
            gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();

        //if (weeklyChallengesManager == null)
        //    weeklyChallengesManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunWeeklyChallenges>();

        if (interfaceSounds == null)
            interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        
#if !UNITY_WEBPLAYER
        /*if (metersCollectButton == null)
            metersCollectButton = centralRoot.FindChild("Goals/meters/GiftButton").gameObject;

        if (coinsCollectButton == null)
            coinsCollectButton = centralRoot.FindChild("Goals/coins/GiftButton").gameObject;*/
#endif

		if( m_pBestMetersText == null )
            m_pBestMetersText = centralRoot.FindChild("AnimAnchor/Meters/BestValueTextField").gameObject.GetComponent<UITextField>();
		
        if( m_pCurrentMetersText == null )
            m_pCurrentMetersText = centralRoot.FindChild("AnimAnchor/Meters/ValueTextField").gameObject.GetComponent<UITextField>();

        if (m_pBestMetersLabel == null)
            m_pBestMetersLabel = centralRoot.FindChild("AnimAnchor/Meters/BestLabelTextField").GetComponent<UITextField>();

        if (newRecord == null)
        {
            newRecord = centralRoot.FindChild("AnimAnchor/Meters/NewRecord").gameObject;
            newRecordInitX = newRecord.transform.localPosition.x;
            newRecord.GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("new_record");
            newRecord.GetComponent<UITextField>().ApplyParameters();
        }

		uiSharedData = m_pUIRoot.GetComponent<UISharedData>();
        hasBeenBestScore = PlayerPersistentData.Instance.GetBestMeters() == uiSharedData.InterfaceDistance && PlayerPersistentData.Instance.GetPrevBestMeters() != uiSharedData.InterfaceDistance;

        UpdateBests();
        
        int sessionMeters = uiSharedData.InterfaceDistance;
        int sessionCoins = gameplayManager.CoinsCollected;

        tierMission = OnTheRunSingleRunMissions.Instance.CurrentTierMission;
        tierMission.Reward.metersReward = sessionMeters;
        int xpGained = tierMission.Reward.quantity;

        willBeRankUp = PlayerPersistentData.Instance.WillRankUpAfterUpdate(xpGained);//sessionMeters);

        if (!alreadyEntered)
        {
            newRecord.transform.localPosition = new Vector3(-8.0f, newRecord.transform.localPosition.y, newRecord.transform.localPosition.z);

            if (hasBeenBestScore)
                m_pBestMetersText.text = m_pUIRoot.FormatTextNumber(PlayerPersistentData.Instance.GetPrevBestMeters());
            else
                m_pBestMetersText.text = m_pUIRoot.FormatTextNumber(PlayerPersistentData.Instance.GetBestMeters());

            m_pCurrentMetersText.text = "0";
            centralRoot.FindChild("AnimAnchor/Coins/ValueTextField").GetComponent<UITextField>().text = "0";
            shareButton.State = UIButton.StateType.Normal;
            FBIcon.SetActive(true);
            FBIconGhost.SetActive(false);

            canAnimationBegin = true;
#if UNITY_WEBPLAYER
            canAnimationBegin = !UIPlatformsPopup.Instance.WillShow();
#endif

            if (canAnimationBegin)
            {
                StartCoroutine(ExpRewardDelayed(tierMission.Reward.ExpForMeters, sessionCoins));
            }
        }
        else
        {
            centralRoot.FindChild("AnimAnchor/Meters/ValueTextField").GetComponent<UITextField>().text = sessionMeters.ToString();
            centralRoot.FindChild("AnimAnchor/Coins/ValueTextField").GetComponent<UITextField>().text = sessionCoins.ToString();
        }

		m_pUIRoot.ShowOffgameBG(true);
		m_pUIRoot.ShowPageBorders(true);
		m_pUIRoot.ShowCommonPageElements(true, false, true, false, false);
		m_pUIRoot.ShowPagesMarks(false);

        //UpdateWeeklyChallenges(xpGained, sessionCoins); //sessionMeters

        //if (OnTheRunSocial.Instance != null)
		//	OnTheRunSocial.Instance.ReportLeaderboardBestMeters(uiSharedData.InterfaceDistance);
        OnTheRunRateIt.Instance.OnRunFinished();
        //transform.FindChild("CenterScreenAnchor/Coins/ValueTextField").gameObject.GetComponent<UITextField>().text = sessionCoins.ToString();
		//m_pUIRoot.UIExperienceBar.GetComponent<UILevelBar>().startedAnimation = true;

#if UNITY_WEBPLAYER
        UIPlatformsPopup.Instance.ShowPopupCheck();
#endif

        if (!alreadyEntered)
        {
#if UNITY_WEBPLAYER
            SubmitScoreButton.SetActive(true);
            SubmitScoreButton.transform.FindChild("text").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_btSubmit");
#endif
        }
    }

    IEnumerator ExpRewardDelayed(int expForMeters, int sessionCoins)
    {
        yield return new WaitForSeconds(0.5f);

        Transform centralRoot = transform.FindChild("CenterScreenAnchor");
        //m_pUIRoot.GetComponent<UIExpAnimation>().StartExpAnimationFake((int)(sessionMeters), centralRoot.FindChild("AnimAnchor/Meters/ValueTextField").gameObject, 3.0f, 0.4f);
        
        if (expForMeters > 0)
            Manager<UIRoot>.Get().GetComponent<UIExpAnimation>().StartExpAnimation((int)(expForMeters), (int)(expForMeters), centralRoot.FindChild("AnimAnchor/Meters/ValueTextField").gameObject, 4.0f, 0f, true, true);
        else
            Manager<UIManager>.Get().ActivePage.SendMessage("StartNewRecordAnimation");

        if (sessionCoins > 0)
            m_pUIRoot.GetComponent<UIExpAnimation>().StartCoinsAnimation(sessionCoins, centralRoot.FindChild("AnimAnchor/Coins/ValueTextField").gameObject, 0.1f, 0.4f);
    }

    void OnPlatformsPopupClosed()
    {
        if (!canAnimationBegin)
        {
            int sessionMeters = uiSharedData.InterfaceDistance;
            int sessionCoins = gameplayManager.CoinsCollected;
            Transform centralRoot = transform.FindChild("CenterScreenAnchor");

            tierMission = OnTheRunSingleRunMissions.Instance.CurrentTierMission;
            int xpGained = tierMission.Reward.quantity;

            if (xpGained > 0)
                m_pUIRoot.GetComponent<UIExpAnimation>().StartExpAnimation(xpGained, sessionMeters, centralRoot.FindChild("AnimAnchor/Meters/ValueTextField").gameObject, 3.0f, 0.4f);  //sessionMeters
            if (sessionCoins > 0)
                m_pUIRoot.GetComponent<UIExpAnimation>().StartCoinsAnimation(sessionCoins, centralRoot.FindChild("AnimAnchor/Coins/ValueTextField").gameObject, 0.1f, 0.4f);

        }
        canAnimationBegin = true;
    }

    public void StopSwitchPanel()
    {
        UIRewardPage.firstStep = false;
        rankPanel.tapTimer = -1.0f;
    }

    void Update()
    {
#if !UNITY_WEBPLAYER
        //TimeSpan time = weeklyChallengesManager.WeeklyChallengeTimer;
        //weeklyTextfield.text = "[" + OnTheRunDataLoader.Instance.GetLocaleString("end_in") + " " + time.Days + "d " + time.Hours + "h " + time.Minutes + "m " + time.Seconds+"s]";
#endif
        //Debug.Log("rankPanel.tapTimer rankPanel.tapTimer rankPanel.tapTimer: " + rankPanel.tapTimer);

        if (rankPanel.tapTimer > 0 &&  Manager<UIManager>.Get().FrontPopup != null)
            rankPanel.tapTimer += TimeManager.Instance.MasterSource.DeltaTime;

        //Debug.Log(rankPanel.tapTimer + " " + (TimeManager.Instance.MasterSource.TotalTime - rankPanel.tapTimer)+" " + overTapTime );

        if (rankPanel.tapTimer > 0 && TimeManager.Instance.MasterSource.TotalTime - rankPanel.tapTimer > overTapTime)
            rankPanel.HideRank();

        if (waitForNoPopups)
        {
            if (Manager<UIManager>.Get().FrontPopup == null)
            {
                waitForNoPopups = false;
                rankPanel.tapTimer = TimeManager.Instance.MasterSource.TotalTime;
            }
        }

        if (rankPanel.IsAnimationPlaying() && rankPanel.scroller.inputLayer>=0)
            rankPanel.scroller.inputLayer = -1;
        else if (!rankPanel.IsAnimationPlaying() && rankPanel.scroller.inputLayer < 0)
            rankPanel.scroller.inputLayer = backupScrollerInputLayer;
    }

	//-------------------------------------------------------//
    protected int backupScrollerInputLayer;
    void Signal_OnEnter(UIPage page)
    {
        iTween.ScaleTo(m_pCurrentMetersText.gameObject, iTween.Hash("scale", new Vector3(1.0f, 1.0f, 1.0f), "easeType", "easeOutBounce", "time", 0.0f));

        backupScrollerInputLayer = rankPanel.scroller.inputLayer;

        rankPanel.SendMessage("InitRankScroller", false);

        if (alreadyEntered)
        {
            rankPanel.gameObject.SetActive(false);
            return;
        }
        //else
        //    alreadyEntered = true;

        if (m_pUIRoot == null)
            m_pUIRoot = Manager<UIRoot>.Get();

        if (uiSharedData == null)
			uiSharedData = m_pUIRoot.GetComponent<UISharedData>();
		
        if (gameplayManager == null)
            gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();

        bottomBar.GetComponent<UIRewardBar>().ActivateContinueButton(false);

        //PlayerPersistentData.Instance.RefreshRentCar();
        PlayerPersistentData.PlayerData playerData = PlayerPersistentData.Instance.GetPlayerData(gameplayManager.PlayerKinematics.carId);
        bool carRented = false; // PlayerPersistentData.Instance.IsCarRented(gameplayManager.PlayerKinematics.carId);
        bool carOwned = playerData.owned;
        /*if (!carRented && !carOwned)
        {
            PlayerGameData gameData = gameplayManager.PlayerKinematics.gameObject.GetComponent<PlayerGameData>();
            Manager<UIManager>.Get().PushPopup("SingleButtonPopup");
            Manager<UIManager>.Get().FrontPopup.gameObject.transform.FindChild("content/popupTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("rent_expired") + " " + gameData.carName.ToString();
            backToCarBase = true;
        }*/

        McSocialApiManager.Instance.PostScore(OnTheRunMcSocialApiData.Instance.GetLeaderboardId(uiSharedData.LastGarageLocationIndex), uiSharedData.InterfaceDistance, HttpMcSocialApiImplementation.GEOIP_COUNTRY);

        if (OnTheRunOmniataManager.Instance != null)
            OnTheRunOmniataManager.Instance.OnRunFinished(gameplayManager.CoinsCollected, uiSharedData.InterfaceDistance, gameplayManager.PlayerKinematics.carId.ToString().ToLowerInvariant(), gameplayManager.SlipstreamRunCount, gameplayManager.MaxRunCombo, gameplayManager.HitsRunCount, gameplayManager.InitBoosters);

        if (OnTheRunAchievements.Instance != null)
            OnTheRunAchievements.Instance.OnRunFinished();

        UpdateMissionPanel(true);

        string skipPopupID = "none";
        if (!willBeRankUp)
        {
            if (!tierMission.Done && !tierMission.JustPassed)
            {
                ++skipMissionCounter;
                if (skipMissionCounter == OnTheRunDataLoader.Instance.GetSkipPopupData("first_time", 0))
                {
                    skipPopupID = "upgrade";
                    PlayerPrefs.SetInt(skipMissionCounterID, 0);
                    OnTheRunUITransitionManager.Instance.OpenPopup("SkipMissionPopup");
                    Manager<UIManager>.Get().FrontPopup.GetComponent<UISkipMissionPopup>().SetupPopup(1);
                }
                else if (skipMissionCounter == OnTheRunDataLoader.Instance.GetSkipPopupData("shown_every", 5))
                {
                    skipPopupID = "buy";
                    OnTheRunUITransitionManager.Instance.OpenPopup("SkipMissionPopup");
                    int diamondsToSkip = PlayerPersistentData.Instance.Level * OnTheRunDataLoader.Instance.GetSkipPopupData("diamonds_to_skip_multiplier", 1);
                    Manager<UIManager>.Get().FrontPopup.GetComponent<UISkipMissionPopup>().SetupPopup(2, diamondsToSkip);
                }
            }
            else
            {
                if (PlayerPrefs.HasKey(skipMissionCounterID))
                    skipMissionCounter = 0;
                else
                    skipMissionCounter = -1;
            }
        }

        if (Manager<UIManager>.Get().FrontPopup != null)
            rankPanel.tapTimer = TimeManager.Instance.MasterSource.TotalTime;
        else
            waitForNoPopups = true;

        if (tierMission.id != oldMissionId[tierMission.Active])
        {
            sameMissionCounter[tierMission.Active] = 1;
            oldMissionId[tierMission.Active] = tierMission.id;
        }
        else
            ++sameMissionCounter[tierMission.Active];

        if (OnTheRunOmniataManager.Instance != null)
            OnTheRunOmniataManager.Instance.TrackMissionCompleted(tierMission.id, tierMission.checkCounter, (int)tierMission.Counter, tierMission.Done, sameMissionCounter[tierMission.Active], skipPopupID);
    }

	//-------------------------------------------------------//
    void Signal_OnExit(UIPage page)
    {
        if (tierMission.Done)
            OnTheRunSingleRunMissions.Instance.EvaluateMissionPassed(tierMission);

        GameObject wheelButton = bottomBar.GetComponent<UIRewardBar>().RewardWheelItem;
        if (wheelButton.GetComponent<BounceBehaviour>() != null)
            DestroyImmediate(wheelButton.GetComponent<BounceBehaviour>());
        wheelButton.transform.localPosition = new Vector3(wheelButton.transform.localPosition.x, 0.54f, wheelButton.transform.localPosition.z);

        if (rankPanel!=null)
            rankPanel.ResetRankScroller();
		m_pUIRoot.ShowOffgameBG(false);
		m_pUIRoot.ShowPageBorders(false);
		m_pUIRoot.ShowCommonPageElements(false, false, false, false, false);
    }

    protected bool showFireworks = false;
    public void StopAllAnimations( )
    {
        showFireworks = false;
        iTween.Stop(newRecord);
        iTween.Stop(m_pCurrentMetersText.gameObject);
        iTween.Stop(m_pBestMetersLabel.gameObject);
        iTween.Stop(m_pBestMetersText.gameObject);
        iTween.Stop();

        interfaceSounds.StopWheelExtraSpinSound();
        StopCoroutine(StartPartTwo(bestMetersDelay));
        StopCoroutine(UpdateBestMeterText(bestMetersDelay));
    }
	//-------------------------------------------------------//
    void UpdateMissionPanel(bool playAnimation)
    {
        tierMission = OnTheRunSingleRunMissions.Instance.CurrentTierMission;
        missionRewardPanel.UpdateRewardPanel(tierMission, playAnimation);
    }

    void UpdateMissionPanelForcePassed()
    {
        tierMission = OnTheRunSingleRunMissions.Instance.CurrentTierMission;
        missionRewardPanel.UpdateRewardPanelForcePassed(tierMission);
    }

    void GiveMissionSkippedReward()
    {
        UpdateMissionPanel(true);
    }

    void ResetRewardDiamond()
    {
        missionRewardPanel.ResetRewardDiamond();
    }

	//-------------------------------------------------------//
    void UpdateBests()
    {
        int currentScore = uiSharedData.InterfaceDistanceScore;
        int bestScore = PlayerPersistentData.Instance.GetBestScore();
        bestScore = (bestScore < currentScore) ? currentScore : bestScore;
        PlayerPersistentData.Instance.SetBestScore(bestScore);

        //PlayerPersistentData.Instance.Save();
    }
	//-------------------------------------------------------//
    public void UpdateWeeklyChallenges(int sessionMeters, int sessionCoins)
    {
        /*
#if !UNITY_WEBPLAYER
        weeklyChallengesManager.UpdateWeeklyChallenges(sessionMeters, sessionCoins);

		Transform pMeters = transform.FindChild("CenterScreenAnchor/Goals/meters");

        pMeters.FindChild("ChallengeTextField").gameObject.GetComponent<UITextField>().text = weeklyChallengesManager.WeeklyMeters.Description;
        pMeters.FindChild("ChallengeValueTextField").gameObject.GetComponent<UITextField>().text = m_pUIRoot.FormatTextNumber(weeklyChallengesManager.WeeklyMeters.currentValue) + "/";
        pMeters.FindChild("ChallengeGoalTextField").gameObject.GetComponent<UITextField>().text = m_pUIRoot.FormatTextNumber(weeklyChallengesManager.WeeklyMeters.goalValue);

		Transform pCoins = transform.FindChild("CenterScreenAnchor/Goals/coins");

        pCoins.FindChild("ChallengeTextField").gameObject.GetComponent<UITextField>().text = weeklyChallengesManager.WeeklyCoins.Description;
        pCoins.FindChild("ChallengeValueTextField").gameObject.GetComponent<UITextField>().text = m_pUIRoot.FormatTextNumber(weeklyChallengesManager.WeeklyCoins.currentValue) + "/";
        pCoins.FindChild("ChallengeGoalTextField").gameObject.GetComponent<UITextField>().text = m_pUIRoot.FormatTextNumber(weeklyChallengesManager.WeeklyCoins.goalValue);

        bool buttonEnabled = weeklyChallengesManager.WeeklyMeters.Passed && !weeklyChallengesManager.WeeklyMeters.RewardTaken;
        metersCollectButton.transform.FindChild("text").gameObject.GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("collect");
        metersCollectButton.transform.FindChild("icon").gameObject.SetActive(true);
        metersCollectButton.SetActive(buttonEnabled ? true : false);
        if (buttonEnabled)
            metersCollectButton.GetComponent<UIButton>().State = UIButton.StateType.Normal;

        buttonEnabled = weeklyChallengesManager.WeeklyCoins.Passed && !weeklyChallengesManager.WeeklyCoins.RewardTaken;
        coinsCollectButton.transform.FindChild("text").gameObject.GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("collect");
        coinsCollectButton.transform.FindChild("icon").gameObject.SetActive(true);
        coinsCollectButton.SetActive(buttonEnabled ? true : false);
        if (buttonEnabled)
            coinsCollectButton.GetComponent<UIButton>().State = UIButton.StateType.Normal;
#endif*/
    }
	//-------------------------------------------------------//
    
	//-------------------------------------------------------//
     void Signal_OnTakeRewardMetersRelease(UIButton button)
     {
         /*Manager<UIRoot>.Get().GetComponent<UIExpAnimation>().StartDiamondsAnimation(1, button.gameObject, 5.0f, 0f);
         interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
         weeklyChallengesManager.WeeklyMeters.RewardTaken = true;
         PlayerPersistentData.Instance.IncrementCurrency(PriceData.CurrencyType.SecondCurrency, weeklyChallengesManager.WeeklyMeters.diamondsReward);
         metersCollectButton.GetComponent<UIButton>().State = UIButton.StateType.Disabled;
         metersCollectButton.transform.FindChild("icon").gameObject.SetActive(false);
         //transform.FindChild("CenterScreenAnchor/Meters/GiftButton").gameObject.GetComponent<UIButton>().State = UIButton.StateType.Disabled;
		 m_pUIRoot.UpdateCurrenciesItem();*/
     }
	//-------------------------------------------------------//
     void Signal_OnTakeRewardCoinsRelease(UIButton button)
     {
         /*Manager<UIRoot>.Get().GetComponent<UIExpAnimation>().StartDiamondsAnimation(1, button.gameObject, 5.0f, 0f);
         interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
         weeklyChallengesManager.WeeklyCoins.RewardTaken = true;
         PlayerPersistentData.Instance.IncrementCurrency(PriceData.CurrencyType.SecondCurrency, weeklyChallengesManager.WeeklyCoins.diamondsReward);
         coinsCollectButton.GetComponent<UIButton>().State = UIButton.StateType.Disabled;
         coinsCollectButton.transform.FindChild("icon").gameObject.SetActive(false);
         //transform.FindChild("CenterScreenAnchor/Coins/GiftButton").gameObject.GetComponent<UIButton>().State = UIButton.StateType.Disabled;
		 m_pUIRoot.UpdateCurrenciesItem();*/
     }
	//-------------------------------------------------------//
     void Signal_OnSubmitScoreRelease(UIButton button)
     {
         //Debug.Log("*** SaveHighscore " + uiSharedData.InterfaceDistance);
#if UNITY_WEBPLAYER
         uint level = 0;
         string levelName = "";
         switch (gameplayManager.GetComponent<OnTheRunEnvironment>().currentEnvironment)
         {
             case OnTheRunEnvironment.Environments.Europe: level = 1; levelName = OnTheRunDataLoader.Instance.GetLocaleString("europeanCarsTitle"); break;
             case OnTheRunEnvironment.Environments.Asia: level = 2; levelName = OnTheRunDataLoader.Instance.GetLocaleString("orientalCarsTitle"); break;
             case OnTheRunEnvironment.Environments.NY: level = 3; levelName = OnTheRunDataLoader.Instance.GetLocaleString("americanCarsTitle"); break;
             case OnTheRunEnvironment.Environments.USA: level = 4; levelName = OnTheRunDataLoader.Instance.GetLocaleString("muscleCarsTitle"); break;
         }
         GameObject.Find("MiniclipAPI").GetComponent<MiniclipAPIManager>().SaveHighscore(uiSharedData.InterfaceDistance, level, levelName);
#endif
         SubmitScoreButton.SetActive(false);
     }
	//-------------------------------------------------------//

     void OnSpacePressed()
     {
         if (bottomBar.activeInHierarchy)
             bottomBar.GetComponent<UIRewardBar>().SendMessage("OnSpacePressed");
     }

     IEnumerator SetRankPanelVisibility( )
     {
         yield return new WaitForSeconds(0.25f);

         rankPanel.gameObject.SetActive(false);
     }


     void OnBackButtonAction()
     {
         if (Manager<UIManager>.Get().FrontPopup == null)
         {
             UIButton okButton = bottomBar.GetComponent<UIRewardBar>().playButton.GetComponent<UIButton>();
             okButton.onReleaseEvent.Invoke(okButton);
         }
     }

     void Signal_OnFBLoginRelease(UIButton button)
     {
         /*Signal_OnGooglePlusLoginRelease(button);
         return;*/

         if (OnTheRunFacebookManager.Instance.IsLoggedIn)
         {
             /*OnTheRunFacebookManager.Instance.Logout();
             McSocialApiManager.Instance.LoginWithSavedGuest();
             UpdateFacebookButtonText(); // tfLogin.text = OnTheRunDataLoader.Instance.GetLocaleString("login");*/
         }
         else
         {
             ShowLoadingPopup();

             OnTheRunFacebookManager.Instance.Login(
                 () =>
                 {
                     OnTheRunMcSocialApiData.Instance.OnFacebookPictureAvailable();
                     McSocialApiManager.Instance.LoginWithFacebook(OnTheRunFacebookManager.Instance.Token, null);
                     HideLoadingPopup();
                 },
                 () => { HideLoadingPopup(); },
                 () => { HideLoadingPopup(); });
         }
     }

     public void ShowLoadingPopup()
     {
         UIManager uiManager = Manager<UIManager>.Get();
         if (!uiManager.IsPopupInStack("LoadingPopup"))
         {
             uiManager.PushPopup("LoadingPopup");
             if (uiManager.FrontPopup != null)
                 uiManager.FrontPopup.GetComponent<UILoadingPopup>().SetText("");//OnTheRunDataLoader.Instance.GetLocaleString("loading"));
         }
     }

     public void HideLoadingPopup()
     {
         UIManager uiManager = Manager<UIManager>.Get();
         if (uiManager.IsPopupInStack("LoadingPopup"))
             uiManager.RemovePopupFromStack("LoadingPopup");
     }

     void Signal_SendFBMessage(UIButton button)
     {
         if (willBeRankUp)
             return;

         if (OnTheRunFacebookManager.Instance.IsLoggedIn)
         {
             string feedMessage = OnTheRunDataLoader.Instance.GetLocaleString("facebook_feed_distance") + uiSharedData.InterfaceDistance + " " + OnTheRunDataLoader.Instance.GetLocaleString("meters").ToLower();
             Debug.Log("SHARE ON FACEBOOK PRESSED - " + feedMessage);
             OnTheRunFacebookManager.Instance.Feed(feedMessage, success => 
                { 
                    if (success) 
					{
                        shareButton.State = UIButton.StateType.Disabled;
                        FBIcon.SetActive(false);
                        FBIconGhost.SetActive(true);
					
						OnTheRunOmniataManager.Instance.TrackFacebookShare(OmniataIds.FacebookShare_DistanceReached);
					}
                });
         }
         else
             Manager<UIRoot>.Get().ShowFBLoginSequence(SuccessCallback);
     }

     void SuccessCallback(bool success)
     {
         Manager<UIRoot>.Get().HideLoadingPopup();

         //if (success)
             //StartCoroutine(GoToNextPage("FBFriendsPopup", false));
     }

     void RefreshSpinWheelButton()
     {
         if (bottomBar.activeInHierarchy)
            bottomBar.GetComponent<UIRewardBar>().RefreshSpinWheelButton();
     }

     #region New Record animation
     private float animationTime = 0.25f;//0.35f;
     private float bestMetersDelay = 0.45f;
     private float scaleFactor = 2.0f; //1.4f;
     public void StartNewRecordAnimation()
     {
         if (hasBeenBestScore)
         {
             //newRecord.transform.localPosition = new Vector3(newRecordInitX, newRecord.transform.localPosition.y, newRecord.transform.localPosition.z);
             iTween.MoveTo(newRecord, iTween.Hash("x", newRecordInitX, "islocal", true, "easeType", "easeOutBack", "delay", (animationTime * 1.5f + bestMetersDelay), "time", animationTime)); //-1.65f

             showFireworks = true;

             StartPartOne();

             StartCoroutine(StartPartTwo(bestMetersDelay));
         }

         UIRewardPage.canSkip = true;
         missionRewardPanel.SendMessage("StartMissionRewardAnimation", tierMission);
     }

     void StartPartOne()
     {
         iTween.ScaleTo(m_pCurrentMetersText.gameObject, iTween.Hash("scale", new Vector3(scaleFactor, scaleFactor, scaleFactor), "easeType", "easeInOutCubic", "time", animationTime));
         iTween.ScaleTo(m_pCurrentMetersText.gameObject, iTween.Hash("scale", new Vector3(1.0f, 1.0f, 1.0f), "delay", animationTime, "easeType", "easeOutBounce", "time", animationTime));

         interfaceSounds.WheelExtraSpin();
     }

     IEnumerator StartPartTwo(float delayTime)
     {
         yield return new WaitForSeconds(delayTime);

         StartCoroutine(UpdateBestMeterText(0.0f));
         iTween.ScaleTo(m_pBestMetersLabel.gameObject, iTween.Hash("scale", new Vector3(scaleFactor, scaleFactor, scaleFactor), "easeType", "easeInOutCubic", "time", animationTime));
         iTween.ScaleTo(m_pBestMetersLabel.gameObject, iTween.Hash("scale", new Vector3(1.0f, 1.0f, 1.0f), "delay", animationTime, "easeType", "easeOutBounce", "time", animationTime));
         iTween.ScaleTo(m_pBestMetersText.gameObject, iTween.Hash("scale", new Vector3(scaleFactor, scaleFactor, scaleFactor), "easeType", "easeInOutCubic", "time", animationTime));
         iTween.ScaleTo(m_pBestMetersText.gameObject, iTween.Hash("scale", new Vector3(1.0f, 1.0f, 1.0f), "delay", animationTime, "easeType", "easeOutBounce", "time", animationTime));
         
     }

     IEnumerator UpdateBestMeterText(float delayTime)
     {
         yield return new WaitForSeconds(delayTime);

         m_pBestMetersText.text = m_pUIRoot.FormatTextNumber(PlayerPersistentData.Instance.GetBestMeters());

         yield return new WaitForSeconds(0.25f);

         if (showFireworks)
            OnTheRunFireworks.Instance.StartFireworksEffect(5, Manager<UIManager>.Get().ActivePage.gameObject.transform.FindChild("fireworks"));
     }

    public void AnimationsEnded()
    {
        rankPanel.SendMessage("GoToSecondStep");
    }
    #endregion
}
