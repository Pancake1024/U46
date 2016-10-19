using System;

namespace SponsorPay
{

	public enum SPLogLevel {
		Verbose = 0,
		Debug,
		Info,
		Warn,
		Error,
		Fatal,
		Off,
	};

	public interface ISponsorPayPlugin
	{
		
		// Start method
		string Start(string appId, string userId, string securityToken);
		
		//Rewarded actions
		//void ReportActionCompletion(string actionId);
		void ReportActionCompletion(string credentialsToken, string actionId);
		
		//OFW
		void LaunchOfferWall(string credentialsToken, string currencyName, string placementId);
		
		//VCS
		void RequestNewCoins(string credentialsToken, string currencyName, string currencyId);
		
		void ShowVCSNotifications (bool showNotification);
		
		//MBE
		void RequestBrandEngageOffers(string credentialsToken, string currencyName, bool queryVCS, string currencyId, string placementId);
		
		void StartBrandEngage();
		
		void ShowBrandEngageRewardNotification(bool showNotification);

		//Interstitial
		void RequestInterstitialAds(string credentialsToken, string placementId);

		void ShowInterstitialAd();

		void VideoDownloadPause(bool pause);

		//Logging
		void EnableLogging(bool shouldEnableLogging);
		void SetLogLevel(SPLogLevel logLevel);


		//Global parameter provider
		void AddParameters(string json);
		void RemoveAllParameters();

	}
	
}

