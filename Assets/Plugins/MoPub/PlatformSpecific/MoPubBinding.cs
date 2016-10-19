using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;



#if UNITY_IPHONE

public enum MoPubBannerType
{
	Size320x50,
	Size300x250,
	Size728x90,
	Size160x600
}


public enum MoPubAdPosition
{
	TopLeft,
	TopCenter,
	TopRight,
	Centered,
	BottomLeft,
	BottomCenter,
	BottomRight
}



public class MoPubBinding
{
	[DllImport("__Internal")]
	private static extern void _moPubEnableLocationSupport( bool shouldUseLocation );

	// Enables/disables location support for banners and interstitials
	public static void enableLocationSupport( bool shouldUseLocation )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_moPubEnableLocationSupport( shouldUseLocation );
	}


	[DllImport("__Internal")]
	private static extern void _moPubCreateBanner( int bannerType, int position, string adUnitId );

	// Creates a banner of the given type at the given position
	public static void createBanner( MoPubBannerType bannerType, MoPubAdPosition position, string adUnitId )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_moPubCreateBanner( (int)bannerType, (int)position, adUnitId );
	}


	[DllImport("__Internal")]
	private static extern void _moPubDestroyBanner();

	// Destroys the banner and removes it from view
	public static void destroyBanner()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_moPubDestroyBanner();
	}


	[DllImport("__Internal")]
	private static extern void _moPubShowBanner( bool shouldShow );

	// Shows/hides the banner
	public static void showBanner( bool shouldShow )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_moPubShowBanner( shouldShow );
	}


	[DllImport("__Internal")]
	private static extern void _moPubRefreshAd( string keywords );

	// Refreshes the ad banner with optional keywords
	public static void refreshAd( string keywords )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_moPubRefreshAd( keywords );
	}


	[DllImport("__Internal")]
	private static extern void _moPubRequestInterstitialAd( string adUnitId, string keywords );

	// Starts loading an interstitial ad
	public static void requestInterstitialAd( string adUnitId, string keywords = "" )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_moPubRequestInterstitialAd( adUnitId, keywords );
	}


	[DllImport("__Internal")]
	private static extern void _moPubShowInterstitialAd( string adUnitId );

	// If an interstitial ad is loaded this will take over the screen and show the ad
	public static void showInterstitialAd( string adUnitId )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_moPubShowInterstitialAd( adUnitId );
	}


	[DllImport("__Internal")]
	private static extern void _moPubReportApplicationOpen( string iTunesAppId );

	// Reports an app download to MoPub
	public static void reportApplicationOpen( string iTunesAppId )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_moPubReportApplicationOpen( iTunesAppId );
	}


}
#endif