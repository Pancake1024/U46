using UnityEngine;
using SBS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

[AddComponentMenu("OnTheRun/UI/UIWheelPopup")]
public class UIWheelPopup : MonoBehaviour
{
    //protected int currentSpinsAmount = 0;
    public static bool ACTIVATE_LIGHTS = false;

    public GameObject wheelGO;
    public UIWheelArrow spinArrow;
    public UITextField spinRemainingText;
    public UIButton spinButton;
    protected UIButton extraSpinButton;
    public Transform extraSpinButtonTr;
    public Transform[] slices;
    public Transform[] physicsAnchorPoints;
    protected OnTheRunGameplay gameplayManager;
    protected List<OnTheRunGameplay.CarId> carAvailableForReward;
    protected string lastWinText;
    protected OnTheRunGameplay.CarId lastCarWon;
    protected bool noSpinPopupShowed = false;
    protected bool backFromCurrencyPopup = false;

    protected OnTheRunInterfaceSounds interfaceSounds;
    protected Rigidbody2D wheelRb = null;
    protected Rigidbody2D redArrow = null;
    protected HingeJoint2D wheelHinge = null;
    protected HingeJoint2D arrowHinge = null;
	protected float wheelSpeed = 0.0f;
    protected bool isWheelRotating = false;
    protected OnTheRunGameplay.CarId nextCarToWin;
    
    protected UIManager uiManager;

    protected bool spinFromSwipe = false;
    protected bool enableTickSoundOnCollision = false;

    public int extraSpinCost;
    protected int extraSpinGained;
    protected int lastSliceIndex = -1;

    protected bool nearWinActive = false;
    protected int nearWinDirection = 0;

    //STATE: Rotate Dots---------------
    public enum WheelStates
    {
        RotateDots = 0,
        Spinning,
        EndSpin,
        CollectReward
    }
    public Transform[] dots;
    protected int[] movingDotsIndexes = { 0, 4, 8, 12, 16, 20 };
    protected float dotsFrequency = 0.1f;
    protected float dotsFrequencyTimer;
    //----------------------------------

    //STATE: End Spin---------------
    protected Transform currentSlice;
    protected float blinkingFrequency = 0.2f;
    protected float blinkingTimer;
    protected bool blinkState;
    protected float endSpinTimer;
    //----------------------------------

    protected List<WheelItem> wheelItems;
    protected List<WheelItem> cheapPrices;
    protected List<WheelItem> mediumPrices;
    protected List<WheelItem> richPrices;

    protected List<WheelItem> wheelCheapPrices;
    protected List<WheelItem> wheelMediumPrices;
    protected List<WheelItem> wheelRichPrices;

    public List<CarSprite> carIcons;
    public float minAngularVelocity = 400.0f;
    public float goalSlice = 0;
    public float wheelSpeedDecreaseFactor = 50.0f;

    protected float currentWheelAngle;
    protected int currentWheelSlice;
    protected WheelItem currentReward;
    
    protected float cheapPerc;
    protected float mediumPerc;
    protected float richPerc;

    [Serializable]
    public class CarSprite
    {
        public OnTheRunGameplay.CarId type;
        public Sprite icon;
    }

	UIRoot UI_Root 
	{
		get
		{
			if( uiRoot == null )
				uiRoot = Manager<UIRoot>.Get();

			return uiRoot;
		}
	}
	UIRoot uiRoot = null;

    public bool EnableTickSoundOnCollision
    {
        get { return enableTickSoundOnCollision; }
    }
    
    public bool IsSpinning
    {
        get { return GetComponent<FiniteStateMachine>().State != (int)WheelStates.RotateDots; }
    }

    public void SetBackCurrencyPopupFlag(bool value)
    {
        backFromCurrencyPopup = value;
    }
    

    void Awake()
    {

#if !UNITY_WEBPLAYER
        cheapPerc = OnTheRunDataLoader.Instance.GetWheelDataPercentage(WheelItem.Level.cheap);
        mediumPerc = OnTheRunDataLoader.Instance.GetWheelDataPercentage(WheelItem.Level.medium);
        richPerc = OnTheRunDataLoader.Instance.GetWheelDataPercentage(WheelItem.Level.rich);
#endif

        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        wheelRb = transform.FindChild("content/wheel/inner").gameObject.GetComponent<Rigidbody2D>();
        wheelHinge = transform.FindChild("content/wheel/inner").gameObject.GetComponent<HingeJoint2D>();
        redArrow = spinArrow.GetComponent<Rigidbody2D>();//transform.FindChild("wheel/red_arrow").gameObject.GetComponent<Rigidbody2D>();
        arrowHinge = spinArrow.GetComponent<HingeJoint2D>();
        wheelRb.isKinematic = true;
        redArrow.isKinematic = true;
		wheelSpeed = 0.0f;
		isWheelRotating = false;
        extraSpinButton = extraSpinButtonTr.GetComponent<UIButton>();
        dotsFrequencyTimer = dotsFrequency;
        OnTheRunUITransitionManager.Instance.InitializeBackground(transform.FindChild("background/bg_black_gradient"));

        extraSpinGained = OnTheRunDataLoader.Instance.GetExtraSpinData()[0];
        extraSpinCost = OnTheRunDataLoader.Instance.GetExtraSpinData()[1];

        extraSpinButton.transform.FindChild("tfTextfield").GetComponent<UITextField>().text = extraSpinCost.ToString();
        transform.FindChild("content/texts/tfTitle").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("spin_and_win");
        transform.FindChild("content/texts/tfStill").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("still");
        transform.FindChild("content/texts/tfExtraSpin").GetComponent<UITextField>().text = extraSpinGained + " " + OnTheRunDataLoader.Instance.GetLocaleString("extra_spins");
        transform.FindChild("content/texts/SpinButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("spin");

        redArrow.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 22.0f);

        spinDragAreaInitialized = false;
    }

    public void SetupAvailableWheelCars()
    {
        carAvailableForReward = new List<OnTheRunGameplay.CarId>();
        for (int i = 0; i < (int)OnTheRunEnvironment.Environments.Count; ++i)
        {
            GameObject[] currentCarsList = gameplayManager.GetCarsFromIndex(i);

            if (!PlayerPersistentData.Instance.IsParkingLotLocked(i))
            {
                OnTheRunGameplay.CarId currentCarId = currentCarsList[3].GetComponent<PlayerKinematics>().carId;
                PlayerPersistentData.PlayerData playerData = PlayerPersistentData.Instance.GetPlayerData(currentCarId);
                if (!playerData.owned)
                    carAvailableForReward.Add(currentCarId);
            }
        }

        if (carAvailableForReward.Count > 0)
        {
            int rndIndex = UnityEngine.Random.Range(0, carAvailableForReward.Count);
            nextCarToWin = carAvailableForReward[rndIndex];
        }
        else
            nextCarToWin = OnTheRunGameplay.CarId.None;
        //foreach (OnTheRunGameplay.CarId carId in carAvailableForReward)
        //    Debug.Log("--<  " + carId);
    }
    
    void InitializeWheel()
    {
        wheelItems = new List<WheelItem>();

        //SETUP PRICES AVAILABLE-------------------------------------------------------------
        cheapPrices = new List<WheelItem>();
        mediumPrices = new List<WheelItem>();
        richPrices = new List<WheelItem>();
        cheapPrices.AddRange(OnTheRunDataLoader.Instance.GetWheelData("cheap"));
        mediumPrices.AddRange(OnTheRunDataLoader.Instance.GetWheelData("medium"));
        richPrices.AddRange(OnTheRunDataLoader.Instance.GetWheelData("rich"));
        SetupAvailableWheelCars();
        //------------------------------------------------------------------------------------

        wheelCheapPrices = new List<WheelItem>();
        wheelMediumPrices = new List<WheelItem>();
        wheelRichPrices = new List<WheelItem>();
        
        string lastWheelSpinConfigDate = EncryptedPlayerPrefs.GetString("wspin_date", string.Empty);

        if (lastWheelSpinConfigDate.Length > 0)
        {
            bool stillValid = true;
            for (int i = 0; i < 8; ++i)
            {
                UIWheelItem.WheelItem item = (UIWheelItem.WheelItem)System.Enum.Parse(typeof(UIWheelItem.WheelItem), EncryptedPlayerPrefs.GetString("wspin_item_" + i, String.Empty));
                WheelItem.Level level = (WheelItem.Level)System.Enum.Parse(typeof(WheelItem.Level), EncryptedPlayerPrefs.GetInt("wspin_item_l_" + i, 1).ToString());
                WheelItem wheelItem = new WheelItem(item, EncryptedPlayerPrefs.GetInt("wspin_item_q_" + i, 1), level);
                wheelItems.Add(wheelItem);
                if (level == WheelItem.Level.cheap) wheelCheapPrices.Add(wheelItem);
                else if (level == WheelItem.Level.medium) wheelMediumPrices.Add(wheelItem);
                else if (level == WheelItem.Level.rich) wheelRichPrices.Add(wheelItem);

                if (item == UIWheelItem.WheelItem.FuelFreeze && OnTheRunFuelManager.Instance.IsFuelFreezeActive())
                    stillValid = false;
                else if (item == UIWheelItem.WheelItem.Car && carAvailableForReward.Count == 0)
                    stillValid = false;
            }

            if (!stillValid)
            {
                lastWheelSpinConfigDate = string.Empty;
                wheelItems = new List<WheelItem>();
            }
        }

        int cheapPricesNumber = OnTheRunDataLoader.Instance.GetWheelDataQuantities(WheelItem.Level.cheap),
            mediumPricesNumber = OnTheRunDataLoader.Instance.GetWheelDataQuantities(WheelItem.Level.medium),
            richPricesNumber = OnTheRunDataLoader.Instance.GetWheelDataQuantities(WheelItem.Level.rich);
        if (lastWheelSpinConfigDate.Length == 0)
        {
            //SETUP CURRENT PRICES----------------------------------------------------------------
            for (int i = 0; i < cheapPricesNumber; ++i)
            {
                int rnd = UnityEngine.Random.Range(0, cheapPrices.Count);
                wheelItems.Add(cheapPrices[rnd]);
                wheelCheapPrices.Add(cheapPrices[rnd]);
                cheapPrices.RemoveAt(rnd);
            }
            for (int i = 0; i < mediumPricesNumber; ++i)
            {
                int rnd = UnityEngine.Random.Range(0, mediumPrices.Count);
                wheelItems.Add(mediumPrices[rnd]);
                wheelMediumPrices.Add(mediumPrices[rnd]);
                mediumPrices.RemoveAt(rnd);
            }
            for (int i = 0; i < richPricesNumber; ++i)
            {
                int rnd = 0;
                do
                {
                    rnd = UnityEngine.Random.Range(0, richPrices.Count);
                } while ((richPrices[rnd].item == UIWheelItem.WheelItem.FuelFreeze && OnTheRunFuelManager.Instance.IsFuelFreezeActive()) || (carAvailableForReward.Count == 0 && richPrices[rnd].item == UIWheelItem.WheelItem.Car));

                wheelItems.Add(richPrices[rnd]);
                wheelRichPrices.Add(richPrices[rnd]);
                richPrices.RemoveAt(rnd);
            }

            OnTheRunUtils.Shuffle<WheelItem>(wheelItems);

            //SAVE CONFIGURATION -------------------------------------------------------------
            SaveWheelConfiguration();
        }

        //SETUP WHEEL GRAPHIC-----------------------------------------------------------------
        RefreshWheelGraphic();

        //------------------------------------------------------------------------------------
    }

    void SaveWheelConfiguration()
    {
        /*
        DateTime now = iOSUtils.GetNetworkDate();
        if (iOSUtils.IsNetworkDateValid)
            EncryptedPlayerPrefs.SetString("wspin_date", now.ToString());
        else
            EncryptedPlayerPrefs.SetString("wspin_date", DateTime.Now.ToString());
        */
        DateTime now;
        bool isDateValid;
        DateTimeManager.Instance.GetDate(out now, out isDateValid);
        if (isDateValid)
            EncryptedPlayerPrefs.SetString("wspin_date", now.ToString());
        else
            EncryptedPlayerPrefs.SetString("wspin_date", DateTime.Now.ToString());


        for (int i = 0; i < 8; ++i)
        {
            EncryptedPlayerPrefs.SetString("wspin_item_" + i, wheelItems[i].item.ToString());
            EncryptedPlayerPrefs.SetInt("wspin_item_q_" + i, wheelItems[i].quantity);
            EncryptedPlayerPrefs.SetInt("wspin_item_l_" + i, (int)wheelItems[i].level);
        }
    }

    void RefreshWheelGraphic()
    {
        for (int i = 0; i < slices.Length; ++i)
        {
            Transform currentSliceIcons = slices[i].FindChild("icons");
            for (int j = 0; j < currentSliceIcons.childCount; ++j)
            {
                Transform currentIcon = currentSliceIcons.GetChild(j);
                UIWheelItem.WheelItem currItem = currentIcon.GetComponent<UIWheelItem>().item;
                if (currItem == wheelItems[i].item)
                {
                    currentIcon.gameObject.SetActive(true);
                    if (currItem == UIWheelItem.WheelItem.FuelFreeze)
                        currentSliceIcons.parent.FindChild("text").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("freeze");
                    else
                        currentSliceIcons.parent.FindChild("text").GetComponent<UITextField>().text = Manager<UIRoot>.Get().FormatTextNumber(wheelItems[i].quantity);

                    if(currItem==UIWheelItem.WheelItem.Car)
                    {
                        int currIndx = -1;
                        for(int k = 0; k<carIcons.Count; ++k)
                        {
                            if(carIcons[k].type == nextCarToWin)
                            {
                                currIndx = k;
                                break;
                            }
                        }
                        currentIcon.GetComponent<SpriteRenderer>().sprite = carIcons[currIndx].icon;
                    }
                }
                else
                {
                    currentIcon.gameObject.SetActive(false);
                }
            }
        }
    }

    void UpdateInterface()
    {
        spinRemainingText.text = PlayerPersistentData.Instance.ExtraSpin.ToString();

        //if (PlayerPersistentData.Instance.ExtraSpin <= 0)
        //    spinButton.State = UIButton.StateType.Disabled;
        //else
		{
			// reset to defaults
            OnTheRunSoundsManager.Instance.MusicVolume = OnTheRunSounds.Instance.offgameMusicBaseVolume;

            spinButton.State = UIButton.StateType.Normal;
		}

        extraSpinButton.State = UIButton.StateType.Normal;
        transform.FindChild("content/texts/SpinButton").GetComponentInChildren<UITextField>().ApplyParameters();
    }

    Vector3 redArrowBakcupPos = Vector3.zero;
    void UpdateKinematic()
    {
        bool isTweening = false;
        for (int i = 0; i < iTween.tweens.Count; ++i)
        {
            Hashtable currentTween = (Hashtable)iTween.tweens[i];
            GameObject target = (GameObject)currentTween["target"];
            if (target == wheelGO)
            {
                isTweening = true;
                break;
            }
        }

        //Debug.Log("tw: " + isTweening);

        if (isTweening && !wheelRb.isKinematic)
        {
            wheelRb.isKinematic = redArrow.isKinematic = true;

            redArrow.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 22.0f);

            wheelRb.transform.localPosition = new Vector3(wheelRb.transform.localPosition.x, 0.0f, wheelRb.transform.localPosition.z);
            redArrow.transform.localPosition = redArrowBakcupPos;
        }
        else if (!isTweening && wheelRb.isKinematic)
        {
            redArrowBakcupPos = redArrow.transform.localPosition;
            wheelRb.isKinematic = redArrow.isKinematic = false;

            redArrow.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 22.0f);

            wheelHinge.connectedAnchor = new Vector2(physicsAnchorPoints[0].transform.position.x, physicsAnchorPoints[0].transform.position.y);
            arrowHinge.connectedAnchor = new Vector2(physicsAnchorPoints[1].transform.position.x, physicsAnchorPoints[1].transform.position.y);
        }

        /*
        if (!isTweening && !redArrow.isKinematic)
        {
            float now = TimeManager.Instance.MasterSource.TotalTime;
            float dt = TimeManager.Instance.MasterSource.DeltaTime;

            float torque = redArrow.transform.rotation.z;
            Debug.Log("torque: " + torque);
            //if (torque >= 0.0f)
                redArrow.AddTorque(Mathf.Clamp(torque * 4000.0f, -800.0f, 800.0f) * dt);
                redArrow.angularVelocity *= 0.8f;
            //else
            //    redArrow.AddTorque(Math.Min(torque * 4000.0f, 800.0f) * dt);
        }
        */
    }
    
    #region States

    #region Rotate Dots
    void OnRotateDotsEnter()
    {
        ACTIVATE_LIGHTS = false;
    }

    void OnRotateDotsExec()
    {
        UpdateKinematic();

        dotsFrequencyTimer -= TimeManager.Instance.MasterSource.DeltaTime;
        if (dotsFrequencyTimer <= 0.0f)
        {
            dotsFrequencyTimer = dotsFrequency;

            for (int j = 0; j < movingDotsIndexes.Length; ++j)
            {
                Transform prevDot = dots[movingDotsIndexes[j]];
                prevDot.GetComponent<UIWheelActiveElement>().selectedGraphic.SetActive(false);
                prevDot.GetComponent<UIWheelActiveElement>().normalGraphic.SetActive(true);

                ++movingDotsIndexes[j];
                if (movingDotsIndexes[j] > 23) movingDotsIndexes[j] = 0;

                Transform currDot = dots[movingDotsIndexes[j]];
                currDot.GetComponent<UIWheelActiveElement>().selectedGraphic.SetActive(true);
                currDot.GetComponent<UIWheelActiveElement>().normalGraphic.SetActive(false);
			}
        }

		if( TimeManager.Instance.MasterSource.TotalTime - blinkingTimer > 0.2f )
		{
			RandomSliceBlink();
		}

        currentWheelAngle = wheelRb.transform.rotation.eulerAngles.z;
        currentWheelSlice = (int)(currentWheelAngle / 45.0f);
        currentReward = wheelItems[currentWheelSlice];
        //X PIETRO
        //Debug.Log("--> currentWheelAngle: " + currentWheelAngle + " - currentWheelSlice: " + currentWheelSlice + " - currentReward: " + currentReward.quantity + " " + currentReward.item);
    }

	int currBlinkSlice = 0;
	bool alternate = false;

	void RandomSliceBlink()
	{
		int iRand = UnityEngine.Random.Range( 0 , 25 );
		blinkingTimer = TimeManager.Instance.MasterSource.TotalTime;
		
		if( currBlinkSlice == slices.Length )
			currBlinkSlice = 0;
		
		// reset to first slice when circular is chosen
		if( iRand == 0 && currBlinkSlice < 1 )
		{
			currBlinkSlice = 0;
		}
		
		if( currBlinkSlice > 0 )
			iRand = 0;
		
		if( iRand == 0 ) // CIRCULAR
		{
			UIWheelActiveElement blinkSlice = null;
			
			// for the old one
			if( currBlinkSlice > 0 )
			{
				blinkSlice = slices[currBlinkSlice-1].GetComponent<UIWheelActiveElement>();
				blinkSlice.selectedGraphic.SetActive( false );
				blinkSlice.normalGraphic.SetActive( true );
			}
			
			// the new one
			blinkSlice = slices[currBlinkSlice].GetComponent<UIWheelActiveElement>();
			blinkSlice.selectedGraphic.SetActive( true );
			blinkSlice.normalGraphic.SetActive( false );
			
			++currBlinkSlice;
			
		}
		else if( iRand > 0 && currBlinkSlice == 0 && ! alternate )   // ALTERNATE BLINKING 
		{
			int iLen = slices.Length;
			
			for( int i = 0 ; i< iLen ; ++i )
			{
				bool b = i%2 == 0;
				slices[i].GetComponent<UIWheelActiveElement>().selectedGraphic.SetActive( b );
				slices[i].GetComponent<UIWheelActiveElement>().normalGraphic.SetActive( !b );
			}
			alternate = !alternate;
		}
		else if( iRand > 0 && currBlinkSlice == 0 && alternate )   // ALTERNATE BLINKING 
		{
			int iLen = slices.Length;
			
			for( int i = 0 ; i< iLen ; ++i )
			{
				bool b = i%2 > 0;
				slices[i].GetComponent<UIWheelActiveElement>().selectedGraphic.SetActive( b );
				slices[i].GetComponent<UIWheelActiveElement>().normalGraphic.SetActive( !b );
			}
			alternate = !alternate;
		}

	}

	public void ResetSlices()
	{
		int iLen = slices.Length;
		
		for( int i = 0 ; i< iLen ; ++i )
		{
			slices[i].GetComponent<UIWheelActiveElement>().selectedGraphic.SetActive( false );
			slices[i].GetComponent<UIWheelActiveElement>().normalGraphic.SetActive( true );
		}
	}



    void OnRotateDotsExit()
    {
        for (int j = 0; j < movingDotsIndexes.Length; ++j)
        {
            Transform prevDot = dots[movingDotsIndexes[j]];
            prevDot.GetComponent<UIWheelActiveElement>().selectedGraphic.SetActive(false);
            prevDot.GetComponent<UIWheelActiveElement>().normalGraphic.SetActive(true);
        }

        ResetSlices();

        ACTIVATE_LIGHTS = true;
    }
    #endregion

    #region Spinning
    //float rotationTime = 4.0f;//seconds

    float prevRotation = 0.0f;
    float swipeRotation = 0.0f;
    float angDec = 0.0f;
    float deltaRot = 0.0f;
    float rt = 0.0f;
    float v0 = 0.0f;

    void OnSpinEnter()
    {
        EncryptedPlayerPrefs.SetString("wspin_date", string.Empty);

        if (wheelHinge != null && !isWheelRotating)
        {
            isWheelRotating = true;
            if (!spinFromSwipe)
            {
                goalSlice = ComputeReward();
                wheelSpeed = UnityEngine.Random.Range(300.0f, 500.0f);  //UnityEngine.Random.Range(80.0f, 200.0f);
            }

            --PlayerPersistentData.Instance.ExtraSpin;
            UpdateInterface();

            spinButton.State = UIButton.StateType.Disabled;
            extraSpinButton.State = UIButton.StateType.Disabled;

            swipeRotation = 0.0f;
            prevRotation = wheelRb.transform.rotation.eulerAngles.z;

            angDec = (wheelSpeed * wheelSpeed) / (2.0f * 360.0f);
            deltaRot = (wheelSpeed * wheelSpeed) / (2.0f * 108.0f);
            //deltaRot = 3600.0f;// Mathf.Ceil(deltaRot / 360.0f) * 360.0f;

            currentWheelAngle = wheelRb.transform.rotation.eulerAngles.z;
            float goalAngle = (7 - goalSlice) * 45.0f;
            int completeWheelRotations = 2;
            if(wheelSpeed>350.0f && wheelSpeed<450.0f)
                completeWheelRotations = 3;
            else if (wheelSpeed > 450.0f)
                completeWheelRotations = 4;
            float maxAngle = completeWheelRotations * 360.0f - (360.0f - currentWheelAngle) + goalAngle + 45.0f;
            float minAngle = maxAngle - 45.0f;

            float sliceAngleRandom = UnityEngine.Random.Range(-12.0f, 12.0f);
            if (nearWinActive)
            {
                sliceAngleRandom = nearWinDirection < 0 ? -13.0f : 24.0f;
                nearWinActive = false;
                //Debug.Log("***NEAR WIN ACTIVE!!!!! " + sliceAngleRandom);
            }
            deltaRot = minAngle + (maxAngle - minAngle) * 0.5f + sliceAngleRandom;

            //Debug.Log("deltaRot: " + deltaRot + " angDec: " + angDec + " wheelSpeed: " + wheelSpeed + " completeWheelRotations: " + completeWheelRotations + " currentWheelAngle: " + currentWheelAngle + " goalAngle: " + goalAngle);
            //deltaRot += 5.0f;
            angDec = (wheelSpeed * wheelSpeed) / (2.0f * deltaRot);
            rt = wheelSpeed / 108.0f;
            v0 = wheelSpeed;
        }
    }

	float tickTimer = -1f;
	float oldRotZ = -1f;

    void OnSpinExec()
    {
		// disable home button
		UI_Root.HomeButton.State = UIButton.StateType.Disabled;
		UI_Root.DisablePlusButtons();

		OnTheRunSoundsManager.Instance.MusicVolume = 0.3f;

        UpdateKinematic();

        float now = TimeManager.Instance.MasterSource.TotalTime;
        float dt = TimeManager.Instance.MasterSource.DeltaTime;//Time.fixedDeltaTime;//TimeManager.Instance.MasterSource.DeltaTime;

        float dr = prevRotation - wheelRb.transform.rotation.eulerAngles.z;
        if (dr < -180.0f)
            dr = dr + 360.0f;
        //Debug.Log("dr: " + dr);

        swipeRotation += dr;
        prevRotation = wheelRb.transform.rotation.eulerAngles.z;

        //Debug.Log("deltaRot: " + deltaRot + " swipeRotation: " + swipeRotation + " aa: " + ((deltaRot - swipeRotation) / deltaRot));

        //wheelSpeed = v0 * ((deltaRot - swipeRotation) / deltaRot);//-= dt * 108.0f;//wheelSpeedDecreaseFactor;
        //wheelSpeed -= dt * angDec;//wheelSpeedDecreaseFactor;
        //wheelSpeed -= angDec * dt;
        if ((deltaRot - swipeRotation) > 0.0f)
            wheelSpeed = Mathf.Sqrt(2.0f * angDec * (deltaRot - swipeRotation));
        else
            wheelSpeed = 0.0f;

        if (wheelSpeed < 20.0f)
            if (swipeRotation >= deltaRot || wheelSpeed <= 0.0f)
                wheelSpeed = 0.0f;

        float rotGap = redArrow.transform.rotation.eulerAngles.z - oldRotZ;
        int iGap = (int)(rotGap * 100f);

        if (iGap < -3 && now - tickTimer > 0.1f)
        {
            tickTimer = now;
            //interfaceSounds.WheelTick();
        }

        oldRotZ = redArrow.transform.rotation.eulerAngles.z;

        if (wheelSpeed <= 0.0f)
        {
            wheelRb.angularVelocity = 0.0f;
            wheelSpeed = 0.0f;
            isWheelRotating = false;
            GetComponent<FiniteStateMachine>().State = (int)WheelStates.EndSpin;

            Debug.Log("swipeRotation: " + swipeRotation);
        }

        JointMotor2D m = new JointMotor2D();
        m.maxMotorTorque = 10000.0f;
        m.motorSpeed = wheelSpeed;
        wheelHinge.motor = m;
    }

    void OnSpinExit()
    {
    }
    #endregion

    #region End Spin
    int lastPrizeIndex = -1;
    void OnEndSpinEnter()
    {
        endSpinTimer = 1.2f; // 3.0f;
        currentSlice = spinArrow.CurrentSliceActive;
        blinkingTimer = blinkingFrequency;
        blinkState = false;

        lastPrizeIndex = int.Parse(currentSlice.gameObject.name.Substring(currentSlice.gameObject.name.Length - 1, 1), CultureInfo.InvariantCulture);
        Transform currentSliceIcons = currentSlice.FindChild("icons");
        for (int j = 0; j < currentSliceIcons.childCount; ++j)
        {
            GameObject currGO = currentSliceIcons.GetChild(j).gameObject;
            if (currGO.activeInHierarchy)
            {
                //Debug.Log("**** YOU WIN:  " + wheelItems[lastPrizeIndex].quantity + " " + wheelItems[lastPrizeIndex].item);
                //GetYourReward(wheelItems[lastPrizeIndex]);
            }
        }
    }

    void OnEndSpinExec()
    {
        UpdateKinematic();

        float dt = TimeManager.Instance.MasterSource.DeltaTime;

        blinkingTimer -= dt;
        if (blinkingTimer <= 0.0f)
        {
            blinkingTimer = blinkingFrequency;
            currentSlice.GetComponent<UIWheelActiveElement>().selectedGraphic.SetActive(blinkState);
            currentSlice.GetComponent<UIWheelActiveElement>().normalGraphic.SetActive(!blinkState);
            blinkState = !blinkState;
        }

        endSpinTimer -= dt;
        if (endSpinTimer <= 0.0f)
        {
            GetComponent<FiniteStateMachine>().State = (int)WheelStates.CollectReward;
            //GetComponent<FiniteStateMachine>().State = (int)WheelStates.RotateDots;
            //UpdateInterface();
        }

     }

    void OnEndSpinExit()
    {
        /*UI_Root.HomeButton.State = UIButton.StateType.Normal;
        UI_Root.EnablePlusButtons();

        OnTheRunUITransitionManager.Instance.OpenPopup("WheelFeedbackPopup");
        if (uiManager == null)
            uiManager = Manager<UIManager>.Get();
        UIWheelFeedbackPopup currPopupComp = uiManager.FrontPopup.gameObject.GetComponent<UIWheelFeedbackPopup>();
        currPopupComp.numText.text = Manager<UIRoot>.Get().FormatTextNumber(wheelItems[lastPrizeIndex].quantity);
        uiManager.FrontPopup.transform.FindChild("content/CollectButton").GetComponent<UIButton>().onReleaseEvent.AddTarget(gameObject, "OnPopupClosed");
        
        Transform currentSliceIcons = currentSlice.FindChild("icons");
        for (int j = 0; j < currentSliceIcons.childCount; ++j)
        {
            GameObject currGO = currentSliceIcons.GetChild(j).gameObject;
            if (currGO.activeInHierarchy)
                currPopupComp.icon.sprite = currGO.GetComponent<SpriteRenderer>().sprite;
        }
        currPopupComp.icon.transform.localPosition = currPopupComp.numText.transform.localPosition + new Vector3(1.2f, 0.0f);
        
        currPopupComp.SetText(OnTheRunDataLoader.Instance.GetLocaleString("great_spin"), OnTheRunDataLoader.Instance.GetLocaleString("you_win")); //TODO RANDOM CODES FOR TEXTS

        OnTheRunFireworks.Instance.StartFireworksEffect(25, transform.FindChild("content/fireworks"));*/
    }

    void OnPopupClosed(UIButton button)
    {
        WheelItem prize = wheelItems[lastPrizeIndex];
        GetYourReward(prize);
        button.onReleaseEvent.RemoveTarget(gameObject);

        if (OnTheRunOmniataManager.Instance != null)
            OnTheRunOmniataManager.Instance.TrackSpinGame(prize.item.ToString(), prize.quantity);

        GetComponent<FiniteStateMachine>().State = (int)WheelStates.RotateDots;
        UpdateInterface();
    }
    #endregion

    #region Collect Reward
    void OnCollectRewardEnter()
    {
        OnTheRunUITransitionManager.Instance.OpenPopup("WheelFeedbackPopup");
        if (uiManager == null)
            uiManager = Manager<UIManager>.Get();
        UIWheelFeedbackPopup currPopupComp = uiManager.FrontPopup.gameObject.GetComponent<UIWheelFeedbackPopup>();
        currPopupComp.numText.text = Manager<UIRoot>.Get().FormatTextNumber(wheelItems[lastPrizeIndex].quantity);
        uiManager.FrontPopup.transform.FindChild("content/CollectButton").GetComponent<UIButton>().onReleaseEvent.AddTarget(gameObject, "OnPopupClosed");

        if (wheelItems[lastPrizeIndex].item == UIWheelItem.WheelItem.Diamond)
            OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.SPIN_DIAMOND);

        Transform currentSliceIcons = currentSlice.FindChild("icons");
        for (int j = 0; j < currentSliceIcons.childCount; ++j)
        {
            GameObject currGO = currentSliceIcons.GetChild(j).gameObject;
            if (currGO.activeInHierarchy)
                currPopupComp.icon.sprite = currGO.GetComponent<SpriteRenderer>().sprite;
        }
        currPopupComp.icon.transform.localPosition = currPopupComp.numText.transform.localPosition + new Vector3(1.2f, 0.1f);

        currPopupComp.SetText(OnTheRunDataLoader.Instance.GetLocaleString("great_spin"), OnTheRunDataLoader.Instance.GetLocaleString("you_win")); //TODO RANDOM CODES FOR TEXTS

        OnTheRunFireworks.Instance.StartFireworksEffect(25, transform.FindChild("content/fireworks"));
    }

    void OnCollectRewardExec()
    {
    }

    void OnCollectRewardExit()
    {
        UI_Root.HomeButton.State = UIButton.StateType.Normal;
        UI_Root.EnablePlusButtons();

    }
    #endregion
    #endregion

    #region Signals
    void Signal_OnEnter(UIPopup popup)
    {
        if (uiManager == null)
            uiManager = Manager<UIManager>.Get();

        OnTheRunFireworks.Instance.ClearFireworks();

        //EncryptedPlayerPrefs.SetString("wspin_date", string.Empty);

        UI_Root.CanSpinWheelButtonBounce = false;

		UI_Root.ShowOffgameBG(true);
		UI_Root.ShowUpperPageBorders(true);
		UI_Root.ShowCommonPageElements(true, true, true, false, false);
        InitializeWheel();

        //currentSpinsAmount += PlayerPersistentData.Instance.ExtraSpin;
        //PlayerPersistentData.Instance.ExtraSpin = 0;
        
        UpdateInterface();

        //extraSpinButtonTr.FindChild("coin").gameObject.SetActive(false);
        //extraSpinButtonTr.FindChild("diamond").gameObject.SetActive(true);

        fingerOnSpinArea = -1;
        spinDragAreaInitialized = false;
    }

    void Signal_OnExit(UIPopup popup)
    {
		UI_Root.ShowOffgameBG(false);
		UI_Root.ShowPageBorders(false);
		UI_Root.ShowCommonPageElements(false, false, false, false, false);

        OnTheRunFireworks.Instance.ClearFireworks();
    }

    public void OnExitWheelPopup()
    {
        //PlayerPersistentData.Instance.ExtraSpin = currentSpinsAmount;

		UI_Root.ShowOffgameBG(false);
		UI_Root.ShowPageBorders(false);
		UI_Root.ShowCommonPageElements(false, false, false, false, false);
    }

    void Signal_OnSpinWheelRelease(UIButton button)
    {
        if (OnTheRunUITransitionManager.Instance.isInTransition)
            return;

        if (PlayerPersistentData.Instance.ExtraSpin <= 0)
        {
            ShowNoMoreSpinPopup(true);
            noSpinPopupShowed = true;
        }
        else
        {
            interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
            //Debug.Log("**** Signal_OnSpinWheelRelease");

            spinFromSwipe = false;
            GetComponent<FiniteStateMachine>().State = (int)WheelStates.Spinning;
        }
    }

    void Signal_OnExtraSpinRelease(UIButton button)
    {
        bool canAfford = PlayerPersistentData.Instance.CanAfford(PriceData.CurrencyType.SecondCurrency, extraSpinCost);
        if (canAfford)
        {
            PlayerPersistentData.Instance.BuyItem(PriceData.CurrencyType.SecondCurrency, extraSpinCost);
            PlayerPersistentData.Instance.ExtraSpin += extraSpinGained;
            UpdateInterface();
			UI_Root.UpdateCurrenciesItem();

            if (OnTheRunOmniataManager.Instance != null)
                OnTheRunOmniataManager.Instance.TrackVirtualPurchase(OmniataIds.Product_ExtraSpin, PriceData.CurrencyType.SecondCurrency, extraSpinCost.ToString(), OmniataIds.Product_Type_Standard);
        }
        else
        {
            UI_Root.ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Diamonds);
        }
    }
    #endregion

    #region Wheel Reward
    protected void GetYourReward(WheelItem priceType)
    {
        lastCarWon = OnTheRunGameplay.CarId.None;
        lastWinText = priceType.item.ToString();
        switch (priceType.item)
        {
            case UIWheelItem.WheelItem.Car:
				interfaceSounds.WheelCarOrFuel();
                if (carAvailableForReward.Count > 0)
                {
                    PlayerPersistentData.Instance.GetPlayerData(nextCarToWin).UnlockCar();
                    lastWinText = nextCarToWin.ToString();
                    lastCarWon = nextCarToWin;
                    CheckForAvailableCars();

                    UIGaragePage.lastWonCarId = nextCarToWin;
                    if (OnTheRunOmniataManager.Instance != null)
                        OnTheRunOmniataManager.Instance.TrackVirtualPurchase(lastCarWon.ToString().ToLowerInvariant(), PriceData.CurrencyType.FirstCurrency, "0", OmniataIds.Product_Type_Unlock);
                }
                else
                    lastWinText = "NO MORE CARS AVAILABLE";
                break;
            case UIWheelItem.WheelItem.Coin:
				interfaceSounds.WheelCoinOrDiamonds();
                PlayerPersistentData.Instance.Coins += priceType.quantity;
                break;
            case UIWheelItem.WheelItem.Diamond:
				interfaceSounds.WheelCoinOrDiamonds();
                PlayerPersistentData.Instance.Diamonds += priceType.quantity;
                break;
            case UIWheelItem.WheelItem.ExtraSpin: 
				interfaceSounds.WheelExtraSpin();
                PlayerPersistentData.Instance.ExtraSpin += priceType.quantity;
                UpdateInterface();
                break;
            case UIWheelItem.WheelItem.Fuel:
				interfaceSounds.WheelCarOrFuel();
                OnTheRunFuelManager.Instance.Fuel += priceType.quantity;
                break;
            case UIWheelItem.WheelItem.FuelFreeze:
				interfaceSounds.WheelCarOrFuel();
                OnTheRunFuelManager.Instance.StartFuelFreeze();
                break;
        }
		UI_Root.UpdateCurrenciesItem();
    }
    #endregion

    #region spin swipe
    //private Rect spinDragAreaUpToDown;
    //private Rect spinDragAreaDownToUp;
    private Rect spinDragArea;
    private Vector2 spinDragAreaCenterPos;
    private int fingerOnSpinArea = -1;
    private Vector2 swipeStartPos;
    private Vector2 swipePrevPos;
    private float swipeStartTime;
    private float swipeDist;
    //private bool swipeUpToDown;
    private bool spinDragAreaInitialized = false;

    void InitSpinDragArea()
    {
        UpdateDragArea();
        swipeDist = 0.0f;

        /* OLD
        spinDragAreaUpToDown = new Rect(wheelScreenPos.x, 0.0f, radius * radiusMult, Screen.height); // new Rect(Screen.width * 0.7f, Screen.height * 0.1f, Screen.width * 0.3f, Screen.height * 0.9f);
        spinDragAreaDownToUp = new Rect(wheelScreenPos.x - radius * radiusMult, 0.0f, radius * radiusMult, Screen.height); //new Rect(Screen.width * 0.345f, Screen.height * 0.1f, Screen.width * 0.33f, Screen.height * 0.9f);
        */
    }

    void UpdateDragArea()
    {
        if (uiManager == null)
            uiManager = Manager<UIManager>.Get();

        BoxCollider dragAreaBtn = transform.FindChild("content/wheel/dragAreaBtn").GetComponent<BoxCollider>();
        Vector3 wheelCenterPos = transform.FindChild("content/wheel").position; // dragAreaBtn.bounds.center;
        Vector3 wheelScreenPos = uiManager.UICamera.WorldToScreenPoint(wheelCenterPos);
#if !(UNITY_ANDROID && !UNITY_EDITOR)
        wheelScreenPos.y = Screen.height - wheelScreenPos.y;
#endif
        wheelScreenPos.z = 0.0f; 
        
        //Debug.Log("wheelScreenPos " + wheelScreenPos);

        Vector3 bgLabelPos = uiManager.UICamera.WorldToScreenPoint(transform.FindChild("content/wheel/bg_label").position);
        Vector2 bgLabelScreenPos = new Vector2(bgLabelPos.x, bgLabelPos.y);
#if !(UNITY_ANDROID && !UNITY_EDITOR)
        bgLabelScreenPos.y = Screen.height - bgLabelScreenPos.y;
#endif
        spinDragAreaCenterPos = new Vector2(wheelScreenPos.x, wheelScreenPos.y);
        float radius = (bgLabelScreenPos - spinDragAreaCenterPos).magnitude;

        spinDragArea = new Rect(wheelScreenPos.x - radius, wheelScreenPos.y - radius, radius * 2.0f, radius * 2.0f);

        //Debug.Log("InitSpinDragArea wheelPos " + wheelCenterPos + " radius " + radius);
    }
    
    /*void OnGUI()
    {
        //GUI.Box(spinDragAreaUpToDown, "");
        //GUI.Box(spinDragAreaDownToUp, "");

        GUI.Box(spinDragArea, "");
        GUI.Box(new Rect(spinDragAreaCenterPos.x - 10, spinDragAreaCenterPos.y - 10, 20, 20), "");
        //GUI.Box(new Rect(bgLabelScreenPos.x - 10, bgLabelScreenPos.y - 10, 20, 20), "");
    }*/
    

    bool ClockwiseTest(Vector2 _startP, Vector2 _endP, Vector2 _centerP)
    {
        Vector3 startP = new Vector3(_startP.x, _startP.y, 0.0f);
        Vector3 endP = new Vector3(_endP.x, _endP.y, 0.0f);
        Vector3 centerP = new Vector3(_centerP.x, _centerP.y, 0.0f);

        bool test = (Vector3.Cross((startP - centerP), (endP - centerP)).z > 0.0f);
#if UNITY_ANDROID && !UNITY_EDITOR
        test = !test;
#endif
        //if (!test)
        //    Debug.Log("ClockwiseTest FAILED");
        return test;

        //Debug.Log("ClockwiseTest = _endP " + _endP + " _centerP " + _centerP + Vector3.Cross((startP - centerP), (endP - centerP)).z);
        return (Vector3.Cross((startP - centerP), (endP - centerP)).z > 0.0f);
    }

    void ResetSwipe()
    {
        swipeDist = 0.0f;
        fingerOnSpinArea = -1;
    }

    void StartSwipe(int fingerId, Vector2 pos)
    {
        fingerOnSpinArea = fingerId;
        swipeStartPos = pos;
        swipePrevPos = swipeStartPos;
        swipeStartTime = TimeManager.Instance.MasterSource.TotalTime;
        swipeDist = 0.0f;
    }

    void UpdateSwipe()
    {
        if (OnTheRunUITransitionManager.Instance.isInTransition)
            return;

        float swipeVel = -1.0f;
        float swipeStepDist = 0.0f;
        bool swipeEnded = false;
        float angVel = 0.0f;
#if ((UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR)
        for (int i = 0; i < Input.touchCount; ++i)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.fingerId < 0 || i >= 4)
                continue;

            Vector2 pos = touch.position;
#if !UNITY_ANDROID
            pos.y = Screen.height - pos.y;
#endif
                
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (spinDragArea.Contains(pos))
                    {
                        noSpinPopupShowed = false;
                        StartSwipe(touch.fingerId, pos);
                    }
                    break;

                case TouchPhase.Stationary:
                    break;

                case TouchPhase.Moved:
                    if (fingerOnSpinArea == touch.fingerId)
                    {
                        if (!ClockwiseTest(swipePrevPos, pos, spinDragAreaCenterPos))
                        {
                            ResetSwipe();
                        }
                        else
                        {
                            Vector2 vectorA = swipePrevPos - spinDragAreaCenterPos;
                            Vector2 vectorB = pos - spinDragAreaCenterPos;
                            angVel = Vector2.Angle(vectorA, vectorB);

                            swipeStepDist = (pos - swipePrevPos).magnitude;
                            swipeDist += swipeStepDist;
                            swipePrevPos = pos;
                        }
                    }
                    else if (fingerOnSpinArea<0.0f)
                    {
                        if (spinDragArea.Contains(pos))
                        {
                            noSpinPopupShowed = false;
                            StartSwipe(touch.fingerId, pos);
                        }
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (fingerOnSpinArea == touch.fingerId)
                    {
                        if (!ClockwiseTest(swipePrevPos, pos, spinDragAreaCenterPos))
                        {
                            ResetSwipe();
                        }
                        else
                        {
                            swipeStepDist = 0.0f;
                            swipeDist += (pos - swipePrevPos).magnitude;
                            swipePrevPos = pos;
                            swipeEnded = true;
                        }
                        swipeVel = swipeDist / (TimeManager.Instance.MasterSource.TotalTime - swipeStartTime);
                    }
                    break;
            }
        }
#else
        Vector2 pos = Input.mousePosition;
        pos.y = Screen.height - pos.y;
        if (fingerOnSpinArea < 0.0f)
        {
            //Debug.Log("mousePos " + mousePos + " Input.GetMouseButtonDown(0) " + Input.GetMouseButton(0));

            if (Input.GetMouseButton(0)) // .GetMouseButtonDown(0))
            {
                if (spinDragArea.Contains(pos))
                {
                    noSpinPopupShowed = false;
                    StartSwipe(1, pos);
                }
            }
        }
        else
        {
            bool outOfArea = !(spinDragArea.Contains(pos));
            if ((!Input.GetMouseButton(0)) || outOfArea)
            {
                if (!ClockwiseTest(swipePrevPos, pos, spinDragAreaCenterPos))
                {
                    ResetSwipe();
                }
                else
                {
                    swipeStepDist = 0.0f;
                    swipeDist += (pos - swipePrevPos).magnitude;
                    swipePrevPos = pos;
                    swipeEnded = true;
                }
                swipeVel = swipeDist / (TimeManager.Instance.MasterSource.TotalTime - swipeStartTime);
            }
            else 
            {
                if (!ClockwiseTest(swipePrevPos, pos, spinDragAreaCenterPos))
                {
                    ResetSwipe();
                }
                else
                {
                    Vector2 vectorA = swipePrevPos - spinDragAreaCenterPos;
                    Vector2 vectorB = pos - spinDragAreaCenterPos;
                    angVel = Vector2.Angle(vectorA, vectorB);

                    swipeStepDist = (pos - swipePrevPos).magnitude;
                    swipeDist += swipeStepDist;
                    swipePrevPos = pos;
                }
            }
        }
#endif
        //Debug.Log("########## swipeVel " + swipeVel + " fingerOnSpinArea " + fingerOnSpinArea);

        if (swipeEnded && PlayerPersistentData.Instance.ExtraSpin > 0)
        {
            if (Mathf.Abs(wheelRb.angularVelocity) > minAngularVelocity && swipeDist > Screen.height * 0.1f) //0.125f
            {
                //Debug.Log("############## wheelRb.angularVelocity " + wheelRb.angularVelocity);
                goalSlice = ComputeReward();
                spinFromSwipe = true;
                wheelSpeed = Math.Abs(wheelRb.angularVelocity * 0.65f);//50.0f + UnityEngine.Random.Range(0.0f, 25.0f) + swipeVelCoeff * 240.0f; // 60.0f + UnityEngine.Random.Range(0.0f, 20.0f) + swipeVelCoeff * 160.0f;
                interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
                GetComponent<FiniteStateMachine>().State = (int)WheelStates.Spinning;
                //Debug.Log("############# START SPIN -> swipeVel: " + swipeVel + " -> speed: " + wheelSpeed + " goalSlice: " + goalSlice + " wheelSpeedDecreaseFactor: " + wheelSpeedDecreaseFactor);
            }
        }
        else
        {
            wheelSpeed = angVel / TimeManager.Instance.MasterSource.DeltaTime;
            //Debug.Log("wheelSpeed (STEP) " + wheelSpeed);
            JointMotor2D m = new JointMotor2D();
            m.maxMotorTorque = 10000.0f;
            m.motorSpeed = wheelSpeed;
            wheelHinge.motor = m;
            //wheelRb.isKinematic = true;
            enableTickSoundOnCollision = true;

            if (swipeEnded && PlayerPersistentData.Instance.ExtraSpin <= 0 && !noSpinPopupShowed)
            {
                ShowNoMoreSpinPopup(true);
                noSpinPopupShowed = true;
            }
        }
    }
    #endregion

    int ComputeReward()
    {
        int tmpGoalSlice = -1;

        List<WheelItem> tmpList = new List<WheelItem>();
        float rndValue = (float)(UnityEngine.Random.Range(0, 100000) % 1000) / 10.0f;

        //Near win logic--------------------------
        float nearMissRandom = (float)(UnityEngine.Random.Range(0, 100000) % 1000);
        if (nearMissRandom < OnTheRunDataLoader.Instance.GetNearMissPercentage() && rndValue > richPerc)
        {
            nearWinActive = true;
            rndValue = richPerc - 0.001f;
        }

        if (rndValue < richPerc)
            tmpList.AddRange(wheelRichPrices);
        else if (rndValue < richPerc + mediumPerc)
            tmpList.AddRange(wheelMediumPrices);
        else
            tmpList.AddRange(wheelCheapPrices);

        int sliceIndex = -1;
        do
        {
            sliceIndex = UnityEngine.Random.Range(0, 10000) % tmpList.Count;
        } while (sliceIndex == lastSliceIndex && tmpList.Count > 1);
        
        //Debug.Log("SELECTED INDEX = " + sliceIndex + " " + tmpList[sliceIndex].quantity + " " + tmpList[sliceIndex].item);
        for (int i = 0; i < wheelItems.Count; ++i)
        {
            if (wheelItems[i] == tmpList[sliceIndex])
            {
                tmpGoalSlice = i;
                break;
            }
        }
        lastSliceIndex = sliceIndex;


        if (nearWinActive)
        {
            float leftRightSlice = (float)(UnityEngine.Random.Range(0, 100000) % 100);
            tmpGoalSlice = leftRightSlice > 50.0f ? tmpGoalSlice + 1 : tmpGoalSlice - 1;
            nearWinDirection = leftRightSlice > 50.0f ? 1 : -1;
            if (tmpGoalSlice < 0)
                tmpGoalSlice = wheelItems.Count - 1;
            else if (tmpGoalSlice > wheelItems.Count - 1)
                tmpGoalSlice = 0;
        }


        //OLD ONE
        /*WheelItem.Level wantedLevel = WheelItem.Level.cheap;
        int rndValue = UnityEngine.Random.Range(0, 10000) % 100;
        if (rndValue < richPerc)
            wantedLevel = WheelItem.Level.rich;
        else if (rndValue < richPerc + mediumPerc)
            wantedLevel = WheelItem.Level.medium;
        //Debug.Log("############# ComputeReward -> wantedLevel: " + wantedLevel + " rndValue: " + rndValue);
        
        int startSliceIndex = UnityEngine.Random.Range(0, 10000) % 8;
        for (int i = 0; i < wheelItems.Count; ++i)
        {
            if (wheelItems[startSliceIndex].level == wantedLevel && startSliceIndex != lastSliceIndex)
            {
                tmpGoalSlice = startSliceIndex;
                break;
            }

            ++startSliceIndex;
            if (startSliceIndex >= wheelItems.Count)
                startSliceIndex = 0;
        }
        lastSliceIndex = tmpGoalSlice;*/

        //Debug.Log("############-> wantedLevel: " + wantedLevel + " - " + wheelItems[tmpGoalSlice].quantity + " " + wheelItems[tmpGoalSlice].item);
        //Debug.Log("############# ComputeReward -> tmpGoalSlice: " + tmpGoalSlice + " - " + wheelItems[tmpGoalSlice].quantity + " " + wheelItems[tmpGoalSlice].item);
        return tmpGoalSlice;
    }

    void ShowNoMoreSpinPopup(bool forceShow=false)
    {
        if (forceShow || backFromCurrencyPopup) //!noSpinPopupShowed || 
        {
            if (OnTheRunOmniataManager.Instance != null)
                OnTheRunOmniataManager.Instance.TrackSpinGame("NULL", 0);

            backFromCurrencyPopup = false;

            //Debug.Log("*** ShowNoMoreSpinPopup " + OnTheRunUITransitionManager.Instance.isInTransition);
            OnTheRunUITransitionManager.Instance.OpenPopup("NoMoreSpinsPopup");

            uiManager.FrontPopup.transform.FindChild("content/popupTitle").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("no_spin_popup_title");
            uiManager.FrontPopup.transform.FindChild("content/popupTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("no_spin_popup_text");
            uiManager.FrontPopup.transform.FindChild("content/tfExtraSpin").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("no_spin_popup_buy_text") + "\n" + extraSpinGained + " " + OnTheRunDataLoader.Instance.GetLocaleString("extra_spins");
            uiManager.FrontPopup.transform.FindChild("content/SkipButton/TextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("skip");
            uiManager.FrontPopup.transform.FindChild("content/ExtraSpinButton/tfTextfield").GetComponent<UITextField>().text = extraSpinCost.ToString();
        }
    }

    void CheckForAvailableCars()
    {
        SetupAvailableWheelCars();

        int carsRewardIndex = -1;
        for (int i = 0; i < wheelItems.Count; ++i)
        {
            if (wheelItems[i].item == UIWheelItem.WheelItem.Car)
            {
                carsRewardIndex = i;
                break;
            }
        }

        if (carAvailableForReward.Count == 0)
        {
            int rnd = 0;
            do
            {
                rnd = UnityEngine.Random.Range(0, richPrices.Count);
            } while (richPrices[rnd].item == UIWheelItem.WheelItem.Car);
            wheelItems[carsRewardIndex] = richPrices[rnd];
        }

        //SAVE CONFIGURATION -------------------------------------------------------------
        SaveWheelConfiguration();
        RefreshWheelGraphic();
    }

    void Update()
    {
        enableTickSoundOnCollision = false;

        if (!spinDragAreaInitialized)
        {
            InitSpinDragArea();
            spinDragAreaInitialized = true;
        }

        if (!isWheelRotating && GetComponent<FiniteStateMachine>().State == (int)WheelStates.RotateDots) // wheelRb.isKinematic)
        {
            if (uiManager == null)
                uiManager = Manager<UIManager>.Get();

            if (uiManager.FrontPopup != gameObject.GetComponent<UIPopup>())
                return;

            if (uiManager.disableInputs)
                return;

            UpdateDragArea(); // just for enter anim
            UpdateSwipe();
        }
        else
        {
            ResetSwipe();
        }
    }

    void FixedUpdate()
    {
        if (!redArrow.isKinematic)
        {
            float torque = redArrow.transform.rotation.z;
            redArrow.angularVelocity = 0.9f;
            redArrow.AddTorque(Mathf.Clamp(-torque * 150.0f, -30.0f, 30.0f));
        }

        if (!wheelRb.isKinematic)
        {

        }
    }
}