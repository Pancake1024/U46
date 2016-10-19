using UnityEngine;
using System.Collections;

public class UnitySdkAppsFlyerImplementation : MonoBehaviour, AppsFlyerImplementation
{
    const bool LOGS_ARE_ENABLED = false;

    public void Initialize(string developerKey, string appleAppId)
    {
        Log("Initialization");

        AppsFlyer.setAppsFlyerKey(developerKey);
        AppsFlyer.setAppID(appleAppId);
        AppsFlyer.trackAppLaunch();
    }

    public void RequestConversionData(string gameObjectName, string callbackMethodName)
    {
        Log("RequestConversionData");

        AppsFlyer.loadConversionData(gameObjectName, callbackMethodName);
    }

    public void TrackEvent(string eventName, string currencyCode, string eventValue)
    {
        Log("TrackEvent - eventName: " + eventName + ", currencyCode: " + currencyCode + ", eventValue: " + eventValue);

        AppsFlyer.setCurrencyCode(currencyCode);
        AppsFlyer.trackEvent(eventName, eventValue);
    }

    void Log(string logStr)
    {
        if (LOGS_ARE_ENABLED)
            Debug.Log("### AppsFlyer Unity SDK - " + logStr);
    }
}