using UnityEngine;
using System.Collections;

public class EditorInAppImplementation : MonoBehaviour, InAppImplementation
{
    public void Init()
    {
        int basePrice = 10;
        for (int i = 0; i < 6;i++ )
        {
            string diamondsProductId = "purchase_diamonds_" + i;
            string coinsProductId = "purchase_coins_" + i;

            int displayPrice = (basePrice * (i + 1));
            string priceSting = displayPrice + " $B$";
            double localizedPrice = 0.09 * displayPrice;
            string currencyCode = "CCD";

            OnTheRunInAppManager.Instance.AddProductPrice(diamondsProductId, priceSting);
            OnTheRunInAppManager.Instance.AddLocalizedPrice(diamondsProductId, localizedPrice);
            OnTheRunInAppManager.Instance.AddCurrencyCode(diamondsProductId, currencyCode);

            displayPrice = ((basePrice - 1) * (i + 1));
            localizedPrice = 0.09 * displayPrice;
            OnTheRunInAppManager.Instance.AddProductPrice(coinsProductId, priceSting);
            OnTheRunInAppManager.Instance.AddLocalizedPrice(coinsProductId, localizedPrice);
            OnTheRunInAppManager.Instance.AddCurrencyCode(coinsProductId, currencyCode);
        }
        
        /*OnTheRunInAppManager.Instance.AddProductPrice("purchase_diamonds_0", "10 $B$");
        OnTheRunInAppManager.Instance.AddProductPrice("purchase_diamonds_1", "20 $B$");
        OnTheRunInAppManager.Instance.AddProductPrice("purchase_diamonds_2", "30 $B$");
        OnTheRunInAppManager.Instance.AddProductPrice("purchase_diamonds_3", "40 $B$");
        OnTheRunInAppManager.Instance.AddProductPrice("purchase_diamonds_4", "50 $B$");
        OnTheRunInAppManager.Instance.AddProductPrice("purchase_diamonds_5", "60 $B$");

        OnTheRunInAppManager.Instance.AddProductPrice("purchase_coins_0", "10 $B$");
        OnTheRunInAppManager.Instance.AddProductPrice("purchase_coins_1", "20 $B$");
        OnTheRunInAppManager.Instance.AddProductPrice("purchase_coins_2", "30 $B$");
        OnTheRunInAppManager.Instance.AddProductPrice("purchase_coins_3", "40 $B$");
        OnTheRunInAppManager.Instance.AddProductPrice("purchase_coins_4", "50 $B$");
        OnTheRunInAppManager.Instance.AddProductPrice("purchase_coins_5", "60 $B$");*/
    }

    public void PurchaseProduct(string productId)
    {
        OnTheRunInAppManager.Instance.ShowProcessingPopup();
        StartCoroutine(EditorPurchaseSuccessCoroutine(productId));
    }

    public void OnApplicationResumed()
    {
        return;
    }

    IEnumerator EditorPurchaseSuccessCoroutine(string productId)
    {
        yield return new WaitForSeconds(0.25f);
        OnTheRunInAppManager.Instance.ShowPurchaseSuccessfulPopup();
        OnTheRunInAppManager.Instance.FinalizeInAppPurchase(productId, true);
#if UNITY_EDITOR
        OnTheRunInAppManager.Instance.InAppPurchaseValidationSuccess(productId);
#endif
    }

    public void PurchaseFreeCoins()
    {
        //OnTheRunCoinsService.Instance.gameObject.SendMessage("onCoinsServiceReward", OnTheRunCoinsService.EDITOR_FREE_COINS_REWARD.ToString());
        OnTheRunCoinsService.Instance.OnFreeCoinsSelected();
    }
}