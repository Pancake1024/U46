using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface AdjustImplementation
{
    void Initialize(string appToken, bool isInProduction, bool enableEventBuffering);

    // void SetAttributionChangedDelegate(Action<AdjustAttribution> callback);

    void SetTrackingEnabled(bool isEnabled);

    void TrackEvent(string eventToken);
    void TrackEvent(string eventToken, Dictionary<string, string> parameters);

    void TrackRevenue(string eventToken, string currencyCode, double amountInCents);
    void TrackRevenue(string eventToken, string currencyCode, double amountInCents, Dictionary<string, string> parameters);
}