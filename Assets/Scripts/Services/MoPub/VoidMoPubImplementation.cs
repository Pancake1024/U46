using UnityEngine;
using System.Collections;
using System;

public class VoidMoPubImplementation : MonoBehaviour, MoPubImplementation
{
    public bool InterstitialIsReady { get { return false; } }

    public void Initialize(Action interstitialDismissedCallback) { }
    public void ReportApplicationOpen() { }
    public void RequestInterstitial(string interstitialAdUnitId, bool showWhenReady) { }
    public void ShowInterstitial(string interstitialAdUnitId) { }
}