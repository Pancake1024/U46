using UnityEngine;
using SBS.Core;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("OnTheRun/UI/UIStartPage")]
public class UIStartPage : MonoBehaviour
{
    public GameObject FreeCoinsItem;
    public GameObject SpinItem;
    public GameObject NewsItem;
    public GameObject FacebookItem;
    public GameObject MoreGamesItem;

    public GameObject MoreGamesItemWeb;
    public GameObject HighScoresItemWeb;
    public GameObject HelpItemWeb;

    public Vector3 moreGamesNormal;
    public Vector3 moreGamesWeb;

    public GameObject topBar;
    public GameObject miniclipLogo;
    public GameObject mainLogo;

    public GameObject FBFriendsButton;

    public UIScroller tierScroller;
    public UIPageIndicators scrollerDots;

    //public GameObject gameCenterButton;

    bool firstTime = true;
    OnTheRunInterfaceSounds interfaceSounds;
    OnTheRunGameplay gameplayManager;
    OnTheRunEnvironment environmentManager;
    private UIManager uiManager;

    private UITextField tfBottomBarFreeCoins;
    private UITextField tfBottomBarNews;
    private UITextField tfBottomBarOptions;
    private UITextField tfBottomBarRanking;
    private UITextField tfBottomBarSpinWheel;
    private UITextField tfLogin;
    private UITextField tfMoreGames;
    
    protected GameObject newsBadge;
    protected SpriteRenderer newsIconRenderer;
    protected UITextField newsIconText;
    protected UITextField newsIconStroke;

    protected int tiersCount = 4;

    void OnEnable()
    {
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        environmentManager = gameplayManager.GetComponent<OnTheRunEnvironment>();
        uiManager = Manager<UIManager>.Get();

        if (newsBadge == null)
        {
            newsBadge = NewsItem.transform.FindChild("Notification").gameObject;
            newsIconRenderer = NewsItem.transform.FindChild("option_med").GetComponent<SpriteRenderer>();
            newsIconText = NewsItem.transform.FindChild("tfOptionsLabel").GetComponent<UITextField>();
            newsIconStroke = NewsItem.transform.FindChild("tfOptionsLabel/stroke").GetComponent<UITextField>();
        }

        UpdateNewsfeedButton();
        Manager<UIRoot>.Get().ShowStartPageBG(false);
    }
    
    void Start()
    {
        InitTierScroller();
        if (firstTime)
        {
            //SBS.Miniclip.UtilsBindings.ConsoleLog("DEBUG LOG -- UIStartPage Start Enter");
            tfBottomBarFreeCoins = transform.Find("AnchorBottomLeft/btFreeCoins/tfOptionsLabel").GetComponent<UITextField>();
            tfBottomBarFreeCoins.text = OnTheRunDataLoader.Instance.GetLocaleString("btFreeCoins");
            tfBottomBarNews = NewsItem.transform.Find("tfOptionsLabel").GetComponent<UITextField>();
            tfBottomBarNews.text = OnTheRunDataLoader.Instance.GetLocaleString("news");
            tfBottomBarOptions = transform.Find("AnchorBottomLeft/btOptions/tfOptionsLabel").GetComponent<UITextField>();
            tfBottomBarOptions.text = OnTheRunDataLoader.Instance.GetLocaleString("options");
            //tfBottomBarRanking = transform.Find("AnchorBottomLeft/btRanking/tfOptionsLabel").GetComponent<UITextField>();
            //tfBottomBarRanking.text = OnTheRunDataLoader.Instance.GetLocaleString("ranking");
            tfBottomBarSpinWheel = transform.Find("AnchorBottomLeft/btSpinWheel/tfOptionsLabel").GetComponent<UITextField>();
            tfBottomBarSpinWheel.text = OnTheRunDataLoader.Instance.GetLocaleString("spin");

            tfLogin = transform.Find("AnchorBottomLeft/btFBLogin/tfOptionsLabel").GetComponent<UITextField>();//transform.Find("AnchorTopLeft/btFBLogin/tfOptionsLabel").GetComponent<UITextField>();
            //tfLogin.text = OnTheRunDataLoader.Instance.GetLocaleString("login");
            UpdateFacebookButtonText();

            tfMoreGames = transform.Find("AnchorBottomRight/btMoreGames/tfOptionsLabel").GetComponent<UITextField>();
            tfMoreGames.text = OnTheRunDataLoader.Instance.GetLocaleString("moregames");
            FBFriendsButton.transform.FindChild("tfOptionsLabel").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("friends");
            topBar.SetActive(false);
            miniclipLogo.SetActive(false);

#if UNITY_WEBPLAYER
            //topBar.SetActive(true);
            miniclipLogo.SetActive(true);
            mainLogo.GetComponent<UIEnterExitAnimations>().animations[0].endPoint = 1.35f;
            mainLogo.transform.localScale = Vector3.one * 0.85f;
            MoreGamesItemWeb.transform.Find("tfOptionsLabel").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("moregames");
            HighScoresItemWeb.transform.Find("tfOptionsLabel").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_highscore");
            HelpItemWeb.transform.Find("tfOptionsLabel").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_help");
            transform.Find("AnchorBottomLeft/btOptions").transform.localPosition = FBFriendsButton.transform.localPosition;
            FBFriendsButton.SetActive(false);
#endif

            firstTime = false;
            //AnimationReset();
            StartCoroutine(AnimationReset());

            //gameCenterButton.transform.FindChild("tfOptionsLabel").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("game_center");

//#if !UNITY_IPHONE && !(UNITY_ANDROID && !UNITY_KINDLE)
//           gameCenterButton.SetActive(false);
//#endif
        }
    }

    void Update()
    {

//#if UNITY_EDITOR
//        if(Input.GetKeyDown(KeyCode.Keypad0))
//        {
//            transform.FindChild("AnchorBottomLeft/btNews/Notification").gameObject.SetActive(true);
//            transform.FindChild("AnchorBottomLeft/btNews/Notification/tfNotification").GetComponent<UITextField>().text ="2";
//        }
//#endif
        GameObject playerRef = GameObject.FindGameObjectWithTag("Player");
        if (playerRef!=null)
            playerRef.transform.FindChild("PlayerCar").gameObject.SetActive(false);
		//SBS.Miniclip.UtilsBindings.ConsoleLog("StarPageUpdate");
    }

    void OnShow(UIPage page)
    {
        UpdateTierScroller();

#if !UNITY_WEBPLAYER
        if (OnTheRunFacebookManager.Instance.IsInitInProgress)
            ShowLoadingPopup();
        else
            Manager<UIRoot>.Get().ShowDailyBonusPopup();
#endif
		//SBS.Miniclip.UtilsBindings.ConsoleLog("***** ONSHOW 1 *****");

        if (Application.internetReachability != NetworkReachability.NotReachable)
            OnTheRunAchievements.Instance.CheckForAchievementsToSend();

        //transform.FindChild("AnchorBottomRight/PlayButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("btPlay");
        Manager<UIRoot>.Get().ShowPageBorders(false);
		//SBS.Miniclip.UtilsBindings.ConsoleLog("***** ONSHOW 2 *****");
        UpdateFacebookButtonText();

        uiManager.disableInputs = true;
        //this.StartCoroutine(this.DailyBonusDelayed());
		//SBS.Miniclip.UtilsBindings.ConsoleLog("***** ONSHOW 3 *****");
        MoreGamesItemWeb.SetActive(false);
        HighScoresItemWeb.SetActive(false);
        HelpItemWeb.SetActive(false);

        RefreshSpinWheelButton();
        //RefreshMissionButton();
		//SBS.Miniclip.UtilsBindings.ConsoleLog("***** ONSHOW 4 *****");

#if UNITY_WEBPLAYER
        MoreGamesItemWeb.SetActive(true);
        HighScoresItemWeb.SetActive(true);
        HelpItemWeb.SetActive(true);
        MoreGamesItem.SetActive(false);
        FacebookItem.SetActive(false);
        FBFriendsButton.SetActive(false);
        SpinItem.SetActive(false);
        FreeCoinsItem.SetActive(false);
        NewsItem.SetActive(false);
        mainLogo.GetComponent<UIEnterExitAnimations>().animations[0].endPoint = 1.35f;
        mainLogo.transform.localScale = Vector3.one * 0.85f;
#elif UNITY_ANDROID || UNITY_WP8
        //FacebookItem.transform.position = NewsItem.transform.position;
        //FreeCoinsItem.transform.position = NewsItem.transform.position;
        NewsItem.SetActive(false);
/*#elif UNITY_WP8 && !UNITY_EDITOR
        FacebookItem.transform.position = NewsItem.transform.position;
        NewsItem.SetActive(false);
*/
#endif
		//SBS.Miniclip.UtilsBindings.ConsoleLog("***** ONSHOW 5 *****");

        RefreshVideoButton();
    }

    void RefreshVideoButton()
	{
		FreeCoinsItem.GetComponent<UIButton>().State = (OnTheRunCoinsService.Instance.IsVideoAdAvailable()) ? UIButton.StateType.Normal : UIButton.StateType.Disabled;
	}

    void RefreshSpinWheelButton( )
    {
        UpdateSpinWheelButton();
        Manager<UIRoot>.Get().SetupWheelButtonBounce(transform.FindChild("AnchorBottomLeft/btSpinWheel").gameObject);
    }
    
    void Signal_OnInviteFBFriendsRelease(UIButton button)
    {
        if (OnTheRunFacebookManager.Instance.IsLoggedIn)
            StartCoroutine(GoToNextPage("FBFriendsPopup", false));
        else
            Manager<UIRoot>.Get().ShowFBLoginSequence(SuccessCallback);
    }

    void SuccessCallback(bool success)
    {
        Manager<UIRoot>.Get().HideLoadingPopup();

        if (success)
            StartCoroutine(GoToNextPage("FBFriendsPopup", false));
    }

    void Signal_OnSpinWheelRelease(UIButton button)
    {
        StartCoroutine(GoToNextPage("WheelPopup", false));
    }

    void Signal_OnHighscoresRelease(UIButton button)
    {
        StartCoroutine(GoToNextPage("RankingsPage", false));
    }

    protected string firstTimeGameStartID = "ftgs_id_sp";
    IEnumerator Signal_OnPlayRelease(UIButton button)
    {
        if (PlayerPersistentData.Instance.FirstTimePlaying)
        {
            EncryptedPlayerPrefs.SetInt("firstTimePlayAlphaVer", 0);
            OnTheRunUITransitionManager.Instance.OnPageExiting("StartPage", "InGamePageDirectly");
            //OnTheRunUITransitionManager.Instance.OnPageExiting("GaragePage", "InGamePageDirectly");

            //long scoresSpread = OnTheRunDataLoader.Instance.GetNumScoresIngameRanks() - 1; //99;
            //McSocialApiManager.Instance.GetScoresForIngame(true, true, McSocialApiUtils.ScoreType.Latest, scoresSpread, OnTheRunMcSocialApiData.Instance.GetLeaderboardId(0));
            McSocialApiManager.Instance.GetScoresForIngame(OnTheRunMcSocialApiData.Instance.GetLeaderboardId(0));

            yield return new WaitForSeconds(OnTheRunUITransitionManager.changePageDelay);

            gameplayManager.SendMessage("ReactivatePlayer");

            gameplayManager.SendMessage("LoadNewLevel", false);

            //OnTheRunDailyBonusManager.Instance.ShowFirstTimeMissionPopup = true;
            if (EncryptedPlayerPrefs.GetInt(firstTimeGameStartID, 1) == 1)
            {
                OnTheRunSingleRunMissions.Instance.UpdateActiveMissions(true);
                EncryptedPlayerPrefs.SetInt(firstTimeGameStartID, 0);
            }

        }
        else
        {
            bool firstTimeWithNewsystem = EncryptedPlayerPrefs.GetInt("firstTimePlayAlphaVer", 1) == 1;

            Manager<UIRoot>.Get().SetupBackground(OnTheRunEnvironment.Environments.Europe);
            StartCoroutine(GoToNextPage("GaragePage", true));

            EncryptedPlayerPrefs.SetInt("firstTimePlayAlphaVer", 0);
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
                () => {
                    OnTheRunMcSocialApiData.Instance.OnFacebookPictureAvailable();
                    McSocialApiManager.Instance.LoginWithFacebook(OnTheRunFacebookManager.Instance.Token, null);
                    UpdateFacebookButtonText();
                    HideLoadingPopup();
                },
                () => { HideLoadingPopup(); },
                () => { HideLoadingPopup(); });
        }
    }

    /*void Signal_OnGooglePlusLoginRelease(UIButton button)
    {
        if (!GooglePlusManager.Instance.IsLoggedIn)
        {
            ShowLoadingPopup();
            GooglePlusManager.Instance.Login((loginWasSuccessful) =>
            {
                UpdateFacebookButtonText();
                HideLoadingPopup();
            });
        }
    }*/


    void Signal_OnMoreGamesRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

#if UNITY_IPHONE && !UNITY_EDITOR
		SBS.Miniclip.MCUtilsBindings.ShowMoreGames();
#elif UNITY_ANDROID && !UNITY_KINDLE && !UNITY_EDITOR
        Application.OpenURL("http://www.miniclip.com");
#elif UNITY_ANDROID && UNITY_KINDLE && !UNITY_EDITOR
        string bundleId = "com.miniclip.ontherunamazon";
        Application.OpenURL("http://www.amazon.com/gp/mas/dl/android?p=" + bundleId + "&showAll=1");
#elif UNITY_WP8 && !UNITY_EDITOR
        SBS.Miniclip.WP8Bindings.OnMoreGames("http://www.miniclip.com");
#elif UNITY_WEBPLAYER
        Application.ExternalEval("window.open('http://www.miniclip.com','_blank')");
#endif
    }

	
	void Signal_OnNewsPressed(UIButton button)
	{
		button.GetComponentInChildren<UITextField>().color = button.GetComponent<UIButtonsColorModified>().onHover;
		button.GetComponentInChildren<UITextField>().ApplyParameters();
	}

    void Signal_OnNewsRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        Debug.Log("NEWS: Signal_OnNewsRelease");
        //UpdateNewsNotification(fakeNews--);

        button.GetComponentInChildren<UITextField>().color = button.GetComponent<UIButtonsColorModified>().normal;
        button.GetComponentInChildren<UITextField>().ApplyParameters();

#if UNITY_EDITOR
        OnTheRunNewsIntegration.Instance.EditorTest_ReadNews();
#elif UNITY_IPHONE || UNITY_ANDROID
		SBS.Miniclip.MCUtilsBindings.ShowBoard();
#endif
        UpdateNewsfeedButton();

        button.GetComponentInChildren<UITextField>().color = button.GetComponent<UIButtonsColorModified>().normal;
        button.GetComponentInChildren<UITextField>().ApplyParameters();
    }

    void Signal_OnFreeCoinsRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        Debug.Log("FREE COINS: Signal_OnFreeCoinsRelease");

        OnTheRunCoinsService.Instance.OnFreeCoinsSelected();

        if (OnTheRunOmniataManager.Instance != null)
            OnTheRunOmniataManager.Instance.TrackWatchVideoAds(OnTheRunCoinsService.WatchVideoPlacement.MainMenuVideoAdsPlacement);

        if (tierScroller.IsDragging)
        {
            tierScroller.IsDragging = false;
            tierScroller.easeToSnapX(tierScroller.snapX);
        }
    }

    void Signal_OnOptionsRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        //OnTheRunUITransitionManager.Instance.OpenPopup("OptionsPopup");
        StartCoroutine(GoToNextPage("OptionsPopup", false));
    }

    void Signal_OnHelpWebRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        Manager<UIRoot>.Get().OpenHelpPopup();
    }

    void Signal_OnHighScoresWebRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        OnTheRunUITransitionManager.Instance.OpenPopup("HighscoresPopup");
    }
    
    public void UpdateSpinWheelButton()
    {
        int remainingSpinds = PlayerPersistentData.Instance.ExtraSpin;
        if (remainingSpinds > 0)
        {
            transform.FindChild("AnchorBottomLeft/btSpinWheel/Remaining").gameObject.SetActive(true);
            transform.FindChild("AnchorBottomLeft/btSpinWheel/Remaining/tfRemaining").GetComponent<UITextField>().text = remainingSpinds.ToString();
        }
        else
            transform.FindChild("AnchorBottomLeft/btSpinWheel/Remaining").gameObject.SetActive(false);
    }

    public void UpdateNewsfeedButton()
    {
        int totalNews = OnTheRunNewsIntegration.Instance.TotalNews;
        int unreadNews = OnTheRunNewsIntegration.Instance.UnreadNews;

        if (totalNews > 0)
        {
            //transform.FindChild("AnchorBottomLeft/btNews").GetComponent<UIButton>().State = UIButton.StateType.Normal;

            //NewsItem.transform.FindChild("Notification").gameObject.SetActive(false);//unreadNews > 0);
            SetNewsButtonState(UIButton.StateType.Normal);
            
            NewsItem.transform.FindChild("Notification/tfNotification").gameObject.SetActive(false);
            if (unreadNews > 0)
            {
                newsBadge.gameObject.SetActive(true);
                NewsItem.transform.FindChild("Notification/tfNotification").gameObject.SetActive(true);
                NewsItem.transform.FindChild("Notification/tfNotification").GetComponent<UITextField>().text = unreadNews.ToString();
            }
            else
                newsBadge.gameObject.SetActive(false);
        }
        else
            SetNewsButtonState(UIButton.StateType.Disabled);
    }

    private void SetNewsButtonState(UIButton.StateType stateType)
    {
        Color normalColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        Color ghostColor = new Color(1.0f, 1.0f, 1.0f, 0.6f);
        Color ghostStrokeColor = new Color(0.0f, 0.0f, 0.0f, 0.55f);
        Color normalStrokeColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

        NewsItem.GetComponent<UIButton>().State = stateType;
        if (stateType == UIButton.StateType.Normal)
        {
            newsIconRenderer.color = normalColor;
            newsIconText.color = normalColor;
            newsIconStroke.color = normalStrokeColor;
        }
        else if (stateType == UIButton.StateType.Disabled)
        {
            newsBadge.SetActive(false);
            newsIconRenderer.color = ghostColor;
            newsIconText.color = ghostColor;
            newsIconStroke.color = ghostStrokeColor;
        }
        newsIconStroke.ApplyParameters();
        newsIconText.ApplyParameters();
    }

    public void UpdateFacebookButtonText(bool onlyRefreshText=false)
    {
        /*UpdateGooglePlusButtonText();
        return;*/
        //Debug.Log("***UpdateFacebookButtonText: " + OnTheRunNotificationManager.Instance.PendingNotifications);

        if (OnTheRunFacebookManager.Instance == null || tfLogin == null || OnTheRunDataLoader.Instance == null)
            return;

        if (OnTheRunFacebookManager.Instance.IsLoggedIn)
        {
            OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.FB_LOGIN);
            tfLogin.text = OnTheRunDataLoader.Instance.GetLocaleString("logged");
            tfLogin.gameObject.transform.parent.GetComponent<UIButtonsColorModified>().onHover = Color.white;
            tfLogin.gameObject.transform.parent.GetComponent<UIButtonsColorModified>().onPressed = Color.white;
            FBFriendsButton.SetActive(true);
            //if (!onlyRefreshText)
            //    FBFriendsButton.transform.position = FacebookItem.transform.position;
            FacebookItem.SetActive(false);

            int pendingInvites = OnTheRunNotificationManager.Instance.PendingNotifications;
            if (pendingInvites > 0)
            {
                FBFriendsButton.transform.FindChild("Remaining").gameObject.SetActive(true);
                FBFriendsButton.transform.FindChild("Remaining/tfRemaining").GetComponent<UITextField>().text = pendingInvites.ToString();
            }
            else
                FBFriendsButton.transform.FindChild("Remaining").gameObject.SetActive(false);
        }
        else
        {
            tfLogin.text = OnTheRunDataLoader.Instance.GetLocaleString("login");
            FacebookItem.SetActive(true);
            FBFriendsButton.SetActive(false);
            FBFriendsButton.transform.FindChild("Remaining").gameObject.SetActive(false);
        }

        FacebookItem.SetActive(false);
        FBFriendsButton.SetActive(true);
        //FBFriendsButton.transform.localPosition = new Vector3(FBFriendsButton.transform.localPosition.x, FBFriendsButton.transform.localPosition.y, FBFriendsButton.transform.localPosition.z);
    }

    /*public void UpdateGooglePlusButtonText()
    {
        if (GooglePlusManager.Instance == null || tfLogin == null || OnTheRunDataLoader.Instance == null)
            return;

        if (GooglePlusManager.Instance.IsLoggedIn)
        {
            tfLogin.text = OnTheRunDataLoader.Instance.GetLocaleString("logged");
            tfLogin.gameObject.transform.parent.GetComponent<UIButtonsColorModified>().onHover = Color.white;
            tfLogin.gameObject.transform.parent.GetComponent<UIButtonsColorModified>().onPressed = Color.white;
        }
        else
        {
            tfLogin.text = OnTheRunDataLoader.Instance.GetLocaleString("login");
        }
    }*/

    public void ShowLoadingPopup()
    {
		//SBS.Miniclip.UtilsBindings.ConsoleLog("Show Loading Popup");
        if (!uiManager.IsPopupInStack("LoadingPopup"))
        {
			//SBS.Miniclip.UtilsBindings.ConsoleLog("Show Loading Popup 1");
            uiManager.PushPopup("LoadingPopup");
            if (uiManager.FrontPopup != null)
			{
				//SBS.Miniclip.UtilsBindings.ConsoleLog("Show Loading Popup 2");
                uiManager.FrontPopup.GetComponent<UILoadingPopup>().SetText("");//OnTheRunDataLoader.Instance.GetLocaleString("loading"));
			}
        }
    }

    public void HideLoadingPopup()
    {
        if (uiManager.IsPopupInStack("LoadingPopup"))
            uiManager.RemovePopupFromStack("LoadingPopup");

        Manager<UIRoot>.Get().ShowDailyBonusPopup();
    }

    #region UI Animations
    IEnumerator AnimationReset()
    {
        //Debug.Log("################################################################################### 1 - tweens count: " + iTween.Count());

        //reset
        Manager<UIRoot>.Get().ShowRewardBar(false);
        OnTheRunUITransitionManager.Instance.OnResetStartPageNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
		//Debug.Log("################################################################################### 2 - tweens count: " + iTween.Count());

		//SBS.Miniclip.UtilsBindings.ConsoleLog("######################### RESET ENTER EXIT ANIMATIONS ***");

		uiManager.ActivePage.BroadcastMessage("ResetEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeOut);
        
		
		const int numFramesToWait = 4;

        //Debug.Log("################################################################################### 3 - tweens count: " + iTween.Count());
        for (int i = 0; i < numFramesToWait; i++)
        {
            //Debug.Log("######################################################################## 3 sub[" + i + "] - tweens count: " + iTween.Count());
            yield return new WaitForEndOfFrame();
        }

        //Debug.Log("################################################################################### 4 - tweens count: " + iTween.Count());
        //yield return new WaitForSeconds(1.0f);

        OnTheRunUITransitionManager.Instance.OnPageChanged("StartPage", "init");
        //Debug.Log("################################################################################### 5 - tweens count: " + iTween.Count());
    }

    IEnumerator GoToNextPage(string nextPage, bool fadeOutBg, string prevPage = "StartPage")
    {
        if (OnTheRunUITransitionManager.Instance.ButtonsCantWork)
            yield break;

        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        OnTheRunUITransitionManager.Instance.OnPageExiting("StartPage", nextPage);

        //yield return new WaitForSeconds(OnTheRunUITransitionManager.changePageDelay);
        while (UIEnterExitAnimations.activeAnimationsCounter>0)
        {
            yield return null;
        }

        if (nextPage == "WheelPopup" || nextPage == "OptionsPopup" || nextPage == "FBFriendsPopup") 
            uiManager.PushPopup(nextPage);
        else
            uiManager.GoToPage(nextPage);
        OnTheRunUITransitionManager.Instance.OnPageChanged(nextPage, prevPage);
        Manager<UIRoot>.Get().lastPageShown = prevPage;
    }
    #endregion

    #region First Time play
    void LevelLoaded()
    {
        Manager<UIRoot>.Get().SetupBackground(OnTheRunEnvironment.Environments.Europe);

        PlayerPersistentData.Instance.SaveFirstTimePlayed();
        this.StartCoroutine(this.StartPlayImmediatly());
    }

    IEnumerator StartPlayImmediatly()
    {
        while (UIEnterExitAnimations.activeAnimationsCounter > 0)
        {
            yield return null;
        }

        uiManager.GoToPage("IngamePage");
        gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Racing);
        gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Start);

        ActivatePlayer(gameplayManager.Cars_EU[0], true);
        gameplayManager.CreatePlayerCarByRef(gameplayManager.Cars_EU[0], gameplayManager.Cars_EU[0]);

        gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.ReadyToRace);

        Manager<UIRoot>.Get().GameLight.SetActive(true);
        Manager<UIRoot>.Get().GarageLight.SetActive(false);

        Manager<UIRoot>.Get().ShowUpperPageBorders(false);
        uiManager.PopPopup();

        //OnTheRunInterfaceSounds interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

        uiManager.GoToPage("IngamePage");
        Manager<UIRoot>.Get().ShowCurrenciesItem(false);
        OnTheRunBooster.Instance.SaveBoosters();
        gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Racing);
        OnTheRunUITransitionManager.Instance.OnPageChanged("InGamePage", "GaragePage");

        if (!OnTheRunSounds.Instance.IsInGameMusicPlaying())
            OnTheRunSounds.Instance.PlayInGameMusic();

        if (OnTheRunIngameHiScoreCheck.Instance)
            OnTheRunIngameHiScoreCheck.Instance.OnGameplayStarted();

        if (OnTheRunOmniataManager.Instance)
            OnTheRunOmniataManager.Instance.OnRunStarted();

        if (OnTheRunAchievements.Instance)
            OnTheRunAchievements.Instance.OnRunStarted();

        if (OnTheRunFyberManager.Instance)
            OnTheRunFyberManager.Instance.AbortAnyPendigVideoRequest();

        gameplayManager.StartReadyGoSequence();
    }

    void ActivatePlayer(GameObject playerRef, bool activate)
    {
        playerRef.tag = activate ? "Player" : "Untagged";
        playerRef.GetComponent<BoxCollider>().enabled = activate;
        playerRef.GetComponent<PlayerInputs>().enabled = activate;
        playerRef.GetComponent<NeverendingPlayer>().enabled = activate;
        //playerRef.GetComponent<NeverendingTutorial>().enabled = activate;
        playerRef.GetComponent<PlayerSounds>().enabled = activate;
        playerRef.GetComponent<PlayerDraft>().enabled = activate;
        GameObject shadow = playerRef.transform.FindChild("PlayerCar/veh_shadow").gameObject;
        shadow.GetComponent<PlayerShadow>().enabled = activate;
        shadow.transform.localPosition = new Vector3(shadow.transform.localPosition.x, shadow.GetComponent<PlayerShadow>().shadowVerticalOffset, shadow.transform.localPosition.z);
    }
    #endregion

    void OnSpacePressed()
    {
        StartCoroutine(Signal_OnPlayRelease(null));
    }

    void OnMiniclipLogoRelease(UIButton button)
    {
#if !UNITY_WP8
        Application.ExternalEval("window.open('http://www.miniclip.com','_blank')");
#endif
    }

    /*void RefreshMissionButton(bool fromDailyBonusPopup = false)
    {
        if (fromDailyBonusPopup)
            return;

        Manager<UIRoot>.Get().SetupMissionsButtonBounce(MissionsItem, OnTheRunDailyBonusManager.Instance.ForceActiveMissionsUpdate);

        if (OnTheRunDailyBonusManager.Instance.ForceActiveMissionsUpdate || OnTheRunSingleRunMissions.Instance.ActiveMissionsRemaining == 0)
            MissionsItem.transform.FindChild("Remaining").gameObject.SetActive(false);
        else
        {
            MissionsItem.transform.FindChild("Remaining").gameObject.SetActive(true);
            MissionsItem.transform.FindChild("Remaining/tfRemaining").GetComponent<UITextField>().text = OnTheRunSingleRunMissions.Instance.ActiveMissionsRemaining.ToString();
        }
    }*/

    public GameObject TierObjPrefab;
    protected List<UITierItem> tiers = new List<UITierItem>();
    void InitTierScroller()
    {
        tiersCount = OnTheRunDataLoader.Instance.GetTiersCount();
        tierScroller.BeginAddElements();
        for (int i = 0; i < tiersCount; i++)
        {
            var newTier = (Instantiate(TierObjPrefab) as GameObject).GetComponent<UITierItem>();
            newTier.transform.position = tierScroller.transform.TransformPoint(new Vector3((4 - 1 - i) * -tierScroller.snapSpace.x, 0.0f, 0.0f));
            newTier.Initialize(i);
            tierScroller.AddElement(newTier.transform);
            tiers.Add(newTier);
            
            newTier.transform.FindChild("PlayButton").GetComponent<UIButton>().onReleaseEvent.AddTarget(gameObject, "PlayButtonReleased");
            newTier.transform.FindChild("ClickArea").GetComponent<UIButton>().onReleaseEvent.AddTarget(gameObject, "ClickAreaReleased");
			newTier.transform.FindChild("LockedArea/btBuyTier/btBuyTierbtn").GetComponent<UIButton>().onReleaseEvent.AddTarget(gameObject, "BuyTierReleased");
        }
        tierScroller.EndAddElements();

        /*for (int i = 0; i < tiers.Count; i++)
        {
            tiers[i].transform.FindChild("PlayButton").GetComponent<UIButton>().inputLayer = 2;
        }*/

        tierScroller.snapStartXInterval = 0;
        tierScroller.snapEndXInterval = 3;

        tierScroller.snapX = tiersCount - 1;

        scrollerDots.InitIndicators(tiersCount);
        scrollerDots.SetSelected((tiersCount - 1 - tierScroller.snapX));
    }

    void UpdateTierScroller()
    {
        for (int i = 0; i < tiers.Count; i++)
        {
            tiers[i].Initialize(i);
        }
    }

    void Signal_OnSnapXChanged(UIScroller scroller)
    {
        //Debug.Log("snapX = " + tierScroller.snapX + " " + (tiersCount - 1 - tierScroller.snapX));
        if (scrollerDots != null)
            scrollerDots.SetSelected((tiersCount - 1 - tierScroller.snapX));
    }

    void ClickAreaReleased(UIButton button)
    {
        if (PlayerPersistentData.Instance.IsParkingLotLocked(button.transform.parent.GetComponent<UITierItem>().tierId))
			BuyTierReleased(button.transform.parent.GetComponent<UITierItem>().BuyButton);
        else
            StartCoroutine(PlayButtonReleased(button));
    }

    IEnumerator PlayButtonReleased(UIButton button)
    {
        int tierId = button.transform.parent.GetComponent<UITierItem>().tierId;
        environmentManager.currentEnvironment = gameplayManager.EnvironmentFromIdx(tierId);
        
        if (PlayerPersistentData.Instance.FirstTimePlaying)
        {

            if (environmentManager.currentEnvironment != OnTheRunEnvironment.Environments.Europe)
            {
                OnTheRunTutorialManager.Instance.LocationAndCarChanged = true;
                environmentManager.currentEnvironment = OnTheRunEnvironment.Environments.Europe;
            }

            EncryptedPlayerPrefs.SetInt("firstTimePlayAlphaVer", 0);
            OnTheRunUITransitionManager.Instance.OnPageExiting("StartPage", "InGamePageDirectly");

            McSocialApiManager.Instance.GetScoresForIngame(OnTheRunMcSocialApiData.Instance.GetLeaderboardId(0));

            yield return new WaitForSeconds(OnTheRunUITransitionManager.changePageDelay);

            gameplayManager.SendMessage("ReactivatePlayer");

            gameplayManager.SendMessage("LoadNewLevel", false);

            //OnTheRunDailyBonusManager.Instance.ShowFirstTimeMissionPopup = true;
            if (EncryptedPlayerPrefs.GetInt(firstTimeGameStartID, 1) == 1)
            {
                OnTheRunSingleRunMissions.Instance.UpdateActiveMissions(true);
                EncryptedPlayerPrefs.SetInt(firstTimeGameStartID, 0);
            }

        }
        else
        {
            bool firstTimeWithNewsystem = EncryptedPlayerPrefs.GetInt("firstTimePlayAlphaVer", 1) == 1;

            Manager<UIRoot>.Get().SetupBackground(environmentManager.currentEnvironment);
            StartCoroutine(GoToNextPage("GaragePage", true));

            EncryptedPlayerPrefs.SetInt("firstTimePlayAlphaVer", 0);
        }
    }

    void BuyTierReleased(UIButton button)
    {
		Debug.LogWarning(button.transform.parent.transform.parent.name);

        UITierItem tierItem = button.transform.parent.transform.parent.GetComponentInParent<UITierItem>();
        int tierId = tierItem.tierId;

		bool bLocked = PlayerPersistentData.Instance.IsParkingLotLocked(tierId);

        int parkingLotCost = (int)OnTheRunDataLoader.Instance.GetTiersBuyFor()[tierId];
        PriceData.CurrencyType parkingLotCurrency = OnTheRunDataLoader.Instance.GetTiersCurrencies()[tierId]; //PriceData.CurrencyType.SecondCurrency; //dani
        bool canAfford = PlayerPersistentData.Instance.CanAfford(parkingLotCurrency, parkingLotCost);


        if (canAfford)
        {
			button.gameObject.SetActive( false );
			button.transform.parent.gameObject.SetActive( false );

            if( bLocked )
				StartCoroutine( UnlockTierTask(tierId,parkingLotCurrency ,parkingLotCost,tierItem) );
			else
				UnlockTierInternal(tierId,parkingLotCurrency ,parkingLotCost,tierItem);
        }
        else
        {
            if (parkingLotCurrency == PriceData.CurrencyType.FirstCurrency)
                Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Money);
            else
                Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Diamonds);
        }
    }

	void UnlockTierInternal( int tierId , PriceData.CurrencyType parkingLotCurrency , int parkingLotCost , UITierItem tierItem )
	{
		string tierName = OnTheRunDataLoader.Instance.GetLocaleString(tierItem.GetTitleCodeFromId(tierId));
		PlayerPersistentData.Instance.BuyItem(parkingLotCurrency, parkingLotCost, true, OnTheRunDataLoader.Instance.GetLocaleString("popup_amazing"), OnTheRunDataLoader.Instance.GetLocaleString("popup_unlocked_tier"), UIBuyFeedbackPopup.ItemBought.ParkingLot, tierName);
		
		if (OnTheRunOmniataManager.Instance != null)
			OnTheRunOmniataManager.Instance.TrackVirtualPurchase(OmniataIds.Product_ParkingLot[tierId]/*OmniataIds.Product_ParkingLot + "_" + garageTitles[currentSetIndex]*/, PriceData.CurrencyType.SecondCurrency, parkingLotCost.ToString(), OmniataIds.Product_Type_Standard);
		
		PlayerPersistentData.Instance.UnlockParkingLot(tierId);
		
		tierItem.Initialize(tierId);
		Manager<UIRoot>.Get().UpdateCurrenciesItem();
	}

	IEnumerator UnlockTierTask( int tierId , PriceData.CurrencyType parkingLotCurrency , int parkingLotCost , UITierItem tierItem )
	{
		tierItem.LockAnimationUnlock.SetActive( false );
		tierItem.LockAnimationUnlock.SetActive( true );
		
		Animation pAnim = tierItem.LockAnimationUnlock.GetComponent<Animation>();
		pAnim[pAnim.clip.name].normalizedTime = 0f;
		pAnim.Play();
		
		yield return new WaitForEndOfFrame();
		
		// hide other anim
		tierItem.LockAnimationBump.SetActive( false );
		
		yield return new WaitForSeconds( 1.5f );

		UnlockTierInternal(tierId,parkingLotCurrency ,parkingLotCost,tierItem);
	}
/*#if SBS_DEBUG
    void OnGUI()
    {
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * 2.0f);

        GUILayout.BeginVertical(GUILayout.Width(240.0f));
        GUILayout.BeginHorizontal(GUILayout.Height(16.0f));
        GUILayout.Label("spring:\t" + tierScroller.mSpringK.ToString("0.00"));
        tierScroller.mSpringK = GUILayout.HorizontalSlider(tierScroller.mSpringK, 250.0f, 350.0f, GUILayout.Width(120.0f));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(GUILayout.Height(16.0f));
        GUILayout.Label("bounce:\t" + tierScroller.mSpringBounce.ToString("0.00"));
        tierScroller.mSpringBounce = GUILayout.HorizontalSlider(tierScroller.mSpringBounce, 40.0f, 160.0f, GUILayout.Width(120.0f));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(GUILayout.Height(16.0f));
        GUILayout.Label("rebound:\t" + tierScroller.mSpringRebound.ToString("0.00"));
        tierScroller.mSpringRebound = GUILayout.HorizontalSlider(tierScroller.mSpringRebound, 40.0f, 160.0f, GUILayout.Width(120.0f));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(GUILayout.Height(16.0f));
        GUILayout.Label("damping:\t" + tierScroller.Damping.ToString("0.00"));
        tierScroller.mDamping = tierScroller.Damping = GUILayout.HorizontalSlider(tierScroller.Damping, 0.0f, 1.0f, GUILayout.Width(120.0f));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(GUILayout.Height(16.0f));
        GUILayout.Label("d time:\t" + tierScroller.dampTime.ToString("0.00"));
        tierScroller.dampTime = GUILayout.HorizontalSlider(tierScroller.dampTime, 0.0f, 4.0f, GUILayout.Width(120.0f));
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUI.matrix = Matrix4x4.identity;
    }
#endif*/
}
