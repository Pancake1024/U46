using UnityEngine;
using SBS.Core;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Globalization;

[AddComponentMenu("OnTheRun/UI/UIGaragePage")]
public class UIGaragePage : MonoBehaviour
{
    public static OnTheRunGameplay.CarId lastWonCarId = OnTheRunGameplay.CarId.None;
    // the cars scrollers
    public UIScroller Cars_EU_Scroller;
    public UIScroller Cars_NY_Scroller;
    public UIScroller Cars_JP_Scroller;
    public UIScroller Cars_USA_Scroller;

    // cars lists
    public GameObject[] Cars_EU;
    public GameObject[] Cars_NY;
    public GameObject[] Cars_JP;
    public GameObject[] Cars_USA;

    public UINewMissionsPanel missionPanel;
    public GameObject[] backgroundCar;

    public UITextField buyCarText;
    public UITextField unlockCarText;
    /*public Sprite[] backgroundCenterSprites;
    public Sprite[] backgroundUpSprites;
    public Sprite[] backgroundDownSprites;*/

    protected Transform changingBackground;
    protected SpriteRenderer changingBackgroundCenter;
    protected SpriteRenderer changingBackgroundTop;
    protected SpriteRenderer changingBackgroundBottom;
    protected float[] carsRotationY = { 58.0f, 128.0f, 197.49f, 266.0f, 342.00f };
    protected Vector3 lastAsianCarOffset = new Vector3(6.54f, -1.0f, -0.99f);
    protected OnTheRunGameplay gameplayManager;
    protected OnTheRunInterfaceSounds interfaceSounds;
    protected OnTheRunEnvironment environmentManager;
    protected GameObject cameraManager;
    protected GameObject[] currentCarsList;
    private UIScroller currentScroller = null;
    protected int currentSetIndex = -1;
    protected GameObject enteringCarGO;
    protected GameObject idleCarGO;
    protected GameObject exitingCarGO;
    protected int movementDirection = 1;
    protected int maxCarSetsNumber = 4;

    protected Vector3 backupSliderPosition = default(Vector3);
    protected int backupCurrentSetIndex = -1;
    protected GameObject[] backupCurrentCarsList;
    protected int backupCategorySelectedCar;

    protected int[] categorySelectedCar = { 0, 0, 0, 0, 0 };
    protected GameObject selectedCar;
    protected GameObject baseParkingLotCar;

    protected Transform carStatsGroup;
    protected GameObject parkingLotLocker;
    protected GameObject parkingLotLockerWeb;
    protected GameObject parkingLotLockerXP;
    protected GameObject parkingLotLockedButton;
    protected GameObject parkingLotBuyForButton;
    protected GameObject currentBackgroundCar;

    protected bool[] comingSoonActive = { false, false, false, false };
    protected GameObject comingSoonPanel;
    public GameObject[] comingSoonObjects;

    protected float parkingAreaWidth;
    protected float carSpeed;
    protected string[] garageTitles;

    protected GameObject uiButtonsBar;
    protected UIButton uiPlayButton;
    protected GameObject playArrowEnable;
    protected GameObject playArrowDisable;
    //protected GameObject uiLocks;
    //protected UIToggleButton uiToggle;
    protected float backupFOV;
    UITextField tfGarageTitle;
    //UITextField lockedPanelText;
    GameObject lockedPanel;

    protected GameObject carRentTimer;

    protected int allUpgradesCost = 0;
    protected int allCarsCost = 0;

    Transform[] infoBars;
    protected int infoBarNum = 4;

    protected bool showParkingLotUnlockedPopup = false;
    protected bool showCarUnlockedPopup = false;
    protected bool canShowReachRankAdvise = false;
    protected int nextTiersIndex = 0;

    protected int garageEnteredTimes = 0;

    protected int rotateToNextCar = 0;
    protected bool currentCarScaling = false;
    protected float currentCarScaleFactor = 1.6f;
    protected Vector3 scaledCarPosition = new Vector3(0.0f, -1.0f, -6.0f);
    protected Vector3 backupCarPosition;
    protected Quaternion backupCarRotation;
    protected bool wasLockedPanelVisible = false;

    protected GameObject gameLight;
    protected GameObject garageLight;

    private static readonly Vector3 GARAGE_CAM_POS = new Vector3(0.076f, 7.7f, -36.13f);
    private static readonly Quaternion GARAGE_CAM_ROT = Quaternion.Euler(new Vector3(32.63f, 0f, 0f));

    protected OnTheRunEnemiesManager enemiesManager;
    private UIManager uiManager;
    private UIRoot uiRoot;

    protected float invisibleButtonsTimer = -1.0f;

    UISharedData uiSharedData;

    protected GameObject purchaseCar;
    protected GameObject purchaseCarLocked;
    protected GameObject purchaseCarUnlocked;

    #region Properties
    public int CurrentSetIndex
    {
        get { return currentSetIndex; }
    }

    public bool ShowParkingLotPopup
    {
        get { return showParkingLotUnlockedPopup; }
        set { showParkingLotUnlockedPopup = value; }
    }

    public bool ShowUnlockedCarPopup
    {
        get { return showCarUnlockedPopup; }
        set { showCarUnlockedPopup = value; }
    }
    
    public bool CanShowAdvicePopup
    {
        get { return canShowReachRankAdvise; }
    }
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        backupFOV = 70.0f;
        GameObject mainCameraGo = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCameraGo != null && mainCameraGo.GetComponent<Camera>() != null)
            backupFOV = mainCameraGo.GetComponent<FollowCharacter>().CurrentFov;//mainCameraGo.camera.fieldOfView;
    }

    void InitializePage()
    {
        playerLevelToUnlockTiers = OnTheRunDataLoader.Instance.GetTiersPlayerLevelThreshold();
        tiersBuyFor = OnTheRunDataLoader.Instance.GetTiersBuyFor();

        uiManager = Manager<UIManager>.Get();
        uiRoot = Manager<UIRoot>.Get();
        enemiesManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunEnemiesManager>();
        changingBackground = Manager<UIRoot>.Get().transform.FindChild("OffgameBG/BgCentral/bg");
        changingBackgroundCenter = Manager<UIRoot>.Get().transform.FindChild("OffgameBG/BgCentral/bg/Center").GetComponent<SpriteRenderer>();
        changingBackgroundTop = Manager<UIRoot>.Get().transform.FindChild("OffgameBG/BgCentral/bg/Up").GetComponent<SpriteRenderer>();
        changingBackgroundBottom = Manager<UIRoot>.Get().transform.FindChild("OffgameBG/BgCentral/bg/Down").GetComponent<SpriteRenderer>();
        garageTitles = new string[4];
        garageTitles[0] = OnTheRunDataLoader.Instance.GetLocaleString("europeanCarsTitle");
        garageTitles[1] = OnTheRunDataLoader.Instance.GetLocaleString("orientalCarsTitle");
        garageTitles[2] = OnTheRunDataLoader.Instance.GetLocaleString("americanCarsTitle");
        garageTitles[3] = OnTheRunDataLoader.Instance.GetLocaleString("muscleCarsTitle");

        purchaseCar = transform.FindChild("AnchorCenter/purchaseCar").gameObject;
        purchaseCarLocked = transform.FindChild("AnchorCenter/LockedPanel/purchaseLockedCar").gameObject;
        purchaseCarUnlocked = transform.FindChild("AnchorCenter/purchaseUnlockedCar").gameObject;

        lockedPanel = transform.FindChild("AnchorCenter/LockedPanel").gameObject;
        //lockedPanelText = transform.FindChild("AnchorCenter/LockedPanel/tfLockedLabel").GetComponent<UITextField>();
        //lockedPanelText.text = OnTheRunDataLoader.Instance.GetLocaleString("unlock_car5");
        transform.FindChild("AnchorBottomRight/PlayButton/TextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("btPlay");
        transform.FindChild("LeftCenterAnchor/InfoPanel/ParkingLotLocked/tfGarageTitle").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("buy_parking_lot");

        purchaseCarLocked.transform.FindChild("tfBuyLabel").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("or");
        purchaseCarUnlocked.transform.FindChild("tfOrLabel").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("or");

        gameLight = uiRoot.GameLight;// GameObject.FindGameObjectWithTag("GameLight");
        garageLight = uiRoot.GarageLight;// GameObject.FindGameObjectWithTag("GarageLight");

        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        cameraManager = GameObject.FindGameObjectWithTag("MainCamera");
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        environmentManager = gameplayManager.GetComponent<OnTheRunEnvironment>();
        tfGarageTitle = transform.FindChild("AnchorCenter/Locations/tfGarageTitle").GetComponent<UITextField>();
        uiButtonsBar = GameObject.Find("UI").transform.FindChild("ButtonsBar").gameObject;

        currentCarsList = Cars_EU;
        parkingAreaWidth = 20.0f;
        carSpeed = 50.0f;

        uiPlayButton = gameObject.transform.FindChild("AnchorBottomRight/PlayButton").GetComponent<UIButton>();
        playArrowEnable = uiPlayButton.transform.FindChild("Arrow_enable").gameObject;
        playArrowEnable.SetActive(true);
        playArrowDisable = uiPlayButton.transform.FindChild("Arrow_disable").gameObject;
        playArrowDisable.SetActive(false);

        carStatsGroup = transform.FindChild("LeftCenterAnchor/InfoPanel/StatsPanel");

        infoBars = new Transform[infoBarNum];
        for (int i = 0; i < infoBarNum; i++)
		{
            infoBars[i] = carStatsGroup.FindChild("CarInfoItem" + (i + 1));
		}

        carRentTimer = transform.FindChild("AnchorCenter/CarRentTimer").gameObject;
        comingSoonPanel = transform.FindChild("LeftCenterAnchor/InfoPanel/ComingSoon").gameObject;
        parkingLotLocker = transform.FindChild("LeftCenterAnchor/InfoPanel/ParkingLotLocked").gameObject;
        parkingLotLockerWeb = transform.FindChild("LeftCenterAnchor/InfoPanel/ParkingLotLockedWeb").gameObject;
        parkingLotLockerXP = transform.FindChild("LeftCenterAnchor/InfoPanel/ParkingLotLockedXP").gameObject;
        parkingLotLockedButton = parkingLotLocker.transform.FindChild("BuyParkingLotButton").gameObject;
        parkingLotBuyForButton = parkingLotLockerXP.transform.FindChild("BuyParkingLotButton").gameObject;
        parkingLotLockedButton.SetActive(false);
        parkingLotLockedButton.SetActive(true);
        parkingLotLockerWeb.SetActive(false);
        parkingLotLockerXP.SetActive(false);

        uiSharedData = Manager<UIRoot>.Get().GetComponent<UISharedData>();

        comingSoonActive = OnTheRunDataLoader.Instance.GetTiersComingSoon();

        transform.FindChild("InvisibleButtonRightWeb").gameObject.SetActive(false);
        transform.FindChild("InvisibleButtonLeftWeb").gameObject.SetActive(false);

#if UNITY_WEBPLAYER
        transform.FindChild("InvisibleButtonRight").gameObject.SetActive(false);
        transform.FindChild("InvisibleButtonLeft").gameObject.SetActive(false);
        transform.FindChild("InvisibleButtonRightWeb").gameObject.SetActive(true);
        transform.FindChild("InvisibleButtonLeftWeb").gameObject.SetActive(true);
#endif

    }

    void DeactivateWebArrows()
    {
        transform.FindChild("InvisibleButtonRightWeb").gameObject.SetActive(false);
        transform.FindChild("InvisibleButtonLeftWeb").gameObject.SetActive(false);
    }

    //-------------------------------------------------//
    float minScale = 0.65f;//0.23f;
    float maxScale = 1.05f;
    float scrollerOffset = 0.65f;
    GameObject[] scrollerCarsList = null;
    Transform mirroredCar = null;
    //-----------------------------------//
    void UpdateVehiclesPositionScale()
    {
        scrollerCarsList = currentCarsList;

        for (int i = 0; i < 5; i++)
        {
            float currScreenXPos = gameplayManager.MainCamera.WorldToViewportPoint(scrollerCarsList[i].transform.position).x;
            currScreenXPos = maxScale - Mathf.Abs(currScreenXPos - scrollerOffset) * 1.2f;
            currScreenXPos = Mathf.Clamp(currScreenXPos, 0.0f, maxScale - 0.05f);
            float currScale = Mathf.Max(minScale, currScreenXPos);
            scrollerCarsList[i].transform.localScale = Vector3.one * currScale;
            mirroredCar = scrollerCarsList[i].transform.parent.FindChild("Mirrored");
            if (mirroredCar != null)
                mirroredCar.localScale = new Vector3(1.0f, -0.8f, 1.0f) * currScale;
        }
    }
    //-------------------------------------------------//

    int oldScrollIndex = 0;
    int nextSnapIndex = -999;
    float currentSnapX = -999.0f;
    bool isCurrentCar = true;
    void UpdateCarScroller()
    {
        if (OnTheRunUITransitionManager.Instance.isInTransition)
            return;

        if (currentScroller == null)
            return;

        if (uiManager.FrontPopup != null)
            currentScroller.IsDragging = false;

        int iScrolledIndex = Math.Abs(currentScroller.snapX);

        if (currentScroller.IsDragging)
        {
            if (currentSnapX == -999.0f)
                currentSnapX = (iScrolledIndex * currentScroller.snapSpace.x);

            float diff = currentScroller.Offset.x + iScrolledIndex * currentScroller.snapSpace.x;
            isCurrentCar = (oldScrollIndex * currentScroller.snapSpace.x) == currentSnapX;
            if (Mathf.Abs(diff) > currentScroller.snapSpace.x * 0.001f)//0.15f
                nextSnapIndex = (int)Mathf.Sign(diff);
        }
        else
        {
            isCurrentCar = (oldScrollIndex * currentScroller.snapSpace.x) == currentSnapX;
            if (nextSnapIndex > -999 && isCurrentCar)
            {
                categorySelectedCar[currentSetIndex] -= nextSnapIndex;
                if (categorySelectedCar[currentSetIndex] < 0)
                    categorySelectedCar[currentSetIndex] = 0;
                else if (categorySelectedCar[currentSetIndex] > 4)
                    categorySelectedCar[currentSetIndex] = 4;

                int iNextSnap = -categorySelectedCar[currentSetIndex];
                currentScroller.easeToSnapX(iNextSnap);
            }

            currentSnapX = -999.0f;
            nextSnapIndex = -999;
        }

        if (oldScrollIndex != iScrolledIndex)
        {
            SelectCar(iScrolledIndex);
            oldScrollIndex = iScrolledIndex;
        }
    }
    /*int oldScrollIndex = 0;
    void UpdateCarScroller()
    {
        if( currentScroller == null )
            return;

        int iScrolledIndex = Math.Abs( currentScroller.snapX );

        if( oldScrollIndex != iScrolledIndex )
        {
            SelectCar( iScrolledIndex );
            oldScrollIndex = iScrolledIndex;
        }
    }*/
    //-------------------------------------------------//
    void Update()
    {
		UpdateMissionPanelOpen();
        UpdateVehiclesPositionScale();
        UpdateCarScroller();

        if (invisibleButtonsTimer >= 0.0f)
            invisibleButtonsTimer -= TimeManager.Instance.MasterSource.DeltaTime;
    }
    #endregion

    #region Functions
    public bool IsComingSoonActive(int index)
    {
        return comingSoonActive[index];
    }

	public GameObject LockAnimationBump;
	public GameObject LockAnimationUnlock;
	public GameObject StarObject;

    void ActivatePanel(string panelName, bool active)
    {
        switch (panelName)
        {
            case "LockedPanel":
                lockedPanel.SetActive(active);
                lockedPanel.GetComponent<UIEnterExitAnimations>().SendMessage("ResetEnterExitAnimation", UIEnterExitAnimations.AnimationType.EnterPage, SendMessageOptions.DontRequireReceiver);
                
			LockAnimationUnlock.SetActive( false );

			if( active )
			{
				LockAnimationBump.SetActive( false );
				LockAnimationBump.SetActive( true );
				StarObject.SetActive( true );
				purchaseCarLocked.transform.FindChild("PurchaseButton").gameObject.SetActive( true );
				
				Transform p = purchaseCarLocked.transform.FindChild("PurchaseButton").gameObject.transform.parent.FindChild("tfBuyLabel");
				if( p != null )
					p.gameObject.SetActive( true );
			}

			break;
            case "InfoPanel":
                for (int i = 0; i < comingSoonObjects.Length; ++i)
                    comingSoonObjects[i].SetActive(comingSoonActive[i] ? true : false);

                if (comingSoonActive[currentSetIndex])
                {
                    comingSoonPanel.gameObject.SetActive(!active);
                }
                else
                {
#if UNITY_WEBPLAYER
                    //parkingLotLockerWeb.SetActive(!active);
                    if (IsTierBlockedByXP(currentSetIndex) && !CheckTierLockedByXP(currentSetIndex))
                    {
                        parkingLotLockerWeb.gameObject.SetActive(!active);
                    }
                    else
                        parkingLotLockerWeb.gameObject.SetActive(!active);
#else
                    if (IsTierBlockedByXP(currentSetIndex) && !CheckTierLockedByXP(currentSetIndex))
                    {
                        parkingLotLockerXP.gameObject.SetActive(!active);
                    }
                    else
                        parkingLotLocker.gameObject.SetActive(!active);
#endif
                }
                
                carStatsGroup.gameObject.SetActive(active);
                for (int i = 0; i < infoBars.Length; i++)
                {
                    UIButton currButton = infoBars[i].FindChild("GreenButtonSquare").transform.GetComponent<UIButton>();
                    if (currButton.State == UIButton.StateType.Normal)
                        currButton.State = active ? UIButton.StateType.Normal : UIButton.StateType.Disabled;
                }
                break;
            case "PurchasePanel":
                purchaseCar.SetActive(active);

                if(purchaseCar.transform.localPosition.x > 5.0f)
                {
                    purchaseCar.transform.localPosition = new Vector3(1.3f, purchaseCar.transform.localPosition.y, purchaseCar.transform.localPosition.z);
                }
                //carStatsGroup.FindChild("purchaseCar").gameObject.SetActive(active);
                break;
        }

    }
    //-------------------------------------------------//
    /*OnTheRunEnvironment.Environments EnvironmentFromIdx(int index)
    {
        OnTheRunEnvironment.Environments retValue = OnTheRunEnvironment.Environments.Europe;
        switch (index)
        {
            case 0: retValue = OnTheRunEnvironment.Environments.Europe; break;
            case 1: retValue = OnTheRunEnvironment.Environments.Asia; break;
            case 2: retValue = OnTheRunEnvironment.Environments.NY; break;
            case 3: retValue = OnTheRunEnvironment.Environments.USA; break;
        }

        return retValue;
    }*/
    //-------------------------------------------------//
    void SetupParkingLot()
    {
        OnTheRunEnvironment.Environments eEnv = gameplayManager.EnvironmentFromIdx(currentSetIndex);

        uiSharedData.OnNewGarageLocationChosen(currentSetIndex);

        if (eEnv == OnTheRunEnvironment.Environments.Europe)
        {
            currentCarsList = Cars_EU;
            currentScroller = Cars_EU_Scroller;
        }
        else if (eEnv == OnTheRunEnvironment.Environments.NY)
        {
            currentCarsList = Cars_NY;
            currentScroller = Cars_NY_Scroller;
        }
        else if (eEnv == OnTheRunEnvironment.Environments.Asia)
        {
            currentCarsList = Cars_JP;
            currentScroller = Cars_JP_Scroller;
        }
        else if (eEnv == OnTheRunEnvironment.Environments.USA)
        {
            currentCarsList = Cars_USA;
            currentScroller = Cars_USA_Scroller;
        }
        else  // DEFAULT 
        {
            currentCarsList = Cars_EU;
            currentScroller = Cars_EU_Scroller;
        }

        currentScroller.maxSpeed = 20.0f;

        if (Cars_EU_Scroller != null)
            Cars_EU_Scroller.gameObject.SetActive(eEnv == OnTheRunEnvironment.Environments.Europe);
        if (Cars_NY_Scroller != null)
            Cars_NY_Scroller.gameObject.SetActive(eEnv == OnTheRunEnvironment.Environments.NY);
        if (Cars_JP_Scroller != null)
            Cars_JP_Scroller.gameObject.SetActive(eEnv == OnTheRunEnvironment.Environments.Asia);
        if (Cars_USA_Scroller != null)
            Cars_USA_Scroller.gameObject.SetActive(eEnv == OnTheRunEnvironment.Environments.USA);
    }
    //-------------------------------------------------//
    void ShowCars()
    {
        int iLen = currentCarsList.Length;

        for (int i = 0; i < iLen; ++i)
        {
            ActivatePlayer(currentCarsList[i], false);
        }

#if UNITY_WEBPLAYER
        int nextIndex = categorySelectedCar[currentSetIndex];
        transform.FindChild("InvisibleButtonRightWeb").gameObject.SetActive(nextIndex != 4);
        transform.FindChild("InvisibleButtonLeftWeb").gameObject.SetActive(nextIndex != 0);
#endif
    }
    //-------------------------------------------------//
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
    //-------------------------------------------------//
    void SetGarageTitle(string text)
    {
        tfGarageTitle.text = text.ToUpper();
    }
    #endregion

    #region Messages
    void OnRefreshRentedCars()
    {
        if (gameObject.activeInHierarchy)
        {
            FillCarInfo();
        }
    }
    //-------------------------------------------------//
    void OnMoveSelectedCarRight()
    {
        MoveCarRight();
        UpdateScroller();
    }
    //-------------------------------------------------//
    void OnMoveSelectedCarLeft()
    {
        MoveCarLeft();
        UpdateScroller();
    }
    //-------------------------------------------------//
    void MoveCarLeft()
    {
        int nextIndex = --categorySelectedCar[currentSetIndex];
        if (nextIndex < 0) nextIndex = 4;
        else if (nextIndex > 4) nextIndex = 0;
        categorySelectedCar[currentSetIndex] = nextIndex;
        FillCarInfo();
        PlayerPersistentData.Instance.SaveGarageSelection(currentSetIndex, categorySelectedCar);

#if UNITY_WEBPLAYER
        if (nextIndex == 0)
            transform.FindChild("InvisibleButtonLeftWeb").gameObject.SetActive(false);
        else
            transform.FindChild("InvisibleButtonRightWeb").gameObject.SetActive(true);
#endif
    }
    //-------------------------------------------------//
    void MoveCarRight()
    {
        int nextIndex = ++categorySelectedCar[currentSetIndex];
        if (nextIndex < 0) nextIndex = 4;
        else if (nextIndex > 4) nextIndex = 0;
        categorySelectedCar[currentSetIndex] = nextIndex;
        FillCarInfo();
        PlayerPersistentData.Instance.SaveGarageSelection(currentSetIndex, categorySelectedCar);

#if UNITY_WEBPLAYER
        if (nextIndex == 4)
            transform.FindChild("InvisibleButtonRightWeb").gameObject.SetActive(false);
        else
            transform.FindChild("InvisibleButtonLeftWeb").gameObject.SetActive(true);
#endif
    }
    //-------------------------------------------------//
    void SelectCar(int iIndex)
    {
        int nextIndex = iIndex;
        if (nextIndex < 0) nextIndex = 4;
        else if (nextIndex > 4) nextIndex = 0;
        categorySelectedCar[currentSetIndex] = nextIndex;
        FillCarInfo(true);
        PlayerPersistentData.Instance.SaveGarageSelection(currentSetIndex, categorySelectedCar);

#if UNITY_WEBPLAYER
        if (nextIndex == 0)
            transform.FindChild("InvisibleButtonLeftWeb").gameObject.SetActive(false);
        else
            transform.FindChild("InvisibleButtonLeftWeb").gameObject.SetActive(true);
        
        if (nextIndex == 4)
            transform.FindChild("InvisibleButtonRightWeb").gameObject.SetActive(false);
        else
            transform.FindChild("InvisibleButtonRightWeb").gameObject.SetActive(true);
#endif
    }
    //-------------------------------------------------//
    void OnLeftArrowReleased()
    {
        --currentSetIndex;

        if (currentSetIndex < 0)
            currentSetIndex = maxCarSetsNumber - 1;

        SetupParkingLot();
        Manager<UIRoot>.Get().SetupBackground(gameplayManager.EnvironmentFromIdx(currentSetIndex));

        ShowCars();
        FillCarInfo();
        UpdateScroller();
    }
    //-------------------------------------------------//
    void OnRightArrowReleased()
    {
        ++currentSetIndex;

        if (currentSetIndex > maxCarSetsNumber - 1)
            currentSetIndex = 0;

        SetupParkingLot();
        Manager<UIRoot>.Get().SetupBackground(gameplayManager.EnvironmentFromIdx(currentSetIndex));

        ShowCars();
        FillCarInfo();
        UpdateScroller();
    }
    //-------------------------------------------------//
    void OnLocationReleased(int locationIndex)
    {
        if (comingSoonActive[locationIndex - 1])
            return;

        if (currentSetIndex != locationIndex - 1)
            interfaceSounds.ChangeLocation();

        currentSetIndex = locationIndex - 1;

        SetGarageTitle(garageTitles[currentSetIndex]);
        SetupParkingLot();
        Manager<UIRoot>.Get().SetupBackground(gameplayManager.EnvironmentFromIdx(currentSetIndex));

        ShowCars();
        FillCarInfo();
        UpdateScroller();
        PlayerPersistentData.Instance.SaveGarageSelection(currentSetIndex, categorySelectedCar);
    }
    //-------------------------------------------------//
    void UpdateScroller()
    {
        int iNextSnap = -categorySelectedCar[currentSetIndex];
        currentScroller.easeToSnapX(iNextSnap);
    }
    //-------------------------------------------------//
    void InitializeButtonsBarStatus()
    {
        //Transform garageButton = uiButtonsBar.transform.FindChild("Toggle/btGarage");
        uiButtonsBar.GetComponent<UIButtonsBar>().SendMessage("OnReset");
        //uiButtonsBar.SendMessage("OnButtonActive", garageButton.GetComponent<UIButton>());
        //uiButtonsBar.transform.FindChild("Toggle").GetComponentInChildren<UIToggleButton>().SetActiveButton(garageButton.GetComponent<UIButton>());
        //garageButton.GetComponent<UIButtonsColorModified>().SendMessage("UIButtonTextColor_OnPressed", garageButton.GetComponent<UIButton>());

        purchaseCar.transform.FindChild("PurchaseButton").gameObject.SetActive(false);
        purchaseCar.transform.FindChild("PurchaseButton").gameObject.SetActive(true);
        //carStatsGroup.FindChild("purchaseCar/PurchaseButton").gameObject.SetActive(false);
        //carStatsGroup.FindChild("purchaseCar/PurchaseButton").gameObject.SetActive(true);
    }
    #endregion
    
    #region Signals
    void Signal_OnEnter(UIPage page)
    {
        //Check if last car of a tier has been unlocked by a car won in the wheel or by daily bonus
        if (UIGaragePage.lastWonCarId != OnTheRunGameplay.CarId.None)
        {
            GameObject[] tierList = Cars_EU;
            int currentCarIndex = (int)UIGaragePage.lastWonCarId;
            if (currentCarIndex > (int)OnTheRunGameplay.CarId.Car_1_europe && currentCarIndex < (int)OnTheRunGameplay.CarId.Car_5_europe)
                tierList = Cars_EU;
            if (currentCarIndex > (int)OnTheRunGameplay.CarId.Car_1_oriental && currentCarIndex < (int)OnTheRunGameplay.CarId.Car_5_oriental)
                tierList = Cars_JP;
            if (currentCarIndex > (int)OnTheRunGameplay.CarId.Car_1_american && currentCarIndex < (int)OnTheRunGameplay.CarId.Car_5_american)
                tierList = Cars_NY;
            if (currentCarIndex > (int)OnTheRunGameplay.CarId.Car_1_muscle && currentCarIndex < (int)OnTheRunGameplay.CarId.Car_5_muscle)
                tierList = Cars_USA;

            CheckForLastCarUnlockedTracking(tierList);

            UIGaragePage.lastWonCarId = OnTheRunGameplay.CarId.None;
        }

        if (backupSliderPosition != default(Vector3))
            Cars_EU_Scroller.transform.localPosition = backupSliderPosition;

        Manager<UIRoot>.Get().ActivateBackground(true);

        bool initButtonsBar = false;

        if (garageTitles == null)
        {
            initButtonsBar = true;
            InitializePage();
        }

        if (gameplayManager.checkForDailyBonusAfterPause)
            Manager<UIRoot>.Get().ShowDailyBonusPopup();

        //Manager<UIRoot>.Get().CreateUICamera();
        Manager<UIRoot>.Get().ShowOffgameBG(true);

        gameLight.SetActive(false);
        garageLight.SetActive(true);

        PlayerPersistentData.Instance.LoadGarageSelection();

        PlayerPersistentData.Instance.SelectedGarageCategory = gameplayManager.EnvironmentIdx(environmentManager.currentEnvironment);

        currentSetIndex = PlayerPersistentData.Instance.SelectedGarageCategory;
        categorySelectedCar = PlayerPersistentData.Instance.SelectedGarageCars;

        if (OnTheRunTutorialManager.Instance.TutorialActive && backupCurrentSetIndex >= 0)
        {
            currentSetIndex = backupCurrentSetIndex;
            currentCarsList = backupCurrentCarsList;
            categorySelectedCar[0] = backupCategorySelectedCar;
            backupCurrentSetIndex = -1;
        }

        //this.StartCoroutine(RefreshRentCar(1.0f));

        cameraManager.GetComponent<FiniteStateMachine>().enabled = false;

        cameraManager.transform.position = GARAGE_CAM_POS;
        cameraManager.transform.rotation = GARAGE_CAM_ROT;

        backupFOV = cameraManager.GetComponent<Camera>().fieldOfView;
        cameraManager.GetComponent<Camera>().fieldOfView = 35.0f;

        if (currentSetIndex == -1)
        {
            currentSetIndex = 0;
            currentCarsList = Cars_EU;
            ShowCars();
        }
        else
        {
            SetupParkingLot();
            Manager<UIRoot>.Get().SetupBackground(gameplayManager.EnvironmentFromIdx(currentSetIndex));
            ShowCars();
        }

        Manager<UIRoot>.Get().ShowPageBorders(true);
        Manager<UIRoot>.Get().ShowCommonPageElements(true, true, true, true, true);

        if (initButtonsBar)
        {
            InitializeButtonsBarStatus();
        }

#if UNITY_WEBPLAYER
        /*
        Transform missionsButton = uiButtonsBar.transform.FindChild("Toggle/btMissions");
        Transform garageButton = uiButtonsBar.transform.FindChild("Toggle/btGarage");
        Transform specialButton = uiButtonsBar.transform.FindChild("Toggle/btTrucks");
        missionsButton.transform.localPosition = new Vector3(1.97f, missionsButton.transform.localPosition.y, missionsButton.transform.localPosition.z);
        garageButton.transform.localPosition = new Vector3(3.17f, garageButton.transform.localPosition.y, garageButton.transform.localPosition.z);
        specialButton.transform.localPosition = new Vector3(4.4f, specialButton.transform.localPosition.y, specialButton.transform.localPosition.z);
        */
        //transform.FindChild("InvisibleButtonRightWeb").gameObject.SetActive(true);
        //transform.FindChild("InvisibleButtonLeftWeb").gameObject.SetActive(true);

        //dani
        uiButtonsBar.transform.FindChild("btRankings").gameObject.SetActive(false);
        uiButtonsBar.transform.FindChild("btFBFriends").gameObject.SetActive(false);
        uiButtonsBar.transform.FindChild("SpinWheelButton").gameObject.SetActive(false);
#endif

        UnlockTierByXP();
        UnlockCarsByXP();

        environmentManager.currentEnvironment = gameplayManager.EnvironmentFromIdx(currentSetIndex);
        SetGarageTitle(garageTitles[currentSetIndex]);

        FillCarInfo();

#if !UNITY_WEBPLAYER
        if (uiManager.FrontPopup == null && OnTheRunFuelManager.Instance.Fuel > 0)
            OnTheRunFBLoginPopupManager.Instance.CheckFBPopupCanShow(true);
        OnTheRunFBLoginPopupManager.Instance.justEnteredInGarage = false;
#endif

        if (showParkingLotUnlockedPopup && uiManager.FrontPopup == null)
        {
            ShowTierUnlockedPopup();
        }

        if (showCarUnlockedPopup && uiManager.FrontPopup == null)
            ShowCarUnlockedPopup();

        if (uiManager.FrontPopup == null && OnTheRunFuelManager.Instance.Fuel > 0)
        {
            CheckForAdvicePopup();
        }

        UpdateScroller();

        //Transform garageButton = uiButtonsBar.transform.FindChild("Toggle/btGarage");
        //uiButtonsBar.transform.FindChild("Toggle").GetComponentInChildren<UIToggleButton>().SetActiveButton(garageButton.GetComponent<UIButton>());
        uiButtonsBar.SendMessage("UpdateBarStatus");

		OpenMissionPanelSafe();
    }
	//---------------------------------------//
	bool m_bOpenMission = false;
	static readonly float WAIT_TIME = 0.35f;
	float m_fWaitToOpenTimer = 0f;
	//---------------------------------------//
	void OpenMissionPanelSafe()
	{
		missionPanel.gameObject.SetActive( false );
		m_bOpenMission = true;
		m_fWaitToOpenTimer = TimeManager.Instance.MasterSource.TotalTime;
	}
	//---------------------------------------//
	void UpdateMissionPanelOpen()
	{
		if( m_bOpenMission == false )
			return;
		
		if( TimeManager.Instance.MasterSource.TotalTime - m_fWaitToOpenTimer > WAIT_TIME )
		{
			OpenMissionPanel();
		}
	}
	//---------------------------------------//
	void OpenMissionPanel()
	{
		bool bAnyPopupOpen = UIManager.Instance.PopupsInStack > 0;
		
//		bAnyPopupOpen = UIManager.Instance.IsPopupInStack("DailyBonusPopup") ||
//				UIManager.Instance.IsPopupInStack("DailyBonusSequencePopup") ||
//				UIManager.Instance.IsPopupInStack("FacebookLoginPopup") ||
//				UIManager.Instance.IsPopupInStack("FBLoginRequest") ||
//				UIManager.Instance.IsPopupInStack("ReachRankPopup") ||
//				UIManager.Instance.IsPopupInStack("RankFeedbackPopup") ||
//				UIManager.Instance.IsPopupInStack("SingleButtonFeedbackPopup");

		// if there are popups open try again 
		if( bAnyPopupOpen )
		{
//			Debug.LogWarning("Trying again , some popup is in the way ...");
			OpenMissionPanelSafe();
			return;
		}

//		Debug.LogWarning("Opening mission panel ! road is clear");
		missionPanel.gameObject.SetActive( true );
		missionPanel.UpdatePanel(OnTheRunSingleRunMissions.Instance.CurrentTierMission);
		m_bOpenMission = false;
	}
	//---------------------------------------//
    public void CheckForLastCarUnlockedTracking(GameObject[] tierList)
    {
        if (tierList == null)
            tierList = currentCarsList;

        if (CheckForLastCarUnlocked(tierList) == tierList.Length - 1)
        {
            if (OnTheRunOmniataManager.Instance != null)
            {
                OnTheRunGameplay.CarId currentCarId = tierList[categorySelectedCar.Length - 1].GetComponent<PlayerKinematics>().carId;
                OnTheRunOmniataManager.Instance.TrackVirtualPurchase(currentCarId.ToString().ToLowerInvariant(), PriceData.CurrencyType.FirstCurrency, "0", OmniataIds.Product_Type_Unlock);
            }
        }
    }

    void CheckForAdvicePopup()
    {
        canShowReachRankAdvise = false;
        int countToShowAdvisePopup = EncryptedPlayerPrefs.GetInt("csc_advise", 0);
        if (countToShowAdvisePopup != -1)
        {
            for (int i = 0; i < maxCarSetsNumber; i++)
            {
                bool stillToUnlock = PlayerPersistentData.Instance.IsParkingLotLocked(i);
                if (stillToUnlock && playerLevelToUnlockTiers[i] > PlayerPersistentData.Instance.Level)
                {
                    nextTiersIndex = i;
                    canShowReachRankAdvise = true;
                    break;
                }
            }

            //if (!IsTierBlockedByXP(nextTiersIndex))
            //    canShowReachRankAdvise = false;

            if (canShowReachRankAdvise)
            {
                if (++garageEnteredTimes > OnTheRunDataLoader.Instance.GetTimeToWaitToShowReachRankAdvise())
                {
                    garageEnteredTimes = -1;
                    OnTheRunUITransitionManager.Instance.OpenPopup("ReachRankPopup");
                    uiManager.FrontPopup.GetComponent<UIReachRankPopup>().tfLevelText.text = playerLevelToUnlockTiers[nextTiersIndex].ToString();
                    uiManager.FrontPopup.GetComponent<UIReachRankPopup>().tfText2.text = OnTheRunDataLoader.Instance.GetLocaleString("reach_lvl_tounlock") + " " + garageTitles[nextTiersIndex];
                }
                EncryptedPlayerPrefs.SetInt("csc_advise", garageEnteredTimes);
            }
        }
    }

    void ShowTierUnlockedPopup()
    {
        showParkingLotUnlockedPopup = false;

        OnTheRunUITransitionManager.Instance.OpenBuyPopup(OnTheRunDataLoader.Instance.GetLocaleString("popup_amazing"), garageTitles[currentSetIndex] + " " + OnTheRunDataLoader.Instance.GetLocaleString("parkinglot_unlocked"));
        UIPopup frontPopup = uiManager.FrontPopup;
        if (frontPopup != null && frontPopup.name.Equals("SingleButtonFeedbackPopup") && frontPopup.GetComponent<UIBuyFeedbackPopup>() != null)
        {
            frontPopup.GetComponent<UIBuyFeedbackPopup>().SetBoughtItem(UIBuyFeedbackPopup.ItemBought.ParkingLot);
            frontPopup.GetComponent<UIBuyFeedbackPopup>().SetFeedItemName(garageTitles[currentSetIndex]);
        }
    }

    void ShowCarUnlockedPopup()
    {
        showCarUnlockedPopup = false;
       
        OnTheRunUITransitionManager.Instance.OpenBuyPopup(OnTheRunDataLoader.Instance.GetLocaleString("popup_amazing"), OnTheRunDataLoader.Instance.GetLocaleString("popup_unlocked_car"));
        UIPopup frontPopup = Manager<UIManager>.Get().FrontPopup;
        if (frontPopup != null && frontPopup.name.Equals("SingleButtonFeedbackPopup") && frontPopup.GetComponent<UIBuyFeedbackPopup>() != null)
        {
            frontPopup.GetComponent<UIBuyFeedbackPopup>().SetBoughtItem(UIBuyFeedbackPopup.ItemBought.Car);
            frontPopup.GetComponent<UIBuyFeedbackPopup>().SetShareButtonVisibility(false);
        }
    }

    void Signal_OnExit(UIPage page)
    {
        // refresh default scale due to scrolling scale for cars
        if (selectedCar != null)
            selectedCar.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        currentCarScaling = false;
        if (gameLight != null)
            gameLight.SetActive(true);
        if (garageLight != null)
            garageLight.SetActive(false);

        this.StopCoroutine("RefreshRentCar");

        PlayerPersistentData.Instance.SaveGarageSelection(currentSetIndex, categorySelectedCar);
        if (cameraManager != null)
        {
            cameraManager.GetComponent<FiniteStateMachine>().enabled = true;
            cameraManager.GetComponent<Camera>().fieldOfView = backupFOV;

            Manager<UIRoot>.Get().ShowPageBorders(false);
            Manager<UIRoot>.Get().ShowCommonPageElements(false, false, false, false, false);
        }
    }

    void Signal_OnBuyParkingLotReleased(UIButton button)
    {
        int parkingLotCost = PlayerPersistentData.Instance.GetParkingLotCost(currentSetIndex);
        PriceData.CurrencyType parkingLotCurrency = OnTheRunDataLoader.Instance.GetTiersCurrencies()[currentSetIndex];
        bool canAfford = PlayerPersistentData.Instance.CanAfford(parkingLotCurrency, parkingLotCost);
        if (canAfford)
        {
            PlayerPersistentData.Instance.BuyItem(parkingLotCurrency, parkingLotCost, true, OnTheRunDataLoader.Instance.GetLocaleString("popup_amazing"), OnTheRunDataLoader.Instance.GetLocaleString("popup_unlocked_tier"), UIBuyFeedbackPopup.ItemBought.ParkingLot, garageTitles[currentSetIndex]);

            if (OnTheRunOmniataManager.Instance != null)
                OnTheRunOmniataManager.Instance.TrackVirtualPurchase(OmniataIds.Product_ParkingLot[currentSetIndex]/*OmniataIds.Product_ParkingLot + "_" + garageTitles[currentSetIndex]*/, PriceData.CurrencyType.SecondCurrency, parkingLotCost.ToString(), OmniataIds.Product_Type_Standard);

            PlayerPersistentData.Instance.UnlockParkingLot(currentSetIndex);
            FillCarInfo();
        }
        else
        {
            if (parkingLotCurrency == PriceData.CurrencyType.FirstCurrency)
                Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Money);
            else
                Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Diamonds);
        }
    }

    void Signal_OnBuyParkingLotForDiamondsReleased(UIButton button)
    {
        int parkingLotCost = (int)tiersBuyFor[currentSetIndex];
        PriceData.CurrencyType parkingLotCurrency = PriceData.CurrencyType.SecondCurrency;
        bool canAfford = PlayerPersistentData.Instance.CanAfford(parkingLotCurrency, parkingLotCost);
        if (canAfford)
        {
            PlayerPersistentData.Instance.BuyItem(parkingLotCurrency, parkingLotCost, true, OnTheRunDataLoader.Instance.GetLocaleString("popup_amazing"), OnTheRunDataLoader.Instance.GetLocaleString("popup_unlocked_tier"), UIBuyFeedbackPopup.ItemBought.ParkingLot, garageTitles[currentSetIndex]);

            if (OnTheRunOmniataManager.Instance != null)
                OnTheRunOmniataManager.Instance.TrackVirtualPurchase(OmniataIds.Product_ParkingLot[currentSetIndex]/*OmniataIds.Product_ParkingLot + "_" + garageTitles[currentSetIndex]*/, PriceData.CurrencyType.SecondCurrency, parkingLotCost.ToString(), OmniataIds.Product_Type_Standard);

            PlayerPersistentData.Instance.UnlockParkingLot(currentSetIndex);
            FillCarInfo();
        }
        else
        {
            if (parkingLotCurrency == PriceData.CurrencyType.FirstCurrency)
                Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Money);
            else
                Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Diamonds);
        }
    }

    void Signal_OnBuyAllUpgradesReleased(UIButton button)
    {
        bool canAfford = PlayerPersistentData.Instance.CanAfford(PriceData.CurrencyType.FirstCurrency, allUpgradesCost);
        if (canAfford)
        {
            PlayerPersistentData.Instance.BuyItem(PriceData.CurrencyType.FirstCurrency, allUpgradesCost);
            PlayerGameData gameData = currentCarsList[categorySelectedCar[currentSetIndex]].GetComponent<PlayerGameData>();
            OnTheRunGameplay.CarId currentCarId = currentCarsList[categorySelectedCar[currentSetIndex]].GetComponent<PlayerKinematics>().carId;
            PlayerPersistentData.Instance.CarList[currentCarId].acceleration = gameData.maxAcceleration;
            PlayerPersistentData.Instance.CarList[currentCarId].maxSpeed = gameData.maxMaxSpeed;
            PlayerPersistentData.Instance.CarList[currentCarId].resistance = gameData.maxResistance;
            PlayerPersistentData.Instance.CarList[currentCarId].turboSpeed = gameData.maxTurboSpeed;
            PlayerPersistentData.Instance.Save();
            FillCarInfo();
        }
        else
        {
            Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Money);
        }
    }

    void Signal_OnRentCarReleased(UIButton button)
    {
        /*OnTheRunGameplay.CarId currCarId = currentCarsList[categorySelectedCar[currentSetIndex]].GetComponent<PlayerKinematics>().carId;
        PlayerPersistentData.PlayerData playerData = PlayerPersistentData.Instance.GetPlayerData(currCarId);
        int currCarRentalCost = playerData.rentCost;
        PriceData.CurrencyType currency = playerData.currency;

        if (PlayerPersistentData.Instance.TryRentCar(currCarId, currCarRentalCost, currency))
        {
            PlayerPersistentData.Instance.StartRentCar(currCarId);
            FillCarInfo();
            RefreshTimeData();
        }
        else
        {
            if (currency == PriceData.CurrencyType.FirstCurrency)
                Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Money);
            else
                Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Diamonds);
        }*/
    }

    void Signal_OnTrucksPageRelease(UIButton button)
    {
        uiManager.GoToPage("TrucksPage");
    }

    void SwapCarFromKeyboard(int index)
    {
        if (index < 1)
            Signal_OnInvisibleButtonLeftRelease(null);
        else
            Signal_OnInvisibleButtonRightRelease(null);
    }

    void Signal_OnInvisibleButtonLeftRelease(UIButton button)
    {
        //Debug.Log("********* Signal_OnInvisibleButtonLeftRelease " + categorySelectedCar[currentSetIndex]);
        if (Mathf.Abs(currentScroller.OffsetVelocity.x) < 2.5f && categorySelectedCar[currentSetIndex] > 0 && invisibleButtonsTimer < 0.0f)
        {
            invisibleButtonsTimer = 0.3f;
            OnMoveSelectedCarLeft();
#if UNITY_WEBPLAYER
            interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
#endif
        }

    }

    void Signal_OnInvisibleButtonRightRelease(UIButton button)
    {
        //Debug.Log("********* Signal_OnInvisibleButtonRightRelease " + categorySelectedCar[currentSetIndex]);
        if (Mathf.Abs(currentScroller.OffsetVelocity.x) < 2.5f && categorySelectedCar[currentSetIndex] < 4 && invisibleButtonsTimer < 0.0f)
        {
            invisibleButtonsTimer = 0.3f;
            OnMoveSelectedCarRight();
#if UNITY_WEBPLAYER
            interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
#endif
        }

    }

    void OnCloseConfirmPopup( )
    {
        OnTheRunUITransitionManager.Instance.ClosePopup(OnCloseConfirmPopupDelayed);
    }

    void OnCloseConfirmPopupDelayed( )
    {
        Signal_OnBuyCarButtonPressed(lastButton);
        uiPlayButton.gameObject.SetActive(true);
    }

    protected UIButton lastButton;
    void Signal_OnPlayRelease(UIButton button)
    {
        PlayerPersistentData.Instance.SaveCars();

		missionPanel.SkipNewMissionAnim();

        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

        OnTheRunGameplay.CarId currentCarId = currentCarsList[categorySelectedCar[currentSetIndex]].GetComponent<PlayerKinematics>().carId;
        PlayerPersistentData.PlayerData playerData = PlayerPersistentData.Instance.GetPlayerData(currentCarId);
        if (!playerData.owned)
        {
            //Signal_OnBuyCarButtonPressed(button);

            lastButton = button;
            OnTheRunUITransitionManager.Instance.OpenPopup("BuyConfirmPopup");
            Transform popup = Manager<UIManager>.Get().FrontPopup.transform;
            popup.SendMessage("Initialize", playerData);
        }
        else
        {
            if (OnTheRunFuelManager.Instance.Fuel <= 0 && !OnTheRunFuelManager.Instance.IsFuelFreezeActive())
            {
                if (PlayerPersistentData.Instance.FirstTimeFuelFinished && OnTheRunDataLoader.Instance.GetFirstFuelGiftEnabled())
                {
                    OnTheRunUITransitionManager.Instance.OpenPopup("FuelGiftPopup");
                }
                else if (OnTheRunCoinsService.Instance.IsFreeFuelVideoAvailable())
                {
                    OnTheRunUITransitionManager.Instance.OpenPopup("FuelRefillPopup");
                }
                else
                {
                    Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Fuel);
                }
            }
            else
            {
                if (OnTheRunTutorialManager.Instance.TutorialActive)
                {
                    if (currentSetIndex != 0 || currentCarsList != Cars_EU || categorySelectedCar[currentSetIndex] != 0)
                        OnTheRunTutorialManager.Instance.LocationAndCarChanged = true;
                    backupCurrentSetIndex = currentSetIndex;
                    backupCurrentCarsList = currentCarsList;
                    backupCategorySelectedCar = categorySelectedCar[0];
                    currentSetIndex = 0;
                    currentCarsList = Cars_EU;
                    categorySelectedCar[currentSetIndex] = 0;
                    //SetupParkingLot();
                    backupSliderPosition = Cars_EU_Scroller.transform.localPosition;
                    Cars_EU_Scroller.gameObject.SetActive(true);
                    Cars_EU_Scroller.transform.localPosition = Vector3.one;

                    if (!OnTheRunFuelManager.Instance.IsFuelFreezeActive())
                        OnTheRunFuelManager.Instance.Fuel--;
                }

                this.StartCoroutine(this.StartPlay());
            }
        }
    }

    public IEnumerator StartPlay()
    {
        if (PlayerPersistentData.Instance.FirstTimePlaying)
            OnTheRunUITransitionManager.Instance.OnPageExiting("GaragePage", "InGamePageDirectly");
        else
            OnTheRunUITransitionManager.Instance.OnPageExiting("GaragePage", "InGamePage");


        //long scoresSpread = OnTheRunDataLoader.Instance.GetNumScoresIngameRanks() - 1; //99;
        //McSocialApiManager.Instance.GetScoresForIngame(true, true, McSocialApiUtils.ScoreType.Latest, scoresSpread, OnTheRunMcSocialApiData.Instance.GetLeaderboardId(currentSetIndex));
        McSocialApiManager.Instance.GetScoresForIngame(OnTheRunMcSocialApiData.Instance.GetLeaderboardId(currentSetIndex));

        yield return new WaitForSeconds(OnTheRunUITransitionManager.changePageDelay);

        if (OnTheRunTutorialManager.Instance.TutorialActive)
            categorySelectedCar[currentSetIndex] = 0;

        gameplayManager.SendMessage("ReactivatePlayer");
        selectedCar = currentCarsList[categorySelectedCar[currentSetIndex]];
        baseParkingLotCar = currentCarsList[0];

        switch (gameplayManager.EnvironmentFromIdx(currentSetIndex))
        {
            case OnTheRunEnvironment.Environments.Europe:
                environmentManager.currentTrafficDirection = OnTheRunEnvironment.TrafficDirectionConfiguration.AllForward;
                environmentManager.currentEnvironment = OnTheRunEnvironment.Environments.Europe;
                break;
            case OnTheRunEnvironment.Environments.Asia:
                environmentManager.currentTrafficDirection = OnTheRunEnvironment.TrafficDirectionConfiguration.ForwardBackward;
                environmentManager.currentEnvironment = OnTheRunEnvironment.Environments.Asia;
                break;
            case OnTheRunEnvironment.Environments.NY:
                environmentManager.currentTrafficDirection = OnTheRunEnvironment.TrafficDirectionConfiguration.AllForward;
                environmentManager.currentEnvironment = OnTheRunEnvironment.Environments.NY;
                break;
            case OnTheRunEnvironment.Environments.USA:
                environmentManager.currentTrafficDirection = OnTheRunEnvironment.TrafficDirectionConfiguration.AvoidCentralLaneForwardBackward;
                environmentManager.currentEnvironment = OnTheRunEnvironment.Environments.USA;
                break;
        }

        gameplayManager.SendMessage("LoadNewLevel", false);
    }

    public void RestartSession()
    {
        OnTheRunSounds.Instance.StopSpeech();
        if (uiManager == null)
        {
            uiManager = Manager<UIManager>.Get();
            enemiesManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunEnemiesManager>();
            interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
            cameraManager = GameObject.FindGameObjectWithTag("MainCamera");
            gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
            environmentManager = gameplayManager.GetComponent<OnTheRunEnvironment>();
        }

        uiManager.ActivePage.GetComponent<UIIngamePage>().DestroyLiveNewsFlyer();

        enemiesManager.ResetForRestart();

        ResetStartSequence();

        gameplayManager.SendMessage("ReactivatePlayer");

        if (OnTheRunTutorialManager.Instance.TutorialActive)
        {
            uiManager.GoToPage("IngamePage");
            gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Racing);
            gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Start);

            if (selectedCar == null)
                selectedCar = gameplayManager.Cars_EU[0];

            ActivatePlayer(selectedCar, true);
            gameplayManager.CreatePlayerCarByRef(selectedCar, baseParkingLotCar);

            gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.ReadyToRace);

            Manager<UIRoot>.Get().ShowUpperPageBorders(false);
            uiManager.PopPopup();

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
        else
        {
            uiManager.GoToPage("IngamePage");
            gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Racing);
            gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Start);

            if (selectedCar == null)
                selectedCar = gameplayManager.Cars_EU[0];

            ActivatePlayer(selectedCar, true);
            gameplayManager.CreatePlayerCarByRef(selectedCar, baseParkingLotCar);

            gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.ReadyToRace);
            uiManager.GoToPage("InAppPage");
            Manager<UIRoot>.Get().ShowCurrenciesItem(true);

            OnTheRunUITransitionManager.Instance.OnPageChanged("InAppPage", "InGamePage");
        }
    }

    void ResetStartSequence()
    {
        IntrusiveList<UIFlyer> flyers = uiManager.GetActiveFlyers("SimpleFlyer");
        UIFlyer node = flyers.Head;
        while (node != null)
        {
            UIFlyer tmp = node;
            node = node.next;
            tmp.onEnd.RemoveAllTargets();
            tmp.Stop();
        }
    }

    void LevelLoaded()
    {
        gameplayManager.FirstTimeSaveMeShown = true;

        if (PlayerPersistentData.Instance.FirstTimePlaying || OnTheRunTutorialManager.Instance.TutorialActive)
        {
            PlayerPersistentData.Instance.SaveFirstTimePlayed();

            this.StartCoroutine(this.StartPlayImmediatly());
        }
        else
        {
            uiManager.GoToPage("IngamePage");
            gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Racing);
            gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Start);

            ActivatePlayer(selectedCar, true);
            gameplayManager.CreatePlayerCarByRef(selectedCar, baseParkingLotCar);

            gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.ReadyToRace);
            uiManager.GoToPage("InAppPage");
            Manager<UIRoot>.Get().ShowCurrenciesItem(true);

            OnTheRunUITransitionManager.Instance.OnPageChanged("InAppPage", "");
        }
    }

    IEnumerator StartPlayImmediatly()
    {
        //OnTheRunUITransitionManager.Instance.OnPageExiting("GaragePage", "InGamePage");

        while (UIEnterExitAnimations.activeAnimationsCounter > 0)
        {
            yield return null;
        }

        uiManager.GoToPage("IngamePage");
        gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Racing);
        gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Start);

        ActivatePlayer(selectedCar, true);
        gameplayManager.CreatePlayerCarByRef(selectedCar, baseParkingLotCar);

        gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.ReadyToRace);

        Manager<UIRoot>.Get().ShowUpperPageBorders(false);
        uiManager.PopPopup();

        //OnTheRunInterfaceSounds interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

        //uiManager.GoToPage("IngamePage");
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

    void Signal_OnCarSelectedChange(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        categorySelectedCar[currentSetIndex] = int.Parse(button.name.Substring(button.name.Length - 1, 1), CultureInfo.InvariantCulture) - 1;
        //carSelected = int.Parse(button.name.Substring(button.name.Length-1, 1))-1;
        FillCarInfo();

        PlayerPersistentData.Instance.SaveGarageSelection(currentSetIndex, categorySelectedCar);
    }

    void Signal_OnBuyCarButtonPressed(UIButton button)
    {
        OnTheRunGameplay.CarId currentCarId = currentCarsList[categorySelectedCar[currentSetIndex]].GetComponent<PlayerKinematics>().carId;

		bool bLock = PlayerPersistentData.Instance.GetPlayerData(currentCarId).locked && StarObject.activeInHierarchy;

//		Debug.Log("locked ============= "+ PlayerPersistentData.Instance.GetPlayerData(currentCarId).locked);
//		Debug.Log("canBeBought ============= "+ PlayerPersistentData.Instance.GetPlayerData(currentCarId).canBeBought);

        float carCost = PlayerPersistentData.Instance.GetPlayerData(currentCarId).cost;
        PriceData.CurrencyType currency = PlayerPersistentData.Instance.GetPlayerData(currentCarId).currency;

		// if its the coins button ...
		if( button.name.Contains("Coins") )
		{
			carCost = PlayerPersistentData.Instance.GetPlayerData(currentCarId).alternativeCost;
			currency = PriceData.CurrencyType.FirstCurrency;
		}

        bool canAfford = PlayerPersistentData.Instance.CanAfford(currency, carCost);

        if (canAfford)
        {
            Transform pBut = purchaseCarLocked.transform.FindChild("PurchaseButton");

            if( pBut!= null )
                pBut.gameObject.SetActive(false);

			Transform p = button.gameObject.transform.parent.FindChild("tfBuyLabel");
			if( p != null )
				p.gameObject.SetActive( false );

			StarObject.SetActive( false );

			if( bLock )
				StartCoroutine( LockAnimTask(currency, carCost , currentCarId ) );
			else
				UnlockCarInternal(currency, carCost , currentCarId );
        }
        else
        {
			if (currency == PriceData.CurrencyType.FirstCurrency)
				Manager<UIRoot>.Get().ShowWarningPopupReally(UICurrencyPopup.ShopPopupTypes.Money);
			else
				Manager<UIRoot>.Get().ShowWarningPopupReally(UICurrencyPopup.ShopPopupTypes.Diamonds);
        }
    }

	void UnlockCarInternal( PriceData.CurrencyType eCurrency , float carCost , OnTheRunGameplay.CarId currentCarId )
	{
		PlayerPersistentData.Instance.BuyItem(eCurrency, carCost, true, OnTheRunDataLoader.Instance.GetLocaleString("popup_amazing"), OnTheRunDataLoader.Instance.GetLocaleString("popup_unlocked_car"), UIBuyFeedbackPopup.ItemBought.Car);
		PlayerPersistentData.Instance.GetPlayerData(currentCarId).BuyCar();
		//PlayerPersistentData.Instance.ResetRentCar(currentCarId);
		FillCarInfo();
		OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.BUY_CAR);
		
		CheckForLastCarUnlockedTracking(currentCarsList);
		
		if (CheckForLastCarUnlocked(currentCarsList) == currentCarsList.Length)
			OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.ALL_SCENARIO_CARS);
		
		if (OnTheRunOmniataManager.Instance != null)
			OnTheRunOmniataManager.Instance.TrackVirtualPurchase(currentCarId.ToString().ToLowerInvariant(), eCurrency, carCost.ToString(), OmniataIds.Product_Type_Standard);
	}

	IEnumerator LockAnimTask( PriceData.CurrencyType eCurrency , float carCost , OnTheRunGameplay.CarId currentCarId )
	{
		LockAnimationUnlock.SetActive( false );
		LockAnimationUnlock.SetActive( true );

		Animation pAnim = LockAnimationUnlock.GetComponent<Animation>();
		pAnim[pAnim.clip.name].normalizedTime = 0f;
		pAnim.Play();

		yield return new WaitForEndOfFrame();

		// hide other anim
		LockAnimationBump.SetActive( false );
		
		yield return new WaitForSeconds( 1.5f );

		// play broom broom
		// TODO 

		// highlight the car 
		// TODO 
		
		UnlockCarInternal(eCurrency,carCost,currentCarId);
	}

    void Signal_OnBuyAlternativeCarButtonPressed(UIButton button)
    {
        OnTheRunGameplay.CarId currentCarId = currentCarsList[categorySelectedCar[currentSetIndex]].GetComponent<PlayerKinematics>().carId;
        float carCost = PlayerPersistentData.Instance.GetPlayerData(currentCarId).alternativeCost;
        PriceData.CurrencyType currency = PlayerPersistentData.Instance.GetPlayerData(currentCarId).alternativeCurrency;
        bool canAfford = PlayerPersistentData.Instance.CanAfford(currency, carCost);
        if (canAfford)
        {
            PlayerPersistentData.Instance.BuyItem(currency, carCost, true, OnTheRunDataLoader.Instance.GetLocaleString("popup_amazing"), OnTheRunDataLoader.Instance.GetLocaleString("popup_unlocked_car"), UIBuyFeedbackPopup.ItemBought.Car);
            PlayerPersistentData.Instance.GetPlayerData(currentCarId).BuyCar();
            //PlayerPersistentData.Instance.ResetRentCar(currentCarId);
            FillCarInfo();
            OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.BUY_CAR);

            CheckForLastCarUnlockedTracking(currentCarsList);

            if (CheckForLastCarUnlocked(currentCarsList) == currentCarsList.Length)
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.ALL_SCENARIO_CARS);

            if (OnTheRunOmniataManager.Instance != null)
                OnTheRunOmniataManager.Instance.TrackVirtualPurchase(currentCarId.ToString().ToLowerInvariant(), currency, carCost.ToString(), OmniataIds.Product_Type_Standard);
        }
        else
        {
            if (currency == PriceData.CurrencyType.FirstCurrency)
                Manager<UIRoot>.Get().ShowWarningPopupReally(UICurrencyPopup.ShopPopupTypes.Money);
            else
                Manager<UIRoot>.Get().ShowWarningPopupReally(UICurrencyPopup.ShopPopupTypes.Diamonds);
        }
    }

    void Signal_OnStatButtonPressed(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

        int index = Array.IndexOf<Transform>(infoBars, button.transform.parent);
        OnTheRunGameplay.CarId currentCarId = currentCarsList[categorySelectedCar[currentSetIndex]].GetComponent<PlayerKinematics>().carId;
        PlayerPersistentData.PlayerData playerData = PlayerPersistentData.Instance.GetPlayerData(currentCarId);
        float upgradeCost = 0.0f;
        switch (index)
        {
            case 0: upgradeCost = OnTheRunEconomy.Instance.GetCarUpgradeCost(playerData, "acceleration"); break;
            case 1: upgradeCost = OnTheRunEconomy.Instance.GetCarUpgradeCost(playerData, "maxSpeed"); break;
            case 2: upgradeCost = OnTheRunEconomy.Instance.GetCarUpgradeCost(playerData, "resistance"); break;
            case 3: upgradeCost = OnTheRunEconomy.Instance.GetCarUpgradeCost(playerData, "turboSpeed"); break;
        }

        bool canAfford = PlayerPersistentData.Instance.CanAfford(PriceData.CurrencyType.FirstCurrency, upgradeCost);
        if (canAfford)
        {
			// play upgrade effect
			int iNextIndex = playerData.GetStatAmountByIndex(index) + 1;
			Transform pStatPoint = infoBars[index].FindChild("Bar/barElement"+ iNextIndex);
			PlayBarElementEffect( pStatPoint.position );

            PlayerPersistentData.Instance.BuyItem(PriceData.CurrencyType.FirstCurrency, upgradeCost);
            string upgradeStr = string.Empty;
            string upgradeType = "";
            int upgradeLevel = -1;
            switch (index)
            {
                case 0:
                    PlayerPersistentData.Instance.CarList[currentCarId].acceleration++;
                    //Debug.Log(currentCarId + ": acceleration: " + PlayerPersistentData.Instance.CarList[currentCarId].acceleration);
                    upgradeStr = "_acceleration_ " + PlayerPersistentData.Instance.CarList[currentCarId].acceleration;
                    upgradeType = "acceleration";
                    upgradeLevel = PlayerPersistentData.Instance.CarList[currentCarId].acceleration;
                    break;
                case 1:
                    PlayerPersistentData.Instance.CarList[currentCarId].maxSpeed++;
                    //Debug.Log(currentCarId + ": maxSpeed: " + PlayerPersistentData.Instance.CarList[currentCarId].maxSpeed);
                    upgradeStr = "_maxSpeed_ " + PlayerPersistentData.Instance.CarList[currentCarId].maxSpeed;
                    upgradeType = "maxSpeed";
                    upgradeLevel = PlayerPersistentData.Instance.CarList[currentCarId].maxSpeed;
                    break;
                case 2:
                    PlayerPersistentData.Instance.CarList[currentCarId].resistance++;
                    //Debug.Log(currentCarId + ": resistance: " + PlayerPersistentData.Instance.CarList[currentCarId].resistance);
                    upgradeStr = "_resistance_ " + PlayerPersistentData.Instance.CarList[currentCarId].resistance;
                    upgradeType = "resistance";
                    upgradeLevel = PlayerPersistentData.Instance.CarList[currentCarId].resistance;
                    break;
                case 3:
                    PlayerPersistentData.Instance.CarList[currentCarId].turboSpeed++;
                    //Debug.Log(currentCarId + ": turbo speed: " + PlayerPersistentData.Instance.CarList[currentCarId].turboSpeed);
                    upgradeStr = "_turboSpeed_" + PlayerPersistentData.Instance.CarList[currentCarId].turboSpeed;
                    upgradeType = "turboSpeed";
                    upgradeLevel = PlayerPersistentData.Instance.CarList[currentCarId].turboSpeed;
                    break;
            }

            //check for all max out
            int maxOutStats = 0;
            int statsNumber = 4;
            for (int i = 0; i < statsNumber; ++i)
            {
                PlayerGameData gameData = currentCarsList[categorySelectedCar[currentSetIndex]].GetComponent<PlayerGameData>();
                switch (i)
                {
                    case 0:
                        if (PlayerPersistentData.Instance.CarList[currentCarId].acceleration == gameData.maxAcceleration)
                            ++maxOutStats;
                        break;
                    case 1:
                        if (PlayerPersistentData.Instance.CarList[currentCarId].maxSpeed == gameData.maxMaxSpeed)
                            ++maxOutStats;
                        break;
                    case 2:
                        if (PlayerPersistentData.Instance.CarList[currentCarId].resistance == gameData.maxResistance)
                            ++maxOutStats;
                        break;
                    case 3:
                        if (PlayerPersistentData.Instance.CarList[currentCarId].turboSpeed == gameData.maxTurboSpeed)
                            ++maxOutStats;
                        break;
                }
            }
            if (maxOutStats == statsNumber)
            {
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.MAX_OUT_CAR);
            }

            if (OnTheRunOmniataManager.Instance != null)
            {
                OnTheRunOmniataManager.Instance.TrackVirtualPurchase(currentCarId.ToString().ToLowerInvariant() + upgradeStr, PriceData.CurrencyType.FirstCurrency, upgradeCost.ToString(), OmniataIds.Product_Type_Upgrade);

                PlayerGameData gameData = currentCarsList[categorySelectedCar[currentSetIndex]].GetComponent<PlayerGameData>();
                OnTheRunOmniataManager.Instance.TrackCarUpdate(OnTheRunDataLoader.Instance.GetLocaleString(gameData.carName.ToString()), "car", upgradeType, upgradeLevel, currentSetIndex);

            }
        }
        else
        {
            Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Money);
        }

        PlayerPersistentData.Instance.Save();
        FillCarInfo();
    }

    /*void Signal_TempResetButtonRelease(UIButton button)
    {
        PlayerGameData gameData = currentCarsList[categorySelectedCar[currentSetIndex]].GetComponent<PlayerGameData>();
        OnTheRunGameplay.CarId currentCarId = currentCarsList[categorySelectedCar[currentSetIndex]].GetComponent<PlayerKinematics>().carId;
        PlayerPersistentData.Instance.CarList[currentCarId].acceleration = gameData.minAcceleration;
        PlayerPersistentData.Instance.CarList[currentCarId].maxSpeed = gameData.minMaxSpeed;
        PlayerPersistentData.Instance.CarList[currentCarId].resistance = gameData.minResistance;
        PlayerPersistentData.Instance.CarList[currentCarId].turboSpeed = gameData.minTurboSpeed;
        PlayerPersistentData.Instance.Save();

        FillCarInfo();
    }*/
    #endregion

    public void FillCarInfo(bool carChanged = false)
    {
        if (OnTheRunUITransitionManager.Instance.isInTransition && carChanged)
            return;

        if (carChanged)
            ClearSteppers();

        int initialIndex = (int)OnTheRunGameplay.CarId.Car_1_europe + currentSetIndex * 5;
        for (int i = 0; i < 5; i++)
        {
            PlayerPersistentData.Instance.GetPlayerData((OnTheRunGameplay.CarId)(initialIndex + i)).UpdateStatus();
        }

        SetupParkingLot();

        PlayerGameData gameData = currentCarsList[categorySelectedCar[currentSetIndex]].GetComponent<PlayerGameData>();
        OnTheRunGameplay.CarId currentCarId = currentCarsList[categorySelectedCar[currentSetIndex]].GetComponent<PlayerKinematics>().carId;

        carStatsGroup.FindChild("tfCarName").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString(gameData.carName.ToString());//gameData.carName.ToString();

        PlayerPersistentData.PlayerData playerData = PlayerPersistentData.Instance.GetPlayerData(currentCarId);
        bool carLocked = playerData.locked && !playerData.owned;
        //bool carLockedByDaily = playerData.lockedByDaily;
        bool carLockedByLevel = playerData.unlockAtLevel > PlayerPersistentData.Instance.Level && !playerData.owned;
        bool carCanBeBought = playerData.canBeBought && !playerData.owned;
        
#if UNITY_WEBPLAYER//dani
        //carLockedByDaily = false;
#endif
        bool carOwned = playerData.owned;
        bool carRented = false; //PlayerPersistentData.Instance.IsCarRented(currentCarId);
        /*UITextField carDescription = transform.FindChild("AnchorCenter/Locations/tfCarDescription").GetComponent<UITextField>();
        if(carLocked)
            carDescription.text = "REACH LEVEL " + playerData.unlockAtLevel + " TO UNLOCK";
        else if (carOwned)
            carDescription.text = "SELECTED";
        else if (carRented)
            carDescription.text = "";//"RENTED"
        else
            carDescription.text = "";
        //carDescription.text = carLocked ? "REACH LEVEL " + playerData.unlockAtLevel + " TO UNLOCK" : ((carOwned || carRented) ? "SELECTED" : "BUY THE CAR");
        
        carDescription.color = carLocked ? Color.red : Color.green;
        carDescription.ApplyParameters();*/

        purchaseCar.SetActive(false);
        purchaseCarLocked.SetActive(false);
        purchaseCarUnlocked.SetActive(false);

        bool buttonHide = (carLocked || playerData.cost<0) || carOwned;
        ActivatePanel("PurchasePanel", !buttonHide);

        purchaseCar.transform.FindChild("PurchaseButton/tfTextfield").GetComponent<UITextField>().text = Manager<UIRoot>.Get().FormatTextNumber(playerData.cost);
        purchaseCar.transform.FindChild("PurchaseButton/coin").gameObject.SetActive(playerData.currency == PriceData.CurrencyType.FirstCurrency);
        purchaseCar.transform.FindChild("PurchaseButton/diamond").gameObject.SetActive(playerData.currency == PriceData.CurrencyType.SecondCurrency);

		/*
        carStatsGroup.FindChild("purchaseCar/PurchaseButton/tfTextfield").GetComponent<UITextField>().text = Manager<UIRoot>.Get().FormatTextNumber(playerData.cost);
        carStatsGroup.FindChild("purchaseCar/PurchaseButton/coin").gameObject.SetActive(playerData.currency == PriceData.CurrencyType.FirstCurrency);
        carStatsGroup.FindChild("purchaseCar/PurchaseButton/diamond").gameObject.SetActive(playerData.currency == PriceData.CurrencyType.SecondCurrency);
*/

        /*bool buttonRentHide = carLocked || carOwned || carRented;
        ActivatePanel("RentPanel", !buttonRentHide);
        transform.FindChild("AnchorCenter/RentButton/tfPrice").GetComponent<UITextField>().text = playerData.rentCost.ToString();
        transform.FindChild("AnchorCenter/RentButton/coin").gameObject.SetActive(playerData.currency == PriceData.CurrencyType.FirstCurrency);
        transform.FindChild("AnchorCenter/RentButton/diamond").gameObject.SetActive(playerData.currency == PriceData.CurrencyType.SecondCurrency);*/

        carRentTimer.SetActive(carRented ? true : false);

        //uiPlayButton.State = ((carLocked || !carOwned || carLockedByLevel) && !carRented) ? UIButton.StateType.Disabled : UIButton.StateType.Normal;

        //Debug.Log("carLocked " + carLocked + " carOwned " + carOwned + " carLockedByLevel " + carLockedByLevel);

        ActivatePanel("InfoPanel", true);

        float nextUpgradeCost = OnTheRunEconomy.Instance.GetCarUpgradeCost(playerData, "acceleration");
        infoBars[0].FindChild("tfDescription").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("acceleration");
        FillInfoBar(infoBars[0], PlayerPersistentData.Instance.CarList[currentCarId].acceleration, gameData.minAcceleration, gameData.maxAcceleration, carOwned, carLockedByLevel, false, nextUpgradeCost);

        nextUpgradeCost = OnTheRunEconomy.Instance.GetCarUpgradeCost(playerData, "maxSpeed");
        infoBars[1].FindChild("tfDescription").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("max_speed");
        FillInfoBar(infoBars[1], PlayerPersistentData.Instance.CarList[currentCarId].maxSpeed, gameData.minMaxSpeed, gameData.maxMaxSpeed, carOwned, carLockedByLevel, false, nextUpgradeCost);

        nextUpgradeCost = OnTheRunEconomy.Instance.GetCarUpgradeCost(playerData, "resistance");
        FillInfoBar(infoBars[2], PlayerPersistentData.Instance.CarList[currentCarId].resistance, gameData.minResistance, gameData.maxResistance, carOwned, carLockedByLevel, false, nextUpgradeCost);
        infoBars[2].FindChild("tfDescription").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("resistance");

        nextUpgradeCost = OnTheRunEconomy.Instance.GetCarUpgradeCost(playerData, "turboSpeed");
        infoBars[3].FindChild("tfDescription").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("turbo_speed");
        FillInfoBar(infoBars[3], PlayerPersistentData.Instance.CarList[currentCarId].turboSpeed, gameData.minTurboSpeed, gameData.maxTurboSpeed, carOwned, carLockedByLevel, false, nextUpgradeCost);

        Manager<UIRoot>.Get().UpdateCurrenciesItem();

        carStatsGroup.gameObject.SetActive(false);
        carStatsGroup.gameObject.SetActive(true);

        //last car---------------------
        /*OnTheRunGameplay.CarId lastTierCarId = currentCarsList[currentCarsList.Length-1].GetComponent<PlayerKinematics>().carId;
        PlayerPersistentData.PlayerData lastTierCarData = PlayerPersistentData.Instance.GetPlayerData(lastTierCarId);
        bool lastCarAlreadyUnlocked = lastTierCarData.locked || lastTierCarData.lockedByDaily;
        */

        //Debug.Log("*** categorySelectedCar[currentSetIndex] " + categorySelectedCar[currentSetIndex] + " carLockedByLevel " + carLockedByLevel + " carCanBeBought " + carCanBeBought);

        ActivatePanel("LockedPanel", false);
//#if !UNITY_WEBPLAYER
        /*if (categorySelectedCar[currentSetIndex] == 4)
        {
            if (CheckForLastCarUnlocked(currentCarsList) < 4)
            {
                ActivatePanel("LockedPanel", true);

                //lockedPanelText.text = OnTheRunDataLoader.Instance.GetLocaleString("unlock_car5");
                //                ActivatePanel("InfoPanel", false);
                ActivatePanel("PurchasePanel", false);
                //ActivatePanel("RentPanel", false);
            }
        }
        else */if (categorySelectedCar[currentSetIndex] == 2)
        {
            if (carLockedByLevel)
            {
                //CAR LOCKED....
                ActivatePanel("LockedPanel", true);
                //lockedPanelText.text = OnTheRunDataLoader.Instance.GetLocaleString("unlock_daily_car");
                ActivatePanel("PurchasePanel", true);

                purchaseCar.SetActive(false);

                purchaseCarLocked.transform.FindChild("PurchaseButton/tfTextfield").GetComponent<UITextField>().text = Manager<UIRoot>.Get().FormatTextNumber(playerData.cost);
                purchaseCarLocked.transform.FindChild("PurchaseButton/coin").gameObject.SetActive(playerData.currency == PriceData.CurrencyType.FirstCurrency);
                purchaseCarLocked.transform.FindChild("PurchaseButton/diamond").gameObject.SetActive(playerData.currency == PriceData.CurrencyType.SecondCurrency);
				purchaseCarLocked.transform.FindChild("Star/tfLevelValue").GetComponent<UITextField>().text = playerData.unlockAtLevel.ToString();

				purchaseCarLocked.transform.FindChild("PurchaseButtonCoins/tfTextfield").GetComponent<UITextField>().text = Manager<UIRoot>.Get().FormatTextNumber(playerData.alternativeCost);

                purchaseCarLocked.SetActive(true);

#if UNITY_WEBPLAYER
                Transform PurchaseButtonCoinsTransform = purchaseCarLocked.transform.FindChild("PurchaseButtonCoins");
                Transform orLabelTransform = purchaseCarLocked.transform.FindChild("tfBuyLabel");
                Transform PurchaseButtonTransform = purchaseCarLocked.transform.FindChild("PurchaseButton");
                Transform lockedAnimTransform = lockedPanel.transform.FindChild("LockAnim");
                Transform starTransform = purchaseCarLocked.transform.FindChild("Star");
                Vector3 lockedAnimOffset = lockedAnimTransform.position - PurchaseButtonCoinsTransform.position;
                Vector3 starOffset = starTransform.position - PurchaseButtonCoinsTransform.position;
                PurchaseButtonCoinsTransform.transform.localPosition = orLabelTransform.localPosition + new Vector3(0.2f, 0.12f, 0.0f);
                lockedAnimTransform.position = PurchaseButtonCoinsTransform.position + lockedAnimOffset;
                starTransform.position = PurchaseButtonCoinsTransform.position + starOffset;
                PurchaseButtonTransform.gameObject.SetActive(false);
                orLabelTransform.gameObject.SetActive(false);
#endif
            }
            else if(carCanBeBought)
            {
                //CAR UNLOCKED BUT STILL TO BUY....
                purchaseCar.SetActive(false);
                purchaseCarUnlocked.SetActive(true);

                purchaseCarUnlocked.transform.FindChild("PurchaseButton/tfTextfield").GetComponent<UITextField>().text = Manager<UIRoot>.Get().FormatTextNumber(playerData.alternativeCost);
                purchaseCarUnlocked.transform.FindChild("PurchaseButton/coin").gameObject.SetActive(playerData.alternativeCurrency == PriceData.CurrencyType.FirstCurrency);
                purchaseCarUnlocked.transform.FindChild("PurchaseButton/diamond").gameObject.SetActive(playerData.alternativeCurrency == PriceData.CurrencyType.SecondCurrency);

                purchaseCarUnlocked.transform.FindChild("PurchaseButton2/tfTextfield").GetComponent<UITextField>().text = Manager<UIRoot>.Get().FormatTextNumber(playerData.cost);
                purchaseCarUnlocked.transform.FindChild("PurchaseButton2/coin").gameObject.SetActive(playerData.currency == PriceData.CurrencyType.FirstCurrency);
                purchaseCarUnlocked.transform.FindChild("PurchaseButton2/diamond").gameObject.SetActive(playerData.currency == PriceData.CurrencyType.SecondCurrency);

#if UNITY_WEBPLAYER
                Transform PurchaseButtonCoinsTransform = purchaseCarUnlocked.transform.FindChild("PurchaseButton2");
                Transform orLabelTransform = purchaseCarUnlocked.transform.FindChild("tfOrLabel");
                Transform PurchaseButtonTransform = purchaseCarUnlocked.transform.FindChild("PurchaseButton");
                PurchaseButtonCoinsTransform.transform.localPosition = orLabelTransform.localPosition + new Vector3(0.43f, 0.075f, 0.0f);
                PurchaseButtonTransform.gameObject.SetActive(false);
                orLabelTransform.gameObject.SetActive(false);
#endif
            }
        }
//#endif
        //parking lot locked-------------------
        comingSoonPanel.SetActive(false);
        parkingLotLocker.SetActive(false);
        parkingLotLockerWeb.SetActive(false);
        parkingLotLockerXP.SetActive(false);
        parkingLotLockedButton.SetActive(true);
        parkingLotLockedButton.SetActive(false);
        Manager<UIRoot>.Get().ShowArrowsButtons(true);
        //transform.FindChild("Toggle").gameObject.SetActive(true);
        if (PlayerPersistentData.Instance.IsParkingLotLocked(currentSetIndex))
        {
            /*uiPlayButton.State = UIButton.StateType.Disabled;
            playArrowEnable.SetActive(uiPlayButton.State == UIButton.StateType.Disabled ? false : true);
            playArrowDisable.SetActive(uiPlayButton.State == UIButton.StateType.Disabled ? true : false);
            */

            if (comingSoonActive[currentSetIndex])
                comingSoonPanel.SetActive(true);
            else
            {
#if UNITY_WEBPLAYER
                //parkingLotLockerWeb.transform.FindChild("tfGarageTitle").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("unlock_tier_prefix") + " " + OnTheRunDataLoader.Instance.GetTiersPrices()[currentSetIndex] + " " + OnTheRunDataLoader.Instance.GetLocaleString("unlock_tier_postfix");
                //parkingLotLockerWeb.SetActive(true);

                if (IsTierBlockedByXP(currentSetIndex) && !CheckTierLockedByXP(currentSetIndex))
                {
                    parkingLotLockerWeb.transform.FindChild("tfXPTitle").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("unlock_tier_prefix");
                    parkingLotLockerWeb.transform.FindChild("tfXP").GetComponent<UITextField>().text = playerLevelToUnlockTiers[currentSetIndex].ToString(); //OnTheRunDataLoader.Instance.GetLocaleString("level") + " " + 
                    parkingLotLockerWeb.transform.FindChild("tfXPText").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("unlock_tier_postfix");

                    parkingLotLockerWeb.SetActive(true);
                }
                else
                    parkingLotLockerWeb.SetActive(true);
#else
                if (IsTierBlockedByXP(currentSetIndex) && !CheckTierLockedByXP(currentSetIndex))
                {
                    parkingLotLockerXP.transform.FindChild("tfXPTitle").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("unlock_tier_prefix");
                    parkingLotLockerXP.transform.FindChild("tfXP").GetComponent<UITextField>().text = playerLevelToUnlockTiers[currentSetIndex].ToString(); //OnTheRunDataLoader.Instance.GetLocaleString("level") + " " +
                    parkingLotLockerXP.transform.FindChild("tfXPText").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("unlock_tier_postfix");
                    parkingLotLockerXP.transform.FindChild("tfBuyFor").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("or_buy_for");

                    float buyForCost = tiersBuyFor[currentSetIndex];
                    if (buyForCost > 0.0f)
                    {
                        parkingLotBuyForButton.SetActive(true);
                        parkingLotBuyForButton.transform.FindChild("coin").gameObject.SetActive(false);
                        parkingLotBuyForButton.transform.FindChild("diamond").gameObject.SetActive(true);
                        parkingLotBuyForButton.transform.FindChild("tfTextfield").GetComponent<UITextField>().text = buyForCost.ToString();
                    }

                    parkingLotLockerXP.SetActive(true);
                }
                else
                    parkingLotLocker.SetActive(true);
#endif
            }

            carStatsGroup.gameObject.SetActive(false);

            if (currentSetIndex > 0)
            {
                Manager<UIRoot>.Get().ShowArrowsButtons(false);
                if (parkingLotLocker.activeInHierarchy)
                {
                    parkingLotLockedButton.SetActive(true);
                    PriceData.CurrencyType parkingLotCurrency = OnTheRunDataLoader.Instance.GetTiersCurrencies()[currentSetIndex];
                    parkingLotLockedButton.transform.FindChild("coin").gameObject.SetActive(parkingLotCurrency == PriceData.CurrencyType.FirstCurrency);
                    parkingLotLockedButton.transform.FindChild("diamond").gameObject.SetActive(parkingLotCurrency == PriceData.CurrencyType.SecondCurrency);
                    parkingLotLockedButton.GetComponentInChildren<UITextField>().text = "" + Manager<UIRoot>.Get().FormatTextNumber(PlayerPersistentData.Instance.GetParkingLotCost(currentSetIndex));
                }
            }
            else
            {
                parkingLotLockedButton.SetActive(false);
            }

            ActivatePanel("LockedPanel", false);
            ActivatePanel("InfoPanel", false);
            ActivatePanel("PurchasePanel", false);
            //ActivatePanel("RentPanel", false);
        }

        Transform currTr;
        GameObject currIcon;
        for (int i = 0; i < 4; ++i)
        {
            currTr = transform.FindChild("AnchorCenter/Locations/locations_container").GetChild(i);
            currIcon = currTr.gameObject;
            currIcon.transform.localScale = Vector3.one * 0.7f;
            currIcon.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 0.5f);

            //Per PIETRO: hidare lucchetto e stellina nel garage
            bool hideLock = !PlayerPersistentData.Instance.IsParkingLotLocked(i);
            Transform lockGroup = currTr.FindChild("lockGroup");
            if (hideLock)
                lockGroup.gameObject.SetActive(false);
            else
                lockGroup.FindChild("levelToUnlock").GetComponent<UITextField>().text = playerLevelToUnlockTiers[i].ToString();
        }

        currIcon = transform.FindChild("AnchorCenter/Locations/locations_container").GetChild(currentSetIndex).gameObject;
        currIcon.transform.localScale = Vector3.one * 0.9f;
        currIcon.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        if (!carOwned && purchaseCar.activeInHierarchy) //carStatsGroup.FindChild("purchaseCar").gameObject.activeInHierarchy)
            uiPlayButton.State = UIButton.StateType.Normal;
        playArrowEnable.SetActive(uiPlayButton.State == UIButton.StateType.Disabled ? false : true);
        playArrowDisable.SetActive(uiPlayButton.State == UIButton.StateType.Disabled ? true : false);

#if !UNITY_WEBPLAYER
        string currencyStr = OnTheRunDataLoader.Instance.GetLocaleString("diamonds");
        if (playerData.locked)
        {
            unlockCarText.text = OnTheRunDataLoader.Instance.GetLocaleString("buy_for") + " " +  playerData.cost.ToString() + " " + currencyStr +" "+ OnTheRunDataLoader.Instance.GetLocaleString("or") +" "+ OnTheRunDataLoader.Instance.GetLocaleString("unlock_specials_prefix") +" "+ playerData.unlockAtLevel + " " + OnTheRunDataLoader.Instance.GetLocaleString("buy_with_coins_postfix");
        }
        else
        {
            buyCarText.text = OnTheRunDataLoader.Instance.GetLocaleString("buy_car");
        }
#else
        string currencyStr = OnTheRunDataLoader.Instance.GetLocaleString("coins");
        if (playerData.locked)
        {
            unlockCarText.text = OnTheRunDataLoader.Instance.GetLocaleString("unlock_specials_prefix") + " " + playerData.unlockAtLevel + " " + OnTheRunDataLoader.Instance.GetLocaleString("buy_with_coins_postfix");
        }
        else
        {
            buyCarText.text = OnTheRunDataLoader.Instance.GetLocaleString("buy_car");
        }
#endif
        buyCarText.gameObject.SetActive(!playerData.locked && !playerData.owned);
        unlockCarText.gameObject.SetActive(playerData.locked && !playerData.owned);
    }

    protected int CheckForLastCarUnlocked(GameObject[] carsList)
    {
#if UNITY_WEBPLAYER
        return 5;
#else
        int howManyBought = 0;
        for (int i = 0; i < carsList.Length; ++i)
        {
            OnTheRunGameplay.CarId currentCarId = carsList[i].GetComponent<PlayerKinematics>().carId;
            PlayerPersistentData.PlayerData playerData = PlayerPersistentData.Instance.GetPlayerData(currentCarId);
            if (playerData.owned && !playerData.lockedByDaily)
                howManyBought++;
        }

        return howManyBought;
#endif
    }

	public GameObject BarElementAnimPrefab;

	List<GameObject> m_kElementsEffects = new List<GameObject>();

	void PlayBarElementEffect( Vector3 kPos )
	{
		OnTheRunInterfaceSounds.Instance.PlayUnlockSound();

		GameObject pEffect = Instantiate( BarElementAnimPrefab , kPos , Quaternion.identity ) as GameObject;
		m_kElementsEffects.Add( pEffect );
		Destroy( pEffect , 1f );
	}

	OnTheRunGameplay.CarId m_eOldCarID = OnTheRunGameplay.CarId.None;

	public void ClearSteppers()
	{
		// destroy all the elements when changin car
		for( int i = 0; i< m_kElementsEffects.Count ; ++i )
		{
			if( m_kElementsEffects[i] != null )
				Destroy( m_kElementsEffects[i] );
		}
	}

	void DestroyBarElements()
	{
		OnTheRunGameplay.CarId currentCarId = currentCarsList[categorySelectedCar[currentSetIndex]].GetComponent<PlayerKinematics>().carId;
		
		// we changed a car
		if( m_eOldCarID != OnTheRunGameplay.CarId.None && m_eOldCarID != currentCarId  )
		{
			// destroy all the elements when changin car
			for( int i = 0; i< m_kElementsEffects.Count ; ++i )
			{
				if( m_kElementsEffects[i] == null )
				{
					m_kElementsEffects.RemoveAt(i);
					continue;
				}
				
				Destroy( m_kElementsEffects[i] );
			}
		}

		m_eOldCarID = currentCarId;
	}

    public void FillInfoBar(Transform parent, int value, int minValue, int maxValue, bool carOwned, bool carLockedByDaily, bool forceEmpty = false, float upgradeCost = 0)
    {
		DestroyBarElements();
	
        GameObject infoBar = parent.FindChild("Bar").gameObject;
        GameObject upgradeButton = parent.FindChild("GreenButtonSquare").gameObject;
        GameObject maxedTextfield = parent.FindChild("tfMaxed").gameObject;
        maxedTextfield.GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("maxed");
        upgradeButton.SetActive(false);
        upgradeButton.SetActive(true);
        maxedTextfield.SetActive(false);
        maxedTextfield.SetActive(true);
        maxedTextfield.SetActive(false);

        for (int i = 0; i < minValue; i++)
        {
            Transform currElement = infoBar.transform.FindChild("barElement" + (i + 1));
            currElement.FindChild("empty").gameObject.SetActive(false);
            currElement.FindChild("full").gameObject.SetActive(true);
        }

        for (int i = minValue; i < maxValue; i++)
        {
            bool isEmpty = true;
            Transform currElement = infoBar.transform.FindChild("barElement" + (i + 1));

            if ((i + 1) <= value)
                isEmpty = false;

			Transform pEmpty = currElement.FindChild("empty");

			pEmpty.gameObject.SetActive(isEmpty || forceEmpty);
            currElement.FindChild("full").gameObject.SetActive(!isEmpty && !forceEmpty);
        }

        for (int i = maxValue; i < infoBar.transform.childCount; i++)
        {
            Transform currElement = infoBar.transform.FindChild("barElement" + (i + 1));
            currElement.FindChild("empty").gameObject.SetActive(false);
            currElement.FindChild("full").gameObject.SetActive(false);
        }

        //upgradeButton.transform.FindChild("coin").gameObject.SetActive(true);
        //upgradeButton.transform.FindChild("diamond").gameObject.SetActive(false);

        if (infoBar.transform.childCount <= value || value >= maxValue || !carOwned || carLockedByDaily)
        {
            upgradeButton.transform.FindChild("tfPrice").GetComponent<UITextField>().text = Manager<UIRoot>.Get().FormatTextNumber((int)upgradeCost);
            upgradeButton.transform.FindChild("tfPrice").GetComponent<UITextField>().ApplyParameters();
            upgradeButton.GetComponent<UIButton>().State = UIButton.StateType.Disabled;
            upgradeButton.GetComponent<UIButtonsColorModified>().SendMessage("UIButtonTextColor_OnDisable", upgradeButton.GetComponent<UIButton>());
            if (value >= maxValue)
            {
                upgradeButton.SetActive(false);
                maxedTextfield.SetActive(true);
            }
        }
        else
        {
            upgradeButton.transform.FindChild("tfPrice").GetComponent<UITextField>().text = Manager<UIRoot>.Get().FormatTextNumber((int)upgradeCost);
            upgradeButton.transform.FindChild("tfPrice").GetComponent<UITextField>().ApplyParameters();
            upgradeButton.GetComponent<UIButton>().State = UIButton.StateType.Normal;
            upgradeButton.GetComponent<UIButtonsColorModified>().SendMessage("UIButtonTextColor_OnEnable", upgradeButton.GetComponent<UIButton>(), SendMessageOptions.DontRequireReceiver);
        }
    }

    public IEnumerator RefreshRentCar(float seconds)
    {
        while (true)
        {
            yield return new WaitForSeconds(seconds);
            //RefreshTimeData();
        }
    }

    protected float[] playerLevelToUnlockTiers;// = { 0, 2000, 0, 0 };
    protected float[] tiersBuyFor;
    public bool IsTierBlockedByXP(int tierIndex)
    {
        return playerLevelToUnlockTiers[tierIndex] > 0;
    }

    public bool CheckTierLockedByXP(int tierIndex)
    {
        return playerLevelToUnlockTiers[tierIndex] <= PlayerPersistentData.Instance.Level;
    }
    
    public void UnlockCarsByXP()
    {
        GameObject[] currCarList = {};
        for (int j = 0; j < 4; ++j)
        {
            switch(j)
            {
                case 0:
                    currCarList = Cars_EU;
                    break;
                case 1:
                    currCarList = Cars_NY;
                    break;
                case 2:
                    currCarList = Cars_JP;
                    break;
                case 3:
                    currCarList = Cars_USA;
                    break;
            }

            for (int i = 0; i < categorySelectedCar.Length; ++i)
            {
                PlayerGameData gameData = currCarList[i].GetComponent<PlayerGameData>();
                OnTheRunGameplay.CarId currentCarId = gameData.GetComponent<PlayerKinematics>().carId;
                PlayerPersistentData.PlayerData playerData = PlayerPersistentData.Instance.GetPlayerData(currentCarId);
                if(playerData.locked && playerData.unlockAtLevel<=PlayerPersistentData.Instance.Level)
                {
                    showCarUnlockedPopup = true;
                    playerData.canBeBought = true;
                    currentSetIndex = j;
                    categorySelectedCar[currentSetIndex] = i;

                    if (OnTheRunOmniataManager.Instance != null)
                        OnTheRunOmniataManager.Instance.TrackVirtualPurchase(playerData.carId.ToString().ToLowerInvariant(), PriceData.CurrencyType.SecondCurrency, "0", OmniataIds.Product_Type_Unlock);

                    break;
                }
            }
        }
    }

    public void UnlockTierByXP()
    {
        for (int i = 0; i < playerLevelToUnlockTiers.Length; ++i)
        {
            if (IsTierBlockedByXP(i) && PlayerPersistentData.Instance.IsParkingLotLocked(i))
            {
                if (CheckTierLockedByXP(i))
                {
                    PlayerPersistentData.Instance.UnlockParkingLot(i);
                    currentSetIndex = i;
                    ShowParkingLotPopup = true;
                    SetupParkingLot();
                    Manager<UIRoot>.Get().SetupBackground(gameplayManager.EnvironmentFromIdx(currentSetIndex));

                    if (OnTheRunOmniataManager.Instance != null)
                        OnTheRunOmniataManager.Instance.TrackVirtualPurchase(OmniataIds.Product_ParkingLot[i]/*OmniataIds.Product_ParkingLot + "_" + garageTitles[currentSetIndex]*/, PriceData.CurrencyType.SecondCurrency, "0", OmniataIds.Product_Type_Unlock);

                    Transform container = transform.FindChild("AnchorCenter/Locations/locations_container");
                    container.GetComponent<UIToggleButton>().SetActiveButton(container.GetChild(i).GetComponent<UIButton>());
                    ShowCars();
                }
            }
        }
    }

    void OnSpacePressed()
    {
        if (uiPlayButton.State != UIButton.StateType.Disabled)
            Signal_OnPlayRelease(null);
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
        if (!uiManager.IsPopupInStack("LoadingPopup"))
        {
            uiManager.PushPopup("LoadingPopup");
            if (uiManager.FrontPopup != null)
                uiManager.FrontPopup.GetComponent<UILoadingPopup>().SetText("");//OnTheRunDataLoader.Instance.GetLocaleString("loading"));
        }
    }

    public void HideLoadingPopup()
    {
        if (uiManager.IsPopupInStack("LoadingPopup"))
            uiManager.RemovePopupFromStack("LoadingPopup");
    }

    void OnApplicationPause(bool paused)
    {
        if (!paused && gameObject.activeInHierarchy && gameplayManager.checkForDailyBonusAfterPause)
            Manager<UIRoot>.Get().ShowDailyBonusPopup();
    }


    void Signal_OnCheatXPRelease(UIButton button)
    {
        int poinToNextLevel = (int)(PlayerPersistentData.Instance.NextExperienceLevelThreshold * 0.33f);
        Manager<UIRoot>.Get().UpdateExperienceBarImmediatly(poinToNextLevel);
    }
}