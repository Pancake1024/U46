using UnityEngine;
using System.Collections;
using System;
using SBS.Core;

public class IosInAppImplementation : MonoBehaviour, InAppImplementation
{
    OnTheRunInAppManager.inAppPurchProduct[] products = null;
    bool isProcessing;

    public void Init()
    {
        products = null;
        isProcessing = false;

		SBS.Miniclip.InAppPurchaseBindings.LoadStore_WithReceiptValidation();
        RegisterToInappEvents();
    }

    public void PurchaseProduct(string productId)
	{
        isProcessing = true;
        OnTheRunInAppManager.Instance.ShowProcessingPopup();
		SBS.Miniclip.InAppPurchaseBindings.PurchaseProduct(GetFullyQualifiedProductIdForProduct(productId));
    }

    public void OnApplicationResumed()
    {
        return;
    }

    public void PurchaseFreeCoins()
    {
        OnTheRunCoinsService.Instance.OnFreeCoinsSelected();
    }

    #region Register To InApp Events

    void RegisterToInappEvents()
    {
        RegisterToLoadStoreInappEvents();
        RegisterToTransactionInappEvents();
    }

    void RegisterToLoadStoreInappEvents()
    {
        ObjCDispatcher.Instance.RegisterEvent("InAppPurchProductsRequestStart", onRequestStart);
        ObjCDispatcher.Instance.RegisterEvent("InAppPurchInvalidProductId", onInvalidProductId);

        ObjCDispatcher.Instance.RegisterEvent("InAppPurchProductId", (str) =>
        {
            onProductData(str, (product, data) =>
            {
                product.id = data;
            });
        });
        ObjCDispatcher.Instance.RegisterEvent("InAppPurchProductTitle", (str) =>
        {
            onProductData(str, (product, data) =>
            {
                product.title = data;
            });
        });
        ObjCDispatcher.Instance.RegisterEvent("InAppPurchProductDesc", (str) =>
		{
            onProductData(str, (product, data) =>
            {
                product.description = data;
            });
        });
        ObjCDispatcher.Instance.RegisterEvent("InAppPurchProductPrice", (str) =>
		{
            onProductData(str, (product, data) =>
            {
                product.price = data;
            });
        });
        ObjCDispatcher.Instance.RegisterEvent("InAppPurchProductCurrencyCode", (str) =>
        {
            onProductData(str, (product, data) =>
            {
                product.currencyCode = data;
            });
        });
        ObjCDispatcher.Instance.RegisterEvent("InAppPurchProductTrackableLocalizedPrice", (str) =>
        {
            onProductData(str, (product, data) =>
            {
                product.localizedPrice = -1.0;
                double parsedPrice;
                if (double.TryParse(data, out parsedPrice))
                    product.localizedPrice = parsedPrice;
            });
        });
        ObjCDispatcher.Instance.RegisterEvent("InAppPurchProductsRequestEnd", onRequestEnd);
        ObjCDispatcher.Instance.RegisterEvent("InAppPurchRequestError", onRequestError);
    }

    void RegisterToTransactionInappEvents()
    {
        ObjCDispatcher.Instance.RegisterEvent("InAppPurchProvideContent", provideContent);
        ObjCDispatcher.Instance.RegisterEvent("InAppPurchRestoreContent", restoreContent);
        ObjCDispatcher.Instance.RegisterEvent("InAppPurchFinishTransaction", onFinishTransaction);
        ObjCDispatcher.Instance.RegisterEvent("InAppPurchProcessing", onProcessing);
        ObjCDispatcher.Instance.RegisterEvent("InAppPurchEndProcessing", onEndProcessing);
		
		ObjCDispatcher.Instance.RegisterEvent("InAppPurchValidated", onInappValidationSuccess);
		ObjCDispatcher.Instance.RegisterEvent("InAppPurchValidationFailed", onInappValidationFailed);
    }

#endregion

    #region Load Store Events

    void onRequestStart(string productsCount)
	{
        int count = int.Parse(productsCount);
        products = new OnTheRunInAppManager.inAppPurchProduct[count];
        
        for (int i = 0; i < count; ++i)
            products[i] = new OnTheRunInAppManager.inAppPurchProduct();
    }

    void onInvalidProductId(string identifier)
	{
    }

    void onProductData(string data, Action<OnTheRunInAppManager.inAppPurchProduct, string> setter)
	{
        string[] indexAndData = data.Split('|');
        int index = int.Parse(indexAndData[0]);
        setter.Invoke(products[index], indexAndData[1]);
    }

    void onRequestEnd(string param)
	{
        OnTheRunInAppManager.Instance.OnInappPurchaseProductsReady(products);
    }

    void onRequestError(string error)
	{
    }
    
    #endregion

    #region Transaction Events

    void provideContent(string productIdentifier)
	{
        isProcessing = false;
        OnTheRunInAppManager.Instance.FinalizeInAppPurchase(productIdentifier, true);
    }

    void restoreContent(string productIdentifier)
	{
    }

    void onFinishTransaction(string param)
	{
        bool wasSuccessful = ('1' == param[0]);

        isProcessing = false;
        if (!wasSuccessful)
            OnTheRunInAppManager.Instance.ShowPurchaseFailedPopup();
		else
			OnTheRunInAppManager.Instance.ShowPurchaseSuccessfulPopup();
    }

    void onProcessing(string productId)
	{
        isProcessing = true;
        OnTheRunInAppManager.Instance.ShowProcessingPopup();
    }

    void onEndProcessing(string productId)
	{
        if (isProcessing)
            OnTheRunInAppManager.Instance.ShowPurchaseAbortedPopup();
        isProcessing = false;
    }

	void onInappValidationSuccess(string productId)
	{
		OnTheRunInAppManager.Instance.InAppPurchaseValidationSuccess(productId);
	}

	void onInappValidationFailed(string productId)
	{
		OnTheRunInAppManager.Instance.InAppPurchaseValidationFailed(productId);
	}

    #endregion

	string GetFullyQualifiedProductIdForProduct(string productName)
	{
		foreach (OnTheRunInAppManager.inAppPurchProduct product in products)
		{
			if (productName.Equals(product.id.Substring(product.id.LastIndexOf('.') + 1)))
				return product.id;
		}
		
		Asserts.Assert(false, "IosInAppImplementation.GetFulltyQualifiedProductIdForProduct(" + productName + ")");
		return string.Empty;
	}
}