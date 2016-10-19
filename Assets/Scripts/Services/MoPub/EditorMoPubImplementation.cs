using UnityEngine;
using System.Collections;
using System;

public class EditorMoPubImplementation : MonoBehaviour, MoPubImplementation
{
    const bool LOGS_ARE_ENABLED = false;

    public bool InterstitialIsReady { get { return interstitialIsReady; } }

    Action interstitialDismissedCallback;

    bool interstitialIsReady = false;
    
#if UNITY_IPHONE || UNITY_ANDROID
    UnitySdkMoPubImplementation.InterstitialLoadData interstitialRequest;
#endif

    public void Initialize(Action interstitialDismissedCallback)
	{
		Log("Initialize()");
        this.interstitialDismissedCallback = interstitialDismissedCallback;
	}

    public void ReportApplicationOpen()
    {
        Log("ReportApplicationOpen()");
    }

    public void RequestInterstitial(string interstitialAdUnitId, bool showWhenReady)
    {
        Log("RequestInterstitial() - interstitialAdUnitId: " + interstitialAdUnitId);
        
#if UNITY_IPHONE || UNITY_ANDROID
        interstitialRequest = new UnitySdkMoPubImplementation.InterstitialLoadData(showWhenReady, interstitialAdUnitId);
#endif
    }

    public void ShowInterstitial(string interstitialAdUnitId)
    {
		Log("ShowInterstitial() - interstitialAdUnitId: " + interstitialAdUnitId);

        if (interstitialDismissedCallback != null)
            interstitialDismissedCallback();
    }

    void Log(string logStr)
    {
        if (LOGS_ARE_ENABLED)
            Debug.Log("### MoPub EDITOR - " + logStr);
    }

#if UNITY_EDITOR
    /*void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            interstitialIsReady = false;

        if (Input.GetKeyDown(KeyCode.W))
            onInterstitialLoadedEvent();
    }*/
#endif

    void onInterstitialLoadedEvent()
    {
        Log("onInterstitialLoadedEvent");
        interstitialIsReady = true;
        
#if UNITY_IPHONE || UNITY_ANDROID
        if (interstitialRequest.ShowWhenReady && OnTheRunInterstitialsManager.Instance.DoesCurrentPagePermitInterstitials())
            ShowInterstitial(interstitialRequest.InterstitialAdUnit);
#endif
    }
}