using UnityEngine;
using System;
using System.Collections;


#if UNITY_IPHONE || UNITY_ANDROID

public static class MoPub
{
	// Enables/disables location support for banners and interstitials
	public static void enableLocationSupport( bool shouldUseLocation )
	{
#if UNITY_IPHONE
		MoPubBinding.enableLocationSupport( true );
#elif UNITY_ANDROID
		MoPubAndroid.setLocationAwareness( MoPubLocationAwareness.NORMAL );
#endif
	}


	// Creates a banner of the given type at the given position. bannerType is iOS only.
#if UNITY_IPHONE
	public static void createBanner( string adUnitId, MoPubAdPosition position, MoPubBannerType bannerType = MoPubBannerType.Size320x50 )
	{
		MoPubBinding.createBanner( bannerType, position, adUnitId );
	}

#elif UNITY_ANDROID

	public static void createBanner( string adUnitId, MoPubAdPosition position )
	{
		MoPubAndroid.createBanner( adUnitId, position );
	}

#endif


	// Destroys the banner and removes it from view
	public static void destroyBanner()
	{
#if UNITY_IPHONE
		MoPubBinding.destroyBanner();
#elif UNITY_ANDROID
		MoPubAndroid.destroyBanner();
#endif
	}


	// Shows/hides the banner
	public static void showBanner( bool shouldShow )
	{
#if UNITY_IPHONE
		MoPubBinding.showBanner( shouldShow );
#elif UNITY_ANDROID
		MoPubAndroid.showBanner( shouldShow );
#endif
	}


	// Starts loading an interstitial ad
	public static void requestInterstitialAd( string adUnitId, string keywords = "" )
	{
#if UNITY_IPHONE
		MoPubBinding.requestInterstitialAd( adUnitId, keywords );
#elif UNITY_ANDROID
		MoPubAndroid.requestInterstitialAd( adUnitId, keywords );
#endif
	}


	// If an interstitial ad is loaded this will take over the screen and show the ad
	public static void showInterstitialAd( string adUnitId )
	{
#if UNITY_IPHONE
		MoPubBinding.showInterstitialAd( adUnitId );
#elif UNITY_ANDROID
		MoPubAndroid.showInterstitialAd( adUnitId );
#endif
	}


	// Reports an app download to MoPub. iTunesAppId is iOS only.
	public static void reportApplicationOpen( string iTunesAppId = null )
	{
#if UNITY_IPHONE
		MoPubBinding.reportApplicationOpen( iTunesAppId );
#elif UNITY_ANDROID
		MoPubAndroid.reportApplicationOpen();
#endif
	}

}

#endif