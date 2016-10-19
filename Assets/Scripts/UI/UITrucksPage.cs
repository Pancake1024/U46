using UnityEngine;
using SBS.Core;
using System.Collections.Generic;
using System;
using System.Collections;

[AddComponentMenu("OnTheRun/UI/UITrucksPage")]
public class UITrucksPage : MonoBehaviour
{
    public GameObject[] specialCarList;
    public GameObject firemenLightsRef;
    public GameObject firemenLightsRefGrey;
    public GameObject lockItem;
    public GameObject buyButtons;
    public GameObject buyAlternative;
    public GameObject buyNormal;

	public UITextField OrTextBuyButtons;
	public UITextField OrText;

	public GameObject BuyPanelLocked;

	public GameObject LockAnimationBump;
	public GameObject LockAnimationUnlock;

	bool lockAnimPlaying = false;

    protected Vector3[] vehiclesPositions;

    protected GameObject uiButtonsBar;
    protected Camera uiCamera;
    protected OnTheRunGameplay gameplayManager;
    protected OnTheRunInterfaceSounds interfaceSounds;
    protected OnTheRunEnvironment environmentManager;
    protected GameObject cameraManager;
    protected GameObject[] currentCarsList;
    protected int currentSpecialCarSelected = 0;
    protected GameObject enteringCarGO;
    protected GameObject idleCarGO;
    protected GameObject exitingCarGO;
    protected int movementDirection = 1;
    protected int maxCarSetsNumber = 4;
    //protected int carSelected = 0;
    protected GameObject selectedCar;
    protected GameObject baseParkingLotCar;

    protected float parkingAreaWidth;
    protected float carSpeed;
    //protected string garageTitle = "SPECIAL VEHICLES";

    //protected UIToggleButton uiToggle;
    protected float backupFOV;
    UITextField tfGarageGenericDescr;

    protected GameObject carRentTimer;

    protected UIScroller vehiclesScroller;
    protected GameObject panel;
    protected float invisibleButtonsTimer = -1.0f;

    //protected int nextTimeToShowTruckAdvisePopup = 0;

    private UIManager uiManager;

    #region Unity Callbacks
    void Awake()
    {
		LockAnimationUnlock.SetActive( false );

        //elementsToFade = new List<List<Transform>>();
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        cameraManager = GameObject.FindGameObjectWithTag("MainCamera");
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        environmentManager = gameplayManager.GetComponent<OnTheRunEnvironment>();
        tfGarageGenericDescr = transform.FindChild("AnchorBottomRight/tfGarageTitle").GetComponent<UITextField>();
        //tfGarageTitle.text = OnTheRunDataLoader.Instance.GetLocaleString("special_generic_descr");
        currentCarsList = specialCarList;
        enteringCarGO = new GameObject();
        enteringCarGO.name = "EnteringCarsGroup";
        idleCarGO = new GameObject();
        idleCarGO.name = "IdleCarsGroup";
        exitingCarGO = new GameObject();
        exitingCarGO.name = "ExitingCarsGroup";

        parkingAreaWidth = 20.0f;
        carSpeed = 50.0f;

        panel = transform.FindChild("LeftCenterAnchor/InfoPanel/BuyPopup").gameObject;

        //uiToggle = gameObject.transform.FindChild("Toggle").GetComponent<UIToggleButton>();

        vehiclesScroller = transform.FindChild("AnchorCenter/VehiclesScroller").GetComponent<UIScroller>();
        vehiclesScroller.maxSpeed = 20.0f;

        uiManager = Manager<UIManager>.Get();

        transform.FindChild("InvisibleButtonRightWeb").gameObject.SetActive(false);
        transform.FindChild("InvisibleButtonLeftWeb").gameObject.SetActive(false);

#if UNITY_WEBPLAYER
        transform.FindChild("InvisibleButtonRight").gameObject.SetActive(false);
        transform.FindChild("LeftCenterAnchor/ButtonLeft").gameObject.SetActive(false);
        transform.FindChild("InvisibleButtonRightWeb").gameObject.SetActive(true);
        transform.FindChild("InvisibleButtonLeftWeb").gameObject.SetActive(true);
#endif
    }

    void DeactivateWebArrows()
    {
        transform.FindChild("InvisibleButtonRightWeb").gameObject.SetActive(false);
        transform.FindChild("InvisibleButtonLeftWeb").gameObject.SetActive(false);
    }

    float minScale = 0.65f,
          maxScale = 0.9f;
    void UpdateVehiclesPositionScale()
    {
        for (int i = 0; i < scrollerCarsList.Length; i++)
        {
            float currScreenXPos = gameplayManager.MainCamera.WorldToViewportPoint(scrollerCarsList[i].transform.position + Vector3.left * 3.0f).x;
            currScreenXPos = maxScale - Mathf.Abs(currScreenXPos - 0.5f) * 1.2f;
            currScreenXPos = Mathf.Clamp(currScreenXPos, 0.0f, maxScale - 0.05f);
            float currScale = Mathf.Max(minScale, currScreenXPos);
            scrollerCarsList[i].transform.localScale = Vector3.one * currScale;
        }
    }
    /*
    void UpdateVehicles2DElementsPosition()
    {
        if (vehiclesScroller.NumberOfElements < 0)
            return;

        if (vehiclesPositions == null || tempObj2D == null || scrollerCarsList == null)
            return;

        for (int i = 0; i < scrollerCarsList.Length; i++)
        {
            if (tempObj2D[i] != null)
            {
                if (scrollerCarsList[i].transform.position != tempObj2D[i].transform.position)
                {
                    Vector3 screenPos = gameplayManager.MainCamera.WorldToViewportPoint(scrollerCarsList[i].transform.position);
                    screenPos.z = 0.0f;
                    screenPos = Manager<UIManager>.Get().UICamera.ViewportToWorldPoint(screenPos);
                    tempObj2D[i].transform.position = screenPos + Vector3.up * 2.3f;
                }
            }
        }
    }

    void UpdateVehicles2DElementsAlpha()
    {
        for (int i = 0; i < tempObj2D.Length; ++i)
        {
            float alphaValue = (2.5f - Mathf.Abs(tempObj2D[i].transform.position.x)) / 2.0f;
            alphaValue = Mathf.Clamp(alphaValue, 0.0f, 1.0f);
            foreach (Transform child in elementsToFade[i])
            {
                if (!child.gameObject.activeInHierarchy)
                    continue;

                SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alphaValue);
                    continue;
                }

                UINineSlice ns = child.GetComponent<UINineSlice>();
                if (ns != null)
                {
                    ns.color.a = alphaValue;
                    ns.ApplyParameters();
                    continue;
                }

                UITextField tx = child.GetComponent<UITextField>();
                if (tx != null)
                {
                    tx.color.a = alphaValue;
                    tx.ApplyParameters();
                    continue;
                }
            }
        }
    }
    */

    int oldScrollIndex = 0;
    int nextSnapIndex = -999;
    float currentSnapX = -999.0f;
    bool isCurrentCar = true;
    void UpdateCarScroller()
    {
        if (vehiclesScroller == null)
            return;

        int iScrolledIndex = Math.Abs(vehiclesScroller.snapX);

        if (vehiclesScroller.IsDragging)
        {
            if(currentSnapX==-999.0f)
                currentSnapX = (iScrolledIndex * vehiclesScroller.snapSpace.x);

            float diff = vehiclesScroller.Offset.x + iScrolledIndex * vehiclesScroller.snapSpace.x;
            isCurrentCar = (oldScrollIndex * vehiclesScroller.snapSpace.x) == currentSnapX;
            if (Mathf.Abs(diff) > vehiclesScroller.snapSpace.x * 0.001f)//0.15f
                nextSnapIndex = (int)Mathf.Sign(diff);
        }
        else
        {
            isCurrentCar = (oldScrollIndex * vehiclesScroller.snapSpace.x) == currentSnapX;
            if (nextSnapIndex > -999 && isCurrentCar)
            {
                currentSpecialCarSelected -= nextSnapIndex;
                if (currentSpecialCarSelected < 0)
                    currentSpecialCarSelected = 0;
                else if (currentSpecialCarSelected > 4)
                    currentSpecialCarSelected = 4;

                int iNextSnap = -currentSpecialCarSelected;
                vehiclesScroller.easeToSnapX(iNextSnap);
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

    void Update()
    {
        this.UpdateCarScroller();
        this.UpdateVehiclesPositionScale();
        //this.UpdateVehicles2DElementsPosition();
        //this.UpdateVehicles2DElementsAlpha();

        float dt = TimeManager.Instance.MasterSource.DeltaTime;

        if (invisibleButtonsTimer >= 0.0f)
            invisibleButtonsTimer -= dt;

        //Car entering.....................................
        if (enteringCarGO.transform.childCount > 0)
        {
            float currentEnteringX = enteringCarGO.transform.position.x;
            if (currentEnteringX != 0.0f)
            {
                bool goalPosReached = movementDirection == 1 ? (currentEnteringX < 0.0f) : currentEnteringX > 0.0f;
                if (goalPosReached)
                {
                    enteringCarGO.transform.position += Vector3.right * dt * carSpeed * movementDirection;
                }
                else
                {
                    SetupPanel();
                    enteringCarGO.transform.position = transform.position + Vector3.down + Vector3.back * 2.0f;
                    ChangeCarsParent(enteringCarGO, idleCarGO);
                    idleCarGO.transform.position = Vector3.zero;
                    HideItemsForTransition(false);
                }
            }
            else if (enteringCarGO.transform.childCount>0)
            {
                ChangeCarsParent(enteringCarGO, idleCarGO);
                idleCarGO.transform.position = Vector3.zero;
                HideItemsForTransition(false);
            }
        }

        //Car exiting.....................................
        if (exitingCarGO.transform.childCount > 0)
        {
            exitingCarGO.transform.position += Vector3.right * dt * carSpeed * movementDirection;
            float endHOffset = (currentCarsList.Length + 1) * (parkingAreaWidth / (currentCarsList.Length + 1)) * movementDirection;
            bool goalPosReached = movementDirection == 1 ? (exitingCarGO.transform.position.x > endHOffset) : exitingCarGO.transform.position.x < endHOffset;
            if (goalPosReached)
            {
                DestroyCarsByParentRef(exitingCarGO);
            }
        }
        
    }
    #endregion

    #region Functions

    GameObject[] scrollerCarsList;
    void InitScroller()
    {
        Vector3 carPositionsOffset = Vector3.up * 0.0f + Vector3.back * 2.0f;
        Vector3 carPosition = vehiclesScroller.transform.position + carPositionsOffset;
        //Vector3 carPosition = transform.position + Vector3.down + Vector3.back * 2.0f;
        //float deltaXPosition = parkingAreaWidth / (currentCarsList.Length + 1);

        scrollerCarsList = new GameObject[currentCarsList.Length];
        //tempObj2D = new GameObject[currentCarsList.Length];
        vehiclesPositions = new Vector3[currentCarsList.Length];

        float snapSpace = 5.5f;
        vehiclesScroller.snapSpace = new Vector2(snapSpace, 0);

        vehiclesScroller.BeginAddElements();

        //elementsToFade.Clear();
        for (int i = 0; i < currentCarsList.Length; ++i)
        {
            carPosition = vehiclesScroller.transform.TransformPoint((new Vector3(i * snapSpace, 0.0f, 0.0f)) + carPositionsOffset);

            if (i == 3)
            {
                //Debug.Log(currentCarsList[i].name);
                carPosition += Vector3.left * 1.0f;
            }

            GameObject currentCar = Instantiate(currentCarsList[i], carPosition, Quaternion.identity) as GameObject;
            string normalizedName = i + "_" + currentCar.name.Substring(0, currentCar.name.IndexOf("(") - 1) + "" + (i + 1);
            currentCar.name = normalizedName;
            ActivatePlayer(currentCar, false, i, UnlockingManager.Instance.GetSpecialCarData(OnTheRunGameplay.CarId.Firetruck).locked);
            
            vehiclesPositions[i] = carPosition;

            Quaternion carRotation = Quaternion.Euler(0.0f, 35.0f, 0.0f);
            currentCar.transform.rotation = carRotation;
            currentCar.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

            scrollerCarsList[i] = currentCar;

            vehiclesScroller.AddElement(scrollerCarsList[i].transform);

            Vector3 screenPos = gameplayManager.MainCamera.WorldToViewportPoint(carPosition);
            screenPos.z = 0.0f;
            screenPos = uiManager.UICamera.ViewportToWorldPoint(screenPos);

            /* redz
            GameObject currCarPopup = Instantiate(temp2DPrefab, screenPos, Quaternion.identity) as GameObject;
            currCarPopup.GetComponent<UIBuyPopup>().specialCarIndex = (TruckBehaviour.TrasformType)i;
            currCarPopup.GetComponent<UIBuyPopup>().truckRef = currentCar;
            tempObj2D[i] = currCarPopup;
            tempObj2D[i].transform.position = Vector3.right * 10.0f;

            Transform[] allChildren = currCarPopup.GetComponentsInChildren<Transform>();
            List<Transform> transformList = new List<Transform>();
            foreach (Transform child in allChildren)
            {
                SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                UINineSlice ns = child.GetComponent<UINineSlice>();
                UITextField tx = child.GetComponent<UITextField>();
                
                if (sr != null || ns != null || tx != null)
                    transformList.Add(child);
            }

            elementsToFade.Add(transformList);
            */
        }
        vehiclesScroller.EndAddElements();

        vehiclesScroller.snapStartXInterval = -scrollerCarsList.Length + 1;
        vehiclesScroller.snapEndXInterval = 0;

        vehiclesScroller.snapX = -currentSpecialCarSelected;
    }

    void DestroyScroller()
    {
        for (int i = 0; i < scrollerCarsList.Length; i++)
        {
            DestroyImmediate(scrollerCarsList[i]);
            //DestroyImmediate(tempObj2D[i]); redz
            scrollerCarsList[i] = null;
            //tempObj2D[i] = null; redz
        }

        vehiclesScroller.Offset = Vector2.zero;
        vehiclesScroller.OffsetVelocity = Vector2.zero;
        vehiclesScroller.UpdateScroller(0.0f, false);
    }
    
    void SetupParkingLot()
    {
        currentCarsList = specialCarList; 
    }

    void HideItemsForTransition(bool visible)
    {
    }

    void ShowCars( )
    {
    }
    
    void SelectCar(int iIndex)
    {
		DestroyBarElements();

        int nextIndex = iIndex;
        if (nextIndex < 0) nextIndex = 4;
        else if (nextIndex > 4) nextIndex = 0;
        currentSpecialCarSelected = nextIndex;
        PlayerPersistentData.Instance.SaveSpecialCarSelection(currentSpecialCarSelected);
        SetupPanel();

#if UNITY_WEBPLAYER
        transform.FindChild("InvisibleButtonRightWeb").gameObject.SetActive(currentSpecialCarSelected != 4);
        transform.FindChild("InvisibleButtonLeftWeb").gameObject.SetActive(currentSpecialCarSelected != 0);
#endif
    }

    void ChangeCarsParent(GameObject oldParent, GameObject newParent)
    {
        newParent.transform.position = oldParent.transform.position;
        int numChild = oldParent.transform.childCount;
        for (int i = numChild-1; i >= 0; --i)
        {
            oldParent.transform.GetChild(i).transform.parent = newParent.transform;
        }
    }

    void DestroyCarsByParentRef(GameObject parentRef)
    {
        int numChild = parentRef.transform.childCount;
        for (int i = numChild - 1; i >= 0; --i)
        {
            Destroy(parentRef.transform.GetChild(i).gameObject);
        }
    }

    void SetLayerRecursively( GameObject obj, int newLayer )
    {
        obj.layer = newLayer;
        foreach( Transform child in obj.transform )
        {
            SetLayerRecursively( child.gameObject, newLayer );
        }
    }

    void ActivatePlayer(GameObject playerRef, bool activate, int index = -1, bool locked = true)
    {
        playerRef.GetComponent<BoxCollider>().enabled = activate;
        playerRef.GetComponent<OpponentKinematics>().enabled = activate;
        playerRef.transform.FindChild("Fx_scrape_sparks1").gameObject.SetActive(activate);
        playerRef.transform.FindChild("Fx_scrape_sparks2").gameObject.SetActive(activate);
        playerRef.transform.FindChild("lights_headlights").gameObject.SetActive(activate);

        GameObject shadow = null;
        shadow = playerRef.transform.FindChild("veh_shadow").gameObject;
        shadow.GetComponent<PlayerShadow>().enabled = activate;
        shadow.transform.localPosition = new Vector3(shadow.transform.localPosition.x, shadow.GetComponent<PlayerShadow>().shadowVerticalOffset, shadow.transform.localPosition.z);

        playerRef.BroadcastMessage("InitializeTruckForGarage", index);
        playerRef.GetComponent<TruckBehaviour>().enabled = activate;

        if ((TruckBehaviour.TrasformType)index == TruckBehaviour.TrasformType.Firetruck)
        {
            GameObject lights;
            if (locked)
                lights = Instantiate(firemenLightsRefGrey, Vector3.zero, Quaternion.identity) as GameObject;
            else
                lights = Instantiate(firemenLightsRef, Vector3.zero, Quaternion.identity) as GameObject;
            lights.transform.parent = playerRef.GetComponent<TruckBehaviour>().FakeCarTransform;
            lights.transform.localPosition = Vector3.zero;
            lights.transform.localRotation = Quaternion.identity;
            lights.transform.localScale = Vector3.one;

        }
    }

    void SetupPanel()
    {
        panel.GetComponent<UIBuyPopup>().specialCarIndex = (TruckBehaviour.TrasformType)currentSpecialCarSelected;
        panel.GetComponent<UIBuyPopup>().truckRef = scrollerCarsList[currentSpecialCarSelected]; // currentCar;
        panel.GetComponent<UIBuyPopup>().FillData(true);
        //OnTheRunGameplay.GetCarLocalizedDescrStr((TruckBehaviour.TrasformType)currentSpecialCarSelected);
    }
    #endregion

    #region Messages
    void OnRefreshRentedCars()
    {
        if(gameObject.activeInHierarchy)
        {
            FillCarInfo();
        }
    }
    
    /*void OnLeftArrowReleased()
    {
        if (idleCarGO.transform.childCount == 0)
            return;

        HideItemsForTransition(true);

        --currentSetIndex;
        if (currentSetIndex < 0)
            currentSetIndex = maxCarSetsNumber - 1;

        SetupParkingLot();

        movementDirection = 1;
        ShowCars();
        FillCarInfo();
    }*/
    /*void OnRightArrowReleased()
    {
        if (idleCarGO.transform.childCount == 0)
            return;

        HideItemsForTransition(true);

        ++currentSetIndex;
        if (currentSetIndex > maxCarSetsNumber - 1)
            currentSetIndex = 0;

        SetupParkingLot();

        movementDirection = -1;
        ShowCars();
        FillCarInfo();
    }*/
    #endregion

    #region Signals
    void Signal_OnEnter(UIPage page)
    {
        if(uiButtonsBar==null)
            uiButtonsBar = GameObject.Find("UI").transform.FindChild("ButtonsBar").gameObject;

        tfGarageGenericDescr.text = OnTheRunDataLoader.Instance.GetLocaleString("special_bottom_descr");

       	OrTextBuyButtons.text = OnTheRunDataLoader.Instance.GetLocaleString("or");
        OrText.text = OnTheRunDataLoader.Instance.GetLocaleString("or");

        //CreateGarageCamera();
        Manager<UIRoot>.Get().ShowOffgameBG(true);
        PlayerPersistentData.Instance.LoadSpecialCarSelection();
        currentSpecialCarSelected = PlayerPersistentData.Instance.SelectedSpecialCar;

        cameraManager.GetComponent<FiniteStateMachine>().enabled = false;
        //cameraManager.transform.position = new Vector3(-0.3544f, 4.53f, -20.165f);
        cameraManager.transform.position = new Vector3(-0.3544f, 5.8f, -20.165f);
        cameraManager.transform.rotation = Quaternion.Euler(18.0f, 0.0f, 0.0f);
        backupFOV = cameraManager.GetComponent<Camera>().fieldOfView;
        cameraManager.GetComponent<Camera>().fieldOfView = 35.0f;
        
        //Denis
        //if (currentSetIndex == -1)
        //{
        //    currentSetIndex = 0;
        //    currentCarsList = specialCarList;
        //    ShowCars();
        //}
        //else
        //{
        //    SetupParkingLot();
        //    if (idleCarGO.transform.childCount == 0)
        //        ShowCars();
        //}

        //OnTheRunDataLoader.Instance.GetLocaleString("special_bottom_descr");
        //SetupPanel();
        Manager<UIRoot>.Get().ShowPageBorders(true);
        Manager<UIRoot>.Get().ShowCommonPageElements(true, true, true, true, false);

        lockItem.SetActive(false);
        buyButtons.SetActive(false);
        if (Manager<UIRoot>.Get().CheckForSpecialVehicleUnlockedPopup())
        {
            switch (Manager<UIRoot>.Get().SpecialVehicleJustUnlockedByXPPopup())
            {
                case TruckBehaviour.TrasformType.Bigfoot: currentSpecialCarSelected = 0; break;
                case TruckBehaviour.TrasformType.Tank: currentSpecialCarSelected = 1; break;
                case TruckBehaviour.TrasformType.Firetruck: currentSpecialCarSelected = 2; break;
                case TruckBehaviour.TrasformType.Ufo: currentSpecialCarSelected = 3; break;
                case TruckBehaviour.TrasformType.Plane: currentSpecialCarSelected = 4; break;
            }
            Manager<UIRoot>.Get().ShowSpecialVehicleUnlockedPopup();
        }
        else
            CheckForSpecialTruckAdvise();

        FillCarInfo();
        InitScroller();
        SetupPanel();

        /*
        if(canShowSpecialTrucksAdvise && nextTimeToShowTruckAdvisePopup == OnTheRunDataLoader.Instance.GetTimeToWaitToShowTrucksAdvise())
        {
            nextTimeToShowTruckAdvisePopup = -1;
            OnTheRunUITransitionManager.Instance.OpenPopup("TrucksAdvisePopup");
        }

        nextTimeToShowTruckAdvisePopup++;

        if (nextTimeToShowTruckAdvisePopup > OnTheRunDataLoader.Instance.GetTimeToWaitToShowTrucksAdvise())
            nextTimeToShowTruckAdvisePopup = 0;
        */
#if UNITY_WEBPLAYER
        transform.FindChild("InvisibleButtonRightWeb").gameObject.SetActive(currentSpecialCarSelected != 4);
        transform.FindChild("InvisibleButtonLeftWeb").gameObject.SetActive(currentSpecialCarSelected != 0);
#endif
    }

    void CheckForSpecialTruckAdvise()
    {
        int countToShowTruckAdvisePopup = EncryptedPlayerPrefs.GetInt("cst_advise", 0);
        if (countToShowTruckAdvisePopup != -1)
        {
            bool canShowSpecialTrucksAdvise = false;
            for (int i = 0; i < (int)TruckBehaviour.TrasformType.Count; ++i)
                if (UnlockingManager.Instance.GetSpecialCarData((TruckBehaviour.TrasformType)i).locked)
                {
                    canShowSpecialTrucksAdvise = true;
                    break;
                }

            if (canShowSpecialTrucksAdvise)
            {
                if (++countToShowTruckAdvisePopup > OnTheRunDataLoader.Instance.GetTimeToWaitToShowTrucksAdvise())
                {
                    OnTheRunUITransitionManager.Instance.OpenPopup("TrucksAdvisePopup");
                    countToShowTruckAdvisePopup = -1;
                }
                EncryptedPlayerPrefs.SetInt("cst_advise", countToShowTruckAdvisePopup);
            }
        }
    }

    void Signal_OnExit(UIPage page)
    {
        DestroyScroller();
        //DestroyGarageCamera();
        DestroyCarsByParentRef(idleCarGO);

        PlayerPersistentData.Instance.SaveSpecialCarSelection(currentSpecialCarSelected);
        cameraManager.GetComponent<FiniteStateMachine>().enabled = true;
        cameraManager.GetComponent<Camera>().fieldOfView = backupFOV;
        Manager<UIRoot>.Get().ShowPageBorders(false);
        Manager<UIRoot>.Get().ShowCommonPageElements(false, false, false, false, false);
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
        if (Mathf.Abs(vehiclesScroller.OffsetVelocity.x) < 2.5f && currentSpecialCarSelected > 0 && invisibleButtonsTimer < 0.0f)
        {
            invisibleButtonsTimer = 0.3f;
            --currentSpecialCarSelected;
            if (currentSpecialCarSelected < 0)
                currentSpecialCarSelected = 0;
            
            SetupPanel();

            int iNextSnap = -currentSpecialCarSelected;
            vehiclesScroller.easeToSnapX(iNextSnap);

#if UNITY_WEBPLAYER
            interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

            if (currentSpecialCarSelected==0)
                transform.FindChild("InvisibleButtonLeftWeb").gameObject.SetActive(false);
            else
                transform.FindChild("InvisibleButtonRightWeb").gameObject.SetActive(true);
#endif
        }
    }

    void Signal_OnInvisibleButtonRightRelease(UIButton button)
    {
        //Debug.Log("********* Signal_OnInvisibleButtonRightRelease " + categorySelectedCar[currentSetIndex]);
        if (Mathf.Abs(vehiclesScroller.OffsetVelocity.x) < 2.5f && currentSpecialCarSelected < 4 && invisibleButtonsTimer < 0.0f)
        {
            invisibleButtonsTimer = 0.3f;
            ++currentSpecialCarSelected;
            if (currentSpecialCarSelected > 4)
                currentSpecialCarSelected = 4;

            SetupPanel();

            int iNextSnap = -currentSpecialCarSelected;
            vehiclesScroller.easeToSnapX(iNextSnap);

#if UNITY_WEBPLAYER
            interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

            if (currentSpecialCarSelected == 4)
                transform.FindChild("InvisibleButtonRightWeb").gameObject.SetActive(false);
            else
                transform.FindChild("InvisibleButtonLeftWeb").gameObject.SetActive(true);
#endif
        }
    }

    void Signal_OnPlayRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        uiManager.GoToPage("GaragePage");
    }

    void Signal_OnCarSelectedChange(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        //categorySelectedCar[currentSetIndex] = int.Parse(button.name.Substring(button.name.Length - 1, 1)) - 1;
        //carSelected = int.Parse(button.name.Substring(button.name.Length-1, 1))-1;
        FillCarInfo();
    }
    
    public void OnBuyPressed(TruckBehaviour.TrasformType carType, GameObject truckRef)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        //UnlockingManager.SpecialCarData currSpecialCarData = UnlockingManager.Instance.GetSpecialCarData((TruckBehaviour.TrasformType)categorySelectedCar[currentSetIndex]);
        UnlockingManager.SpecialCarData currSpecialCarData = UnlockingManager.Instance.GetSpecialCarData(carType);
        int carCost = currSpecialCarData.cost;
        bool canAfford = PlayerPersistentData.Instance.CanAfford(currSpecialCarData.currency, carCost);

		bool bLock = currSpecialCarData.locked && lockItem.activeInHierarchy;

        if (canAfford)
        {
			BuyPanelLocked.SetActive( false );

			if( bLock )
				StartCoroutine( BuyTruckTask(currSpecialCarData, carType , carCost,truckRef ) );
			else
				UnlockTruckInternal(currSpecialCarData, carType , carCost,truckRef );
        }
        else
        {
            if (currSpecialCarData.currency==PriceData.CurrencyType.SecondCurrency)
                Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Diamonds);
            else
                Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Money);
        }
    }

	IEnumerator BuyTruckTask( UnlockingManager.SpecialCarData currSpecialCarData , TruckBehaviour.TrasformType carType, int carCost , GameObject truckRef )
	{
		lockAnimPlaying = true;

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

		UnlockTruckInternal(currSpecialCarData, carType , carCost,truckRef );

		lockAnimPlaying = false;
	}

	void UnlockTruckInternal( UnlockingManager.SpecialCarData currSpecialCarData , TruckBehaviour.TrasformType carType, int carCost , GameObject truckRef )
	{
		PlayerPersistentData.Instance.BuyItem(currSpecialCarData.currency/*PriceData.CurrencyType.SecondCurrency*/, carCost, true, OnTheRunDataLoader.Instance.GetLocaleString("popup_amazing"), OnTheRunDataLoader.Instance.GetLocaleString("popup_unlocked_special"), UIBuyFeedbackPopup.ItemBought.SpecialUnlock, OnTheRunGameplay.GetCarLocalizedStr(currSpecialCarData.type));
		UnlockingManager.Instance.BuySpecialCar(currSpecialCarData.type);
		FillCarInfo();
		ActivatePlayer(truckRef, false, (int)carType, false);
		OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.BUY_SPECIAL_VEHICLE);
		
		if (OnTheRunOmniataManager.Instance != null)
			OnTheRunOmniataManager.Instance.TrackVirtualPurchase(OmniataIds.Product_Special + "_" + carType.ToString().ToLowerInvariant(), PriceData.CurrencyType.SecondCurrency, carCost.ToString(), OmniataIds.Product_Type_Standard);

		SetupLockedPanel( currSpecialCarData );


        panel.GetComponent<UIBuyPopup>().SendMessage("UpdatePopupStatus");
	}


    public void OnBuyAlternativePressed(TruckBehaviour.TrasformType carType, GameObject truckRef)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        //UnlockingManager.SpecialCarData currSpecialCarData = UnlockingManager.Instance.GetSpecialCarData((TruckBehaviour.TrasformType)categorySelectedCar[currentSetIndex]);
        UnlockingManager.SpecialCarData currSpecialCarData = UnlockingManager.Instance.GetSpecialCarData(carType);
        int carCost = currSpecialCarData.alternativeCost;
        bool canAfford = PlayerPersistentData.Instance.CanAfford(currSpecialCarData.alternativeCurrency, carCost);
        if (canAfford)
        {
            PlayerPersistentData.Instance.BuyItem(currSpecialCarData.alternativeCurrency/*PriceData.CurrencyType.SecondCurrency*/, carCost, true, OnTheRunDataLoader.Instance.GetLocaleString("popup_amazing"), OnTheRunDataLoader.Instance.GetLocaleString("popup_unlocked_special"), UIBuyFeedbackPopup.ItemBought.SpecialUnlock, OnTheRunGameplay.GetCarLocalizedStr(currSpecialCarData.type));
            UnlockingManager.Instance.BuySpecialCar(currSpecialCarData.type);
            FillCarInfo();
            ActivatePlayer(truckRef, false, (int)carType, false);
            OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.BUY_SPECIAL_VEHICLE);

            if (OnTheRunOmniataManager.Instance != null)
                OnTheRunOmniataManager.Instance.TrackVirtualPurchase(OmniataIds.Product_Special + "_" + carType.ToString().ToLowerInvariant(), PriceData.CurrencyType.SecondCurrency, carCost.ToString(), OmniataIds.Product_Type_Standard);
        }
        else
        {
            if (currSpecialCarData.alternativeCurrency == PriceData.CurrencyType.SecondCurrency)
                Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Diamonds);
            else
                Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Money);
        }
    }

	public GameObject BarElementAnimPrefab;
	
	void PlayBarElementEffectTrucks( Vector3 kPos )
	{
		OnTheRunInterfaceSounds.Instance.PlayUnlockSound();

		GameObject pContainer = new GameObject() ;
		pContainer.name = "BarAnim"+pContainer.GetInstanceID();
		pContainer.transform.position = kPos;

		GameObject pEffect = Instantiate( BarElementAnimPrefab , kPos , Quaternion.identity ) as GameObject;

		pEffect.transform.parent = pContainer.transform;
		pContainer.transform.localScale = new Vector3( 1.125f, 0.875f , 1f );

		m_kElementsEffects.Add( pContainer );

		Destroy( pContainer , 1f );
	}

	List<GameObject> m_kElementsEffects = new List<GameObject>();
	
	public void ClearSteppers()
	{
		// destroy all the elements when changin car
		for( int i = 0; i< m_kElementsEffects.Count ; ++i )
		{
			if( m_kElementsEffects[i] != null )
				Destroy( m_kElementsEffects[i] );
		}
	}

	int m_eOldCarID = -1;

	void DestroyBarElements()
	{
		// we changed a car
		if( m_eOldCarID != -1 && currentSpecialCarSelected != m_eOldCarID  )
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
		
		m_eOldCarID = currentSpecialCarSelected;
	}

	public Transform InfoItem;

    //void Signal_OnMoreDurationButtonPressed(UIButton button)
    public void OnMoreDurationPressed(TruckBehaviour.TrasformType carType)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        UnlockingManager.SpecialCarData currSpecialCarData = UnlockingManager.Instance.GetSpecialCarData(carType);
        int carCost = currSpecialCarData.upgradeCost;
        bool canAfford = PlayerPersistentData.Instance.CanAfford(PriceData.CurrencyType.FirstCurrency, carCost);
        if (canAfford)
        {
			// play upgrade effect
			int iNextIndex = currSpecialCarData.level +1;
			Transform pStatPoint = InfoItem.FindChild("Bar/barElement"+ iNextIndex);
			PlayBarElementEffectTrucks( pStatPoint.position );


            int prevLevel = currSpecialCarData.level;
            PlayerPersistentData.Instance.BuyItem(PriceData.CurrencyType.FirstCurrency, carCost);
            UnlockingManager.Instance.UpgradeSpecialCarLevel(currSpecialCarData.type);
            FillCarInfo();

            if (prevLevel == 9 && currSpecialCarData.level==10)
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.MAX_OUT_SPECIAL_VEHICLE);

            if (OnTheRunOmniataManager.Instance != null)
            {
                OnTheRunOmniataManager.Instance.TrackVirtualPurchase(OmniataIds.Product_Special + "_" + currSpecialCarData.type.ToString().ToLowerInvariant() + "_" + OmniataIds.Product_SpecialDuration + "_" + currSpecialCarData.level, PriceData.CurrencyType.FirstCurrency, carCost.ToString(), OmniataIds.Product_Type_Upgrade);
                OnTheRunOmniataManager.Instance.TrackCarUpdate(currSpecialCarData.type.ToString(), "special", "duration", currSpecialCarData.level, - 1);
            }
        }
        else
        {
            Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Money);
        }
    }
    #endregion

    public void FillCarInfo()
    {
        Manager<UIRoot>.Get().UpdateCurrenciesItem();
    }

    public void SetupLockedPanel(UnlockingManager.SpecialCarData data)
    {
        if (data.canBeBought)
        {
            lockItem.SetActive(false);//...show two buttons: it's unlocked
            buyButtons.SetActive(true);
#if UNITY_WEBPLAYER
            buyAlternative.transform.localPosition = OrTextBuyButtons.transform.localPosition + new Vector3(0.3f, 0.12f, 0.0f);
            OrTextBuyButtons.gameObject.SetActive(false);
            buyNormal.gameObject.SetActive(false);
#endif
        }
        else if (data.locked)
        {
			lockItem.SetActive(true);//...show star & buy in diamonds
			buyButtons.SetActive(false);

			BuyPanelLocked.SetActive( lockAnimPlaying == false  );
//			lockItem.transform.FindChild("BuyPanel").gameObject.SetActive( lockAnimPlaying == false );

			if( lockAnimPlaying == false )
			{
				LockAnimationBump.SetActive( true );
				LockAnimationUnlock.SetActive( false );
			}

#if UNITY_WEBPLAYER
            Transform PurchaseButtonCoinsTransform = lockItem.transform.FindChild("PurchaseButtonCoins");
            Transform orLabelTransform = lockItem.transform.FindChild("BuyPanelLocked/tfOr");
            Transform PurchaseButtonTransform = lockItem.transform.FindChild("BuyPanelLocked/PurchaseButton");
            Transform lockedAnimTransform = lockItem.transform.FindChild("LockAnim");
            Transform starTransform = lockItem.transform.FindChild("BuyPanelLocked/Star");
            Vector3 lockedAnimOffset = lockedAnimTransform.position - PurchaseButtonCoinsTransform.position;
            Vector3 starOffset = starTransform.position - PurchaseButtonCoinsTransform.position;
            PurchaseButtonCoinsTransform.transform.localPosition = orLabelTransform.localPosition + new Vector3(0.3f, 0.12f, 0.0f);
            lockedAnimTransform.position = PurchaseButtonCoinsTransform.position + lockedAnimOffset;
            starTransform.position = PurchaseButtonCoinsTransform.position + starOffset;
            PurchaseButtonTransform.gameObject.SetActive(false);
            orLabelTransform.gameObject.SetActive(false);
#endif
        }
        else
        {
            lockItem.SetActive(false);
            buyButtons.SetActive(false);
        }

        lockItem.transform.position = new Vector3(1.45f, 0.0f, 0.0f);
        buyButtons.transform.position = new Vector3(2.02f, 0.16f, 0.0f);

        if (lockItem.activeInHierarchy)
        {
            int unlockAtXPLevel = data.unlockAtLevel;
			lockItem.transform.FindChild("BuyPanelLocked/Star/tfLevelValue").GetComponent<UITextField>().text = unlockAtXPLevel.ToString();
#if !UNITY_WEBPLAYER
			lockItem.transform.FindChild("BuyPanelLocked/PurchaseButton/tfTextfield").GetComponent<UITextField>().text = data.cost.ToString();
#else
            lockItem.transform.FindChild("BuyPanelLocked/PurchaseButton/tfTextfield").GetComponent<UITextField>().text = Manager<UIRoot>.Get().FormatTextNumber(data.cost);
#endif
			lockItem.transform.FindChild("BuyPanelLocked/PurchaseButton/coin").gameObject.SetActive(data.currency == PriceData.CurrencyType.FirstCurrency);
			lockItem.transform.FindChild("BuyPanelLocked/PurchaseButton/diamond").gameObject.SetActive(data.currency == PriceData.CurrencyType.SecondCurrency);

            lockItem.transform.FindChild("PurchaseButtonCoins/tfTextfield").GetComponent<UITextField>().text = Manager<UIRoot>.Get().FormatTextNumber(data.alternativeCost);
        } 
        else if(buyButtons.activeInHierarchy)
        {
            buyAlternative.transform.FindChild("PriceTextField").GetComponent<UITextField>().text = Manager<UIRoot>.Get().FormatTextNumber(data.alternativeCost);
            buyAlternative.transform.FindChild("Currency/coin").gameObject.SetActive(data.alternativeCurrency == PriceData.CurrencyType.FirstCurrency);
            buyAlternative.transform.FindChild("Currency/diamond").gameObject.SetActive(data.alternativeCurrency == PriceData.CurrencyType.SecondCurrency);

            buyNormal.transform.FindChild("PriceTextField").GetComponent<UITextField>().text = Manager<UIRoot>.Get().FormatTextNumber(data.cost);
            buyNormal.transform.FindChild("Currency/coin").gameObject.SetActive(data.currency == PriceData.CurrencyType.FirstCurrency);
            buyNormal.transform.FindChild("Currency/diamond").gameObject.SetActive(data.currency == PriceData.CurrencyType.SecondCurrency);
        }
    }
}