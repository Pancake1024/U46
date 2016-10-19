using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorAdjustImplementation : MonoBehaviour, AdjustImplementation
{
    bool trackingIsEnabled = true;
    
    public void Initialize(string appToken, bool isInProduction, bool enableEventBuffering)
    {
        Log("Initialize");
    }

    public void SetTrackingEnabled(bool isEnabled)
    {
        trackingIsEnabled = isEnabled;
    }

    public void TrackEvent(string eventToken)
    {
        if (!trackingIsEnabled)
            return;

        Log("TrackEvent - eventToken: " + eventToken);
    }

    public void TrackEvent(string eventToken, Dictionary<string, string> parameters)
    {
        if (!trackingIsEnabled)
            return;

        string paramsStr = string.Empty;
        foreach (var kvp in parameters)
            paramsStr += kvp.Key + ": " + kvp.Value + "\n";

        Log("TrackEvent - eventToken: " + eventToken + "\n" + paramsStr);
    }

    public void TrackRevenue(string eventToken, string currencyCode, double amountInCents)
    {
        if (!trackingIsEnabled)
            return;
        
        Log("TrackRevenue - eventToken: " + eventToken + " - currencyCode: " + currencyCode + " - amountInCents: " + amountInCents);
    }

    public void TrackRevenue(string eventToken, string currencyCode, double amountInCents, Dictionary<string, string> parameters)
    {
        if (!trackingIsEnabled)
            return;

        string paramsStr = string.Empty;
        foreach (var kvp in parameters)
            paramsStr += kvp.Key + ": " + kvp.Value + "\n";

        Log("TrackRevenue - eventToken: " + eventToken + " - currencyCode: " + currencyCode + " - amountInCents: " + amountInCents + "\n" + paramsStr);
    }

    void Log(string logStr)
    {
        Debug.Log("### Editor Adjust Implementation - " + logStr);
    }
}