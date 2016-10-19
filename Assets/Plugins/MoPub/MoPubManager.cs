using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


#if UNITY_IPHONE || UNITY_ANDROID
public class MoPubManager : MonoBehaviour
{
	// Fired when an ad loads in the banner. Includes the ad height.
	public static event Action<float> onAdLoadedEvent;

	// Fired when an ad fails to load for the banner
	public static event Action onAdFailedEvent;

	// Android only. Fired when a banner ad is clicked
	public static event Action onAdClickedEvent;

	// Android only. Fired when a banner ad expands to encompass a greater portion of the screen
	public static event Action onAdExpandedEvent;

	// Android only. Fired when a banner ad collapses back to its initial size
	public static event Action onAdCollapsedEvent;

	// Fired when an interstitial ad is loaded and ready to be shown
	public static event Action onInterstitialLoadedEvent;

	// Fired when an interstitial ad fails to load
	public static event Action onInterstitialFailedEvent;

	// Fired when an interstitial ad is dismissed
	public static event Action onInterstitialDismissedEvent;

	// iOS only. Fired when an interstitial ad expires
	public static event Action onInterstitialExpiredEvent;

	// Android only. Fired when an interstitial ad is displayed
	public static event Action onInterstitialShownEvent;

	// Android only. Fired when an interstitial ad is clicked
	public static event Action onInterstitialClickedEvent;



	static MoPubManager()
	{
		Debug.Log ("### ### ### MopubManager - begin");
		var type = typeof( MoPubManager );
		try
		{
			// first we see if we already exist in the scene
			var obj = FindObjectOfType( type ) as MonoBehaviour;
			if( obj != null )
				return;

			// create a new GO for our manager
			var managerGO = new GameObject( type.ToString() );
			managerGO.AddComponent( type );
			DontDestroyOnLoad( managerGO );
		}
		catch( UnityException )
		{
			Debug.LogWarning( "It looks like you have the " + type + " on a GameObject in your scene. Please remove the script from your scene." );
		}
		Debug.Log ("### ### ### MopubManager - end");
	}



	public void onAdLoaded( string height )
	{
		if( onAdLoadedEvent != null )
			onAdLoadedEvent( float.Parse( height ) );
	}


	public void onAdFailed( string empty )
	{
		if( onAdFailedEvent != null )
			onAdFailedEvent();
	}


	public void onAdClicked( string empty )
	{
		if ( onAdClickedEvent != null )
			onAdClickedEvent();
	}


	public void onAdExpanded( string empty )
	{
		if ( onAdExpandedEvent != null )
			onAdExpandedEvent();
	}


	public void onAdCollapsed( string empty )
	{
		if ( onAdCollapsedEvent != null )
			onAdCollapsedEvent();
	}


	public void onInterstitialLoaded( string empty )
	{
		if( onInterstitialLoadedEvent != null )
			onInterstitialLoadedEvent();
	}


	public void onInterstitialFailed( string empty )
	{
		if( onInterstitialFailedEvent != null )
			onInterstitialFailedEvent();
	}


	public void onInterstitialDismissed( string empty )
	{
		if( onInterstitialDismissedEvent != null )
			onInterstitialDismissedEvent();
	}


	public void interstitialDidExpire( string empty )
	{
		if( onInterstitialExpiredEvent != null )
			onInterstitialExpiredEvent();
	}

	public void onInterstitialShown( string empty )
	{
		if ( onInterstitialShownEvent != null )
			onInterstitialShownEvent();
	}


	public void onInterstitialClicked( string empty )
	{
		if ( onInterstitialClickedEvent != null )
			onInterstitialClickedEvent();
	}

}
#endif
