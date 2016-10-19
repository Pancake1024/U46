using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SBS.Core;

#if UNITY_ANDROID && UNITY_KINDLE
public class KindleInAppImplementation : MonoBehaviour, InAppImplementation
{
    OnTheRunInAppManager.inAppPurchProduct[] products = null;
    string suspendedPurchaseProductId;
    
    public void Init()
    {
        gameObject.AddComponent<AmazonIAPManager>();
        products = null;
        suspendedPurchaseProductId = string.Empty;

        LogIn();
    }
    
    public void PurchaseProduct(string productId)
    {
        OnTheRunInAppManager.Instance.ShowProcessingPopup();

        string productName = productId.Substring(productId.LastIndexOf('.') + 1);
        OnTheRunInAppManager.inAppPurchProduct product = GetProductFromName(productName);
        if (product != null)
        {
            AmazonIAP.initiatePurchaseRequest(product.id);
            suspendedPurchaseProductId = product.id;
        }
    }

    public void OnApplicationResumed()
    {
        LogIn();
    }

    public void PurchaseFreeCoins()
    {
        OnTheRunCoinsService.Instance.OnFreeCoinsSelected();
    }

    void LogIn()
    {
        AmazonIAP.initiateItemDataRequest(OnTheRunInAppManager.Instance.GetProductsSkusList("com.miniclip.ontherunamazon.").ToArray());
    }

    void SetSuspendedPurchaseProductId(string sku)
    {
        suspendedPurchaseProductId = sku;
    }

    void OnEnable()
    {
        // Listen to all events for illustration purposes
        AmazonIAPManager.itemDataRequestFailedEvent += itemDataRequestFailedEvent;
        AmazonIAPManager.itemDataRequestFinishedEvent += itemDataRequestFinishedEvent;
        AmazonIAPManager.purchaseFailedEvent += purchaseFailedEvent;
        AmazonIAPManager.purchaseSuccessfulEvent += purchaseSuccessfulEvent;
        AmazonIAPManager.purchaseUpdatesRequestFailedEvent += purchaseUpdatesRequestFailedEvent;
        AmazonIAPManager.purchaseUpdatesRequestSuccessfulEvent += purchaseUpdatesRequestSuccessfulEvent;
        AmazonIAPManager.onSdkAvailableEvent += onSdkAvailableEvent;
        AmazonIAPManager.onGetUserIdResponseEvent += onGetUserIdResponseEvent;
    }


    void OnDisable()
    {
        // Remove all event handlers
        AmazonIAPManager.itemDataRequestFailedEvent -= itemDataRequestFailedEvent;
        AmazonIAPManager.itemDataRequestFinishedEvent -= itemDataRequestFinishedEvent;
        AmazonIAPManager.purchaseFailedEvent -= purchaseFailedEvent;
        AmazonIAPManager.purchaseSuccessfulEvent -= purchaseSuccessfulEvent;
        AmazonIAPManager.purchaseUpdatesRequestFailedEvent -= purchaseUpdatesRequestFailedEvent;
        AmazonIAPManager.purchaseUpdatesRequestSuccessfulEvent -= purchaseUpdatesRequestSuccessfulEvent;
        AmazonIAPManager.onSdkAvailableEvent -= onSdkAvailableEvent;
        AmazonIAPManager.onGetUserIdResponseEvent -= onGetUserIdResponseEvent;
    }

    void itemDataRequestFailedEvent()
    {
    }

    void itemDataRequestFinishedEvent(List<string> unavailableSkus, List<AmazonItem> availableItems)
    {
        SetInventory(availableItems);
        OnTheRunInAppManager.Instance.OnInappPurchaseProductsReady(products);
    }

    void SetInventory(List<AmazonItem> availableItems)
    {
        products = new OnTheRunInAppManager.inAppPurchProduct[availableItems.Count];
        int i = 0;
        foreach (AmazonItem item in availableItems)
        {
            OnTheRunInAppManager.inAppPurchProduct product = new OnTheRunInAppManager.inAppPurchProduct();
            product.description = availableItems[i].description;
            product.title = availableItems[i].title;
            product.price = availableItems[i].price;
            product.id = availableItems[i].sku;
            product.type = availableItems[i].type;
            product.isConsumable = string.Equals(availableItems[i].type, "CONSUMABLE");

            products[i++] = product;
        }
    }

    void purchaseFailedEvent(string reason)
    {
        // The reasons for failure are:
        // INVALID_SKU
        // "Already Entitled"
        // FAILED

        if (reason.Equals("Already Entitled"))
            OnTheRunInAppManager.Instance.FinalizeInAppPurchase(suspendedPurchaseProductId, false);

        suspendedPurchaseProductId = string.Empty;

        OnTheRunInAppManager.Instance.HideProcessingPopup();
    }

    void purchaseSuccessfulEvent(AmazonReceipt receipt)
    {
        OnTheRunInAppManager.Instance.FinalizeInAppPurchase(receipt.sku, true);
        suspendedPurchaseProductId = string.Empty;

        OnTheRunInAppManager.Instance.HideProcessingPopup();
    }

    void purchaseUpdatesRequestFailedEvent()
    {
        suspendedPurchaseProductId = string.Empty;

        OnTheRunInAppManager.Instance.HideProcessingPopup();
    }

    void purchaseUpdatesRequestSuccessfulEvent(List<string> revokedSkus, List<AmazonReceipt> receipts)
    {
        foreach (var receipt in receipts)
            OnTheRunInAppManager.Instance.FinalizeInAppPurchase(receipt.sku, false);

        OnTheRunInAppManager.Instance.HideProcessingPopup();
    }

    void onSdkAvailableEvent(bool isTestMode)
    {
    }

    void onGetUserIdResponseEvent(string userId)
    {
    }

    OnTheRunInAppManager.inAppPurchProduct GetProductFromName(string productName)
    {
        if (null == products)
            return null;

        foreach (OnTheRunInAppManager.inAppPurchProduct product in products)
        {
            string name = product.id.Substring(product.id.LastIndexOf('.') + 1);
            if (productName == name)
                return product;
        }
        return null;
    }
}
#endif