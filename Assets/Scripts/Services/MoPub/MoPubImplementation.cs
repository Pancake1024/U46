using UnityEngine;
using System.Collections;
using System;

public interface MoPubImplementation
{
    bool InterstitialIsReady { get; }

	void Initialize(Action interstitialDismissedCallback);
    void ReportApplicationOpen();
    void RequestInterstitial(string interstitialAdUnitId, bool showWhenReady);
    void ShowInterstitial(string interstitialAdUnitId);
}