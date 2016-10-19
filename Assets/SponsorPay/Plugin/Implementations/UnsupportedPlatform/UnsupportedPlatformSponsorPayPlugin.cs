using System;

namespace SponsorPay
{
	public class UnsupportedPlatformSponsorPayPlugin : ISponsorPayPlugin
	{

		public UnsupportedPlatformSponsorPayPlugin()
		{
			UnityEngine.Debug.Log( "WARNING: SponsorPay plugin is not available on this platform." );
		}

		public string Start(string appId, string userId, string securityToken)
		{
			SPUtils.printWarningMessage();
			return "NOT A VALID TOKEN";
		}
		
		public void ReportActionCompletion(string credentialsToken, string actionId) 
		{
			SPUtils.printWarningMessage();
		}
		
		public void LaunchOfferWall(string credentialsToken, string currencyName, string placementId)
		{
			SPUtils.printWarningMessage();
		}

		public void RequestNewCoins(string credentialsToken, string currencyName, string currencyId)
		{
			SPUtils.printWarningMessage();
		}
    
		public void ShowVCSNotifications(bool showNotification)
		{
			SPUtils.printWarningMessage();
		}

		// BrandEngage
		public void RequestBrandEngageOffers(string credentialsToken, string currencyName, bool queryVCS, string currencyId, string placementId)
		{
			SPUtils.printWarningMessage();
		}
		
		public void StartBrandEngage()
		{
			SPUtils.printWarningMessage();
		}
		
		public void ShowBrandEngageRewardNotification(bool showNotification)
		{
			SPUtils.printWarningMessage();
		}

		//Interstitial
		public void RequestInterstitialAds(string credentialsToken, string placementId) 
		{
			SPUtils.printWarningMessage();
		}

		public void ShowInterstitialAd() 
		{
			SPUtils.printWarningMessage();
		}

		public void VideoDownloadPause(bool pause)
		{
			SPUtils.printWarningMessage();
		}

		//Logging
		public void EnableLogging(bool shouldEnableLogging)
		{
			SPUtils.printWarningMessage();
		}

		public void SetLogLevel(SPLogLevel logLevel) {
			SPUtils.printWarningMessage();
		}

		//Global parameter provider
		public void AddParameters(string json)
		{
			SPUtils.printWarningMessage();
		}
		
		public void RemoveAllParameters()
		{
			SPUtils.printWarningMessage();
		}

	}

}


