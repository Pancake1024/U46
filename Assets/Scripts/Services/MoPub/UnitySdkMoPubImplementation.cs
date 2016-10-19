﻿using UnityEngine;
using System.Collections;
using System;

#if UNITY_IPHONE || UNITY_ANDROID

public class UnitySdkMoPubImplementation : MonoBehaviour, MoPubImplementation
{
    public struct InterstitialLoadData
    {
        bool showWhenReady;
        string interstitialAdUnit;
        
        public bool ShowWhenReady { get { return showWhenReady; } }
        public string InterstitialAdUnit { get { return interstitialAdUnit; } }

        public InterstitialLoadData(bool showWhenReady, string interstitialAdUnit)
        {
            this.showWhenReady = showWhenReady;
            this.interstitialAdUnit = interstitialAdUnit;
        }
    }

    const bool LOGS_ARE_ENABLED = false;

    bool interstitialIsReady = false;
    InterstitialLoadData interstitialRequest;
    Action interstitialDismissedCallback;

    void OnEnable()
    {
		Log("On Enable - adding events");

        MoPubManager.onAdLoadedEvent += onAdLoadedEvent;
        MoPubManager.onAdFailedEvent += onAdFailedEvent;
        MoPubManager.onAdClickedEvent += onAdClickedEvent;
        MoPubManager.onAdExpandedEvent += onAdExpandedEvent;
        MoPubManager.onAdCollapsedEvent += onAdCollapsedEvent;
        MoPubManager.onInterstitialLoadedEvent += onInterstitialLoadedEvent;
        MoPubManager.onInterstitialFailedEvent += onInterstitialFailedEvent;
        MoPubManager.onInterstitialShownEvent += onInterstitialShownEvent;
        MoPubManager.onInterstitialClickedEvent += onInterstitialClickedEvent;
        MoPubManager.onInterstitialDismissedEvent += onInterstitialDismissedEvent;
        MoPubManager.onInterstitialExpiredEvent += onInterstitialExpiredEvent;
    }

    void OnDisable()
	{
		Log("On Disable - removing events");
		
        MoPubManager.onAdLoadedEvent -= onAdLoadedEvent;
        MoPubManager.onAdFailedEvent -= onAdFailedEvent;
        MoPubManager.onAdClickedEvent -= onAdClickedEvent;
        MoPubManager.onAdExpandedEvent -= onAdExpandedEvent;
        MoPubManager.onAdCollapsedEvent -= onAdCollapsedEvent;
        MoPubManager.onInterstitialLoadedEvent -= onInterstitialLoadedEvent;
        MoPubManager.onInterstitialFailedEvent -= onInterstitialFailedEvent;
        MoPubManager.onInterstitialShownEvent -= onInterstitialShownEvent;
        MoPubManager.onInterstitialClickedEvent -= onInterstitialClickedEvent;
        MoPubManager.onInterstitialDismissedEvent -= onInterstitialDismissedEvent;
        MoPubManager.onInterstitialExpiredEvent -= onInterstitialExpiredEvent;
    }

    public bool InterstitialIsReady { get { return interstitialIsReady; } }

    public void Initialize(Action interstitialDismissedCallback)
	{
		Log("Initialize()");
		//gameObject.AddComponent<MoPubEventListener>();
		this.interstitialDismissedCallback = interstitialDismissedCallback;
	}

    public void ReportApplicationOpen()
    {
        Log("ReportApplicationOpen()");
        MoPub.reportApplicationOpen();
    }

    public void RequestInterstitial(string interstitialAdUnitId, bool showWhenReady)
    {
        Log("RequestInterstitial() - interstitialAdUnitId: " + interstitialAdUnitId);
        interstitialIsReady = false;
        interstitialRequest = new InterstitialLoadData(showWhenReady, interstitialAdUnitId);

        MoPub.requestInterstitialAd(interstitialAdUnitId);
    }
    
    public void ShowInterstitial(string interstitialAdUnitId)
    {
		Log("ShowInterstitial() - interstitialAdUnitId: " + interstitialAdUnitId);
        interstitialIsReady = false;
        MoPub.showInterstitialAd(interstitialAdUnitId);
    }

    #region Events Callbacks

    void onAdLoadedEvent(float height)
    {
        Log("onAdLoadedEvent - height: " + height);
    }

    void onAdFailedEvent()
    {
        Log("onAdFailedEvent");
    }

    void onAdClickedEvent()
    {
        Log("onAdClickedEvent");
    }

    void onAdExpandedEvent()
    {
        Log("onAdExpandedEvent");
    }

    void onAdCollapsedEvent()
    {
        Log("onAdCollapsedEvent");
    }

    void onInterstitialLoadedEvent()
    {
        Log("onInterstitialLoadedEvent");
        interstitialIsReady = true;

        if (interstitialRequest.ShowWhenReady && OnTheRunInterstitialsManager.Instance.DoesCurrentPagePermitInterstitials())
            ShowInterstitial(interstitialRequest.InterstitialAdUnit);
    }

    void onInterstitialFailedEvent()
    {
		Log("onInterstitialFailedEvent");
		interstitialIsReady = false;
    }

    void onInterstitialShownEvent()
    {
        Log("onInterstitialShownEvent ---------------------------------------------------------------- INTERSTITIAL SHOWING - NO GAME SOUNDS");
        interstitialIsReady = false;
    }

    void onInterstitialClickedEvent()
    {
        Log("onInterstitialClickedEvent ---------------------------------------------------------------- INTERSTITIAL CLICKED - ALLOW GAME SOUNDS");
		interstitialIsReady = false;
    }

    void onInterstitialDismissedEvent()
    {
        Log("onInterstitialDismissedEvent ---------------------------------------------------------------- INTERSTITIAL DISMISSED - ALLOW GAME SOUNDS");
        interstitialIsReady = false;

        if (interstitialDismissedCallback != null)
            interstitialDismissedCallback();
    }

    void onInterstitialExpiredEvent()
    {
        Log("onInterstitialExpiredEvent ---------------------------------------------------------------- INTERSTITIAL EXPIRED - ALLOW GAME SOUNDS");
		interstitialIsReady = false;
    }
    #endregion

    void Log(string logStr)
    {
        if (LOGS_ARE_ENABLED)
            Debug.Log("### MoPub Unity SDK - " + logStr);
    }
}
#endif