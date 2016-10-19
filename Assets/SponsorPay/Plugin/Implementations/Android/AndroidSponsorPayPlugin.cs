using UnityEngine;
using System;

namespace SponsorPay
{
	#if UNITY_ANDROID
	public class AndroidSponsorPayPlugin : ISponsorPayPlugin
	{
		
		public AndroidSponsorPayPlugin(string gameObjectName)
		{
			AndroidJNI.AttachCurrentThread();
			CallbackGameObjectName = gameObjectName;
			InitPlugin();
		}
		
		private string CallbackGameObjectName { get; set; }
		
		private AndroidJavaObject sponsorPayWrapper;
		private AndroidJavaObject spOfferWall;
		private AndroidJavaObject spCurrency;
		private AndroidJavaObject spBrandEngage;
		private AndroidJavaObject spInterstitial;
		private AndroidJavaObject spRewardedAction;
		
		private AndroidJavaObject SPOfferWall
		{
			get
			{
				if (null == spOfferWall)
				{
					spOfferWall = new AndroidJavaObject("com.sponsorpay.unity.SPUnityOfferWallWrapper", CallbackGameObjectName);
				}
				return spOfferWall;
			}
		}
		
		private AndroidJavaObject SPCurrency
		{
			get
			{
				if (null == spCurrency)
				{
					spCurrency = new AndroidJavaObject("com.sponsorpay.unity.SPUnityCurrencyWrapper", CallbackGameObjectName);
				}
				return spCurrency;
			}
		}
		
		private AndroidJavaObject SPWrapper
		{
			get
			{
				if (sponsorPayWrapper == null)
				{
					sponsorPayWrapper = new AndroidJavaObject("com.sponsorpay.unity.SPUnityWrapper", CallbackGameObjectName);
				}
				return sponsorPayWrapper;
			}
		}
		
		private AndroidJavaObject SPBrandEngage
		{
			get
			{
				if (spBrandEngage == null)
				{
					spBrandEngage = new AndroidJavaObject("com.sponsorpay.unity.SPUnityBrandEngageWrapper", CallbackGameObjectName);
				}
				return spBrandEngage;
			}
		}
		
		private AndroidJavaObject SPInterstitial
		{
			get
			{
				if (spInterstitial == null)
				{
					spInterstitial = new AndroidJavaObject("com.sponsorpay.unity.SPUnityInterstitialWrapper", CallbackGameObjectName);
				}
				return spInterstitial;
			}
		}
		
		private AndroidJavaObject SPRewardedAction
		{
			get
			{
				if (spRewardedAction == null)
				{
					spRewardedAction = new AndroidJavaObject("com.sponsorpay.unity.SPUnityRewardedActionWrapper", CallbackGameObjectName);
				}
				return spRewardedAction;
			}
		}
		
		public string Start(string appId, string userId, string securityToken)
		{
			return SPWrapper.Call<string>("start", appId, userId, securityToken);
		}
		
		public void ReportActionCompletion(string credentialsToken, string actionId) 
		{
			SPRewardedAction.Call("reportActionCompletion", credentialsToken, actionId);
		}
		
		public void LaunchOfferWall(string credentialsToken, string currencyName, string placementId)
		{
			SPOfferWall.Call("launchOfferWall", credentialsToken, currencyName, placementId);
		}
		
		public void RequestNewCoins(string credentialsToken, string currencyName, string currencyId)
		{
			SPCurrency.Call("requestNewCoins", credentialsToken, currencyName, currencyId);
		}
		
		public void ShowVCSNotifications (bool showNotification)
		{
			SPCurrency.Call("showVCSNotification", showNotification);
		}
		
		// BrandEngage
		public void RequestBrandEngageOffers(string credentialsToken, string currencyName, bool queryVCS, string placementId, string currencyId)
		{
			SPBrandEngage.Call("requestOffers", credentialsToken, currencyName, queryVCS, placementId, currencyId);
		}
		
		public void StartBrandEngage()
		{
			SPBrandEngage.Call("startEngagement");
		}
		
		public void ShowBrandEngageRewardNotification(bool showNotification)
		{
			SPBrandEngage.Call("showRewardsNotification", showNotification);
		}
		
		//Interstitial
		public void RequestInterstitialAds(string credentialsToken, string placementId) 
		{
			SPInterstitial.Call("requestAds", credentialsToken, placementId);
		}
		
		public void ShowInterstitialAd() 
		{
			SPInterstitial.Call("showAd");
		}
		
		//Logging
		public void EnableLogging(bool shouldEnableLogging)
		{
			using (AndroidJavaObject spUnityPlugin = new AndroidJavaObject("com.sponsorpay.unity.SPLoggerWrapper")) 
			{ 
				spUnityPlugin.Call("enableLogging", shouldEnableLogging);
			}
		}

		public void SetLogLevel(SPLogLevel logLevel)
		{
			SPUtils.printWarningMessage();
		}
		
		//Global parameter provider
		public void AddParameters(string json)
		{
			using (AndroidJavaObject parameterProvider = new AndroidJavaObject("com.sponsorpay.unity.SPUnityParameterProvider"))
			{
				parameterProvider.Call("addParameters", json);
			}
		}
		
		public void RemoveAllParameters()
		{
			using (AndroidJavaObject parameterProvider = new AndroidJavaObject("com.sponsorpay.unity.SPUnityParameterProvider")) 
			{ 
				parameterProvider.Call("clear");
			}
		}

		public void  VideoDownloadPause(bool pause)
		{
			using (AndroidJavaObject cacheManager = new AndroidJavaObject("com.sponsorpay.unity.SPCacheWrapper"))
			{
				cacheManager.Call("setPrecachingState", pause);
			}
		}
		
		
		// Helper method
		private void InitPlugin()
		{
			using (AndroidJavaObject spUnityPlugin = new AndroidJavaObject("com.sponsorpay.unity.SPUnityPlugin")) 
			{ 
				spUnityPlugin.Call("setPluginVersion", SponsorPayPlugin.PluginVersion);
				var infoLocation = System.IO.Path.Combine(Application.streamingAssetsPath, "adapters.info");
				var configLocation = System.IO.Path.Combine(Application.streamingAssetsPath, "adapters.config"); 
				spUnityPlugin.Call("setAdaptersInfoLocation", infoLocation);
				spUnityPlugin.Call("setAdaptersConfigLocation", configLocation);
			}
		}
		
	}
	#endif
	
}
