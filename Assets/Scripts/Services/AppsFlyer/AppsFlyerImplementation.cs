using UnityEngine;
using System.Collections;

public interface AppsFlyerImplementation
{
    void Initialize(string developerKey, string appleAppId);
    void RequestConversionData(string gameObjectName, string callbackMethodName);
    void TrackEvent(string eventName, string currencyCode, string eventValue);
}