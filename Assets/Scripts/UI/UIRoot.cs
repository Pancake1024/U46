using UnityEngine;
using SBS.Core;
using System.Collections;
using System;
using System.Collections.Generic;

[AddComponentMenu("OnTheRun/UI/UIRoot")]
public class UIRoot : Manager<UIRoot>
{
    public UIFlyer[] flyers;

    [NonSerialized]
    public string lastPageShown = "StartPage";
    public string lastPageVisited = "StartPage";
    public string lastPageBeforePopup = "GaragePage";
    public string lastPageBeforeFriendsPopup = "";
    
    protected OnTheRunGameplay gameplayManager;
    protected OnTheRunInterfaceSounds interfaceSounds;
    protected GameObject UICurrenciesGO;
    protected GameObject UIHUDGO;
    protected GameObject UIButtonsBar;
    protected GameObject UITopLeftObjects;
    protected GameObject UIArrows;
    protected GameObject UIMarks;

    [HideInInspector]
    public GameObject UIExperienceBar;

    public GameObject OffgameBackground;

    protected Camera uiCamera;
    protected GameObject player;
    protected PlayerKinematics playerKinematics;
    protected bool continueRace = false;
    protected bool passedAnimationShown = false;
    protected bool missionPageFromIngameFlag = false;

    public Sprite[] backgroundCenterSprites;
    public Sprite[] backgroundUpSprites;
    public Sprite[] backgroundDownSprites;
    protected Transform changingBackground;
    protected SpriteRenderer changingBackgroundCenter;
    protected SpriteRenderer changingBackgroundTop;
    protected SpriteRenderer changingBackgroundBottom;
    protected GameObject gameLight;
    protected GameObject garageLight;

    protected bool canSpinWheelButtonBounce = false;

    protected string separator = ",";


    [HideInInspector]
    public bool advanceToNextPage = false;

    public string FormatTextNumber(int num)
    {
        return StringUtils.FormatGroupSeparator(num, separator);
    }

    public bool CanSpinWheelButtonBounce
    {
        get { return canSpinWheelButtonBounce; }
        set { canSpinWheelButtonBounce = value; }
    }
    //------------------------------------//
    public GameObject GameLight
    {
        get { return gameLight; }
    }
    //------------------------------------//
    public GameObject GarageLight
    {
        get { return garageLight; }
    }
    //------------------------------------//
    public bool IsMissionAnimationPlaying
    {
        get { return passedAnimationShown; }
        set { passedAnimationShown = value; }
    }
    //------------------------------------//
    public bool IsMissionPageFromIngameFlag
    {
        get { return missionPageFromIngameFlag; }
        set { missionPageFromIngameFlag = value; }
    }
    //------------------------------------//
	public UIButton HomeButton
	{
		get
		{
			if( m_pHomeButton == null )
				m_pHomeButton = UITopLeftObjects.transform.FindChild("btHome").gameObject.GetComponent<UIButton>();

			return m_pHomeButton;
		}
	}
	UIButton m_pHomeButton = null;
	//------------------------------------//
	public UIButton FuelPlusButton
	{
		get
		{
            if (m_pFuelPlusButton == null)
                m_pFuelPlusButton = UICurrenciesGO.transform.FindChild("FuelItem").gameObject.GetComponentInChildren<UIButton>();
			
			return m_pFuelPlusButton;
		}
	}
    UIButton m_pFuelPlusButton = null;
	//------------------------------------//
	public UIButton DiamondsPlusButton
	{
		get
		{
			if( m_pDiamondsPlusButton == null )
				m_pDiamondsPlusButton = UICurrenciesGO.transform.FindChild("DiamondsItem").gameObject.GetComponentInChildren<UIButton>();
			
			return m_pDiamondsPlusButton;
		}
	}
	UIButton m_pDiamondsPlusButton = null;
	//------------------------------------//
	public UIButton CoinPlusButton
	{
		get
		{
			if( m_pCoinPlusButton == null )
				m_pCoinPlusButton = UICurrenciesGO.transform.FindChild("CoinsItem").gameObject.GetComponentInChildren<UIButton>();
			
			return m_pCoinPlusButton;
		}
	}
	UIButton m_pCoinPlusButton = null;

	//------------------------------------//
	public void DisablePlusButtons()
	{
        UICurrenciesGO.SendMessage("SetFuelVisibility", false);
        FuelPlusButton.State = UIButton.StateType.Disabled;
        FuelPlusButton.transform.FindChild("icon_more").GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        DiamondsPlusButton.State = UIButton.StateType.Disabled;
        DiamondsPlusButton.transform.FindChild("icon_more").GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        CoinPlusButton.State = UIButton.StateType.Disabled;
        CoinPlusButton.transform.FindChild("icon_more").GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
	}
	//------------------------------------//
	public void EnablePlusButtons()
    {
        UICurrenciesGO.SendMessage("SetFuelVisibility", true);
        FuelPlusButton.State = UIButton.StateType.Normal;
        FuelPlusButton.transform.FindChild("icon_more").GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        DiamondsPlusButton.State = UIButton.StateType.Normal;
        DiamondsPlusButton.transform.FindChild("icon_more").GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        CoinPlusButton.State = UIButton.StateType.Normal;
        CoinPlusButton.transform.FindChild("icon_more").GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
	}
	//------------------------------------//

    public void LaunchFlyer(string name, string text, Vector3 position, Quaternion rotation)
    {
        Vector3 gOffset = Vector3.zero;
        if (Manager<UIManager>.Get().ActivePageName.Equals("IngamePage"))
            gOffset = Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().gOffset;

        UIFlyer currentFlyer = Manager<UIManager>.Get().PlayFlyer(name, position+gOffset, rotation);

        UITextField tf = currentFlyer.gameObject.GetComponentInChildren<UITextField>();
        tf.text = text;
        tf.ApplyParameters();

        currentFlyer.onEnd.AddTarget(gameObject, "OnFlyerEnded");
    }

    public void OnFlyerEnded(UIFlyer flyer)
    {
        flyer.onEnd.RemoveTarget(gameObject);
    }

    public void LaunchTimeOutFlyer()
    {
        Vector3 gOffset = Vector3.zero;
        if (Manager<UIManager>.Get().ActivePageName.Equals("IngamePage"))
            gOffset = Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().gOffset;

        UIFlyer timeout = Manager<UIManager>.Get().PlayFlyer("SimpleFlyer", Vector3.zero + gOffset, Quaternion.identity);

        UITextField tf = timeout.gameObject.GetComponentInChildren<UITextField>();
        tf.text = OnTheRunDataLoader.Instance.GetLocaleString("timeout");
        tf.ApplyParameters();

        timeout.onEnd.AddTarget(gameObject, "OnTimeOutEnded");
    }

    void OnTimeOutEnded(UIFlyer flyer)
    {
        flyer.onEnd.RemoveTarget(gameObject);
        this.GoToRewardSequence();
    }

    public void GoToRewardSequence()
    {
        StartCoroutine("DelaySequence");
    }

    public void StopGoingRewardIfNecessary()
    {
        StopCoroutine("DelaySequence");
    }

    IEnumerator DelaySequence()
    {
        yield return new WaitForSeconds(1.2f);

        gameplayManager.MainCamera.SendMessage("OnFinalCameraEvent");
        OnTheRunSingleRunMissions.Instance.SendMessage("OnEndSession");
        Manager<UIManager>.Get().PushPopup("SaveMePopup");
    }

    void OnCheckpointEnded(UIFlyer flyer)
    {
        flyer.onEnd.RemoveTarget(gameObject);

        Vector3 gOffset = Vector3.zero;
        if (Manager<UIManager>.Get().ActivePageName.Equals("IngamePage"))
            gOffset = Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().gOffset;

        GetReferences();

        UIFlyer cpReached = Manager<UIManager>.Get().PlayFlyer("SimpleFlyer", Vector3.zero + gOffset, Quaternion.identity);

        UITextField tf = cpReached.gameObject.GetComponentInChildren<UITextField>();
        tf.text = OnTheRunDataLoader.Instance.GetLocaleString("time_extended") + " " + gameplayManager.GameplayTimeForCheckpoint;
        tf.ApplyParameters();

        OnTheRunSounds.Instance.PlayMetersOk();
    }

    public void OnCheckpointEnter()
    {
        GetReferences();

        Vector3 gOffset = Vector3.zero;
        if (Manager<UIManager>.Get().ActivePageName.Equals("IngamePage"))
            gOffset = Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().gOffset;

        Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().StartProgressBarAnimation(); 

        //if (!playerKinematics.PlayerIsDead)
        {
            //Manager<UIManager>.Get().GetPage("IngamePage").gameObject.GetComponent<UIIngamePage>().IncCheckpointDistance();
            gameObject.GetComponent<UISharedData>().IncCheckpointDistance();

            gameplayManager.gameObject.SendMessage("OnChangeVehicleSpawnData");
            gameplayManager.SendMessage("OnUpdateGameplayTime", false);
            playerKinematics.SendMessage("OnCheckpointEnter");

            UIFlyer cpReached = Manager<UIManager>.Get().PlayFlyer("SimpleFlyer", Vector3.zero + gOffset, Quaternion.identity);

            UITextField tf = cpReached.gameObject.GetComponentInChildren<UITextField>();
            tf.text = OnTheRunDataLoader.Instance.GetLocaleString("checkpoint") + " " + (gameplayManager.CheckpointCounter - 1) + " " + OnTheRunDataLoader.Instance.GetLocaleString("reached"); // "CHECKPOINT " + (gameplayManager.CheckpointCounter - 1) + " REACHED!";
            tf.ApplyParameters();

            cpReached.onEnd.AddTarget(gameObject, "OnCheckpointEnded");

            //OnTheRunSounds.Instance.PlayMetersOk();

            OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.Checkpoint);
            LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.CheckpointPassed);
            if (playerKinematics.TurboOn)
                LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.CheckpointPassedInTurbo);
            interfaceSounds.SendMessage("PlayCheckpointSound");
            interfaceSounds.SendMessage("StopEndingTimeSound");
        }
    }

    public void OnChangePlayerCar()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerKinematics = player.GetComponent<PlayerKinematics>();
    }

    new void Awake()
    {
		base.Awake();

		DontDestroyOnLoad(transform.gameObject);

        transform.parent = Manager<UIManager>.Get().transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        GetReferences();

        UIManager mng = Manager<UIManager>.Get();
        
        foreach (UIFlyer flyer in flyers)
            mng.RegisterFlyerPrefab(flyer, 5);

        UIHUDGO = transform.FindChild("IngamePage").gameObject;
        UICurrenciesGO = transform.FindChild("CurrencyBar").gameObject;
        UIButtonsBar = transform.FindChild("ButtonsBar").gameObject;
        UITopLeftObjects = transform.FindChild("TopLeftObjects").gameObject;
        UIArrows = transform.FindChild("ArrowsButtons").gameObject;
        UIMarks = transform.FindChild("PagesMarks").gameObject;
        UIExperienceBar = UITopLeftObjects.transform.FindChild("LevelBar").gameObject;

        OffgameBackground = transform.FindChild("OffgameBG/BgCentral").gameObject;

        changingBackground = transform.FindChild("OffgameBG/BgCentral/bg");
        changingBackgroundCenter = transform.FindChild("OffgameBG/BgCentral/bg/Center").GetComponent<SpriteRenderer>();
        changingBackgroundTop = transform.FindChild("OffgameBG/BgCentral/bg/Up").GetComponent<SpriteRenderer>();
        changingBackgroundBottom = transform.FindChild("OffgameBG/BgCentral/bg/Down").GetComponent<SpriteRenderer>();

        if (gameLight == null)
            gameLight = GameObject.FindGameObjectWithTag("GameLight");
        if (garageLight == null)
            garageLight = GameObject.FindGameObjectWithTag("GarageLight");

        ShowCurrenciesItem(false);
        ShowButtonsBar(false);
        ShowTopLeftObjects(false, false);
        ShowArrowsButtons(false);
        ShowPagesMarks(false);
        ShowOffgameBG(false);
        
        if (UITopLeftObjects.transform.FindChild("btHome").gameObject != null && OnTheRunBackButtonManager.Instance != null)
            OnTheRunBackButtonManager.Instance.SetHomeButton(UITopLeftObjects.transform.FindChild("btHome").GetComponent<UIHomeButton>());
    }

    void Start()
    {
        separator = OnTheRunDataLoader.Instance.GetLocaleString("num_separator");
    }

    void OnReadyEnded(UIFlyer flyer)
    {
        Vector3 gOffset = Vector3.zero;
        if (Manager<UIManager>.Get().ActivePageName.Equals("IngamePage"))
            gOffset = Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().gOffset;

        flyer.onEnd.RemoveTarget(gameObject);

        GetReferences();

        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Go);

        UIFlyer go = Manager<UIManager>.Get().PlayFlyer("SimpleFlyer", Vector3.zero + gOffset, Quaternion.identity);

        UITextField tf = go.GetComponentInChildren<UITextField>();
        tf.text = OnTheRunDataLoader.Instance.GetLocaleString("go");
        tf.ApplyParameters();

        go.onEnd.AddTarget(gameObject, "OnGoEnded");
    }

    void OnGoEnded(UIFlyer flyer)
    {
        flyer.onEnd.RemoveTarget(gameObject);
        
        GetReferences();

        player.SendMessage("RaceStarted", SendMessageOptions.DontRequireReceiver);
        gameplayManager.SendMessage("RaceStarted", SendMessageOptions.DontRequireReceiver);

        OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.Initial);
        
    }

    void GetReferences()
    {
        if (gameplayManager == null)
            gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();

        if (interfaceSounds == null)
            interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
                
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (playerKinematics == null)
            playerKinematics = player.GetComponent<PlayerKinematics>();
    }
    
    public void ShowCurrenciesItem(bool visible)
    {
        UICurrenciesGO.SetActive(visible);
        if (visible)
            UICurrenciesGO.SendMessage("OnShowBar");
        UpdateCurrenciesItem();
    }
    
    public void ShowHudPage(bool visible)
    {
        UIHUDGO.SetActive(visible);
    }
    
    public void UpdateCurrenciesItem()
    {
        GameObject coinsBar = UICurrenciesGO.transform.FindChild("CoinsItem").gameObject;
        coinsBar.transform.FindChild("ValueTextField").GetComponent<UITextField>().text = FormatTextNumber((int)PlayerPersistentData.Instance.Coins);

        GameObject diamondsBar = UICurrenciesGO.transform.FindChild("DiamondsItem").gameObject;
        diamondsBar.transform.FindChild("ValueTextField").GetComponent<UITextField>().text = FormatTextNumber(PlayerPersistentData.Instance.Diamonds);

        UICurrenciesGO.GetComponent<UICurrencyBarItem>().SetFuelValue();
    }
   
    public void ShowButtonsBar(bool visible)
    {
        UIButtonsBar.SetActive(visible);
        if (visible)
            UIButtonsBar.SendMessage("OnShowButtonsBar");
    }

    public void RefreshSpinWheelButton( )
    {
        if (UIButtonsBar.activeInHierarchy)
            UIButtonsBar.SendMessage("RefreshSpinWheelButton");
    }

    /*public void RefreshMissionButton(bool fromDailyBonusPopup = false)
    {
        if (UIButtonsBar.activeInHierarchy)
            UIButtonsBar.SendMessage("RefreshMissionButton", true);
        else if (Manager<UIManager>.Get().ActivePageName.Equals("StartPage"))
            Manager<UIManager>.Get().ActivePage.SendMessage("RefreshMissionButton", fromDailyBonusPopup);
    }*/

    public void ShowTopLeftObjects(bool homeVisible, bool barVisible)
    {
        UITopLeftObjects.transform.FindChild("btHome").gameObject.SetActive(homeVisible);
        UITopLeftObjects.transform.FindChild("LevelBar").gameObject.SetActive(barVisible);
    }

    public void MoveHomeButton(UIEnterExitAnimations.AnimationType type)
    {
        Transform homeBut = UITopLeftObjects.transform.FindChild("btHome");
        if (homeBut.gameObject.activeInHierarchy)
            homeBut.SendMessage("StartEnterExitAnimation", type);
    }

    public void OpenCurrencyPopup(UICurrencyPopup.ShopPopupTypes popupType)
    {
        this.StartCoroutine(UICurrenciesGO.GetComponent<UICurrencyBarItem>().GoToNextPage("CurrencyPopup", popupType));
    }

    public void OpenHelpPopup( )
    {
        UIManager uiManager = Manager<UIManager>.Get();

        if (TimeManager.Instance.MasterSource.IsPaused)
        {
            uiManager.PushPopup("HelpPopup");
            uiManager.FrontPopup.gameObject.transform.FindChild("content").transform.localPosition = Vector3.zero;
        }
        else
            OnTheRunUITransitionManager.Instance.OpenPopup("HelpPopup");

        uiManager.FrontPopup.gameObject.transform.FindChild("content/moveTitle").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_help_directions");
        uiManager.FrontPopup.gameObject.transform.FindChild("content/collectTitle").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_help_collectbolts");
        uiManager.FrontPopup.gameObject.transform.FindChild("content/collectText").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_help_bolttext");
        uiManager.FrontPopup.gameObject.transform.FindChild("content/slipstreamTitle").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_help_slipstream");
        uiManager.FrontPopup.gameObject.transform.FindChild("content/trucksTitle").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_help_trucks");

        uiManager.FrontPopup.gameObject.transform.FindChild("content/slipstreamDesc").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_help_slipstreamtext");
        uiManager.FrontPopup.gameObject.transform.FindChild("content/trucksDesc").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_help_truckstext");

        uiManager.FrontPopup.gameObject.transform.FindChild("content/OkButton/tfTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_help_back");
        uiManager.FrontPopup.gameObject.transform.FindChild("content/OkButton/tfTextField").GetComponent<UITextField>().ApplyParameters();

    }

    #region restart/are you sure popups
    public enum AreYouSureType
    {
        Restart = 0,
        Exit
    }

    public void ShowAreYouSurePopup(AreYouSureType type, GameObject target = null)
    {
        string popupText = "",
               okText = "",
               cancelText = "",
               callback = "";
        GameObject targetGO = gameObject;

        switch (type)
        {
            case AreYouSureType.Restart:
                popupText = OnTheRunDataLoader.Instance.GetLocaleString("web_restart_game");
                okText = OnTheRunDataLoader.Instance.GetLocaleString("yes");
                cancelText = OnTheRunDataLoader.Instance.GetLocaleString("no");
                callback = "Signal_OnRestart";
                Manager<UIManager>.Get().PushPopup("AreYouSurePopup");
                break;
            case AreYouSureType.Exit:
                popupText = OnTheRunDataLoader.Instance.GetLocaleString("quit_confirm");
                okText = OnTheRunDataLoader.Instance.GetLocaleString("yes");
                cancelText = OnTheRunDataLoader.Instance.GetLocaleString("no");
                callback = "Signal_OnExit";
                Manager<UIManager>.Get().PushPopup("AreYouSurePopup");
                break;
        }

        Transform popup = Manager<UIManager>.Get().FrontPopup.transform;
        UIButton okButton = popup.FindChild("content/OkButton").GetComponent<UIButton>();
        UIButton cancelButton = popup.FindChild("content/CancelButton").GetComponent<UIButton>();
        okButton.transform.FindChild("TextField").GetComponent<UITextField>().text = okText;
        popup.FindChild("content/CancelButton/TextField").GetComponent<UITextField>().text = cancelText;
        popup.FindChild("content/popupTextField").GetComponent<UITextField>().text = popupText;
        okButton.transform.FindChild("TextField").GetComponent<UITextField>().ApplyParameters();

        okButton.onReleaseEvent.RemoveTarget(targetGO);
        //cancelButton.onReleaseEvent.RemoveTarget(gameObject);

        okButton.onReleaseEvent.AddTarget(targetGO, callback);
        //cancelButton.onReleaseEvent.AddTarget(gameObject, "Signal_CloseAreYouSurePopup");
    }

    void Signal_OnRestart(UIButton button)
    {
        OnTheRunSingleRunMissions.Instance.SendMessage("OnEndSession");
        OnTheRunTutorialManager.Instance.StopRallenty();

        Manager<UIManager>.Get().PopPopup();
        RestartFromGame();
    }

    void Signal_OnExit(UIButton button)
    {
        OnTheRunSingleRunMissions.Instance.SendMessage("OnEndSession");
        OnTheRunTutorialManager.Instance.StopRallenty();

        Manager<UIManager>.Get().PopPopup();
        QuitFromGame();
    }
    #endregion

    #region no currency popup
    protected UICurrencyPopup.ShopPopupTypes currencyMissing;
    public void ShowWarningPopup(UICurrencyPopup.ShopPopupTypes currency)
    {
#if !(UNITY_WEBPLAYER || UNITY_WEBGL)
        //directly to the shop
        currencyMissing = currency;
        this.StartCoroutine(this.AfterPopupClosed());
        if (currency==UICurrencyPopup.ShopPopupTypes.Money)
            UICurrencyPopup.isFromNotEnoughCoins = true;
#else
        Manager<UIRoot>.Get().ShowWarningPopupReally(currency);
#endif

        /*
        UIManager uiManager = Manager<UIManager>.Get();

        if (uiManager.FrontPopup != null && Manager<UIManager>.Get().FrontPopup.name.Equals("WheelPopup"))
            uiManager.BroadcastMessage("SetBackCurrencyPopupFlag", true);
        
#if UNITY_WEBPLAYER
        OnTheRunUITransitionManager.Instance.OpenPopup("SingleButtonPopup");
        uiManager.FrontPopup.gameObject.transform.FindChild("content/popupTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_not_enough_coins");
        uiManager.FrontPopup.gameObject.transform.FindChild("content/OkButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("ok");
        
#else
        OnTheRunUITransitionManager.Instance.OpenPopup("SinglePopup");
        string popupText = "",
               cancelText = "",
               okText = "";
        switch (currency)
        {
            case UICurrencyPopup.ShopPopupTypes.Money:
                popupText = OnTheRunDataLoader.Instance.GetLocaleString("not_enough_coins");
                okText = OnTheRunDataLoader.Instance.GetLocaleString("buy_coins");
                cancelText = OnTheRunDataLoader.Instance.GetLocaleString("cancel");
                break;
            case UICurrencyPopup.ShopPopupTypes.Diamonds:
                popupText = OnTheRunDataLoader.Instance.GetLocaleString("not_enough_diamonds");
                okText = OnTheRunDataLoader.Instance.GetLocaleString("buy_diamonds");
                cancelText = OnTheRunDataLoader.Instance.GetLocaleString("cancel");
                break;
            case UICurrencyPopup.ShopPopupTypes.Fuel:
                popupText = OnTheRunDataLoader.Instance.GetLocaleString("not_enough_fuel");
                okText = OnTheRunDataLoader.Instance.GetLocaleString("buy_fuel");
                cancelText = OnTheRunDataLoader.Instance.GetLocaleString("cancel");
                break;
        }

        Transform popup = Manager<UIManager>.Get().FrontPopup.transform;
        UIButton okButton = popup.FindChild("content/OkButton").GetComponent<UIButton>();
        UIButton cancelButton = popup.FindChild("content/CancelButton").GetComponent<UIButton>();
        okButton.transform.FindChild("TextField").GetComponent<UITextField>().text = okText;
        popup.FindChild("content/CancelButton/TextField").GetComponent<UITextField>().text = cancelText;
        popup.FindChild("content/popupTextField").GetComponent<UITextField>().text = popupText;

        okButton.transform.FindChild("TextField").GetComponent<UITextField>().ApplyParameters();
        
        currencyMissing = currency;

        okButton.onReleaseEvent.AddTarget(gameObject, "Signal_OpenCurrencyPopup");
        cancelButton.onReleaseEvent.AddTarget(gameObject, "Signal_CloseWarningPopup");
#endif
         */

    }
    
    public void ShowWarningPopupReally(UICurrencyPopup.ShopPopupTypes currency)
    {
        UIManager uiManager = Manager<UIManager>.Get();

        if (uiManager.FrontPopup != null && Manager<UIManager>.Get().FrontPopup.name.Equals("WheelPopup"))
            uiManager.BroadcastMessage("SetBackCurrencyPopupFlag", true);

#if UNITY_WEBPLAYER
        OnTheRunUITransitionManager.Instance.OpenPopup("SingleButtonPopup");
        uiManager.FrontPopup.gameObject.transform.FindChild("content/popupTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_not_enough_coins");
        uiManager.FrontPopup.gameObject.transform.FindChild("content/OkButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("ok");
        
#else
        OnTheRunUITransitionManager.Instance.OpenPopup("SinglePopup");
        string popupText = "",
               cancelText = "",
               okText = "";
        switch (currency)
        {
            case UICurrencyPopup.ShopPopupTypes.Money:
                popupText = OnTheRunDataLoader.Instance.GetLocaleString("not_enough_coins");
                okText = OnTheRunDataLoader.Instance.GetLocaleString("buy_coins");
                cancelText = OnTheRunDataLoader.Instance.GetLocaleString("cancel");
                break;
            case UICurrencyPopup.ShopPopupTypes.Diamonds:
                popupText = OnTheRunDataLoader.Instance.GetLocaleString("not_enough_diamonds");
                okText = OnTheRunDataLoader.Instance.GetLocaleString("buy_diamonds");
                cancelText = OnTheRunDataLoader.Instance.GetLocaleString("cancel");
                break;
            case UICurrencyPopup.ShopPopupTypes.Fuel:
                popupText = OnTheRunDataLoader.Instance.GetLocaleString("not_enough_fuel");
                okText = OnTheRunDataLoader.Instance.GetLocaleString("buy_fuel");
                cancelText = OnTheRunDataLoader.Instance.GetLocaleString("cancel");
                break;
        }

        Transform popup = Manager<UIManager>.Get().FrontPopup.transform;
        UIButton okButton = popup.FindChild("content/OkButton").GetComponent<UIButton>();
        UIButton cancelButton = popup.FindChild("content/CancelButton").GetComponent<UIButton>();
        okButton.transform.FindChild("TextField").GetComponent<UITextField>().text = okText;
        popup.FindChild("content/CancelButton/TextField").GetComponent<UITextField>().text = cancelText;
        popup.FindChild("content/popupTextField").GetComponent<UITextField>().text = popupText;

        okButton.transform.FindChild("TextField").GetComponent<UITextField>().ApplyParameters();

        currencyMissing = currency;

        okButton.onReleaseEvent.AddTarget(gameObject, "Signal_OpenCurrencyPopup");
        cancelButton.onReleaseEvent.AddTarget(gameObject, "Signal_CloseWarningPopup");
#endif
    }

    void Signal_OpenCurrencyPopup(UIButton button)
    {
        if (currencyMissing==UICurrencyPopup.ShopPopupTypes.Money)
            UICurrencyPopup.isFromNotEnoughCoins = true;

        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        OnTheRunUITransitionManager.Instance.ClosePopup();
        button.onReleaseEvent.RemoveTarget(gameObject);

        this.StartCoroutine(this.AfterPopupClosed());
    }

    IEnumerator AfterPopupClosed()
    {
        while (UIEnterExitAnimations.activeAnimationsCounter > 0)
        {
            yield return null;
        }

        Manager<UIRoot>.Get().OpenCurrencyPopup(currencyMissing);
    }

    void Signal_CloseWarningPopup(UIButton button)
    {
        if (Manager<UIManager>.Get().IsPopupInStack("WheelPopup"))
            Manager<UIManager>.Get().BroadcastMessage("SetBackCurrencyPopupFlag", false);

        Transform popup = Manager<UIManager>.Get().FrontPopup.transform;
        UIButton okButton = popup.FindChild("content/OkButton").GetComponent<UIButton>();
        okButton.onReleaseEvent.RemoveTarget(gameObject);
    }
#endregion

    public void ActivateBackground(bool active)
    { 
        OffgameBackground.gameObject.SetActive(active);
    }

    public void ShowArrowsButtons(bool visible)
    {
        UIArrows.SetActive(visible);
    }

    public void ShowPagesMarks(bool visible, int pages = 0, int currentPage = 0)
    {
        UIMarks.SetActive(visible);
        if (visible)
            UIMarks.GetComponent<UIPageMarks>().ShowMarks(pages, currentPage);
    }

    public void ShowOffgameBG(bool visible)
    {
        transform.FindChild("OffgameBG/BgCentral/bg").gameObject.SetActive(visible);
    }

    public void ShowStartPageBG(bool visible)
    {
        transform.FindChild("OffgameBG/BGStartPage").gameObject.SetActive(visible);
    }

    public void ShowPageBorders(bool visible)
    {
        transform.FindChild("OffgameBG/BgDown").gameObject.SetActive(visible);
        transform.FindChild("OffgameBG/BGup").gameObject.SetActive(visible);
    }

    public void ShowMainLogo(bool visible)
    {
        transform.FindChild("StartPage/Logo/otr_logo_main").gameObject.SetActive(visible);
    }

    public void ShowUpperPageBorders(bool visible)
    {
        transform.FindChild("OffgameBG/BGup").gameObject.SetActive(visible);
    }

    public void ShowBottomPageBorders(bool visible)
    {
        transform.FindChild("OffgameBG/BgDown").gameObject.SetActive(visible);
    }

    public void ShowRewardBar(bool visible, bool tipsVisible=true)
    {
        transform.FindChild("RewarBottomBar").gameObject.SetActive(visible);

        int[] tipAvailable = new int[]{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        int tipRandomId = tipAvailable[UnityEngine.Random.Range(0, tipAvailable.Length)];

#if UNITY_WEBPLAYER
        tipAvailable = new int[] { 1, 2, 3, 7, 8, 9, 10, 11, 12, 13 };
        tipRandomId = tipAvailable[UnityEngine.Random.Range(0, tipAvailable.Length)];
#endif

        transform.FindChild("RewarBottomBar/BottomCenterAnchor/BestLabelTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("tips_tip" + tipRandomId);
        transform.FindChild("RewarBottomBar/BottomCenterAnchor/BestLabelTextField").gameObject.SetActive(tipsVisible);

        transform.FindChild("RewarBottomBar/BottomLeftAnchor/HallButton/text").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("garage");
        transform.FindChild("RewarBottomBar/BottomLeftAnchor/SpinWheelButton/text").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("spin");
        transform.FindChild("RewarBottomBar/BottomRightAnchor/PlayButton/TextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("btPlay");
        transform.FindChild("RewarBottomBar/BottomLeftAnchor/btRankings/tfLabel").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("ranking");

        UpdateSpinWheelButton();
        SetupWheelButtonBounce(transform.FindChild("RewarBottomBar/BottomLeftAnchor/SpinWheelButton").gameObject);
    }

    public void UpdateSpinWheelButton()
    {
        int remainingSpinds = PlayerPersistentData.Instance.ExtraSpin;
        if (remainingSpinds > 0)
        {
            transform.FindChild("RewarBottomBar/BottomLeftAnchor/SpinWheelButton/Remaining").gameObject.SetActive(true);
            transform.FindChild("RewarBottomBar/BottomLeftAnchor/SpinWheelButton/Remaining/tfRemaining").GetComponent<UITextField>().text = remainingSpinds.ToString();
        }
        else
            transform.FindChild("RewarBottomBar/BottomLeftAnchor/SpinWheelButton/Remaining").gameObject.SetActive(false);
    }

    public void ShowRewardBarLeftButtons(bool visible)
    {
        transform.FindChild("RewarBottomBar/BottomLeftAnchor").gameObject.SetActive(visible);
    }

    public void ShowCommonPageElements(bool currenciesVis, bool homeVis, bool levelBarVis, bool buttonsBarVis, bool arrowsVis)
    {
        ShowCurrenciesItem(currenciesVis);
        ShowTopLeftObjects(homeVis, levelBarVis);
        ShowButtonsBar(buttonsBarVis);
        ShowArrowsButtons(arrowsVis);
    }

    public void UpdateAvatarPicture()
    {
        UIExperienceBar.GetComponent<UILevelBar>().UpdateAvatarPicture();
    }

    public void UpdateExperienceBarImmediatly(int experienceGained)
    {
        UIExperienceBar.GetComponent<UILevelBar>().UpdateExperienceBarImmediatly(experienceGained * gameplayManager.ExperienceMultiplier);
    }

    public void UpdateExperienceBarAnimated(float delay, int experienceGained, float speed)
    {
        UIExperienceBar.GetComponent<UILevelBar>().UpdateExperienceBarAnimated(delay, experienceGained * gameplayManager.ExperienceMultiplier, speed);
    }

    public void SetExperienceBarValue(int experience)
    {
        UIExperienceBar.GetComponent<UILevelBar>().SetExperienceBarValue(experience);
    }

    public void PauseExperienceBarAnimation(bool pause)
    {
        if (pause)
        {
            transform.GetComponent<UIExpAnimation>().PauseAnimations();
            UIExperienceBar.GetComponent<UILevelBar>().PauseExperienceLevelAnimation();
        }
        else
        {
            transform.GetComponent<UIExpAnimation>().ResumeAnimations();
            UIExperienceBar.GetComponent<UILevelBar>().ResumeExperienceLevelAnimation();
        }
    }

    public void SetupWheelButtonBounce(GameObject button)
    {
        if (button.GetComponent<BounceBehaviour>() != null)
            DestroyImmediate(button.GetComponent<BounceBehaviour>());

        if (canSpinWheelButtonBounce)
            button.AddComponent<BounceBehaviour>();
    }

    public void SetupMissionsButtonBounce(GameObject button, bool activate)
    {
        if (button.GetComponent<BounceBehaviour>() != null)
            DestroyImmediate(button.GetComponent<BounceBehaviour>());

        ResetAlertBadge(button, activate);
        //GameObject alert = button.transform.FindChild("Alert").gameObject;
        //alert.SetActive(activate);

        if (activate)
            button.AddComponent<BounceBehaviour>();
    }

    public void ResetAlertBadge(GameObject button, bool activate)
    {
        GameObject alert = button.transform.FindChild("Alert").gameObject;
        alert.SetActive(activate);
    }

    #region Show Daily Bonus Popup
    public void ShowDailyBonusPopup()
    {
        if (Manager<UIManager>.Get().ActivePageName == "StartPage")
            StartCoroutine(ShowDelayedDailyBonusPopup(1.6f));
        else
            StartCoroutine(ShowDelayedDailyBonusPopup(0.0f));
    }

    public IEnumerator ShowDelayedDailyBonusPopup(float time)
    {
        yield return new WaitForSeconds(time);

        gameplayManager.checkForDailyBonusAfterPause = false;

#if !UNITY_WEBPLAYER
        if (OnTheRunDailyBonusManager.Instance.DailyPopupHasToShow)
        {
            OnTheRunDailyBonusManager.Instance.DailyPopupHasToShow = false;
            OnTheRunUITransitionManager.Instance.OpenPopup("DailyBonusPopup");
        }
        else if (OnTheRunDailyBonusManager.Instance.DailyBonusSequencePopupHasToShow)
        {
            OnTheRunDailyBonusManager.Instance.DailyBonusSequencePopupHasToShow = false;
            OnTheRunUITransitionManager.Instance.OpenPopup("DailyBonusSequencePopup");
        }

        OnTheRunDailyBonusManager.Instance.DailyPopupHasToShow = false;
        OnTheRunDailyBonusManager.Instance.DailyBonusSequencePopupHasToShow = false;
#endif
    }
    #endregion

    #region Show Special Vehicle Unlocked Popup
    protected string alreadyUnlockedBaseId = "kbsunlck_bsid";
    public bool CheckForSpecialVehicleUnlockedPopup()
    {
        TruckBehaviour.TrasformType justUnlocked = SpecialVehicleJustUnlockedByXPPopup();
        bool canShowPopup = justUnlocked != TruckBehaviour.TrasformType.None && EncryptedPlayerPrefs.GetInt(alreadyUnlockedBaseId + "_" + justUnlocked.ToString(), 0) == 0;
        return canShowPopup;
    }

    public TruckBehaviour.TrasformType SpecialVehicleJustUnlockedByXPPopup()
    {
        TruckBehaviour.TrasformType carType = UnlockingManager.Instance.GetUnlockedVehicleByXP();
        return carType;
    }

    public void ShowSpecialVehicleUnlockedPopup()
    {
        TruckBehaviour.TrasformType carType = UnlockingManager.Instance.GetUnlockedVehicleByXP();
        EncryptedPlayerPrefs.SetInt(alreadyUnlockedBaseId + "_" + carType.ToString(), 1);
        if (carType != TruckBehaviour.TrasformType.None)
        {
            UnlockingManager.SpecialCarData currSpecialCarData = UnlockingManager.Instance.GetSpecialCarData(carType);

            float carCost = 0.0f;
            PlayerPersistentData.Instance.BuyItem(PriceData.CurrencyType.SecondCurrency, carCost, true, OnTheRunDataLoader.Instance.GetLocaleString("popup_amazing"), OnTheRunDataLoader.Instance.GetLocaleString("popup_unlocked_special"), UIBuyFeedbackPopup.ItemBought.SpecialUnlock, OnTheRunGameplay.GetCarLocalizedStr(currSpecialCarData.type));
            UnlockingManager.Instance.UnlockSpecialCar(currSpecialCarData.type);
            OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.BUY_SPECIAL_VEHICLE);

            if (OnTheRunOmniataManager.Instance != null)
                OnTheRunOmniataManager.Instance.TrackVirtualPurchase(OmniataIds.Product_Special + "_" + carType.ToString().ToLowerInvariant(), PriceData.CurrencyType.SecondCurrency, carCost.ToString(), OmniataIds.Product_Type_Unlock);
        }
    }
    #endregion

    /*#region Tier Just Unlocked Logic
    public bool justUnlockedTier = 
    public bool CheckForTierJustUnlocked()
    {
        TruckBehaviour.TrasformType justUnlocked = SpecialVehicleJustUnlockedByXPPopup();
        bool canShowPopup = justUnlocked != TruckBehaviour.TrasformType.None && EncryptedPlayerPrefs.GetInt(alreadyUnlockedBaseId + "_" + justUnlocked.ToString(), 0) == 0;
        if (canShowPopup)
            EncryptedPlayerPrefs.SetInt(alreadyUnlockedBaseId + "_" + justUnlocked.ToString(), 1);
        return canShowPopup;
    }
    #endregion*/

    #region Setup Background
    public void SetupBackground(OnTheRunEnvironment.Environments currEnv)
    {
        Manager<UIRoot>.Get().CreateUICamera();

        float vOffset = 0.0f;
        switch (currEnv)
        {
            case OnTheRunEnvironment.Environments.Europe: changingBackgroundCenter.sprite = backgroundCenterSprites[0];
                changingBackgroundTop.sprite = backgroundUpSprites[0];
                changingBackgroundBottom.sprite = backgroundDownSprites[0];
                changingBackground.localRotation = Quaternion.Euler(0.0f, 0.0f, 7.0f);
                break;
            case OnTheRunEnvironment.Environments.NY: changingBackgroundCenter.sprite = backgroundCenterSprites[1];
                changingBackgroundTop.sprite = backgroundUpSprites[1];
                changingBackgroundBottom.sprite = backgroundDownSprites[1];
                changingBackground.localRotation = Quaternion.Euler(0.0f, 0.0f, 7.0f);
                break;
            case OnTheRunEnvironment.Environments.Asia: changingBackgroundCenter.sprite = backgroundCenterSprites[2];
                changingBackgroundTop.sprite = backgroundUpSprites[2];
                changingBackgroundBottom.sprite = backgroundDownSprites[2];
                changingBackground.localRotation = Quaternion.Euler(0.0f, 0.0f, 7.0f);
                break;
            case OnTheRunEnvironment.Environments.USA: changingBackgroundCenter.sprite = backgroundCenterSprites[3];
                changingBackgroundTop.sprite = backgroundUpSprites[3];
                changingBackgroundBottom.sprite = backgroundDownSprites[3];
                changingBackground.localRotation = Quaternion.Euler(0.0f, 0.0f, 7.0f);
                vOffset = 0.0035f;
                break;
        }
        Vector3 topPos = changingBackgroundTop.transform.localPosition;
        Vector3 bottomPos = changingBackgroundBottom.transform.localPosition;
        topPos.z = 0.0f;
        topPos.y = changingBackgroundCenter.transform.localPosition.y + changingBackgroundCenter.sprite.bounds.max.y;
        topPos.y -= vOffset;
        bottomPos.z = 0.0f;
        bottomPos.y = changingBackgroundCenter.transform.localPosition.y + changingBackgroundCenter.sprite.bounds.min.y;
        changingBackgroundTop.transform.localPosition = topPos;
        changingBackgroundBottom.transform.localPosition = bottomPos;
    }
    #endregion

    #region Camera
    public void CreateUICamera()
    {
        GameObject camGO = GameObject.Find("uiCameraGarage");
        if (camGO != null)
            uiCamera = camGO.GetComponent<Camera>();

        UIManager uiManager = Manager<UIManager>.Get();
        GameObject uiButtonsBar = GameObject.Find("UI").transform.FindChild("ButtonsBar").gameObject;

        if (null == uiCamera)
        {
            GameObject go = new GameObject("uiCameraGarage");
            //go.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.DontSave;
            go.transform.position = uiManager.transform.position;
            go.transform.rotation = uiManager.transform.rotation;

            uiCamera = /*gameObject*/go.AddComponent<Camera>();

            //uiCamera.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.DontSave;
            uiCamera.useOcclusionCulling = false;
            uiCamera.hdr = false;
            uiCamera.orthographic = true;
            uiCamera.clearFlags = CameraClearFlags.SolidColor;
            uiCamera.backgroundColor = Color.black;
            uiCamera.depth = float.MinValue;
            uiCamera.nearClipPlane = 0.0f;
            //uiCamera.farClipPlane = 1.0f;

            uiCamera.eventMask = 0;
        }

        uiCamera.cullingMask = uiButtonsBar.GetComponent<UIButtonsBar>().uiLayersGarage;
        uiCamera.orthographicSize = uiManager.baseScreenHeight * 0.5f / Manager<UIManager>.Get().pixelsPerUnit;
        uiCamera.farClipPlane = uiManager.maxDepth;
    }

    protected void DestroyGarageCamera()
    {
        if (uiCamera != null)
        {
            Camera.DestroyImmediate(uiCamera.gameObject);
            uiCamera = null;
        }
    }
    #endregion

    #region Restart/Exit Popups
    public void QuitFromGame()
    {
        Manager<UIManager>.Get().PopPopup();

        if (OnTheRunTutorialManager.Instance.TutorialActive)
        {
            OnTheRunTutorialManager.Instance.StopTutorial();
        }

        IntrusiveList<UIFlyer> flyers = Manager<UIManager>.Get().GetActiveFlyers("SimpleFlyer");
        UIFlyer node = flyers.Head;
        while (node != null)
        {
            UIFlyer tmp = node;
            node = node.next;
            tmp.onEnd.RemoveAllTargets();
            tmp.Stop();
        }

        OnTheRunGameplay gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Offgame);
        Manager<UIRoot>.Get().StopGoingRewardIfNecessary();
        Manager<UIManager>.Get().GoToPage("GaragePage");
        OnTheRunUITransitionManager.Instance.OnPageChanged("GaragePage", "InGamePage");
        GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>().SendMessage("ResetEnvironment");

        if (OnTheRunIngameHiScoreCheck.Instance)
            OnTheRunIngameHiScoreCheck.Instance.OnGameplayFinished();

        //if (OnTheRunFuelManager.Instance.Fuel <= 0 && !OnTheRunFuelManager.Instance.IsFuelFreezeActive())
        //   Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Fuel);
    }

    public void RestartFromGame()
    {
        UISharedData uiSharedData = Manager<UIRoot>.Get().GetComponent<UISharedData>();
        if (OnTheRunFuelManager.Instance.Fuel <= 0 && !OnTheRunFuelManager.Instance.IsFuelFreezeActive())
        {
            Manager<UIRoot>.Get().StopGoingRewardIfNecessary();

            if (OnTheRunTutorialManager.Instance.TutorialActive)
            {
                OnTheRunTutorialManager.Instance.StopTutorial();
            }

            QuitFromGame();
            if (!OnTheRunFuelManager.Instance.IsFuelFreezeActive() && !OnTheRunDailyBonusManager.Instance.ShowFirstTimeMissionPopup)
            {
                if (PlayerPersistentData.Instance.FirstTimeFuelFinished && OnTheRunDataLoader.Instance.GetFirstFuelGiftEnabled())
                    OnTheRunUITransitionManager.Instance.OpenPopup("FuelGiftPopup");
                else
                {
                    OnTheRunUITransitionManager.Instance.OpenPopup("FuelRefillPopup");
                    //Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Fuel);
                }
            }
        }
        else
        {
            Manager<UIRoot>.Get().StopGoingRewardIfNecessary();
            interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

            Manager<UIManager>.Get().PopPopup();

            if (OnTheRunTutorialManager.Instance.TutorialActive)
            {
                OnTheRunTutorialManager.Instance.StopTutorial();
            }

            Manager<UIManager>.Get().GetPage("GaragePage").GetComponent<UIGaragePage>().RestartSession();

            if (OnTheRunIngameHiScoreCheck.Instance)
            {
                //long scoresSpread = OnTheRunDataLoader.Instance.GetNumScoresIngameRanks() - 1; //99;
                //McSocialApiManager.Instance.GetScoresForIngame(true, true, McSocialApiUtils.ScoreType.Latest, scoresSpread, OnTheRunMcSocialApiData.Instance.GetLeaderboardId(uiSharedData.LastGarageLocationIndex));
                McSocialApiManager.Instance.GetScoresForIngame(OnTheRunMcSocialApiData.Instance.GetLeaderboardId(uiSharedData.LastGarageLocationIndex));

                OnTheRunIngameHiScoreCheck.Instance.OnGameplayRestart();
            }
        }
    }
    #endregion

    #region Mission From Ingame
    void OnMissionPageAdvance( )
    {
        UIRewardPage.alreadyEntered = false;

        UIRoot rootManager = Manager<UIRoot>.Get();
        rootManager.GetComponent<UIExpAnimation>().DisableFloatingStuff();

        if (Manager<UIManager>.Get().ActivePageName == "RewardPage")
            Manager<UIManager>.Get().ActivePage.GetComponent<UIRewardPage>().StopAllAnimations();

        OnTheRunInterfaceSounds.Instance.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

        /*if (Manager<UIRoot>.Get().IsMissionAnimationPlaying)
        {
            Manager<UIRoot>.Get().IsMissionPageFromIngameFlag = false;
            Manager<UIRoot>.Get().IsMissionAnimationPlaying = false;

            StartCoroutine(SkipMissionsPage());
        }
        else
        {*/
            if (Manager<UIRoot>.Get().UIExperienceBar.GetComponent<UILevelBar>().startedAnimation)
                Manager<UIRoot>.Get().UIExperienceBar.GetComponent<UILevelBar>().StopExperienceLevelAnimation();
            
            if (Manager<UIManager>.Get().FrontPopup == null)
                AdvanceToNextPage();
            else
                Manager<UIRoot>.Get().advanceToNextPage = true;
        //}
    }

    public IEnumerator SkipMissionsPage()
    {
        OnTheRunUITransitionManager.Instance.OnPageExiting("MissionsPage", "RewardPage");

        while (UIEnterExitAnimations.activeAnimationsCounter > 0)
        {
            yield return null;
        }
        //playButton.SetActive(true);

        Manager<UIManager>.Get().GoToPage("RewardPage");

        OnTheRunUITransitionManager.Instance.OnPageChanged("RewardPage", "MissionsPage");
    }

    void AdvanceToNextPage()
    {
        if (CheckForSpecialVehicleUnlockedPopup())
            this.StartCoroutine(this.GoToSpecialVehiclesPage());
        else
            this.StartCoroutine(this.GoToGarage());
    }

    IEnumerator GoToSpecialVehiclesPage()
    {
        OnTheRunFBLoginPopupManager.Instance.justEnteredInGarage = true;
        Manager<UIRoot>.Get().lastPageShown = "GaragePage";
        OnTheRunUITransitionManager.Instance.OnPageExiting("RewardPage", "TrucksPage");

        while (UIEnterExitAnimations.activeAnimationsCounter > 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(OnTheRunUITransitionManager.changePageDelay);

        Manager<UIManager>.Get().GoToPage("TrucksPage");
        OnTheRunUITransitionManager.Instance.OnPageChanged("TrucksPage", "RewardPage");
    }

    IEnumerator GoToGarage()
    {
        OnTheRunFBLoginPopupManager.Instance.justEnteredInGarage = true;

        OnTheRunUITransitionManager.Instance.OnPageExiting("RewardPage", "GaragePage");

        while (UIEnterExitAnimations.activeAnimationsCounter > 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(OnTheRunUITransitionManager.changePageDelay);

        Manager<UIManager>.Get().GoToPage("GaragePage");
        OnTheRunUITransitionManager.Instance.OnPageChanged("GaragePage", "RewardPage");
    }

    IEnumerator GoToMissionPage()
    {
        OnTheRunFBLoginPopupManager.Instance.justEnteredInGarage = true;

        OnTheRunUITransitionManager.Instance.OnPageExiting("RewardPage", "MissionsPage");

        while (UIEnterExitAnimations.activeAnimationsCounter > 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(OnTheRunUITransitionManager.changePageDelay);

        Manager<UIManager>.Get().GoToPage("MissionsPage");
        OnTheRunUITransitionManager.Instance.OnPageChanged("MissionsPage", "RewardPage");
    }
    #endregion

    #region FB Login for Share
    protected UIButton loginButton;
    protected Action<bool> successCallbackBackup;
    public void ShowFBLoginSequence(Action<bool> successCallback)
    {
        successCallbackBackup = successCallback;
        OnTheRunUITransitionManager.Instance.OpenPopup("FBLoginRequest");
    }

    public void ShowFBLogin(UIButton button)
    {
        ShowLoadingPopup();

        OnTheRunFacebookManager.Instance.Login(
            () =>
            {
                OnTheRunMcSocialApiData.Instance.OnFacebookPictureAvailable();
                McSocialApiManager.Instance.LoginWithFacebook(OnTheRunFacebookManager.Instance.Token, successCallbackBackup);
            },
            () => { HideLoadingPopup(); },
            () => { HideLoadingPopup(); });
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
    #endregion

}