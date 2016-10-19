using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SponsorPay
{
	
#if UNITY_IPHONE || UNITY_IOS
	public class IOSSponsorPayPlugin : ISponsorPayPlugin
	{
		[DllImport ("__Internal")]
		public static extern void _SPSetCallbackGameObjectName(string name);

		[DllImport ("__Internal")]
		private static extern void _SPSetPluginVersion(string pluginVersion);

		[DllImport ("__Internal")]
		public static extern string _SPStartSDK(string appId, string userId, string secretToken);

		[DllImport ("__Internal")]
		public static extern void _SPLaunchOfferWall(string credentialsToken, string currencyName, string placementId);

		[DllImport ("__Internal")]
		public static extern void _SPSendDeltaOfCoinsRequest(string credentialsToken, string currencyId); 

		[DllImport ("__Internal")]
	 	public static extern void _SPSetShouldShowNotificationOnVCSCoins(int should);

		[DllImport ("__Internal")]
		public static extern void _SPRequestBrandEngageOffers(string credentialsToken, string currencyName, int queryVCS, string currencyId, string placementId);

		[DllImport ("__Internal")]
		public static extern void _SPStartBrandEngage();

		[DllImport ("__Internal")]
		public static extern void _SPRequestIntersitialAds(string credentialsToken, string placementId);

		[DllImport ("__Internal")]
		public static extern void _SPShowInterstitialAd();

		[DllImport ("__Internal")]
		public static extern void _SPSetShouldShowBrandEngageRewardNotification(int should);

		[DllImport ("__Internal")]
		public static extern void _SPReportActionCompletion(string credentialsToken, string actionId);

		[DllImport ("__Internal")]
		public static extern void _SPEnableLogging(int should);

		[DllImport ("__Internal")]
		public static extern void _SPVideoDownloadPause(bool pause);

		[DllImport ("__Internal")]
		public static extern void _SPSetLogLevel(int logLevel);

		[DllImport ("__Internal")]
		public static extern void _SPAddParameters(string json);
		
		[DllImport ("__Internal")]
		public static extern void _SPClearParameters();

		private static readonly Dictionary<SPLogLevel, int> logLevelMapping = new Dictionary<SPLogLevel, int>
		{
			// Those mappings must be keep synched with the SPLogLevel.h
			{ SPLogLevel.Verbose, 0 },
			{ SPLogLevel.Debug, 10 },
			{ SPLogLevel.Info, 20 },
			{ SPLogLevel.Warn, 30 },
			{ SPLogLevel.Error, 40 },
			{ SPLogLevel.Fatal, 50 },
			{ SPLogLevel.Off, 60 },

		};


		public IOSSponsorPayPlugin(string objectGameName)
		{
			_SPSetCallbackGameObjectName(objectGameName);
			_SPSetPluginVersion(SponsorPayPlugin.PluginVersion);
		}

		//Start method
		public string Start(string appId, string userId, string securityToken)
		{
			return _SPStartSDK(appId, userId, securityToken);
		}

		// Rewarded Actions
		public void ReportActionCompletion(string credentialsToken, string actionId)
		{
			_SPReportActionCompletion(credentialsToken, actionId);
		}

		//OFW
		public void LaunchOfferWall(string credentialsToken, string currencyName, string placementId)
		{
			_SPLaunchOfferWall(credentialsToken, currencyName, placementId);
		}

		//VCS
   
		public void RequestNewCoins(string credentialsToken, string currencyName, string currencyId)
		{
			if(string.IsNullOrEmpty(currencyName)) {
				_SPSendDeltaOfCoinsRequest(credentialsToken, currencyId);
			} else {
				throw new System.InvalidOperationException("This method is not supported on this platform");
			}
		}
			
		public void ShowVCSNotifications(bool showNotification)
		{
			_SPSetShouldShowNotificationOnVCSCoins(showNotification ? 1 : 0);
		}

		//BrandEngage
		public void RequestBrandEngageOffers(string credentialsToken, string currencyName, bool queryVCS, string currencyId, string placementId)
		{
			_SPRequestBrandEngageOffers(credentialsToken, currencyName, queryVCS ? 1 : 0, currencyId, placementId);
		}
		
		public void StartBrandEngage()
		{
			_SPStartBrandEngage();
		}
		
		public void ShowBrandEngageRewardNotification(bool showNotification)
		{
			_SPSetShouldShowBrandEngageRewardNotification(showNotification ? 1: 0);
		}

		//Interstitial
		public void RequestInterstitialAds(string credentialsToken, string placementId) 
		{
			_SPRequestIntersitialAds(credentialsToken, placementId);
		}

		public void ShowInterstitialAd() 
		{
			_SPShowInterstitialAd();
		}

		// CacheManager
		public void VideoDownloadPause(bool pause)
		{
			_SPVideoDownloadPause(pause);
		}
		
		//Logging
		public void EnableLogging(bool shouldEnableLogging)
		{
			_SPEnableLogging(shouldEnableLogging ? 1 : 0);
		}

		public void SetLogLevel(SponsorPay.SPLogLevel logLevel)
		{
			_SPSetLogLevel(logLevelMapping[logLevel]);
		}

		//Global parameter provider
		public void AddParameters(string json)
		{
			_SPAddParameters(json);
		}
		
		public void RemoveAllParameters()
		{
			_SPClearParameters();
		}
		
	}
#endif

}
