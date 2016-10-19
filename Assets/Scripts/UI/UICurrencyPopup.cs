using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[AddComponentMenu("OnTheRun/UI/UICurrencyPopup")]
public class UICurrencyPopup : MonoBehaviour
{
    const int freeCoinsIndex = 5;

    protected int[] tempMoneyQuantity = { 10000, 5000, 3000, 2000, 1000, 500 };                 // Now from XML...
    protected int[] tempDiamondsQuantity = { 200, 150, 100, 50, 40, 30 };                       // Now from XML...
    protected int[] tempFuelQuantity = { 700, 250, 150, 50, 20, 10 };                           // Now from XML...
    protected float[] tempMoneyPrice = { 16, 15, 10, 5, 2, 1 };                                 // Now from XML...
    protected float[] tempFuelPrice = { 100, 30, 16, 9, 3, 1 };                                 // Now from XML...
    protected float[] tempDiamondsPrice = { 100.99f, 30.99f, 15.99f, 8.99f, 2.99f, 1.9f };

    protected UISharedData sharedData;
    protected OnTheRunInterfaceSounds interfaceSounds;

    protected UICurrencyShopItem[] items = new UICurrencyShopItem[6];

    protected ShopPopupTypes currentPopupType = ShopPopupTypes.Money;

    protected int tempDiamondsIndex = -1;
    protected int tempMoneyIndex = -1;

    protected ShopPopupTypes currencyMissing;

    const bool PURCHASE_COINS_WITH_DIAMONDS = true; // Instead of real money

    [HideInInspector]
    static public bool isFromNotEnoughCoins = false;

    public GameObject[] freeMoneyIcons;

    public class ShopOffers
    {
        public ShopPopupTypes type;
        public List<int> specialOffers;
        public List<int> bestOffers;

        public ShopOffers()
        {
            specialOffers = new List<int>();
            bestOffers = new List<int>();
        }
    }

    public enum ShopPopupTypes
    { 
        None = -1,
        Money,
        Diamonds,
        Fuel,
        FuelFreeze
    }

    protected UICurrencyPopup.ShopPopupTypes lastPopupType = ShopPopupTypes.None;
    void OnEnable()
    {
        if (lastPopupType != ShopPopupTypes.None)
        {
            Initialize(lastPopupType);
        }
    }

    void Awake()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        sharedData = GameObject.Find("UI").GetComponent<UISharedData>();

        for (int i = 0; i < 6; i++)
            items[i] = transform.FindChild("content/ShopItem" + (i + 1).ToString()).gameObject.GetComponent<UICurrencyShopItem>();

        OnTheRunUITransitionManager.Instance.InitializeBackground(transform.FindChild("background/bg_black_gradient"));

        List<int[]> currList = OnTheRunDataLoader.Instance.GetShopData(ShopPopupTypes.Money);
        for (int i = 0; i < currList.Count; ++i)
        {
            tempMoneyQuantity[i] = currList[currList.Count - 1 - i][0];
            tempMoneyPrice[i] = currList[currList.Count - 1 - i][1];
        }

        currList = OnTheRunDataLoader.Instance.GetShopData(ShopPopupTypes.Diamonds);
        for (int i = 0; i < currList.Count; ++i)
        {
            tempDiamondsQuantity[i] = currList[currList.Count - 1 - i][0];
            tempDiamondsPrice[i] = currList[currList.Count - 1 - i][1];
        }

        currList = OnTheRunDataLoader.Instance.GetShopData(ShopPopupTypes.Fuel);
        for (int i = 0; i < currList.Count; ++i)
        {
            tempFuelQuantity[i] = currList[currList.Count - 1 - i][0];
            tempFuelPrice[i] = currList[currList.Count - 1 - i][1];
        }
    }

    #region Signals
    void Signal_OnShow(UIPopup button)
    {
    }
    
    void Signal_OnHide(UIPopup button)
    {
    }

    void Signal_OnAddCurrencyRelease(UIButton button)
    {
        //Debug.Log("Adding Currency: " + currentPopupType+" "+button.name);
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

        if (button.transform.parent.name.Equals("ShopItem6") && currentPopupType==ShopPopupTypes.Fuel)
        {
            currentPopupType = ShopPopupTypes.FuelFreeze;
        }
        else if(currentPopupType == ShopPopupTypes.FuelFreeze)
            currentPopupType = ShopPopupTypes.Fuel;

        int currentIndex = 0;
        switch (currentPopupType)
        {
            case ShopPopupTypes.Diamonds:
                tempDiamondsIndex = Array.IndexOf<UICurrencyShopItem>(items, button.gameObject.transform.parent.GetComponent<UICurrencyShopItem>());

                OnTheRunInAppManager.Instance.PurchaseProduct(tempDiamondsIndex, tempDiamondsQuantity[tempDiamondsIndex], PriceData.CurrencyType.SecondCurrency);
                break;
            case ShopPopupTypes.Money:
                
                tempMoneyIndex = Array.IndexOf<UICurrencyShopItem>(items, button.gameObject.transform.parent.GetComponent<UICurrencyShopItem>());

                if (tempMoneyIndex == 5)
                {
                    if (OnTheRunOmniataManager.Instance != null)
                    {
                        if (UICurrencyPopup.isFromNotEnoughCoins)
                            OnTheRunOmniataManager.Instance.TrackWatchVideoAds(OnTheRunCoinsService.WatchVideoPlacement.NotEnoughCoinsVideoAdsPlacement);
                        else
                            OnTheRunOmniataManager.Instance.TrackWatchVideoAds(OnTheRunCoinsService.WatchVideoPlacement.ShopCoinsVideoAdsPlacement);
                    }

                    OnTheRunInAppManager.Instance.PurchaseFreeCoins();
                }
                else
                {
                    if (PURCHASE_COINS_WITH_DIAMONDS)
                    {
                        currentIndex = tempMoneyIndex;  //Array.IndexOf<UICurrencyShopItem>(items, button.gameObject.transform.parent.GetComponent<UICurrencyShopItem>());
                        bool canAffordCoins = PlayerPersistentData.Instance.CanAfford(PriceData.CurrencyType.SecondCurrency, tempMoneyPrice[currentIndex]);
                        if (canAffordCoins)
                        {
                            PlayerPersistentData.Instance.IncrementCurrency(PriceData.CurrencyType.FirstCurrency, tempMoneyQuantity[currentIndex]);
                            PlayerPersistentData.Instance.IncrementCurrency(PriceData.CurrencyType.SecondCurrency, -(int)tempMoneyPrice[currentIndex]);
                            PlayerPersistentData.Instance.Save();
                            Debug.Log("Coins = " + PlayerPersistentData.Instance.Coins);
                            string formattedValue = Manager<UIRoot>.Get().FormatTextNumber(tempMoneyQuantity[currentIndex]);
                            ShowFeedbackPopup(formattedValue + " " + OnTheRunDataLoader.Instance.GetLocaleString("coins"));

                            if (OnTheRunOmniataManager.Instance != null)
                                OnTheRunOmniataManager.Instance.TrackVirtualPurchase(OnTheRunInAppManager.Instance.GetProcuctIdForButtonIndex(PriceData.CurrencyType.FirstCurrency, currentIndex),
                                                                                     PriceData.CurrencyType.SecondCurrency,
                                                                                     ((int)tempMoneyPrice[currentIndex]).ToString(), 
                                                                                     OmniataIds.Product_Type_Standard);
                        }
                        else
                            Manager<UIRoot>.Get().ShowWarningPopup(ShopPopupTypes.Diamonds);
                    }
                    else
                        OnTheRunInAppManager.Instance.PurchaseProduct(tempMoneyIndex, tempMoneyQuantity[tempMoneyIndex], PriceData.CurrencyType.FirstCurrency);
                }
                break;
            case ShopPopupTypes.Fuel:
                currentIndex = Array.IndexOf<UICurrencyShopItem>(items, button.gameObject.transform.parent.GetComponent<UICurrencyShopItem>());
                bool canAfford = PlayerPersistentData.Instance.CanAfford(PriceData.CurrencyType.SecondCurrency, (int)tempFuelPrice[currentIndex]);
                if (canAfford)
                {
                    OnTheRunFuelManager.Instance.Fuel += tempFuelQuantity[currentIndex];
                    PlayerPersistentData.Instance.BuyItem(PriceData.CurrencyType.SecondCurrency, (int)tempFuelPrice[currentIndex]);
                    Debug.Log("Fuel = " + OnTheRunFuelManager.Instance.Fuel);

                    if (OnTheRunOmniataManager.Instance != null)
                        OnTheRunOmniataManager.Instance.TrackVirtualPurchase(OmniataIds.Product_Fuel[currentIndex], PriceData.CurrencyType.SecondCurrency, ((int)tempFuelPrice[currentIndex]).ToString(), OmniataIds.Product_Type_Standard);

                    string formattedValue = Manager<UIRoot>.Get().FormatTextNumber(tempFuelQuantity[currentIndex]);
                    ShowFeedbackPopup(formattedValue + " " + OnTheRunDataLoader.Instance.GetLocaleString("fuel"));
                }
                else
                    Manager<UIRoot>.Get().ShowWarningPopup(ShopPopupTypes.Diamonds);
                break;
            case ShopPopupTypes.FuelFreeze:
                currentIndex = Array.IndexOf<UICurrencyShopItem>(items, button.gameObject.transform.parent.GetComponent<UICurrencyShopItem>());
                canAfford = PlayerPersistentData.Instance.CanAfford(PriceData.CurrencyType.SecondCurrency, (int)tempFuelPrice[currentIndex]);
                if (canAfford)
                {
                    /*OnTheRunFuelManager.Instance.Fuel += tempFuelQuantity[currentIndex];
                    PlayerPersistentData.Instance.BuyItem(PriceData.CurrencyType.SecondCurrency, (int)tempFuelPrice[currentIndex]);*/

                    if (OnTheRunOmniataManager.Instance != null)
                        OnTheRunOmniataManager.Instance.TrackVirtualPurchase(OmniataIds.Product_FuelFreeze, PriceData.CurrencyType.SecondCurrency, ((int)tempFuelPrice[currentIndex]).ToString(), OmniataIds.Product_Type_Standard);

                    OnTheRunFuelManager.Instance.StartFuelFreeze();
                    items[currentIndex].SetItemType(ShopPopupTypes.FuelFreeze);
                    items[currentIndex].SetTypeText("", Manager<UIRoot>.Get().FormatTextNumber((int)tempFuelPrice[currentIndex]), ShopPopupTypes.FuelFreeze);
                    ShowFeedbackPopup(OnTheRunDataLoader.Instance.GetLocaleString("fuel_freeze"));
                }
                else
                    Manager<UIRoot>.Get().ShowWarningPopup(ShopPopupTypes.Diamonds);
                break;
        }
        Manager<UIRoot>.Get().UpdateCurrenciesItem();
    }

    void OnExitCurrencyPopup( )
    {
        Manager<UIManager>.Get().FrontPopup.pausesGame = false;
        Manager<UIManager>.Get().PopPopup();
        Manager<UIManager>.Get().BroadcastMessage("OnCurrencyPopupClosed", SendMessageOptions.DontRequireReceiver);
    }

    void Initialize(UICurrencyPopup.ShopPopupTypes popupType)
    {
        lastPopupType = popupType;

        //interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        int[] currentQuantity;// = sharedData.CurrentCurrency == PriceData.CurrencyType.FirstCurrency ? tempMoneyQuantity : tempDiamondsQuantity;
        float[] currentPrice;// = sharedData.CurrentCurrency == PriceData.CurrencyType.SecondCurrency ? tempMoneyPrice : tempDiamondsPrice;
        float baseCoeff = 1;

        switch (popupType)
        { 
            default:
            case ShopPopupTypes.Money:
                sharedData.CurrentCurrency = PriceData.CurrencyType.FirstCurrency;
                currentPopupType = ShopPopupTypes.Money;
                currentQuantity = tempMoneyQuantity;
                currentPrice = tempMoneyPrice;
                baseCoeff = (float)currentQuantity[4] / currentPrice[4];
                //baseQuantity = 
                break;
            case ShopPopupTypes.Diamonds:
                sharedData.CurrentCurrency = PriceData.CurrencyType.SecondCurrency;
                currentPopupType = ShopPopupTypes.Diamonds;
                currentQuantity = tempDiamondsQuantity;
                currentPrice = tempDiamondsPrice;
                baseCoeff = (float)currentQuantity[5] / currentPrice[5];
                break;
            case ShopPopupTypes.Fuel:
                currentPopupType = ShopPopupTypes.Fuel;
                currentQuantity = tempFuelQuantity;
                currentPrice = tempFuelPrice;
                baseCoeff = (float)currentQuantity[4] / currentPrice[4];
                break;
        }
        
        for (int i = 0; i < items.Length; i++)
        {
            items[i].gameObject.SetActive(currentQuantity[i] > 0);
            items[i].ActivateBestTag(false);
            items[i].ActivateSpecialOffer(false);
            items[i].SetItemType(popupType);
            if (popupType == ShopPopupTypes.Fuel && i == items.Length - 1)
                items[i].SetItemType(ShopPopupTypes.FuelFreeze);

            float computedQuantity = baseCoeff * currentPrice[i];
            float diff = (currentQuantity[i] - computedQuantity);
            float finalPerc = (diff / computedQuantity) * 100.0f;
            finalPerc = Mathf.CeilToInt(finalPerc);
            //Debug.Log("---->>> SCONTO: " + finalPerc);

            freeMoneyIcons[0].SetActive(false);
            freeMoneyIcons[1].SetActive(false);

            if (currentQuantity[i] > 0)
            {
                bool showDiamondIcon = currentPopupType == ShopPopupTypes.Fuel;
                if ((PURCHASE_COINS_WITH_DIAMONDS && currentPopupType == ShopPopupTypes.Money && i != freeCoinsIndex))
                    showDiamondIcon = true;
                items[i].SetType(currentPopupType, showDiamondIcon);

                if (currentPopupType == ShopPopupTypes.Diamonds)
                {
#if UNITY_ANDROID && !UNITY_EDITOR && ENABLE_GETJAR
                    items[i].EnableButton();
#else
                    if (OnTheRunInAppManager.Instance.IsPriceAvailableForPurchaseButton(i, PriceData.CurrencyType.SecondCurrency))
                        items[i].EnableButton();
                    else
                        items[i].DisableButton();
#endif

                    items[i].SetTypeText(currentQuantity[i].ToString(), OnTheRunInAppManager.Instance.GetPriceForPurchaseDiamondButton(i), currentPopupType, finalPerc);
                }
                else if (currentPopupType == ShopPopupTypes.Money)
                {
                    if (!PURCHASE_COINS_WITH_DIAMONDS)
                    {
#if UNITY_ANDROID && !UNITY_EDITOR && ENABLE_GETJAR
                        items[i].EnableButton();
#else
                        if (OnTheRunInAppManager.Instance.IsPriceAvailableForPurchaseButton(i, PriceData.CurrencyType.FirstCurrency) || (i == freeCoinsIndex && OnTheRunCoinsService.Instance.IsVideoAdAvailable()))
                            items[i].EnableButton();
                        else
                            items[i].DisableButton();
#endif
                    }
                    else
                        items[i].EnableButton();

                    if (i == freeCoinsIndex)
                    {
#if UNITY_ANDROID //&& !UNITY_EDITOR && !ENABLE_GETJAR
                        //items[i].SetTypeText(string.Empty, OnTheRunDataLoader.Instance.GetLocaleString("btFreeCoins"));
                        items[i].SetTypeText(OnTheRunCoinsService.Instance.FreeCoinsReward.ToString(), OnTheRunDataLoader.Instance.GetLocaleString("btFreeCoins"), currentPopupType);
#else
                        items[i].SetTypeText(OnTheRunCoinsService.Instance.FreeCoinsReward.ToString(), OnTheRunDataLoader.Instance.GetLocaleString("btFreeCoins"), currentPopupType, finalPerc);     // items[i].SetTypeText(currentQuantity[i].ToString(), OnTheRunDataLoader.Instance.GetLocaleString("btFreeCoins"));
#endif

                        bool videoAvailable = OnTheRunCoinsService.Instance.IsVideoAdAvailable();
                        if (videoAvailable)
                            items[i].EnableButton();
                        else
                            items[i].DisableButton();

                        freeMoneyIcons[0].SetActive(videoAvailable);
                        freeMoneyIcons[1].SetActive(!videoAvailable);
                    }
                    else
                    {
                        if (PURCHASE_COINS_WITH_DIAMONDS)
                            items[i].SetTypeText(currentQuantity[i].ToString(), currentPrice[i].ToString(), currentPopupType, finalPerc);
                        else
                            items[i].SetTypeText(currentQuantity[i].ToString(), OnTheRunInAppManager.Instance.GetPriceForPurchaseCoinsButton(i), currentPopupType, finalPerc);
                    }
                }
                else
                {
                    items[i].EnableButton();
                    items[i].SetTypeText(currentQuantity[i].ToString(), currentPrice[i].ToString(), ShopPopupTypes.Fuel, finalPerc);
                }

                /////////////////// GET PRICE FOR CURRENCY AND SET BEST TAG
                //float relation = ((float)currentQuantity[i]) / ((float)currentPrice[i]);
                //float currMax = Math.Max(best, relation);
                ////Debug.Log("currMax = " + currMax + " relation = " + relation + " best = " + best + " ((float)currentQuantity[i]) = " + ((float)currentQuantity[i])  + " ((float)currentPrice[i]) = " + ((float)currentPrice[i]));
                //if (currMax > best)
                //{
                //    bestIndex = i;
                //    best = currMax;
                //}
            }
        }

        ShopOffers currentOffers = OnTheRunDataLoader.Instance.GetOffersData(popupType);
        for (int i = 0; i < currentOffers.specialOffers.Count; ++i)
        {
            items[currentOffers.specialOffers[i]-1].ActivateSpecialOffer(true);
        }
        for (int i = 0; i < currentOffers.bestOffers.Count; ++i)
        {
            items[currentOffers.bestOffers[i]-1].ActivateBestTag(true);
        }

        //int bestIndex = 2;
        //items[bestIndex].ActivateBestTag(true);

        //int[] specialOfferIndexes = {0, 1};
        //for(int y=0; y<specialOfferIndexes.Length; ++y)
        //    items[specialOfferIndexes[y]].ActivateSpecialOffer(true);
    }
    #endregion

    #region Functions

    void RefreshVideoButton()
    {
        if (items != null && items[freeCoinsIndex] != null)
        {
            if (OnTheRunCoinsService.Instance.IsVideoAdAvailable())
                items[freeCoinsIndex].EnableButton();
            else
                items[freeCoinsIndex].DisableButton();
        }
    }

    void ShowFeedbackPopup(string text)
    {
        Debug.Log("Feedback Popup!!!");
        OnTheRunUITransitionManager.Instance.OpenPopup("SingleButtonPopup");
        UIPopup popup = Manager<UIManager>.Get().FrontPopup; 
        popup.transform.FindChild("content/popupTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("you_bought") + " " + text;
        popup.transform.FindChild("content/OkButton").gameObject.SetActive(true);
    }

    #endregion
}

