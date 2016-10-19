using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoidAdjustImplementation : MonoBehaviour, AdjustImplementation
{
    public void Initialize(string appToken, bool isInProduction, bool enableEventBuffering) { }

    public void SetTrackingEnabled(bool isEnabled) { }

    public void TrackEvent(string eventToken) { }
    public void TrackEvent(string eventToken, Dictionary<string, string> parameters) { }

    public void TrackRevenue(string eventToken, string currencyCode, double amountInCents) { }
    public void TrackRevenue(string eventToken, string currencyCode, double amountInCents, Dictionary<string, string> parameters) { }
}