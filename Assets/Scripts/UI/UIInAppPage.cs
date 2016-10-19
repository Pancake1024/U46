using UnityEngine;
using SBS.Core;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("OnTheRun/UI/UIInAppPage")]
public class UIInAppPage : MonoBehaviour
{
	public UIButton PlayButton;
    //public GameObject ShopElementPrefab;
    public int numItems = 6; //8;
    public UINewMissionsPanel missionPanel;
    public UITextField freeBoosterText;
    public UITextField getBoosterText;
    public GameObject[] pageElements;
    public GameObject freeBoosterElement;

    OnTheRunGameplay gameplayManager;
    OnTheRunInterfaceSounds interfaceSounds;
    bool elementsHeveBeenAdded = false;
	bool purchasedRandomPowerup = false;
    UIButton freeBoosterButton;

    float spaceLeftBelow = 0.14f;
    float aspectRatio;
    int numColumns = 0;
    int numRows = 0; 
	UIRoot uiRoot = null;

    protected GameObject playArrowEnable;
    protected GameObject playArrowDisable;

    protected bool oncePerRun = true;
    
    void OnEnable()
    {
/*#if UNITY_WEBPLAYER
        transform.FindChild("BottomLeftAnchor/tfBarLabel").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_inapp_descr");
#else
        transform.FindChild("BottomLeftAnchor/tfBarLabel").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("inapp_descr");
#endif*/
        
        transform.FindChild("BottomRightAnchor/PlayButton/TextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("btPlay");
        playArrowEnable = PlayButton.transform.FindChild("Arrow_enable").gameObject;
        playArrowEnable.SetActive(true);
        playArrowDisable = PlayButton.transform.FindChild("Arrow_disable").gameObject;
        playArrowDisable.SetActive(false);

        if (gameplayManager==null)
            gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();

        if (interfaceSounds == null)
            interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();

		if( uiRoot == null )
			uiRoot = Manager<UIRoot>.Get();

        aspectRatio = Screen.width / Screen.height;
        if (!elementsHeveBeenAdded)
            AddShopElements();

        if (freeBoosterButton == null)
        {
            freeBoosterButton = freeBoosterElement.transform.FindChild("BuyBoosterButton").GetComponent<UIButton>();
        }

//#if UNITY_ANDROID || UNITY_WP8 || UNITY_WEBPLAYER
        //transform.FindChild("TopLeftAnchor/CheatButton").gameObject.SetActive(false);
//#endif
    }

	List<UIShopItem> shopItems = new List<UIShopItem>();

    void AddShopElements()
    {
		shopItems.Clear();

        int elementIndex = 0;
        for (int i = 0; i < pageElements.Length; ++i)
        {
            GameObject currentElement = pageElements[i];
            //currentElement.transform.parent = gameObject.transform;

            currentElement.transform.FindChild("Background_active").gameObject.SetActive(false);
#if UNITY_WEBPLAYER
                currentElement.transform.FindChild("btMark/icon").GetComponent<UITextField>().text = "?";
#endif

            shopItems.Add(currentElement.GetComponent<UIShopItem>());

            //FillData(currentElement, elementIndex);
            ++elementIndex;
        }
        elementsHeveBeenAdded = true;


        /*GameObject firstElement = GameObject.Instantiate(ShopElementPrefab) as GameObject;
        UINineSlice shopElementBg = firstElement.transform.FindChild("Background_normal").GetComponent<UINineSlice>();

		shopItems.Add( firstElement.GetComponent<UIShopItem>() );

        float elementWidth = shopElementBg.width;
        float elementHeight = shopElementBg.height;

        //Debug.Log("elementWidth: " + elementWidth + ", elementHeight: " + elementHeight);

        float baseScreenWidth = Manager<UIManager>.Get().baseScreenWidth;
        float baseScreenHeight = Manager<UIManager>.Get().baseScreenHeight;
        float pixelsPerUnit = Manager<UIManager>.Get().pixelsPerUnit;
        int maxColumns = Mathf.FloorToInt((baseScreenWidth / pixelsPerUnit) / elementWidth);
        maxColumns = 6;
        //Debug.Log("maxColumns: " + maxColumns);
        
        numColumns = numItems < maxColumns ? numItems : maxColumns;
        numRows = Mathf.CeilToInt(numItems / maxColumns);

        //Debug.Log("numRows: " + numRows + ", " + "numColumns: " + numColumns);

        float horizontalResidue = baseScreenWidth / pixelsPerUnit - numColumns * elementWidth - aspectRatio;
        float horizontalSpacing = (horizontalResidue / (numColumns + 1));

        float verticalResidue = baseScreenHeight / pixelsPerUnit - numRows * elementHeight;
        float verticalSpacing = verticalResidue / (numRows + 1);

        verticalSpacing = 0.2f;

        //Debug.Log("@@@ horizontalSpacing: " + horizontalSpacing + ", " + "verticalSpacing: " + verticalSpacing);

        Vector3 offset = new Vector3(-1.0f * (elementWidth + horizontalSpacing) * (numColumns - 1) * 0.5f, (elementHeight + verticalSpacing) * (numRows - 1) * 0.5f + spaceLeftBelow, 0.0f);
        float horizontalIncrement = elementWidth + horizontalSpacing;
        float verticalIncrement = elementHeight + verticalSpacing;
        
        int elementIndex = 0;
        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numColumns; col++)
            {
                GameObject currentElement = null;
                if (row == 0 && col == 0)
                    currentElement = firstElement;
                else
                    currentElement = GameObject.Instantiate(ShopElementPrefab) as GameObject;

                currentElement.transform.position = gameObject.transform.position + offset + new Vector3(col * horizontalIncrement, -1.0f * row * verticalIncrement, 0.0f) + Vector3.down * 0.1f;
                currentElement.transform.parent = gameObject.transform;

                currentElement.transform.FindChild("Background_active").gameObject.SetActive(false);
				//currentElement.transform.FindChild("Price/PriceFrame_normal").gameObject.SetActive(true);
				//currentElement.transform.FindChild("Price/PriceFrame_active").gameObject.SetActive(false);
#if UNITY_WEBPLAYER
                currentElement.transform.FindChild("btMark/icon").GetComponent<UITextField>().text = "?";
#endif

				shopItems.Add( currentElement.GetComponent<UIShopItem>() );

                FillData(currentElement, elementIndex);
                ++elementIndex;
            }
        }
        elementsHeveBeenAdded = true;*/
    }


    void DisableBoosters(bool active)
    {
        for (int i = 0; i < shopItems.Count; ++i)
        {
            shopItems[i].transform.FindChild("btMark").GetComponent<BoxCollider2D>().enabled = !active;
            shopItems[i].transform.FindChild("BuyBoosterButton").GetComponent<BoxCollider2D>().enabled = !active;
            shopItems[i].transform.FindChild("Item/ItemButton").gameObject.SetActive(!active);
        }
    }

    void OnShopEnter(UIPage page)
    {
        //UpdateCheatButton(transform.FindChild("TopLeftAnchor/CheatButton").GetComponent<UIButton>());

        UpdateFreeVideoButtonText(OnTheRunBooster.Instance.GetBoosterById(OnTheRunBooster.BoosterType.SurprisePack));
        getBoosterText.text = OnTheRunDataLoader.Instance.GetLocaleString("get_boosters");

        Manager<UIRoot>.Get().ActivateBackground(false);

        Manager<UIRoot>.Get().ShowUpperPageBorders(true);
        Manager<UIRoot>.Get().ShowCommonPageElements(true, true, true, false, false);

        int elementIndex = 0;
        int childrensNum = pageElements.Length;
        int howManyBought = 0;
        for (int i = 0; i < childrensNum; i++)
        {
            GameObject currentElement = pageElements[i];
            if (currentElement.name.Contains("ShopItem"))
            {
                currentElement.transform.FindChild("Background_active").gameObject.SetActive(false);

                bool bought = FillData(currentElement, elementIndex);
                if (bought)
                    ++howManyBought;
                ++elementIndex;
            }
        }

        UpdateFreeBoosterButton(howManyBought);

        if (!OnTheRunSounds.Instance.IsInGameMusicPlaying())
            OnTheRunSounds.Instance.PlayInGameMusic( );

        gameplayManager.MainCamera.SendMessage("OnResetCameraEvent");
        gameplayManager.MainCamera.SendMessage("OnSlideshowEvent");
        gameplayManager.ResetRunParameter();

        missionPanel.UpdatePanel(OnTheRunSingleRunMissions.Instance.CurrentTierMission);
    }

    void UpdateFreeBoosterButton(int howManyBought)
    {
#if UNITY_WEBPLAYER
        freeBoosterButton.transform.parent.gameObject.SetActive(false);
#else
        freeBoosterButton.State = UIButton.StateType.Normal;
        if (purchasedRandomPowerup || howManyBought == shopItems.Count || !ShouldOfferVideoAdForBooster(OnTheRunBooster.BoosterType.SurprisePack))
        {
            freeBoosterButton.State = UIButton.StateType.Disabled;
            freeBoosterButton.GetComponent<Animation>().Stop();
        }
        else
        {
            freeBoosterButton.GetComponent<Animation>().Stop();
            freeBoosterButton.GetComponent<Animation>().Play();
        }

        if (!oncePerRun)
        {
            freeBoosterButton.State = UIButton.StateType.Disabled;
            freeBoosterButton.GetComponent<Animation>().Stop();
        }
#endif
    }

    void OnShopExit(UIPage page)
    {
        Manager<UIRoot>.Get().ShowCommonPageElements(false, false, false, false, false);
    }

    bool FillData(GameObject shopElement, int elementIndex)
    {
        UIShopItem shopItemComp = shopElement.GetComponent<UIShopItem>();

        OnTheRunBooster.Booster currBooster = null;
        switch (elementIndex)
        {
            case 0:
                currBooster = OnTheRunBooster.Instance.GetBoosterById(OnTheRunBooster.BoosterType.SpecialCar);
                OnTheRunSingleRunMissions.DriverMission currentMission = OnTheRunSingleRunMissions.Instance.CurrentTierMission;
                bool missionRequireSpecialVehicle = currentMission.SpecialVehicleNeeded != TruckBehaviour.TrasformType.None && !currentMission.Done;
                shopItemComp.transform.FindChild("bg_selection_prize").gameObject.SetActive(false);
                if (TruckBehaviour.randomTruckFromInAppPage == TruckBehaviour.TrasformType.None || missionRequireSpecialVehicle)
                {
                    if (missionRequireSpecialVehicle)
                    {
                        shopElement.transform.FindChild("Background_normal").gameObject.SetActive(false);
                        shopItemComp.transform.FindChild("bg_selection_prize").gameObject.SetActive(true);
                        TruckBehaviour.randomTruckFromInAppPage = currentMission.SpecialVehicleNeeded;
                    }
                    else
                        TruckBehaviour.randomTruckFromInAppPage = gameplayManager.gameObject.GetComponent<OnTheRunSpawnManager>().GetNextSpecialVehicles();

                }
                SpriteRenderer spriteRenderer = shopItemComp.transform.FindChild("Item/Icons/Icon5").GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = OnTheRunRankManager.Instance.specialVehiclesSprites[(int)TruckBehaviour.randomTruckFromInAppPage];
                break;
            case 1:
                currBooster = OnTheRunBooster.Instance.GetBoosterById(OnTheRunBooster.BoosterType.Turbo);
                break;
            case 2:
                currBooster = OnTheRunBooster.Instance.GetBoosterById(OnTheRunBooster.BoosterType.Shield);
                break;
            case 3:
                currBooster = OnTheRunBooster.Instance.GetBoosterById(OnTheRunBooster.BoosterType.DoubleCoins);
                break;
            case 4:
                currBooster = OnTheRunBooster.Instance.GetBoosterById(OnTheRunBooster.BoosterType.DoubleExp);
                break;
            case 5:
                currBooster = OnTheRunBooster.Instance.GetBoosterById(OnTheRunBooster.BoosterType.MoreCheckpointTime);
                break;
        }
        shopItemComp.boosterRef = currBooster;
        currBooster.shopItemComp = shopItemComp;

        bool boosterEquipped = currBooster.equipped;
        if (currBooster.type == OnTheRunBooster.BoosterType.SurprisePack || currBooster.type == OnTheRunBooster.BoosterType.MultiplePack)
            boosterEquipped = currBooster.used;

        shopElement.transform.FindChild("BuyBoosterButton").gameObject.SetActive(false);
        shopElement.transform.FindChild("BuyBoosterButton").gameObject.SetActive(!boosterEquipped);

        shopElement.transform.FindChild("BuyBoosterButton/tfTextfield").GetComponent<UITextField>().text = Manager<UIRoot>.Get().FormatTextNumber((int)currBooster.priceData.Price);

        shopElement.transform.FindChild("BuyBoosterButton/coin").gameObject.SetActive(currBooster.priceData.Currency == PriceData.CurrencyType.FirstCurrency);
        shopElement.transform.FindChild("BuyBoosterButton/diamond").gameObject.SetActive(currBooster.priceData.Currency == PriceData.CurrencyType.SecondCurrency);

        shopElement.transform.FindChild("Background_normal").gameObject.SetActive(!boosterEquipped);
        shopElement.transform.FindChild("Background_active").gameObject.SetActive(boosterEquipped);
        if (boosterEquipped && elementIndex==0)
            shopItemComp.transform.FindChild("bg_selection_prize").gameObject.SetActive(false);

        if (currBooster.type == OnTheRunBooster.BoosterType.SurprisePack || currBooster.type == OnTheRunBooster.BoosterType.MultiplePack)
        {
            shopElement.transform.FindChild("Background_normal").gameObject.SetActive(true);
            shopElement.transform.FindChild("Background_active").gameObject.SetActive(false);
        }

        //Icon----------------------------
        Transform node = shopElement.transform.FindChild("Item/Icons");
        for (int i = 0; i < node.childCount; ++i)
        {
            node.GetChild(i).gameObject.SetActive(false);
        }

        switch (currBooster.type)
        {
            case OnTheRunBooster.BoosterType.MultiplePack:
                node.FindChild("Icon1").gameObject.SetActive(true);
                break;
            case OnTheRunBooster.BoosterType.SurprisePack:
                node.FindChild("Icon2").gameObject.SetActive(true);
                break;
            case OnTheRunBooster.BoosterType.DoubleCoins:
                node.FindChild("Icon3").gameObject.SetActive(true);
                break;
            case OnTheRunBooster.BoosterType.MoreCheckpointTime:
                node.FindChild("Icon4").gameObject.SetActive(true);
                break;
            case OnTheRunBooster.BoosterType.SpecialCar:
                node.FindChild("Icon5").gameObject.SetActive(true);
                break;
            case OnTheRunBooster.BoosterType.Turbo:
                node.FindChild("Icon6").gameObject.SetActive(true);
                break;
            case OnTheRunBooster.BoosterType.DoubleExp:
                node.FindChild("Icon7").gameObject.SetActive(true);
                break;
            case OnTheRunBooster.BoosterType.Shield:
                node.FindChild("Icon8").gameObject.SetActive(true);
                break;
        }
        
		shopItemComp.UpdateButtonText();

        return boosterEquipped;

        //shopElement.transform.FindChild("Badge").gameObject.SetActive(false);
        /*if (currBooster.type == OnTheRunBooster.BoosterType.MultiplePack || currBooster.type == OnTheRunBooster.BoosterType.SurprisePack)
        {
            shopElement.transform.FindChild("Badge").gameObject.SetActive(false);
        }*/
    }
	//-------------------------------------------//
	public void SpinForSurprisePack()
	{
		if (!elementsHeveBeenAdded)
			return;

        DisableBoosters(true);

		// disable stuff when waiting for prize...
#if !UNITY_WEBPLAYER
		uiRoot.DisablePlusButtons();
#endif
		uiRoot.HomeButton.State = UIButton.StateType.Disabled;
        PlayButton.State = UIButton.StateType.Disabled;
        playArrowEnable.SetActive(PlayButton.State == UIButton.StateType.Disabled ? false : true);
        playArrowDisable.SetActive(PlayButton.State == UIButton.StateType.Disabled ? true : false); 

		randomSpinTime = UnityEngine.Random.Range(1.0f,6.0f);
		blinkTimeout = 0.1f;
        currentRandomIndex = UnityEngine.Random.Range(0, 5);
        
		UIShopItem lastItemLeft = FindLeftItem();

		if( lastItemLeft != null )
		{
			lastItemSelected = lastItemLeft;
			EndSpinForSurprisePack();
		}
		else
			spinning = true;	
	}
	//-------------------------------------------//
	void EndSpinForSurprisePack()
	{
		if (!elementsHeveBeenAdded)
			return;

        DisableBoosters(false);

		// re enable buttons and give prize ...
#if !UNITY_WEBPLAYER
		uiRoot.EnablePlusButtons();
#endif
        uiRoot.HomeButton.State = UIButton.StateType.Normal;
        PlayButton.State = UIButton.StateType.Normal;
        playArrowEnable.SetActive(PlayButton.State == UIButton.StateType.Disabled ? false : true);
        playArrowDisable.SetActive(PlayButton.State == UIButton.StateType.Disabled ? true : false); 
		lastItemSelected.BgActive.SetActive( true );
		lastItemSelected.BgRandomSelection.SetActive( false );
		lastItemSelected.GivePrize();
		purchasedRandomPowerup = true;

		spinning = false;
	}
	//-------------------------------------------//
	float randomBlinkTimer = 0f;
	float randomSpinTime = 0f;
	float blinkTimeout = 0.1f;
	int currentRandomIndex = 0;
	UIShopItem lastItemSelected = null;
	bool spinning = false;
	const float MAX_BLINK_TIMEOUT = 0.5f;
	//-------------------------------------------//
	void Update()
	{
		if (!elementsHeveBeenAdded)
			return;

		if( uiRoot == null )
			return;

		float now = TimeManager.Instance.MasterSource.TotalTime;
		randomSpinTime -= TimeManager.Instance.MasterSource.DeltaTime;

		// end of blinking ...
		if( spinning && randomSpinTime <= 0f && now - randomBlinkTimer > blinkTimeout && blinkTimeout > MAX_BLINK_TIMEOUT )
		{
			EndSpinForSurprisePack();
			spinning = false;
			return;
		}
		
		// blink !
		if( spinning && now - randomBlinkTimer > blinkTimeout )
		{
			randomBlinkTimer = now;
			blinkTimeout *= 1.1f;

			// end cycle and disable the last one
			/*if( currentRandomIndex == shopItems.Count )
			{
				currentRandomIndex = 0;
				shopItems[ shopItems.Count-1 ].BgRandomSelection.SetActive( false );
                randomBlinkTimer = now - 1f;
			}*/

			// keep cycling...
			UIShopItem item = shopItems[ currentRandomIndex ];


			// disable the old one
			if( currentRandomIndex-1 >= 0 )
				shopItems[ currentRandomIndex-1 ].BgRandomSelection.SetActive( false );
            else
                shopItems[shopItems.Count - 1].BgRandomSelection.SetActive(false);

			if( item.IsSelected() || item.IsPurchased() )
			{
				++currentRandomIndex;
                randomBlinkTimer = now - 1f;
				return;
			}

			lastItemSelected = item;
			item.BgRandomSelection.SetActive( true );
			interfaceSounds.WheelTick(); 
			++currentRandomIndex;

            if (currentRandomIndex == shopItems.Count)
                currentRandomIndex = 0;
		}
	}
	//-------------------------------------------//
	UIShopItem FindLeftItem()
	{
		int iLen = shopItems.Count;
		int iCount = 0;
		UIShopItem kFound= null;

		for ( int i= 0; i< iLen -2 ; ++i )
		{
			if( shopItems[i].IsSelected() == false && shopItems[i].IsPurchased() == false )
			{
				++iCount;
				kFound = shopItems[i];
			}
		}

		if( iCount == 1 )
			return kFound;
		else 
			return null;
	}
    //-------------------------------------------//
    #region Free Video Booster
    void Signal_OnFreeBoosterPressed(UIButton button)
    {
        OnTheRunBooster.Booster boosterRef = OnTheRunBooster.Instance.GetBoosterById(OnTheRunBooster.BoosterType.SurprisePack);
        if (ShouldOfferVideoAdForBooster(OnTheRunBooster.BoosterType.SurprisePack))
		{
            OnTheRunCoinsService.Instance.WatchVideoForCallback((success) =>
            {
                if (success)
                {
                    oncePerRun = false;
                    bool bought = boosterRef.TryBuying(1, true, false);
                    OnBoosterPurchase(bought, button.transform.parent, boosterRef, button);

                    if (OnTheRunDaysCounterForAdvertising.Instance != null)
                        OnTheRunDaysCounterForAdvertising.Instance.OnVideoAdShown(OnTheRunCoinsService.VideoType.Booster);

                    if (OnTheRunOmniataManager.Instance != null)
                        OnTheRunOmniataManager.Instance.TrackWatchVideoAds(OnTheRunCoinsService.WatchVideoPlacement.ShopCoinsVideoAdsPlacement);
                }
                UpdateFreeVideoButtonText(boosterRef);
            });

            UpdateFreeVideoButtonText(boosterRef);

            if (!oncePerRun)
            {
                freeBoosterButton.State = UIButton.StateType.Disabled;
                freeBoosterButton.GetComponent<Animation>().Stop();
            }
		}
		else
        {
			bool bought = boosterRef.TryBuying(1, true);
            OnBoosterPurchase(bought, button.transform.parent, boosterRef, button);
		}
    }

    bool ShouldOfferVideoAdForBooster(OnTheRunBooster.BoosterType boosterType)
    {
        bool isAllowedBooster = boosterType == OnTheRunBooster.BoosterType.SurprisePack;
        return isAllowedBooster && OnTheRunCoinsService.Instance.IsBoosterVideoAvailable();
        /*
        bool videosAreAvailable = false;

#if UNITY_ANDROID
        videosAreAvailable = AndroidVideoAdsManager.Instance.AreVideoAdsAvailable();
#else
        videosAreAvailable = OnTheRunCoinsService.Instance.AreVideoAdsAvailable();
#endif

        int alreadyShown = OnTheRunDaysCounterForAdvertising.Instance.BoosterVideosShownToday;
        int limit = OnTheRunDataLoader.Instance.GetBoostVideoAdLimitPerDay();
        bool reachedAdLimit = (alreadyShown >= limit);

        const bool CHEAT_AD_LIMIT = false;
        if (CHEAT_AD_LIMIT)
            reachedAdLimit = false;

        return isAllowedBooster && videosAreAvailable && !reachedAdLimit;
        */
    }

    void OnBoosterPurchase(bool bought, Transform shopItemTr, OnTheRunBooster.Booster boosterRef, UIButton button)
    {
        if (bought && !boosterRef.equipped)
        {
            boosterRef.TryEquipping();

            button.State = UIButton.StateType.Disabled;

            Manager<UIManager>.Get().GetPage("InAppPage").SendMessage("UpdateAllButtons");
        }
    }

    void UpdateFreeVideoButtonText(OnTheRunBooster.Booster boosterRef)
    {
        //if (ShouldOfferVideoAdForBooster(OnTheRunBooster.BoosterType.SurprisePack))
            freeBoosterText.text = OnTheRunDataLoader.Instance.GetLocaleString("btVideo");
        //else
        //    freeBoosterText.text = uiRoot.FormatTextNumber((int)boosterRef.priceData.Price);
    }
    #endregion

    IEnumerator Signal_OnPlayPressed(UIButton button)
    {
        UISaveMeItem.IsFirstTimeInSession = true;

        gameplayManager.MainCamera.SendMessage("OnSlideshowEvent");

        OnTheRunUITransitionManager.Instance.OnPageExiting("InAppPage", "InGamePage");

        if (!OnTheRunFuelManager.Instance.IsFuelFreezeActive())
            OnTheRunFuelManager.Instance.Fuel--;

        //float initTime = Time.realtimeSinceStartup;

        PlayerPersistentData.Instance.Save(false);
        //Debug.Log("11--> " + (Time.realtimeSinceStartup-initTime));

        while (UIEnterExitAnimations.activeAnimationsCounter > 0)
        {
            yield return null;
        }

        Manager<UIRoot>.Get().UpdateCurrenciesItem();

        Manager<UIRoot>.Get().ShowUpperPageBorders(false);
        Manager<UIManager>.Get().PopPopup();

        //Debug.Log("22--> " + (Time.realtimeSinceStartup - initTime));
        //OnTheRunInterfaceSounds interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

        Manager<UIManager>.Get().GoToPage("IngamePage");
        Manager<UIRoot>.Get().ShowCurrenciesItem(false);
        OnTheRunBooster.Instance.SaveBoosters();
        gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Racing);
        OnTheRunUITransitionManager.Instance.OnPageChanged("InGamePage", "InAppPage");

        if (OnTheRunIngameHiScoreCheck.Instance)
            OnTheRunIngameHiScoreCheck.Instance.OnGameplayStarted();

        if (OnTheRunOmniataManager.Instance)
            OnTheRunOmniataManager.Instance.OnRunStarted();

        if (OnTheRunAchievements.Instance)
            OnTheRunAchievements.Instance.OnRunStarted();

        if (OnTheRunFyberManager.Instance)
            OnTheRunFyberManager.Instance.AbortAnyPendigVideoRequest();

        purchasedRandomPowerup = false;
        oncePerRun = true;

        //Debug.Log("33--> " + (Time.realtimeSinceStartup - initTime));
        gameplayManager.StartReadyGoSequence();
    }

    void RefreshVideoButton()
    {
        UpdateAllButtons();
    }
    
    void UpdateAllButtons()
    {
        int boosterEquippedCount = 0;
        GameObject buttonsRoot = gameObject.transform.FindChild("CenterAnchor/Buttons").gameObject;
        for (int i = 0; i < buttonsRoot.transform.childCount; ++i)
        {
            GameObject currGo = buttonsRoot.transform.GetChild(i).gameObject;
            if (currGo.name.Contains("ShopItem"))
            {
                currGo.GetComponent<UIShopItem>().SendMessage("EquipAndUpdateInAppButton", currGo.transform);
                if (currGo.GetComponent<UIShopItem>().boosterRef.equipped)
                    ++boosterEquippedCount;
            }
        }

        UpdateFreeBoosterButton(boosterEquippedCount);

        if (boosterEquippedCount == 6)
        {
            for (int i = 0; i < buttonsRoot.transform.childCount; ++i)
            {
                GameObject currGo = buttonsRoot.transform.GetChild(i).gameObject;
                UIShopItem currShopItem = currGo.GetComponent<UIShopItem>();
                if (currGo.name.Contains("ShopItem"))
                {
                    if(currShopItem.boosterRef.type == OnTheRunBooster.BoosterType.SurprisePack || currShopItem.boosterRef.type == OnTheRunBooster.BoosterType.MultiplePack)
                    {
                        currShopItem.boosterRef.used = true;
                        currGo.GetComponent<UIShopItem>().SendMessage("EquipAndUpdateInAppButton", currGo.transform);
                    }
                }
            }
        }
    }

    void OnSpacePressed()
    {
        StartCoroutine(Signal_OnPlayPressed(null));
    }


    /*void OnCheatButtonPressed(UIButton button)
    {
        OnTheRunDataLoader.ABTesting_Flag = !OnTheRunDataLoader.ABTesting_Flag;
        UpdateCheatButton(button);
    }

    void UpdateCheatButton(UIButton button)
    {
        UITextField text = button.gameObject.transform.FindChild("tfTextfield").GetComponent<UITextField>();

        if (OnTheRunDataLoader.ABTesting_Flag)
            text.text = "NEW MODE ON";
        else
            text.text = "OLD MODE ON";

        text.ApplyParameters();
    }*/
}