using UnityEngine;
using System.Collections;

public class VoidAppsFlyerImplementation : MonoBehaviour, AppsFlyerImplementation
{
    public void Initialize(string developerKey, string appleAppId) { }
    public void RequestConversionData(string gameObjectName, string callbackMethodName) { }
    public void TrackEvent(string eventName, string currencyCode, string eventValue) { }
}