using UnityEngine;
using System.Collections;
using System;

public class Wp8InAppImplementation : MonoBehaviour, InAppImplementation
{
    public class ProductPurchaseArgs : EventArgs
    {
        string identifier;

        public string Identifier { get { return identifier; } }

        public ProductPurchaseArgs(string id)
        {
            identifier = id;
        }
    }

    public delegate void EventHandler(EventArgs args);
    public static event EventHandler LoadStoreEvent = delegate { };
    public static event EventHandler PurchaseProductEvent = delegate { };
    public static event EventHandler RestorePurchasesEvent = delegate { };

    static Wp8InAppImplementation instance = null;

    bool storeIsLoaded = false;

    #region InAppImplementation Methods

    public void Init()
    {
        instance = this;

        storeIsLoaded = false;
        
        LoadStoreEvent(new EventArgs());
    }

    public void PurchaseProduct(string productId)
    {
        OnTheRunInAppManager.Instance.ShowProcessingPopup();
        PurchaseProductEvent(new ProductPurchaseArgs(productId));
    }

    public void OnApplicationResumed()
    {
        return;
    }

    public void PurchaseFreeCoins()
    {
        return;
    }

    #endregion

    #region Static Response Callbacks

    public static void OnStoreLoaded(OnTheRunInAppManager.inAppPurchProduct[] newProducts)
    {
        instance.storeIsLoaded = true;
        OnTheRunInAppManager.Instance.OnInappPurchaseProductsReady(newProducts);
    }

    public static void OnProductPurchased(string productIdentifier)
    {
        instance.OnPurchaseSuccess(productIdentifier);
    }

    public static void OnPurchaseRestored(string productIdentifier)
    {
        instance.OnRestoreSuccess(productIdentifier);
    }

    public static void OnTransactionAborted()
    {
        instance.OnPurchaseAborted();
    }

    public static void OnTransactionFailed()
    {
        instance.OnPurchaseFailed();
    }

    #endregion

    #region Instance Response Coroutines

    void OnPurchaseSuccess(string productId)
    {
        StartCoroutine(WP8DeferPurchaseSuccess(productId));
    }

    IEnumerator WP8DeferPurchaseSuccess(string productId)
    {
        yield return new WaitForEndOfFrame();

        OnTheRunInAppManager.Instance.ShowPurchaseSuccessfulPopup();
        OnTheRunInAppManager.Instance.FinalizeInAppPurchase(productId, true);

        PlayerPersistentData.Instance.Save();
    }

    void OnRestoreSuccess(string productId)
    {
        StartCoroutine(WP8DeferPurchaseRestore(productId));
    }

    IEnumerator WP8DeferPurchaseRestore(string productId)
    {
        yield return new WaitForEndOfFrame();

        // No restore purchases for On The Run - just consumable products
    }

    void OnPurchaseAborted()
    {
        StartCoroutine(WP8DeferPurchaseAbort());
    }

    IEnumerator WP8DeferPurchaseAbort()
    {
        yield return new WaitForEndOfFrame();

        OnTheRunInAppManager.Instance.ShowPurchaseAbortedPopup();
    }

    void OnPurchaseFailed()
    {
        StartCoroutine(WP8DeferPurchaseFail());
    }

    IEnumerator WP8DeferPurchaseFail()
    {
        yield return new WaitForEndOfFrame();

        OnTheRunInAppManager.Instance.ShowPurchaseFailedPopup();
    }

    #endregion
}