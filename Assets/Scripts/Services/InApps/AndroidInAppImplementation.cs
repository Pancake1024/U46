//#define ENABLE_GETJAR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SBS.Core;
using SBS_MiniJSON;
using System.Text;
#if UNITY_ANDROID && ENABLE_GETJAR
using GetJar.Android.SDK.Unity;
#endif

#if UNITY_ANDROID
public class AndroidInAppImplementation : MonoBehaviour, InAppImplementation
{
    public const string ANDROID_PUBLIC_KEY = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAiGQNfmAuOhtjCwov1SMTo0T5OYwKex2/TUGcI2gn1D+A/wQfk5M+FUen7ldKDZe/o4t0B31fOPFWhKrkG/5cm3sl0eq2Gt9jLg8KzfMNOx1/MengdPwaoVFrj1F4sgXchhzvJF5lYOfB8/4qyH68bvCLDVMGUeT3hjBHRtAZa3pI2p89rY1DhtXf5JRbzR9aokVFtFl+OKJNgs2jznx2QgrPxbb7rBNVgeOkAUMZdEa7f7V0i0ok/khEhXTEjLszlww5utq8Q0E1yhCkH5Q1LVgGPNn49nvlKZtIu6qTThPJAhhl40u7t6/zXam7lk8dEbJNDp6SnZzUybg7ITk1pQIDAQAB";

    const bool LOGS_ARE_ENABLED = false;

    const string FREE_COINS_PRODUCT_ID = "purchase_money_0";
    bool getJarReceivedSuccessEvent = false;
    bool googleIsBillingSupported = false;
    bool googleIsQueryInventoryLocked = false;
    bool googleIsQueryInventoryOk = false;
    string suspendedPurchaseProductId = string.Empty;

    OnTheRunInAppManager.inAppPurchProduct[] products = null;

#if ENABLE_GETJAR
    int lastGetJarLoginFrame = 0;
    Dictionary<string, string> getJarProductsDefaultName = null;
#endif

    public void Init()
    {
        Log("Init");

        products = null;

#if ENABLE_GETJAR
        GameObject getJarManager = GameObject.Instantiate(Resources.Load("GetJarManager")) as GameObject;
        getJarManager.transform.parent = transform;
        LogInGetJar();
        SetGetJarProductsDefaultName();
#endif
        //if (LOGS_ARE_ENABLED)
        //    GoogleIAB.enableLogging(true);

        GoogleIAB.init(ANDROID_PUBLIC_KEY);
        GoogleIABManager.billingSupportedEvent += BillingSupportedEvent;

        RegisterToGoogleIabManagerEvents();
#if ENABLE_GETJAR
        RegisterToGetJarAndroidManagerEvents();
#endif
        RegisterToObjDispatcherEvents();
    }

    public void PurchaseProduct(string productId)
    {
        Log("PurchaseProduct: " + productId);

        if (googleIsBillingSupported)
        {
            suspendedPurchaseProductId = productId.Substring(productId.LastIndexOf('.') + 1);
            
#if ENABLE_GETJAR
            Nullable<int> getJarPrice = GetJarAndroidManager.Instance.GetProductPrice(suspendedPurchaseProductId);

            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.ontherun.OnTheRunActivity"))
            {
                OnTheRunInAppManager.inAppPurchProduct product = GetProductFromName(suspendedPurchaseProductId);
                
                string productTitle = product != null ? product.title : string.Empty;
                string getJarPriceStr = getJarPrice.HasValue ? getJarPrice.Value.ToString() : string.Empty;
                string googlePrice = product != null ? product.price : string.Empty;

                jc.CallStatic("unityShowBuyDialog", productTitle, getJarPriceStr, googlePrice);
            }
#else
            BuyWithGoogle();
#endif
        }
    }

    public void OnApplicationResumed()
    {
        if (googleIsBillingSupported && !googleIsQueryInventoryOk && !googleIsQueryInventoryLocked)
            QueryInventory();

#if ENABLE_GETJAR
        if (!GetJarAndroidManager.Instance.IsLogged && Time.frameCount > lastGetJarLoginFrame + 5)
        {
            lastGetJarLoginFrame = Time.frameCount;
            LogInGetJar();
        }
#endif
    }

    void OnApplicationQuit()
    {
        GoogleIAB.unbindService();
    }

#if ENABLE_GETJAR
    void LogInGetJar()
    {
        Dictionary<string, Pricing> getJarProducts = new Dictionary<string, Pricing>();

        // TODO: set the correct prices. These come from Hot Rod
        getJarProducts.Add("purchase_diamonds_0", new Pricing(110));
        getJarProducts.Add("purchase_diamonds_1", new Pricing(220));
        getJarProducts.Add("purchase_diamonds_2", new Pricing(560));
        getJarProducts.Add("purchase_diamonds_3", new Pricing(1120));
        getJarProducts.Add("purchase_diamonds_4", new Pricing(1780));
        getJarProducts.Add("purchase_diamonds_5", new Pricing(2890));
        getJarProducts.Add(FREE_COINS_PRODUCT_ID, new Pricing(50));

        GetJarAndroidManager.Instance.StartUserAuth(getJarProducts);
    }

    void SetGetJarProductsDefaultName()
    {
        getJarProductsDefaultName = new Dictionary<string, string>();
        getJarProductsDefaultName.Add("purchase_diamonds_0", "Diamonds Pack 0");
        getJarProductsDefaultName.Add("purchase_diamonds_1", "Diamonds Pack 1");
        getJarProductsDefaultName.Add("purchase_diamonds_2", "Diamonds Pack 2");
        getJarProductsDefaultName.Add("purchase_diamonds_3", "Diamonds Pack 3");
        getJarProductsDefaultName.Add("purchase_diamonds_4", "Diamonds Pack 4");
        getJarProductsDefaultName.Add("purchase_diamonds_5", "Diamonds Pack 5");
        getJarProductsDefaultName.Add(FREE_COINS_PRODUCT_ID, "Free Coins Pack");
    }
#endif

    void BillingSupportedEvent()
    {
        Log("BillingSupportedEvent");

        googleIsBillingSupported = true;
        QueryInventory();
    }

    #region Query Inventory

    void QueryInventory()
    {
        Log("Query Inventory");

        googleIsQueryInventoryLocked = true;
        GoogleIABManager.queryInventorySucceededEvent += QueryInventorySucceded;
        GoogleIABManager.queryInventoryFailedEvent += QueryInventoryFailed;
        GoogleIAB.queryInventory(OnTheRunInAppManager.Instance.GetProductsSkusList("com.miniclip.ontherun.").ToArray());
    }

    void QueryInventorySucceded(List<GooglePurchase> purchases, List<GoogleSkuInfo> infos)
    {
        Log("Query inventory - SUCCESS - purchases.count: " + purchases.Count + " - infos.count: " + infos.Count);

        googleIsQueryInventoryLocked = false;
        googleIsQueryInventoryOk = true;

        SetInventory(infos);
        RestorePurchases(purchases);
        OnTheRunInAppManager.Instance.OnInappPurchaseProductsReady(products);

        RemoveQueryInventoryEvents();
    }

    void QueryInventoryFailed(string error)
    {
        Log("Query inventory - FAILED - " + error);
        googleIsQueryInventoryOk = false;

        RemoveQueryInventoryEvents();
    }

    void RemoveQueryInventoryEvents()
    {
        GoogleIABManager.queryInventorySucceededEvent -= QueryInventorySucceded;
        GoogleIABManager.queryInventoryFailedEvent -= QueryInventoryFailed;
    }

#endregion

    #region Events

    void RegisterToGoogleIabManagerEvents()
    {
        GoogleIABManager.purchaseFailedEvent += (str, response) =>
        {
            Log("purchaseFailedEvent");
            OnTheRunInAppManager.Instance.ShowPurchaseFailedPopup();
        };

        GoogleIABManager.purchaseSucceededEvent += (purchase) =>
        {
            Log("purchaseSucceededEvent");
            PurchaseSucceeded(true, purchase);
        };

        GoogleIABManager.consumePurchaseFailedEvent += (str) =>
        {
            Log("ConsumePurchaseFailedEcent - " + str);
            OnTheRunInAppManager.Instance.ShowPurchaseFailedPopup();
        };

        GoogleIABManager.consumePurchaseSucceededEvent += (purchase) =>
        {
            Log("ConsumePurchaseSucceededEvent - productId: " + purchase.productId);
            OnTheRunInAppManager.Instance.FinalizeInAppPurchase(purchase.productId, true);
            OnTheRunInAppManager.Instance.ShowPurchaseSuccessfulPopup();
        };

        GoogleIABManager.purchaseCompleteAwaitingVerificationEvent += (purchaseData, signature) => { ValidateInAppPurchase(purchaseData, signature); };
    }

#if ENABLE_GETJAR
    void RegisterToGetJarAndroidManagerEvents()
    {
        GetJarAndroidManager.Instance.purchaseSucceededEvent += (name) => {
            getJarReceivedSuccessEvent = true;
            OnTheRunInAppManager.Instance.FinalizeInAppPurchase(name);
        };

        GetJarAndroidManager.Instance.purchaseCancelledEvent += () => {
            if (!getJarReceivedSuccessEvent)
                OnTheRunInAppManager.Instance.ShowPurchaseAbortedPopup();
        };

        GetJarAndroidManager.Instance.purchaseFailedEvent += (name) => {
            OnTheRunInAppManager.Instance.ShowPurchaseFailedPopup();
        };
    }
#endif

    void RegisterToObjDispatcherEvents()
    {
#if ENABLE_GETJAR
        ObjCDispatcher.Instance.RegisterEvent("getJarBuyWithGetJar", (str) => { BuyWithGetJar(); });
        ObjCDispatcher.Instance.RegisterEvent("getJarBuyWithGoogle", (str) => { BuyWithGoogle(); });
        ObjCDispatcher.Instance.RegisterEvent("getJarDismiss", (str) => { });
#endif
    }

    #endregion

    void SetInventory(List<GoogleSkuInfo> infos)
    {
        const double MICRO_UNITS_COEFF = 1000000.0;

        products = new OnTheRunInAppManager.inAppPurchProduct[infos.Count];
        int i = 0;
        foreach (GoogleSkuInfo info in infos)
        {
            OnTheRunInAppManager.inAppPurchProduct product = new OnTheRunInAppManager.inAppPurchProduct();
            product.id = info.productId;
            product.price = info.price;
            product.title = info.title;
            product.description = info.description;
            product.currencyCode = info.priceCurrencyCode;
            product.type = info.type;
            product.isConsumable = true;
            product.localizedPrice = (double)info.priceAmountMicros / MICRO_UNITS_COEFF;

            Log("SetInventory - PRODUCT - id: " + product.id + " - price: " + product.price + " - trackablePrice: " + product.localizedPrice + " - currencyCode: " + product.currencyCode + " - isConsumable: " + product.isConsumable);

            products[i++] = product;
        }
    }

    void RestorePurchases(List<GooglePurchase> purchases)
    {
        Log("Restore Purchases - count: " + (purchases == null ? "NULL" : purchases.Count.ToString()));

        foreach (GooglePurchase purchase in purchases)
            PurchaseSucceeded(false, purchase);
    }

    void PurchaseSucceeded(bool showPopup, GooglePurchase purchase)
    {
        Log("PurchaseSucceeded - showPopup: " + showPopup + " - purchase: " + purchase.ToString());

        string productName = purchase.productId.Substring(purchase.productId.LastIndexOf('.') + 1);
        Log("PurchaseSucceeded - productName: " + productName);
        OnTheRunInAppManager.inAppPurchProduct product = GetProductFromName(productName);
        if (product == null)
            return;

        Log("PurchaseSucceeded - product.id: " + product.id);
        if (product.isConsumable)
        {
            GoogleIAB.consumeProduct(product.id);
        }
        else
        {
            /* FROM SLOT DOZER - non comsumable items
            Shop.ShopItem item = Shop.Instance.GetShopItemByProductId(product.id);
            if (0 == item.NumOwn)
            {
                if (showPopup)
                {
                    productName = product.id.Substring(product.id.LastIndexOf('.') + 1);
                    Shop.Instance.provideContent(productName);//product.id);
                }
                else
                    Shop.Instance.onPurchaseSuccessful(product.id, true);
            }
            */
        }
    }

#if ENABLE_GETJAR
    void BuyWithGetJar()
    {
        OnTheRunInAppManager.Instance.ShowProcessingPopup();

        OnTheRunInAppManager.inAppPurchProduct product = GetProductFromName(suspendedPurchaseProductId);
        
        string getJarProdId = suspendedPurchaseProductId;
        string elementName = product != null ? product.title : getJarProductsDefaultName[getJarProdId];
        string elementDescription = product != null ? product.description : string.Empty;

        GetJarAndroidManager.Instance.BuyProduct(getJarProdId, elementName, elementDescription);
    }
#endif

    void BuyWithGoogle()
    {
        Log("BuyWithGoogle");

        OnTheRunInAppManager.Instance.ShowProcessingPopup();

        string fullyQualifiedProductId = GetFullyQualifiedProductIdForProduct(suspendedPurchaseProductId);
        if (!string.IsNullOrEmpty(fullyQualifiedProductId))
            GoogleIAB.purchaseProduct(fullyQualifiedProductId);
    }

    public void PurchaseFreeCoins()
    {
#if ENABLE_GETJAR
        if (GetJarAndroidManager.Instance != null)
        {
            if (GetJarAndroidManager.Instance.IsLogged)
            {
                OnTheRunInAppManager.inAppPurchProduct product = GetProductFromName(FREE_COINS_PRODUCT_ID);
                string productTitle = product != null ? product.title : getJarProductsDefaultName[FREE_COINS_PRODUCT_ID];
                string productDescription = product != null ? product.description : string.Empty;
                GetJarAndroidManager.Instance.BuyProduct(FREE_COINS_PRODUCT_ID, productTitle, productDescription);
            }
            else
            {
                SBS.Miniclip.AlertViewBindings.AlertBox("", OnTheRunDataLoader.Instance.GetLocaleString("no_internet_connection"), OnTheRunDataLoader.Instance.GetLocaleString("ok"), new string[0], false);
            }
        }
#else
        OnTheRunCoinsService.Instance.OnFreeCoinsSelected();
#endif
    }

    string GetFullyQualifiedProductIdForProduct(string productName)
    {
        foreach (OnTheRunInAppManager.inAppPurchProduct product in products)
        {
            if (productName.Equals(product.id.Substring(product.id.LastIndexOf('.') + 1)))
                return product.id;
        }

        Asserts.Assert(false, "AndroidInAppImplementation.GetFullyQualifiedProductIdForProduct(" + productName + ")");
        return string.Empty;
    }

    OnTheRunInAppManager.inAppPurchProduct GetProductFromName(string productName)
    {
        Log("GetProductFromName - productName: " + productName);

        if (null == products)
        {
            Log("GetProductFromName - null == products");
            return null;
        }

        foreach (OnTheRunInAppManager.inAppPurchProduct product in products)
        {
            string name = product.id.Substring(product.id.LastIndexOf('.') + 1);
            if (productName == name)
                return product;
        }
        Log("GetProductFromName - returning null");
        return null;
    }

    #region In App Validation

    const string VALIDATION_URL = "http://services.miniclippt.com/receiptValidation/index.php";

    void ValidateInAppPurchase(string purchaseData, string signature)
    {
        Log("ValidateInAppPurchase\nPURCHASE DATA:\n" + purchaseData + "\nSIGNATURE:\n" + signature);

        string productId = GetProductIdFromPurchaseData(purchaseData);

        // TO TEST VALIDATION FAILURE --->
        //purchaseData = "Receipt to test purchase validation failure";
        // ---> TO TEST VALIDATION FAILURE

        //Debug.Log("### ### ### Android Inapp Implementation - ValidateInAppPurchase - productId: " + productId);

        byte[] postData = CreateValidationRequestParameters(purchaseData, signature);
        WWW response = new WWW(VALIDATION_URL, postData);
        StartCoroutine(WaitForInAppValidationResponse(response, productId));
    }

    string GetProductIdFromPurchaseData(string purchaseData)
    {
        Dictionary<string, object> dict = Json.Deserialize(purchaseData) as Dictionary<string, object>;
        if (dict.ContainsKey("productId"))
            return (string)dict["productId"];

        return string.Empty;
    }

    byte[] CreateValidationRequestParameters(string purchaseData, string signature)
    {
        string bundleIdentifier = "com.miniclip.ontherun";

        Dictionary<string, object> payloadDict = new Dictionary<string, object>();
        payloadDict.Add("bundle_id", bundleIdentifier);
        payloadDict.Add("os_type", "android");
        payloadDict.Add("receipt", purchaseData);
        payloadDict.Add("signed_receipt", signature);

        string payload = Json.Serialize(payloadDict);

        Log("CreateValidationRequestParameters - payload:\n" + payload);

        return Encoding.UTF8.GetBytes(payload);
    }

    IEnumerator WaitForInAppValidationResponse(WWW response, string productId)
    {
        yield return response;

        if (response.error == null)
            handleInappValidationResult(response.text, productId);
        else
            Debug.Log("InApp Validation - Error in response: " + response.error);
    }

    void handleInappValidationResult(string responseText, string productId)
    {
        Log("Validation Result: " + responseText);

        string dataString = string.Empty;
        long resultCode = -1;

        Dictionary<string, object> responseDict = Json.Deserialize(responseText) as Dictionary<string, object>;
        if (responseDict.ContainsKey("data"))
            dataString = (string)responseDict["data"];

        if (responseDict.ContainsKey("result_code"))
            resultCode = (long)responseDict["result_code"];

        bool inAppIsValid = true;

        switch (resultCode)
        {
            case 0:                     // Validation successful - do not block user
                inAppIsValid = true;
                break;

            case 1:                     // Already used receipt
                inAppIsValid = false;
                break;

            case 2:                     // Invalid receipt
                inAppIsValid = false;
                break;

            case 3:                     // Invalid request
                inAppIsValid = false;
                break;

            case 4:                     // Invalid apple response
                inAppIsValid = false;
                break;

            case 5:                     // Database issue - do no block user
                inAppIsValid = false;
                break;

            case 6:                     // Specific internal errors - do no block user
                inAppIsValid = false;
                break;
        }

        Log("HandleInappValidationResult() - resultCode: " + resultCode + ", inAppIsValid: " + inAppIsValid + "\nData:\n" + dataString);

        if (inAppIsValid)
		    OnTheRunInAppManager.Instance.InAppPurchaseValidationSuccess(productId);
        else
            OnTheRunInAppManager.Instance.InAppPurchaseValidationFailed(productId);
    }

    #endregion

    void Log(string logStr)
    {
        if (LOGS_ARE_ENABLED)
            Debug.Log("### Android InApp Implementation - " + logStr);
    }
}
#endif