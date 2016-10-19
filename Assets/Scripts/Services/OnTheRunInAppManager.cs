//#define TEST_INAPPS

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SBS.Core;

public class OnTheRunInAppManager : MonoBehaviour
{
    public class inAppPurchProduct
    {
        public string description;
        public string title;
        public string price;
        public string id;

        public string currencyCode;
        public double localizedPrice; 

#if UNITY_ANDROID
        public string type;
        public bool isConsumable;
#endif

        public override string ToString()
        {
            string str = id + ", " + title + " (" + description + "), price: " + price + ", currency: " + currencyCode + ", localizedPrice: " + localizedPrice;
#if UNITY_ANDROID
            str += ", type: " + type;
#endif
            return str;
        }
    }

    #region Singleton instance
    protected static OnTheRunInAppManager instance = null;

    public static OnTheRunInAppManager Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    const bool LOGS_ARE_ENABLED = false;

    const string USD_CURRENCY_CODE = "USD";
    public const string PRICE_NOT_AVAILABLE = "---";

	const string kUserHackedPurchasesKey = "user_hacked_purchases";
    const string kUserIsPurchaserKey = "user_is_purchaser";

    InAppImplementation implementation;
    Dictionary<int, string> diamondIdsByButtonIndex;
    Dictionary<int, string> coinIdsByButtonIndex;

    Dictionary<string, string> pricesByProductIds;
    Dictionary<string, double> localizedPricesByProductIds;
    Dictionary<string, string> currencyCodesByProductIds;

    int purchasedQuantity = 0;

	bool userHackedPurchases = false;
	public bool UserHasHackedPurchases { get { return userHackedPurchases; } }

    bool userIsPurchaser = false;
    public bool UserIsPurchaser { get { return userIsPurchaser; } }

    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;
        DontDestroyOnLoad(gameObject);

        Init();
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }

    void Init()
    {
        Log("Init");

        userHackedPurchases = EncryptedPlayerPrefs.GetInt(kUserHackedPurchasesKey, 0) == 1;
        userIsPurchaser = EncryptedPlayerPrefs.GetInt(kUserIsPurchaserKey, 0) == 1;

        pricesByProductIds = new Dictionary<string, string>();
        localizedPricesByProductIds = new Dictionary<string, double>();
        currencyCodesByProductIds = new Dictionary<string, string>();

        InitializeDiamondIdsByButtonIndex();
        InitializeCoinIdsByButtonIndex();
        SetImplementation();
        implementation.Init();
    }

    void InitializeDiamondIdsByButtonIndex()
    {
        diamondIdsByButtonIndex = new Dictionary<int, string>();

        diamondIdsByButtonIndex.Add(5, "purchase_diamonds_0");
        diamondIdsByButtonIndex.Add(4, "purchase_diamonds_1");
        diamondIdsByButtonIndex.Add(3, "purchase_diamonds_2");
        diamondIdsByButtonIndex.Add(2, "purchase_diamonds_3");
        diamondIdsByButtonIndex.Add(1, "purchase_diamonds_4");
        diamondIdsByButtonIndex.Add(0, "purchase_diamonds_5");
    }

    void InitializeCoinIdsByButtonIndex()
    {
        coinIdsByButtonIndex = new Dictionary<int, string>();

        coinIdsByButtonIndex.Add(5, "purchase_coins_missing");
        coinIdsByButtonIndex.Add(4, "purchase_coins_0");
        coinIdsByButtonIndex.Add(3, "purchase_coins_1");
        coinIdsByButtonIndex.Add(2, "purchase_coins_2");
        coinIdsByButtonIndex.Add(1, "purchase_coins_3");
        coinIdsByButtonIndex.Add(0, "purchase_coins_4");
    }

    void SetImplementation()
    {
#if UNITY_EDITOR //|| TEST_INAPPS
        implementation = gameObject.AddComponent<EditorInAppImplementation>();
#elif UNITY_IPHONE
		if (SBS.Miniclip.MCUtilsBindings.isMassiveTestBuild())
			implementation = gameObject.AddComponent<EditorInAppImplementation>();
		else
        	implementation = gameObject.AddComponent<IosInAppImplementation>();
#elif UNITY_ANDROID && !UNITY_KINDLE
        implementation = gameObject.AddComponent<AndroidInAppImplementation>();
#elif UNITY_ANDROID && UNITY_KINDLE
        implementation = gameObject.AddComponent<KindleInAppImplementation>();
#elif UNITY_WP8
        implementation = gameObject.AddComponent<Wp8InAppImplementation>();
#else
        implementation = gameObject.AddComponent<EditorInAppImplementation>();
#endif
    }

    // Refactoring: make this private and use callbacksHandler
    public void AddProductPrice(string productId, string price)
    {
        Log("AddProductPrice - productId: " + productId + " - price: " + price);

        if (pricesByProductIds.ContainsKey(productId))
            pricesByProductIds[productId] = price;
        else
            pricesByProductIds.Add(productId, price);
    }

    public void AddLocalizedPrice(string productId, double localizedPrice)
    {
        Log("AddLocalizedPrice - productId: " + productId + " - localizedPrice: " + localizedPrice);

        if (localizedPricesByProductIds.ContainsKey(productId))
            localizedPricesByProductIds[productId] = localizedPrice;
        else
            localizedPricesByProductIds.Add(productId, localizedPrice);
    }

    public void AddCurrencyCode(string productId, string currencyCode)
    {
        Log("AddCurrencyCode - productId: " + productId + " - currencyCode: " + currencyCode);

        if (currencyCodesByProductIds.ContainsKey(productId))
            currencyCodesByProductIds[productId] = currencyCode;
        else
            currencyCodesByProductIds.Add(productId, currencyCode);
    }

    public bool IsPriceAvailableForPurchaseButton(int buttonIndex, PriceData.CurrencyType currency)
    {
        Asserts.Assert(diamondIdsByButtonIndex.ContainsKey(buttonIndex), "OnTheRunInAppManager.IsPriceAvailableForPurchaseButton(" + buttonIndex + ", currency: " + currency + ")");
        
        string productId = "";
        switch (currency)
        {
            case PriceData.CurrencyType.FirstCurrency:
                productId = coinIdsByButtonIndex[buttonIndex];
                break;
            case PriceData.CurrencyType.SecondCurrency:
                productId = diamondIdsByButtonIndex[buttonIndex];
                break;
        }
        return pricesByProductIds.ContainsKey(productId);
    }

    public string GetPriceForPurchaseDiamondButton(int buttonIndex)
    {
        Asserts.Assert(diamondIdsByButtonIndex.ContainsKey(buttonIndex), "OnTheRunInAppManager.GetCostForPurchaseButton(" + buttonIndex + ")");

        string productId = diamondIdsByButtonIndex[buttonIndex];
        return GetPriceForProductId(productId);
    }

    public string GetPriceForPurchaseCoinsButton(int buttonIndex)
    {
        Asserts.Assert(coinIdsByButtonIndex.ContainsKey(buttonIndex), "OnTheRunInAppManager.GetCostForPurchaseButton(" + buttonIndex + ")");

        string productId = coinIdsByButtonIndex[buttonIndex];
        return GetPriceForProductId(productId);
    }

    string GetPriceForProductId(string productId)
    {
        if (pricesByProductIds.ContainsKey(productId))
            return pricesByProductIds[productId];
        else
            return PRICE_NOT_AVAILABLE;
    }

    double GetLocalizedPriceForProductId(string productId)
    {
        if (localizedPricesByProductIds.ContainsKey(productId))
            return localizedPricesByProductIds[productId];
        else
            return -1.0;
    }

    string GetCurrencyCodeForProductId(string productId)
    {
        if (currencyCodesByProductIds.ContainsKey(productId))
            return currencyCodesByProductIds[productId];
        else
            return string.Empty;
    }

    public string GetProcuctIdForButtonIndex(PriceData.CurrencyType currency, int buttonIndex)
    {
        Dictionary<int, string> idsDict = null;
        switch (currency)
        {
            case PriceData.CurrencyType.FirstCurrency:
                idsDict = coinIdsByButtonIndex;
                break;
            case PriceData.CurrencyType.SecondCurrency:
                idsDict = diamondIdsByButtonIndex;
                break;
        }

        if (idsDict.ContainsKey(buttonIndex))
            return idsDict[buttonIndex];
        else
            return string.Empty;
    }

    public void PurchaseProduct(int buttonIndex, int quantity, PriceData.CurrencyType currency)
    {
        string productId = "";
        switch (currency)
        {
            case PriceData.CurrencyType.FirstCurrency:
                Asserts.Assert(coinIdsByButtonIndex.ContainsKey(buttonIndex), "OnTheRunInAppManager.PurchaseProduct(" + buttonIndex + ")");

                productId = coinIdsByButtonIndex[buttonIndex];
                break;

            case PriceData.CurrencyType.SecondCurrency:
                Asserts.Assert(diamondIdsByButtonIndex.ContainsKey(buttonIndex), "OnTheRunInAppManager.PurchaseProduct(" + buttonIndex + ")");

                productId = diamondIdsByButtonIndex[buttonIndex];
                break;
        }

        Log("PurchaseProduct(" + buttonIndex + ", " + quantity + ", " + currency + ")");

        purchasedQuantity = quantity;
        implementation.PurchaseProduct(productId);
    }

    void OnApplicationPause(bool paused)
    {
        if (!paused)
            implementation.OnApplicationResumed();
    }

    public List<string> GetProductsSkusList(string skuHeader)
    {
        List<string> skus = new List<string>();

        foreach (KeyValuePair<int, string> kvp in diamondIdsByButtonIndex)
            skus.Add(kvp.Value);

        for (int i = 0; i < skus.Count; i++)
            skus[i] = skuHeader + skus[i];

        return skus;
    }


    public void OnInappPurchaseProductsReady(OnTheRunInAppManager.inAppPurchProduct[] products)
    {
        foreach (OnTheRunInAppManager.inAppPurchProduct product in products)
        {
            string cost = string.Empty;
            if (product != null)
                cost = iOSUtils._YenHack(product.price);
            else
                cost = OnTheRunInAppManager.PRICE_NOT_AVAILABLE;

            string headerLessProductId = product.id.Substring(product.id.LastIndexOf('.') + 1);
            AddProductPrice(headerLessProductId, cost);
            AddLocalizedPrice(headerLessProductId, product.localizedPrice);
            AddCurrencyCode(headerLessProductId, product.currencyCode);
        }
    }

    public void PurchaseFreeCoins()
    {
        implementation.PurchaseFreeCoins();
    }

    #region InApp Popups

    UIInAppFeedbackPopup inappFeedbackPopup;

    // Refactoring: make this private and use callbacksHandler
    public void FinalizeInAppPurchase(string productId, bool shouldTrackPurchase)
    {
        Log("FinalizeInAppPurchase - productId: " + productId + " - shouldTrackPurchase: " + shouldTrackPurchase);

        productId = productId.Substring(productId.LastIndexOf('.') + 1);
        if (productId.Contains("diamonds"))
            PlayerPersistentData.Instance.IncrementCurrency(PriceData.CurrencyType.SecondCurrency, purchasedQuantity);
        else
            PlayerPersistentData.Instance.IncrementCurrency(PriceData.CurrencyType.FirstCurrency, purchasedQuantity);

		/*
        if (shouldTrackPurchase)
            TrackPurchase(productId);
		*/

        /*if (OnTheRunChartboostManager.Instance != null)
            OnTheRunChartboostManager.Instance.OnInappDone();*/
        SaveUserIsPurchaser();

        purchasedQuantity = 0;
        Manager<UIRoot>.Get().UpdateCurrenciesItem();
    }

    void SaveUserIsPurchaser() { 
    {
        Log("Save User Is Purchaser");

        userIsPurchaser = true;
        EncryptedPlayerPrefs.SetInt(kUserIsPurchaserKey, 1);
        EncryptedPlayerPrefs.Save();
    }}

	public void InAppPurchaseValidationSuccess(string productId)
	{
		productId = productId.Substring(productId.LastIndexOf('.') + 1);
		TrackPurchase(productId, false);
	}

	public void InAppPurchaseValidationFailed(string productId)
	{
		OnUserHackedPurchases();

		productId = productId.Substring(productId.LastIndexOf('.') + 1);
		TrackPurchase(productId, true);
	}
	
	void OnUserHackedPurchases()
	{
		userHackedPurchases = true;
		EncryptedPlayerPrefs.SetInt(kUserHackedPurchasesKey, 1);
		EncryptedPlayerPrefs.Save();
	}

    void TrackPurchase_configUsdValues(string productId, bool hackedPurchase)
    {
        //Debug.Log("### OnTheRunInAppManager - TrackPurchase");

        bool inappPriceIsAvailable = OnTheRunDataLoader.Instance.IsInappPriceAvailable(productId);
        
        float dollarsExpense = 0.0f;
        if (inappPriceIsAvailable)
            dollarsExpense = OnTheRunDataLoader.Instance.GetInappPriceInDollars(productId);

        if (inappPriceIsAvailable && OnTheRunAppsFlyerManager.Instance != null)
            OnTheRunAppsFlyerManager.Instance.OnInAppDone(USD_CURRENCY_CODE, dollarsExpense, hackedPurchase);
        
        if (inappPriceIsAvailable && OnTheRunOmniataManager.Instance != null)
            OnTheRunOmniataManager.Instance.TrackPurchase(productId, USD_CURRENCY_CODE, dollarsExpense, hackedPurchase);

        if (inappPriceIsAvailable && OnTheRunAdjustManager.Instance != null)
            OnTheRunAdjustManager.Instance.TrackPurchase(USD_CURRENCY_CODE, dollarsExpense, hackedPurchase);
    }

    void TrackPurchase(string productId, bool hackedPurchase)
    {
        double localizedPrice = GetLocalizedPriceForProductId(productId);
        string currencyCode = GetCurrencyCodeForProductId(productId);

        if (localizedPrice < 0 || string.IsNullOrEmpty(currencyCode))
        {
            TrackPurchase_configUsdValues(productId, hackedPurchase);
        }
        else
        {
            if (OnTheRunAppsFlyerManager.Instance != null)
                OnTheRunAppsFlyerManager.Instance.OnInAppDone(currencyCode, (float)localizedPrice, hackedPurchase);

            if (OnTheRunOmniataManager.Instance != null)
                OnTheRunOmniataManager.Instance.TrackPurchase(productId, currencyCode, (float)localizedPrice, hackedPurchase);

            if (OnTheRunAdjustManager.Instance != null)
                OnTheRunAdjustManager.Instance.TrackPurchase(currencyCode, localizedPrice, hackedPurchase);
        }
    }

    // Refactoring: make this private and use callbacksHandler
    public void ShowProcessingPopup()
    {
        Log("ShowProcessingPopup");

        ShowInAppFeedbackPopup();
        inappFeedbackPopup.SetText(OnTheRunDataLoader.Instance.GetLocaleString("purchase_processing"));     // Manager<UIManager>.Get().FrontPopup.transform.FindChild("popupTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("purchase_processing");
        inappFeedbackPopup.ShowPurchaseIcon();                                                              // Manager<UIManager>.Get().FrontPopup.transform.FindChild("icon_purchase").gameObject.SetActive(true);
        inappFeedbackPopup.HideOkButton();                                                                  // Manager<UIManager>.Get().FrontPopup.transform.FindChild("OkButton").gameObject.SetActive(false);
        
        //Manager<UIManager>.Get().FrontPopup.transform.FindChild("OkButton").transform.position += Vector3.down * 20.0f;
    }

    public void HideProcessingPopup()
    {
        Log("HideProcessingPopup");

        if (Manager<UIManager>.Get().IsPopupInStack("InAppFeedbackPopup"))
            Manager<UIManager>.Get().RemovePopupFromStack("InAppFeedbackPopup");
    }

    // Refactoring: make this private and use callbacksHandler
    public void ShowPurchaseSuccessfulPopup()
    {
        Log("ShowPurchaseSuccessfulPopup");

        ShowInAppFeedbackPopup();
        inappFeedbackPopup.SetText(OnTheRunDataLoader.Instance.GetLocaleString("purchase_successful"));     // Manager<UIManager>.Get().FrontPopup.transform.FindChild("popupTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("purchase_successful");
        inappFeedbackPopup.HidePurchaseIcon();                                                              // Manager<UIManager>.Get().FrontPopup.transform.FindChild("icon_purchase").gameObject.SetActive(false);
        inappFeedbackPopup.ShowOkButton();                                                                  // Manager<UIManager>.Get().FrontPopup.transform.FindChild("OkButton").gameObject.SetActive(true);
        
        //Manager<UIManager>.Get().FrontPopup.transform.FindChild("OkButton").transform.position -= Vector3.down * 20.0f;
    }

    // Refactoring: make this private and use callbacksHandler
    public void ShowPurchaseFailedPopup()
    {
        Log("ShowPurchaseFailedPopup");

        ShowInAppFeedbackPopup();
        inappFeedbackPopup.SetText(OnTheRunDataLoader.Instance.GetLocaleString("purchase_failed"));         // Manager<UIManager>.Get().FrontPopup.transform.FindChild("popupTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("purchase_failed");
        inappFeedbackPopup.HidePurchaseIcon();                                                              // //Manager<UIManager>.Get().FrontPopup.transform.FindChild("icon_purchase").gameObject.SetActive(false);
        inappFeedbackPopup.ShowOkButton();                                                                  // Manager<UIManager>.Get().FrontPopup.transform.FindChild("OkButton").gameObject.SetActive(true);
        
        //Manager<UIManager>.Get().FrontPopup.transform.FindChild("OkButton").transform.position -= Vector3.down * 20.0f;
    }

    // Refactoring: make this private and use callbacksHandler
    public void ShowPurchaseAbortedPopup()
    {
        Log("ShowPurchaseAbortedPopup");

        ShowInAppFeedbackPopup();
        inappFeedbackPopup.SetText(OnTheRunDataLoader.Instance.GetLocaleString("purchase_aborted"));        // Manager<UIManager>.Get().FrontPopup.transform.FindChild("popupTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("purchase_aborted");
        inappFeedbackPopup.HidePurchaseIcon();                                                              // //Manager<UIManager>.Get().FrontPopup.transform.FindChild("icon_purchase").gameObject.SetActive(false);
        inappFeedbackPopup.ShowOkButton();                                                                  // Manager<UIManager>.Get().FrontPopup.transform.FindChild("OkButton").gameObject.SetActive(true);
        
        //Manager<UIManager>.Get().FrontPopup.transform.FindChild("OkButton").transform.position -= Vector3.down * 20.0f;
    }

    void ShowInAppFeedbackPopup()
    {
        Log("ShowInAppFeedbackPopup");

        if (Manager<UIManager>.Get().IsPopupInStack("InAppFeedbackPopup"))
            Manager<UIManager>.Get().BringPopupToFront("InAppFeedbackPopup");
        else
            OnTheRunUITransitionManager.Instance.OpenPopup("InAppFeedbackPopup");
            //Manager<UIManager>.Get().PushPopup("InAppFeedbackPopup");

        if (TimeManager.Instance.MasterSource.IsPaused)
            TimeManager.Instance.MasterSource.Resume();

        /*OnTheRunUITransitionManager.Instance.OpenPopup("SingleButtonPopup");
        UIPopup popup = Manager<UIManager>.Get().FrontPopup;
        popup.transform.FindChild("content/popupTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("you_bought") + " " + text;
        popup.transform.FindChild("content/OkButton").gameObject.SetActive(true);*/

        if (inappFeedbackPopup == null)
            inappFeedbackPopup = Manager<UIManager>.Get().FrontPopup.GetComponent<UIInAppFeedbackPopup>();
    }

    #endregion

    void Log(string logStr)
    {
        if (LOGS_ARE_ENABLED)
            Debug.Log("### INAPP MANAGER - " + logStr);
    }
}