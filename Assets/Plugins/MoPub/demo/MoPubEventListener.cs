using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class MoPubEventListener : MonoBehaviour
{
#if UNITY_ANDROID || UNITY_IPHONE

	void OnEnable()
	{
		// Listen to all events for illustration purposes
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
		// Remove all event handlers
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

	void onAdLoadedEvent( float height )
	{
		Log("onAdLoadedEvent. height: " + height );
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
	}

	void onInterstitialFailedEvent()
	{
		Log("onInterstitialFailedEvent");
	}

	void onInterstitialShownEvent()
	{
		Log("onInterstitialShownEvent");
	}

	void onInterstitialClickedEvent()
	{
		Log("onInterstitialClickedEvent");
	}


	void onInterstitialDismissedEvent()
	{
		Log("onInterstitialDismissedEvent");
	}

	void onInterstitialExpiredEvent()
	{
		Log("onInterstitialExpiredEvent");
	}

	void Log(string logStr)
	{
		Debug.Log("### MoPub EventListener - " + logStr);
	}

#endif
}


