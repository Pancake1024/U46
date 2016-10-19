using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using com.adjust.sdk;

public class UnitySdkAdjustImplementation : MonoBehaviour, AdjustImplementation
{
    public void Initialize(string appToken, bool isInProduction, bool enableEventBuffering)
    {
        Adjust adjustComponent = gameObject.AddComponent<Adjust>();
        adjustComponent.appToken = appToken;
        adjustComponent.environment = isInProduction ? AdjustEnvironment.Production : AdjustEnvironment.Sandbox;
        adjustComponent.eventBuffering = enableEventBuffering;

        adjustComponent.StartWithCurrentParameters();
    }

    public void SetTrackingEnabled(bool isEnabled)
    {
        Adjust.setEnabled(isEnabled);
    }

    public void TrackEvent(string eventToken)
    {
        AdjustEvent adjEvent = new AdjustEvent(eventToken);
        Adjust.trackEvent(adjEvent);
    }

    public void TrackEvent(string eventToken, Dictionary<string, string> parameters)
    {
        AdjustEvent adjustEvent = new AdjustEvent(eventToken);
        foreach (var kvp in parameters)
            adjustEvent.addCallbackParameter(kvp.Key, kvp.Value);

        Adjust.trackEvent(adjustEvent);
    }

    public void TrackRevenue(string eventToken, string currencyCode, double amountInCents)
    {
        AdjustEvent adjustEvent = new AdjustEvent(eventToken);
        adjustEvent.setRevenue(amountInCents, currencyCode);

        Adjust.trackEvent(adjustEvent);
    }

    public void TrackRevenue(string eventToken, string currencyCode, double amountInCents, Dictionary<string, string> parameters)
    {
        AdjustEvent adjustEvent = new AdjustEvent(eventToken);
        adjustEvent.setRevenue(amountInCents, currencyCode);
        foreach (var kvp in parameters)
            adjustEvent.addCallbackParameter(kvp.Key, kvp.Value);

        Adjust.trackEvent(adjustEvent);
    }
}