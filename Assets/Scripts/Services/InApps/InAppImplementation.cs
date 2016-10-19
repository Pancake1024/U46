using UnityEngine;
using System.Collections;

public interface InAppImplementation
{
    void Init();
    void PurchaseProduct(string productId);
    void OnApplicationResumed();
    void PurchaseFreeCoins();
}