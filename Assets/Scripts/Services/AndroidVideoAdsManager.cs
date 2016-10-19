using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

#if UNITY_ANDROID

public class AndroidVideoAdsManager : Manager<AndroidVideoAdsManager>
{
    #region Singleton instance
    protected static AndroidVideoAdsManager instance = null;

    public static AndroidVideoAdsManager Instance
    {
        get
        {
            return Manager<AndroidVideoAdsManager>.Get();
        }
    }
    #endregion

    public enum AdProvider
    {
        Flurry,
        AdColony
    }

    // From now on we'll use a "DEVELOPMENT_BUILD" global define symbol to do the same thing
    //const bool IS_INTERNAL_BUILD = false; 

    const string SBS_ANDROID_ADCOLONY_APPID = "app72a94c8d918b456fae";
    const string SBS_ANDROID_ADCOLONY_ZONEID = "vz9de018a4a0a54996a3";

    const string MINICLIP_ANDROID_ADCOLONY_APPID = "app820f3ef61a654f8b8c";
    const string MINICLIP_ANDROID_ADCOLONY_ZONEID = "vz16b234b96d9a4d999a";

    const string SBS_KINDLE_ADCOLONY_APPID = "appdc1c6e97bea24941be";
    const string SBS_KINDLE_ADCOLONY_ZONEID = "vzce757bb68464485aba";

    const string MINICLIP_KINDLE_ADCOLONY_APPID = "";
    const string MINICLIP_KINDLE_ADCOLONY_ZONEID = "";

    List<AdProvider> prioritizedProviders;  // Higher value = higher priority (sorted by decreasing priority)
    Dictionary<AdProvider, Action<bool>> adAvailabilityDelegatesByProvider;

    Action rewardCallback;

    new void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        StartCoroutine(InitializeWhenConfigDataIsReady());
    }

    IEnumerator InitializeWhenConfigDataIsReady()
    {
        while (!OnTheRunDataLoader.Instance.DataIsLoaded)
            yield return new WaitForEndOfFrame();

        Initialize();
    }

    void Initialize()
    {
        InitializePriorities();
        InitializeAdColony();
    }

    void InitializePriorities()
    {
        Dictionary<AdProvider, int> prioritiesByAdProvider = new Dictionary<AdProvider, int>();
        prioritiesByAdProvider.Add(AdProvider.Flurry, OnTheRunDataLoader.Instance.GetAndroidFlurryAdsPriority());
        prioritiesByAdProvider.Add(AdProvider.AdColony, OnTheRunDataLoader.Instance.GetAndroidAdcolonyPriority());

        prioritizedProviders = new List<AdProvider>();
        for (int i = 0; i < Enum.GetValues(typeof(AdProvider)).Length; i++)
            prioritizedProviders.Add((AdProvider)i);

        prioritizedProviders.Sort(
            delegate(AdProvider provider1, AdProvider provider2)
            {
                return -1 * prioritiesByAdProvider[provider1].CompareTo(prioritiesByAdProvider[provider2]);
            });
    }

    public void RequestVideoAd(Action rewardCallback)
    {
        this.rewardCallback = rewardCallback;

        foreach (AdProvider provider in prioritizedProviders)
        {
            //Debug.Log("####################### checking " + provider.ToString() + " ... IsAdAvailable: " + IsAdAvailable(provider));
            if (IsAdAvailable(provider))
            {
                RequestVideoAd(provider);
                return;
            }
        }

        NotifyNoVideosAvailable();
    }

    public bool AreVideoAdsAvailable()
    {
        foreach (AdProvider provider in prioritizedProviders)
            if (IsAdAvailable(provider))
                return true;

        return false;
    }

    bool IsAdAvailable(AdProvider provider)
    {
        switch (provider)
        {
            case AdProvider.Flurry:
                return IsFlurryAdAvailable();

            case AdProvider.AdColony:
                //return AdColony.IsVideoAvailable(DetermineAdColonyZoneId());
                return AdColony.IsV4VCAvailable(DetermineAdColonyZoneId());
        }

        return false;
    }

    void RequestVideoAd(AdProvider provider)
    {
        switch (provider)
        {
            case AdProvider.Flurry: RequestFlurryAdVideo(); break;
            case AdProvider.AdColony: RequestAdColonyAdVideo(); break;
        }
    }
    
    #region Flurry Ads
    bool IsFlurryAdAvailable()
    {
#if UNITY_EDITOR
        bool editorReturnValue = true;
        //Debug.Log("### ### ### IsFlurryAdAvailable - editor: returning " + editorReturnValue);

        return editorReturnValue;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        bool isFlurryAdAvailable = false;
        using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.ontherun.OnTheRunActivity"))
            isFlurryAdAvailable = jc.CallStatic<bool>("IsFlurryAdAvailable");
        
        //Debug.Log("### ### ### IsFlurryAdAvailable - " + isFlurryAdAvailable);
        return isFlurryAdAvailable;
#endif
    }

    void RequestFlurryAdVideo()
    {
        //Debug.Log("### ### ### RequestFlurryAdVideo");

#if UNITY_EDITOR
        FlurryAdVideoCallback("");
#endif
        
#if UNITY_ANDROID && !UNITY_EDITOR
        NativeDispatcher.Instance.RegisterEvent("onFlurryVideoAdWatched", FlurryAdVideoCallback);

        using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.ontherun.OnTheRunActivity"))
            jc.CallStatic("RequestFlurryAd");
#endif
    }

    void FlurryAdVideoCallback(string param)
    {
        //Debug.Log("### ### ### FlurryAdVideoCallback");
#if !UNITY_EDITOR
        NativeDispatcher.Instance.RemoveEvent("onFlurryVideoAdWatched");
#endif

        if (rewardCallback != null)
            rewardCallback();
        rewardCallback = null;
    }

    #endregion

    #region AdColony

    string DetermineAdColonyAppId()
    {
#if DEVELOPMENT_BUILD
#   if UNITY_ANDROID && UNITY_KINDLE
        return SBS_KINDLE_ADCOLONY_APPID;
#   else
        return SBS_ANDROID_ADCOLONY_APPID;
#   endif
#else
#   if UNITY_ANDROID && UNITY_KINDLE
        return MINICLIP_KINDLE_ADCOLONY_APPID;
#   else
        return MINICLIP_ANDROID_ADCOLONY_APPID;
#   endif
#endif
    }

    string DetermineAdColonyZoneId()
    {
#if DEVELOPMENT_BUILD
#   if UNITY_ANDROID && UNITY_KINDLE
        return SBS_KINDLE_ADCOLONY_ZONEID;
#   else
        return SBS_ANDROID_ADCOLONY_ZONEID;
#   endif
#else
#   if UNITY_ANDROID && UNITY_KINDLE
        return MINICLIP_KINDLE_ADCOLONY_ZONEID;
#   else
        return MINICLIP_ANDROID_ADCOLONY_ZONEID;
#   endif
#endif
    }

    void InitializeAdColony()
    {
        //AdColony.OnVideoFinished = AdColonyAdVideoCallback;
        AdColony.OnV4VCResult = AdColonyV4VCResult;

        string versionNum = "1.0";
        string storeName = string.Empty;
#if UNITY_ANDROID && UNITY_KINDLE
        storeName = "amazon";
#else
        storeName = "google";
#endif
        string app_version = "version:" + versionNum + ",store:" + storeName;

        string app_id = DetermineAdColonyAppId();
        string[] zone_ids = { DetermineAdColonyZoneId() };

        //Debug.Log("### ### ### InitializeAdColony - AdColony.InitializeAdColony - app_version: " + app_version + ", app_id: " + app_id);
        AdColony.Configure(app_version, app_id, zone_ids);
    }

    void RequestAdColonyAdVideo()
    {
        //Debug.Log("### ### ### RequestAdColonyAdVideo - AdColony.IsVideoAvailable: " + AdColony.IsVideoAvailable(DetermineAdColonyZoneId()));

        /*if (AdColony.IsVideoAvailable(DetermineAdColonyZoneId()))
            AdColony.ShowVideoAd(DetermineAdColonyZoneId());*/

        if (AdColony.IsV4VCAvailable(DetermineAdColonyZoneId()))
            AdColony.ShowV4VC(false, DetermineAdColonyZoneId());
    }

    void AdColonyAdVideoCallback(bool adWasShown)
    {
        //Debug.Log("### ### ### AdColonyAdVideoCallback - AdColonyAdVideoCallback - adWasShown: " + adWasShown);

        if (adWasShown && rewardCallback != null)
            rewardCallback();
        rewardCallback = null;
    }

    void AdColonyV4VCResult(bool success, string name, int amount)
    {
        if (success && rewardCallback != null)
            rewardCallback();
        rewardCallback = null;
    }
    #endregion

    void NotifyNoVideosAvailable()
    {
        //Debug.Log("### ### ### NotifyNoVideosAvailable");

        string alertDialogTitle = OnTheRunDataLoader.Instance.GetLocaleString("sorry");
        string alertDialogMessage = OnTheRunDataLoader.Instance.GetLocaleString("no_video_available");
        string okButtonText = OnTheRunDataLoader.Instance.GetLocaleString("ok");

        rewardCallback = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.AlertDialogBindings"))
            jc.CallStatic("OpenSingleButtonAlertDialogMessage", alertDialogTitle, alertDialogMessage, okButtonText);
#endif
    }

    public void NotifyCoinsReward()
    {
        string alertDialogTitle = OnTheRunDataLoader.Instance.GetLocaleString("congratulations");
        //string alertDialogMessage = OnTheRunDataLoader.Instance.GetLocaleString("coins_reward_prefix") + " " + OnTheRunDataLoader.Instance.GetAndroidVideoCoinsReward() + " " + OnTheRunDataLoader.Instance.GetLocaleString("coins_reward_postfix");
        string alertDialogMessage = OnTheRunDataLoader.Instance.GetLocaleString("coins_reward_prefix") + " " + OnTheRunCoinsService.Instance.FreeCoinsReward + " " + OnTheRunDataLoader.Instance.GetLocaleString("coins_reward_postfix");
        string okButtonText = OnTheRunDataLoader.Instance.GetLocaleString("ok");

        rewardCallback = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.AlertDialogBindings"))
            jc.CallStatic("OpenSingleButtonAlertDialogMessage", alertDialogTitle, alertDialogMessage, okButtonText);
#endif
    }
}

#endif