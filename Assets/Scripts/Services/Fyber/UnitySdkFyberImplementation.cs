using UnityEngine;
using System.Collections;
using System;
#if UNITY_IPHONE || UNITY_ANDROID_
using SponsorPay;

public class UnitySdkFyberImplementation : MonoBehaviour, FyberImplementation
{
    const bool LOGS_ARE_ENABLED = false;

    struct VideoRequestData
    {
        bool showWhenReady;
        Action<bool> callback;
        
        public bool ShowWhenReady { get { return showWhenReady; } }
        public Action<bool> Callback { get { return callback; } }

        public VideoRequestData(bool showWhenReady, Action<bool> callback)
        {
            this.showWhenReady = showWhenReady;
            this.callback = callback;
        }
    }

    public bool RewardedVideoIsReady { get { return videoIsReady; } }
    public bool InterstitialIsReady { get { return InterstitialIsReady; } }
    public bool VideoRequestResponseIsReceived { get { return videoRequestResponseIsReceived; } }

    bool intersitialIsReady = false;
    bool videoIsReady = false;
    bool videoRequestResponseIsReceived = false;

    SponsorPayPlugin sponsorPayPlugin;

    VideoRequestData videoRequest;
    
    public void Initialize(string appId, string securityToken, string userId)
    {
        Log("Initialize");

        //SponsorPayPluginMonoBehaviour[] sponsorPayComponents = GameObject.FindObjectsOfType<SponsorPayPluginMonoBehaviour>();
        //Asserts.Assert(sponsorPayComponents == null);
        gameObject.AddComponent<SponsorPayPluginMonoBehaviour>();

        sponsorPayPlugin = SponsorPayPluginMonoBehaviour.PluginInstance;

        sponsorPayPlugin.EnableLogging(true);
        sponsorPayPlugin.SetLogLevel(SPLogLevel.Debug);

        RegisterDelegates();

		sponsorPayPlugin.Start(appId, userId, securityToken);
    }

    void RegisterDelegates()
    {
        Log("Register Delegates");

        // Offer Wall
        sponsorPayPlugin.OnOfferWallResultReceived += new OfferWallResultHandler(OnOfferWallResult);

        // Rewarded Video Request
        sponsorPayPlugin.OnBrandEngageRequestResponseReceived += new BrandEngageRequestResponseReceivedHandler(OnRewardedVideoRequestResponse);
        sponsorPayPlugin.OnBrandEngageRequestErrorReceived += new BrandEngageRequestErrorReceivedHandler(OnRewardedVideoRequestError);

        // Rewarded Video Show
        sponsorPayPlugin.OnBrandEngageResultReceived += new BrandEngageResultHandler(OnRewardedVideoResult);

        // Interstitial Request
        sponsorPayPlugin.OnInterstitialRequestResponseReceived += new InterstitialRequestResponseReceivedHandler(OnInterstitialRequestResponse);
        sponsorPayPlugin.OnInterstitialRequestErrorReceived += new InterstitialRequestErrorReceivedHandler(OnInterstitialRequestError);

        // Interstitial Show
        sponsorPayPlugin.OnInterstitialStatusCloseReceived += new InterstitialStatusCloseHandler(OnInterstitialClosed);
        sponsorPayPlugin.OnInterstitialStatusErrorReceived += new InterstitialStatusErrorHandler(OnInterstitialError);
		
    }

    public void LaunchOfferWall()
    {
        Log("Launch OfferWall");

        sponsorPayPlugin.LaunchOfferWall(null);
    }

    public void CacheRewardedVideo()
    {
        RequestRewardedVideo(false, null);
    }

    public void CacheAndLaunchRewardedVideo(Action<bool> callback)
    {
        RequestRewardedVideo(true, callback);
    }

    void RequestRewardedVideo(bool showWhenReady, Action<bool> callback)
    {
        Log("Request Rewarded Video");

        if (showWhenReady)
            ShowLoadingPopup();

        videoIsReady = false;
        videoRequestResponseIsReceived = false;
        videoRequest = new VideoRequestData(showWhenReady, callback);

        sponsorPayPlugin.RequestBrandEngageOffers(null, false);
    }

    public void LaunchRewardedVideo(Action<bool> callback)
    {
        Log("Launch Rewarded Video - videoIsReady: " + videoIsReady);
        
        if (!videoIsReady)
        {
            callback(false);
            callback = null;
            return;
        }

        ShowLoadingPopup();

        videoRequest = new VideoRequestData(false, callback);

        sponsorPayPlugin.StartBrandEngage();
        videoIsReady = false;
    }

    public void RequestInterstitial(string placement)
    {
        Log("RequestInterstitial: " + placement);
        sponsorPayPlugin.RequestInterstitialAds(null, placement);
    }

    public void ShowInterstitial()
    {
        Log("Show Interstitial - intersitialIsReady: " + intersitialIsReady);

        if (!intersitialIsReady)
        {
            return;
        }

        sponsorPayPlugin.ShowInterstitialAd();
    }

    public void AbortAnyPendigVideoRequest()
    {
        videoRequest = new VideoRequestData(false, null);
    }

    #region Delegate Methods

    void OnOfferWallResult(string message)
    {
        Log("OfferWall Result: " + message);
    }

    void OnRewardedVideoRequestResponse(bool offersAreAvailable)
    {
        Log("Rewarded Video Request response - offersAreAvailable: " + offersAreAvailable);
        videoIsReady = offersAreAvailable;
        videoRequestResponseIsReceived = true;

        if (videoIsReady &&
            videoRequest.ShowWhenReady &&
            OnTheRunFyberManager.Instance.CurrentPageAllowsVideos())
            LaunchRewardedVideo(videoRequest.Callback);

        SendButtonRefreshSignal();
    }

    void OnRewardedVideoRequestError(string message)
    {
        Log("Rewarded Video Request Error - " + message);
        videoIsReady = false;
        videoRequestResponseIsReceived = true;

        SendButtonRefreshSignal();
    }

    void OnRewardedVideoResult(string message)
    {
        const string CLOSE_FINISHED = "CLOSE_FINISHED";
        const string CLOSE_ABORTED = "CLOSE_ABORTED";
        const string ERROR = "ERROR";

        Log("Rewarded Video Result - " + message);

        SendButtonRefreshSignal();

        HideLoadingPopup();

        bool success = message.Equals(CLOSE_FINISHED);

        if (videoRequest.Callback != null )
            videoRequest.Callback(success);
        videoRequest = new VideoRequestData(false, null);

        CacheRewardedVideo();
    }

    void OnInterstitialRequestResponse(bool adsAreAvailable)
    {
        Log("Interstitial Request Response - adsAreAvailable: " + adsAreAvailable);
        intersitialIsReady = adsAreAvailable;
    }

    void OnInterstitialRequestError(string errorMessage)
    {
        Log("Interstitial Request Error - " + errorMessage);
    }
    
    public void OnInterstitialClosed(string closeReason)
    {
        Log("Interstitial Closed - " + closeReason);
    }
    
    public void OnInterstitialError(string errorMessage)
    {
        Log("Interstitial Error - " + errorMessage);
    }

    #endregion

    void SendButtonRefreshSignal()
    {
        if (Manager<UIManager>.Get().ActivePage != null)
        {
            Log("------------------ Refreshing Free Coins Button on Active Page");
            Manager<UIManager>.Get().ActivePage.SendMessage("RefreshVideoButton", SendMessageOptions.DontRequireReceiver);
        }
        if (Manager<UIManager>.Get().FrontPopup != null)
        {
            Log("------------------ Refreshing Free Coins Button on Front Popup");
            Manager<UIManager>.Get().FrontPopup.SendMessage("RefreshVideoButton", SendMessageOptions.DontRequireReceiver);
        }
    }

    void Log(string logStr)
    {
        if (LOGS_ARE_ENABLED)
            Debug.Log("### Fyber Unity SDK - " + logStr);
    }

    #region Loading Popup

    bool loadingPoupIsTriggered = false;

    void ShowLoadingPopup()
    {
        if (!loadingPoupIsTriggered)
        {
			//Debug.Log("                                           >>>>>>>>>>>>>>>> SHOW LOADING POPUP");
            loadingPoupIsTriggered = true;
            Manager<UIRoot>.Get().ShowLoadingPopup();
            StartCoroutine(LoadingPopupTimeoutCoroutine());
        }
    }

    void HideLoadingPopup()
    {
        if (loadingPoupIsTriggered)
		{
			//Debug.Log("                                           >>>>>>>>>>>>>>>> HIDE LOADING POPUP");
            Manager<UIRoot>.Get().HideLoadingPopup();
            loadingPoupIsTriggered = false;
            //StopAllCoroutines();
        }
    }

    IEnumerator LoadingPopupTimeoutCoroutine()
    {
        const float TIMEOUT_SECONDS = 5.0f;

        //Debug.Log("                                           >>>>>>>>>>>>>>>> STARTING LOADING POPUP TIMEOUT");
        float timeBefore = TimeManager.Instance.MasterSource.TotalTime;
        
        while (TimeManager.Instance.MasterSource.TotalTime - timeBefore < TIMEOUT_SECONDS)
		{
			//Debug.Log("                                           >>>>>>>>>>>>>>>> LOADING POPUP TIMEOUT IN PROGRESS - ELAPSED : " + (TimeManager.Instance.MasterSource.TotalTime - timeBefore).ToString());
            if (!loadingPoupIsTriggered)
            {
                //Debug.Log("                                           >>>>>>>>>>>>>>>> LOADING POPUP HAS BEEN DISMISSED BEFORE TIMOUT - ELAPSED TIME: " + (TimeManager.Instance.MasterSource.TotalTime - timeBefore).ToString());
                yield break;
            }

            yield return new WaitForEndOfFrame();
        }

        //Debug.Log("                                           >>>>>>>>>>>>>>>> LOADING POPUP TIMED OUT - DISMISSING POPUP AND ABORTING PENDING REQUESTS");
        HideLoadingPopup();
        AbortAnyPendigVideoRequest();
    }

    #endregion
}

#endif