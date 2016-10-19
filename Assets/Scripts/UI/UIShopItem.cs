using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIShopItem")]
public class UIShopItem : MonoBehaviour
{
    public Sprite imageRandomWeb;
    public OnTheRunBooster.Booster boosterRef;
    OnTheRunInterfaceSounds interfaceSounds;
    protected UIRoot uiRoot;

	[HideInInspector]
	public GameObject 	BgRandomSelection, 
						BgActive, 
						BgPrize, 
						BuyButton,
						FxSoldOut;

	ParticleSystem FxStars = null;

    GameObject CoinIcon;
    GameObject DiamondIcon;
	UITextField ButtonPriceTf;
    Animation FxSoldOutAnim;
	//---------------------------------------------------//
	public bool IsSelected()
	{
		return BgActive.activeSelf;
	}
	//---------------------------------------------------//
	public bool IsPurchased()
	{
		return BuyButton.activeSelf == false;
	}
	//---------------------------------------------------//
	void Awake()
	{
		BuyButton = transform.FindChild("BuyBoosterButton").gameObject;
		BgRandomSelection = transform.Find("Background_random_selection").gameObject;
		BgPrize = transform.Find("bg_selection_prize").gameObject;
		BgActive = transform.Find("Background_active").gameObject;
		FxSoldOut = transform.Find("FxSoldOut").gameObject;
        FxSoldOut.transform.FindChild("SoldOut").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("item_purchased");
		FxStars = transform.Find("FxStars").gameObject.GetComponent<ParticleSystem>();
		BgRandomSelection.SetActive( false );
		BgActive.SetActive( false );
		BgPrize.SetActive( false );
		FxStars.Clear();
		FxStars.Stop();
        FxSoldOutAnim = FxSoldOut.GetComponent<Animation>();
		FxSoldOut.SetActive( false );
		CoinIcon = transform.FindChild("BuyBoosterButton/coin").gameObject;
        DiamondIcon = transform.FindChild("BuyBoosterButton/diamond").gameObject;
		ButtonPriceTf = transform.FindChild("BuyBoosterButton/tfTextfield").GetComponent<UITextField>();

#if UNITY_WEBPLAYER
        transform.Find("Item/Icons/Icon2").GetComponent<SpriteRenderer>().sprite = imageRandomWeb;
#endif

	}
	//---------------------------------------------------//
	public void GivePrize()
	{
		interfaceSounds.WheelCoinOrDiamonds(); 

		bool bought = boosterRef.TryBuying(1, true, true);

		if (bought && !boosterRef.equipped)
		{
			boosterRef.TryEquipping();
			transform.FindChild("BuyBoosterButton").gameObject.SetActive(false);
			Manager<UIManager>.Get().GetPage("InAppPage").SendMessage("UpdateAllButtons");
		}
		
		FxStars.GetComponent<ParticleSystem>().Play();
		
	}
	//---------------------------------------------------//
    void OnEnable()
    {
        uiRoot = Manager<UIRoot>.Get();
		FxSoldOut.SetActive( false );
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();

        //transform.FindChild("Badge/FxStars2D").particleSystem.renderer.sortingOrder = transform.FindChild("Badge/BadgeTextfield").GetComponent<UITextField>().sortingOrder + 1;
        
        //if (purchasedAnim == null)
        //    purchasedAnim = transform.FindChild("AnimatedBadge/Purchased").GetComponent<Animation>();

        //purchasedAnim.gameObject.SetActive(false);
    }
	//---------------------------------------------------//
	public void Disable()
	{
		BuyButton.SetActive( false );
		BgRandomSelection.SetActive( false );
		BgActive.SetActive( false );
        BgPrize.SetActive(false);
	}
	//---------------------------------------------------//
	public void Enable()
	{
		BuyButton.SetActive( true );
		BgRandomSelection.SetActive( false );
		BgActive.SetActive( false );
        BgPrize.SetActive(false);
	}

    void Signal_OnInfoReleased(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        //OnTheRunUITransitionManager.Instance.OpenPopup("SingleButtonPopup");
        OnTheRunUITransitionManager.Instance.OpenPopup("BoostDescriptionPopup");
        Manager<UIManager>.Get().FrontPopup.gameObject.transform.FindChild("content/popupTitle").GetComponent<UITextField>().text = button.transform.parent.GetComponent<UIShopItem>().boosterRef.displayedName;
        Manager<UIManager>.Get().FrontPopup.gameObject.transform.FindChild("content/popupTextField").GetComponent<UITextField>().text = button.transform.parent.GetComponent<UIShopItem>().boosterRef.description;
    }

    void Signal_OnMoreOver(UIButton button)
    {
        Transform shopItemTr = button.transform.parent;
        if (button.name.Contains("ItemButton"))
            shopItemTr = button.transform.parent.parent;
        shopItemTr.FindChild("BuyBoosterButton").GetComponent<UIButton>().State = UIButton.StateType.Pressed;
        
    }

    void Signal_OnMoreOut(UIButton button)
    {
        Transform shopItemTr = button.transform.parent;
        if(button.name.Contains("ItemButton"))
            shopItemTr = button.transform.parent.parent;
        shopItemTr.FindChild("BuyBoosterButton").GetComponent<UIButton>().State = UIButton.StateType.Normal;
    }

    void Signal_OnMoreReleased(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
		//if (boosterRef.type == OnTheRunBooster.BoosterType.SurprisePack && OnTheRunCoinsService.Instance.AreVideoAdsAvailable())
        if (ShouldOfferVideoAdForBooster(boosterRef.type))
		{
            OnTheRunCoinsService.Instance.WatchVideoForCallback((success) =>
                {
                    if (success)
                    {
                        bool bought = boosterRef.TryBuying(1, true, false);
                        OnBoosterPurchase(bought, button.transform.parent);

                        if (OnTheRunDaysCounterForAdvertising.Instance != null)
                            OnTheRunDaysCounterForAdvertising.Instance.OnVideoAdShown(OnTheRunCoinsService.VideoType.Booster);

                        if (OnTheRunOmniataManager.Instance != null)
                            OnTheRunOmniataManager.Instance.TrackWatchVideoAds(OnTheRunCoinsService.WatchVideoPlacement.ShopCoinsVideoAdsPlacement);

                        UpdateButtonText();
                    }
                });

			UpdateButtonText();
			//UpdateButtonText();
		}
		else
        {
			bool bought = boosterRef.TryBuying(1);
			OnBoosterPurchase(bought, button.transform.parent);
			/*if (bought && !boosterRef.equipped)
	        {
	            boosterRef.TryEquipping();
	            Transform shopItemTr = button.transform.parent;
	            UpdateInAppButton(shopItemTr);
	            button.transform.parent.FindChild("BuyBoosterButton").gameObject.SetActive(false);
	            Manager<UIManager>.Get().GetPage("InAppPage").SendMessage("UpdateAllButtons");
				FxStars.GetComponent<ParticleSystem>().Play();
				FxSoldOut.SetActive( false );
				FxSoldOut.SetActive( true );
	        }*/
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

    void Signal_OnItemPressed(UIButton button)
    {
        Transform shopItemTr = button.transform.parent.parent;

        if (shopItemTr.FindChild("BuyBoosterButton").gameObject.activeInHierarchy)
        {
            interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

            if (ShouldOfferVideoAdForBooster(boosterRef.type))
            {
                OnTheRunCoinsService.Instance.WatchVideoForCallback((success) =>
                {
                    if (success)
                    {
                        bool bought = boosterRef.TryBuying(1, true, false);
                        OnBoosterPurchase(bought, shopItemTr);

                        if (OnTheRunDaysCounterForAdvertising.Instance != null)
                            OnTheRunDaysCounterForAdvertising.Instance.OnVideoAdShown(OnTheRunCoinsService.VideoType.Booster);

                        if (OnTheRunOmniataManager.Instance != null)
                            OnTheRunOmniataManager.Instance.TrackWatchVideoAds(OnTheRunCoinsService.WatchVideoPlacement.ShopCoinsVideoAdsPlacement);
                    }
                    UpdateButtonText();
                });

                UpdateButtonText();
                //UpdateButtonText();
            }
            else
            {
                bool bought = boosterRef.TryBuying(1);
                OnBoosterPurchase(bought, shopItemTr);
            }
        }
    }

	void OnBoosterPurchase(bool bought, Transform shopItemTr)
	{
		if (bought && !boosterRef.equipped)
		{
			boosterRef.TryEquipping();
			UpdateInAppButton(shopItemTr);
			shopItemTr.FindChild("BuyBoosterButton").gameObject.SetActive(false);
			Manager<UIManager>.Get().GetPage("InAppPage").SendMessage("UpdateAllButtons");
			FxStars.GetComponent<ParticleSystem>().Play();
			FxSoldOut.SetActive( false );
			FxSoldOut.SetActive( true );
            FxSoldOutAnim.Play(FxSoldOut.transform.parent.localPosition.x > 0 ? "SoldOutLeft@" : "SoldOutRight@");
		}
	}

	public void UpdateButtonText()
    {
        if (OnTheRunGameplay.Instance == null)
            return;

        Log("UpdateButtonText - boosterType: " + boosterRef.type + " - GameState: " + OnTheRunGameplay.Instance.GameState + " - shouldOfferVideo: " + ShouldOfferVideoAdForBooster(boosterRef.type));
        
		if (OnTheRunGameplay.Instance.GameState == OnTheRunGameplay.GameplayStates.Start)
			return;

		//if (boosterRef.type == OnTheRunBooster.BoosterType.SurprisePack && OnTheRunCoinsService.Instance.AreVideoAdsAvailable())
        if (ShouldOfferVideoAdForBooster(boosterRef.type))
		{
            DiamondIcon.SetActive(false);
			CoinIcon.SetActive(false);
			ButtonPriceTf.resizeButton = false;
            ButtonPriceTf.text = OnTheRunDataLoader.Instance.GetLocaleString("btVideo");
		}
		else
        {
            /*DiamondIcon.SetActive(false);
            CoinIcon.SetActive(true);
            if (boosterRef.type == OnTheRunBooster.BoosterType.DoubleCoins)
            {
                DiamondIcon.SetActive(true);
                CoinIcon.SetActive(false);
            }*/
            CoinIcon.SetActive(boosterRef.priceData.Currency == PriceData.CurrencyType.FirstCurrency);
            DiamondIcon.SetActive(boosterRef.priceData.Currency == PriceData.CurrencyType.SecondCurrency);
            ButtonPriceTf.resizeButton = false; // true;
            ButtonPriceTf.text = uiRoot.FormatTextNumber((int)boosterRef.priceData.Price);
		}
	}

    void EquipAndUpdateInAppButton(Transform shopItemTr)
    {
        if (boosterRef.type != OnTheRunBooster.BoosterType.SurprisePack && boosterRef.type != OnTheRunBooster.BoosterType.MultiplePack)
        {
            UpdateInAppButton(shopItemTr);
            shopItemTr.FindChild("BuyBoosterButton").gameObject.SetActive(!boosterRef.equipped);
        }
        else
        {
            if (boosterRef.used)
            {
                shopItemTr.FindChild("Background_normal").gameObject.SetActive(true);
                shopItemTr.FindChild("Background_active").gameObject.SetActive(false);
                shopItemTr.FindChild("BuyBoosterButton").gameObject.SetActive(false);
            }
        }

        if (boosterRef.type == OnTheRunBooster.BoosterType.SpecialCar && boosterRef.equipped)
            shopItemTr.FindChild("bg_selection_prize").gameObject.SetActive(false);
    }

    void UpdateInAppButton(Transform shopItemTr)
    {
        shopItemTr.FindChild("Background_normal").gameObject.SetActive(!boosterRef.equipped);
        shopItemTr.FindChild("Background_active").gameObject.SetActive(boosterRef.equipped);
    }

    void PlayBoosterPurchasedAnim(OnTheRunBooster.Booster booster)
    {
        if (booster == boosterRef)
        {
            if (booster.type != OnTheRunBooster.BoosterType.MultiplePack && boosterRef.type != OnTheRunBooster.BoosterType.SurprisePack)
            {
                //transform.FindChild("Badge/FxStars2D").particleSystem.Play();
                //purchasedAnim.gameObject.SetActive(true);
                //purchasedAnim.Play();
            }
        }
    }

    void Log(string logStr)
    {
        //Debug.Log("@@@ UIShopItem - " + logStr);
    }
}
