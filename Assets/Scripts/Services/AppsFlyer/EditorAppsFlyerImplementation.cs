using UnityEngine;
using System.Collections;

public class EditorAppsFlyerImplementation : MonoBehaviour, AppsFlyerImplementation
{
    public void Initialize(string developerKey, string appleAppId)
    {
        Debug.Log("### AppsFlyer - EDITOR - Initialization");
    }

    public void RequestConversionData(string gameObjectName, string callbackMethodName)
    {
		Debug.Log("### AppsFlyer - EDITOR  - RequestConversionData");
    }

    public void TrackEvent(string eventName, string currencyCode, string eventValue)
    {
        Debug.Log("### AppsFlyer - EDITOR  - TrackEvent - eventName: " + eventName + ", currencyCode: " + currencyCode + ", eventValue: " + eventValue);
    }
}