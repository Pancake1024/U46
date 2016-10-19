using UnityEngine;
using System.Collections;
using System;

public class OnTheRunFyberManager : Manager<OnTheRunFyberManager>
{
    #region Singleton instance
    protected static OnTheRunFyberManager instance = null;

    public static OnTheRunFyberManager Instance
    {
        get
        {
            return Manager<OnTheRunFyberManager>.Get();
        }
    }
    #endregion
	
	const bool LOGS_ARE_ENABLED = false;

    public string productionAppId;
    public string productionSecurityToken;

    public string devAppId;
    public string devSecurityToken;

    string appId;
    string securityToken;

    FyberImplementation implementation;

    public bool VideoIsReady { get { return implementation.RewardedVideoIsReady; } }
    public bool VideoRequestResponseIsReceived { get { return implementation.VideoRequestResponseIsReceived; } }

    new void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    void Initialize()
    {
		Log("Initialize");

        SetImplementation();
        SetIds();
        implementation.Initialize(appId, securityToken, OnTheRunOmniataManager.UserId);
        StartCoroutine(VideoCacheRefreshCoroutine());   //CacheRewardedVideo();
    }

    void SetImplementation()
	{
#if UNITY_EDITOR
        implementation = gameObject.AddComponent<EditorFyberImplementation>();
#elif UNITY_IPHONE
        implementation = gameObject.AddComponent<UnitySdkFyberImplementation>();
#elif UNITY_ANDROID
		implementation = gameObject.AddComponent<EditorFyberImplementation>();
        //implementation = gameObject.AddComponent<UnitySdkFyberImplementation>();
#elif UNITY_WP8
        implementation = gameObject.AddComponent<VoidFyberImplementation>();
#elif UNITY_WEBPLAYER
        implementation = gameObject.AddComponent<VoidFyberImplementation>();
#else
        implementation = gameObject.AddComponent<EditorFyberImplementation>();
#endif
    }

    void SetIds()
    {
#if UNITY_IPHONE && !UNITY_EDITOR
		if (SBS.Miniclip.MCUtilsBindings.isMassiveTestBuild())
        {
            appId = devAppId;
            securityToken = devSecurityToken;
        }
        else
        {
            appId = productionAppId;
            securityToken = productionSecurityToken;
        }
#elif UNITY_ANDROID
#   if DEVELOPMENT_BUILD
        appId = devAppId;
        securityToken = devSecurityToken;
#   else
        appId = productionAppId;
        securityToken = productionSecurityToken;
#   endif
#else
        appId = devAppId;
        securityToken = devSecurityToken;
#endif
    }

    public void LaunchOfferWall()
	{
		Log("LaunchOfferWall");
        implementation.LaunchOfferWall();
    }

    public void CacheRewardedVideo()
	{
        Log("CacheRewardedVideo");
        implementation.CacheRewardedVideo();
    }

    public void LaunchRewardedVideo(Action<bool> callback)
    {
        Log("LaunchRewardedVideo - ready: " + implementation.RewardedVideoIsReady + " - Current Page Allows Videos: " + CurrentPageAllowsVideos());

        if (CurrentPageAllowsVideos())
        {
            if (implementation.RewardedVideoIsReady)
                implementation.LaunchRewardedVideo(callback);
            else
                implementation.CacheAndLaunchRewardedVideo(callback);
        }
    }

    void RequestInterstitial()
	{
		Log("RequestInterstitial");
        const string TEST_PLACEMENT = "TEST_PLACEMENT";
        implementation.RequestInterstitial(TEST_PLACEMENT);
    }

    public void ShowInterstitial()
	{
		Log("ShowInterstitial");
        implementation.ShowInterstitial();
    }

	void Log(string logStr)
	{
		if (LOGS_ARE_ENABLED)
			Debug.Log("### Fyber Manager - " + logStr);
	}

    public bool CurrentPageAllowsVideos()
    {
        if (!Manager<UIManager>.Get())
            return false;

        string activePageName = Manager<UIManager>.Get().ActivePageName;
        string frontPopupName = Manager<UIManager>.Get().FrontPopup ? Manager<UIManager>.Get().FrontPopup.name : string.Empty;

        // This is a more restrictive condition: we allow just where there are the free video buttons.
        /*bool returnValue = false;
        if (activePageName.Equals("StartPage") ||
            activePageName.Equals("InAppPage") ||
            activePageName.Equals("IngamePage") && frontPopupName.Equals("SaveMePopup"))
            returnValue = true;*/

        // This is a less restrictive condition: allows everywhere with exception to the ingame page (unless the saveme popup is visible on the ingame page)
        bool returnValue = true;
        if (activePageName.Equals("IngamePage") && !frontPopupName.Equals("SaveMePopup"))
            returnValue = false;

        Log("------------------------------------------------------------ ACTIVE PAGE: " + activePageName + " - FRONT POPUP: " + frontPopupName + " - RETURNING: " + returnValue);

        return returnValue;
    }

    public void AbortAnyPendigVideoRequest()
    {
        implementation.AbortAnyPendigVideoRequest();
    }

    const float startingCacheDelay = 6.0f;
    const float videoCacheRefreshTime = 60.0f;

    IEnumerator VideoCacheRefreshCoroutine()
    {
        yield return new WaitForSeconds(startingCacheDelay);
    
        while (true)
        {
            Log("----------------------------------------------------------- VideoCacheRefreshCoroutine");

            if (!implementation.RewardedVideoIsReady)
            {
                Log("Triggering Video Cache Refresh.");
                CacheRewardedVideo();
            }
            else
                Log("It should be time to refresh the Video Cache, but video is ready.");

            yield return new WaitForSeconds(videoCacheRefreshTime);
        }
    }
}